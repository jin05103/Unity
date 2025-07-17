using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading;
using System;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public static Lobby CurrentLobby { get; private set; }
    [SerializeField] LobbyPanelManager lobbyPanelManager;
    [SerializeField] NetworkInitializer networkInitializer;
    [SerializeField] MainMenuUI mainMenuUI;
    [SerializeField] GameObject loadingPanel;

    private CancellationTokenSource heartbeatCts;
    private CancellationTokenSource pollingCts;
    private Task pollingTask;

    bool isPolling = false;
    bool hasEnteredGameScene = false;
    bool hasReadyed = false;
    const int maxPlayers = 4;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        loadingPanel.SetActive(true);
    }

    private async void Start()
    {
        if (CurrentLobby != null)
        {
            hasReadyed = false;
            hasEnteredGameScene = false;
            bool isHost = CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;
            if (isHost)
            {
                // ResetLobbySetting();

                await LobbyService.Instance.UpdateLobbyAsync(
                    CurrentLobby.Id,
                    new UpdateLobbyOptions
                    {
                        IsLocked = false,
                    }
                );

                if (isHost)
                {
                    StartHeartbeatLoop();
                }
            }

            // mainMenuUI.OpenLobbyPanel();

            lobbyPanelManager.SetLobbyPlayers(CurrentLobby);
            StartLobbyPolling();

            loadingPanel.SetActive(false);
        }

        loadingPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        StopLobbyPolling();
    }

    Dictionary<string, PlayerDataObject> GetPlayerData()
    {
        var user = FirebaseManager.Instance.CurrentUserData;
        return new Dictionary<string, PlayerDataObject>
        {
            { "nickname", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, user.Nickname) },
            { "wins", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, user.Wins.ToString()) },
            { "losses", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, user.Losses.ToString()) }
        };
    }

    public Lobby CurrentLobbySetter
    {
        set { CurrentLobby = value; }
    }

    public async Task<Lobby> CreateLobby(string lobbyName)
    {
        var options = new CreateLobbyOptions
        {
            Player = new Player(AuthenticationService.Instance.PlayerId, data: GetPlayerData()),
            Data = new Dictionary<string, DataObject>
            {
                { "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, "") },
                { "playClicked", new DataObject(DataObject.VisibilityOptions.Public, "false") }
            }
        };

        try
        {
            var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            CurrentLobby = lobby;
            FlagReset();
            lobbyPanelManager.SetLobbyPlayers(CurrentLobby);
            StartLobbyPolling();

            bool isHost = lobby.HostId == AuthenticationService.Instance.PlayerId;
            if (isHost)
            {
                StartHeartbeatLoop();
            }
            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("CreateLobby 실패: " + e.Message);
            return null;
        }
    }

    public async Task<Lobby> JoinLobbyByCode(string lobbyCode)
    {
        var options = new JoinLobbyByCodeOptions
        {
            Player = new Player(AuthenticationService.Instance.PlayerId, data: GetPlayerData())
        };

        try
        {
            var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            CurrentLobby = lobby;
            lobbyPanelManager.SetLobbyPlayers(CurrentLobby);
            StartLobbyPolling();
            FlagReset();

            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("JoinLobby 실패: " + e.Message);
            return null;
        }
    }

    public async Task<Lobby> JoinLobbyById(string lobbyId)
    {
        var options = new JoinLobbyByIdOptions
        {
            Player = new Player(AuthenticationService.Instance.PlayerId, data: GetPlayerData())
        };

        try
        {
            var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            CurrentLobby = lobby;
            lobbyPanelManager.SetLobbyPlayers(CurrentLobby);
            StartLobbyPolling();
            FlagReset();

            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("JoinLobbyById 실패: " + e.Message);
            return null;
        }
    }

    private void FlagReset()
    {
        hasEnteredGameScene = false;
        hasReadyed = false;
    }

    private async void StartHeartbeatLoop()
    {
        heartbeatCts = new CancellationTokenSource();
        try
        {
            while (!heartbeatCts.Token.IsCancellationRequested)
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
                await Task.Delay(TimeSpan.FromSeconds(15), heartbeatCts.Token);
            }
        }
        catch (TaskCanceledException) { }
    }

    public void StartLobbyPolling()
    {
        // 중복 폴링 방지
        if (pollingTask != null && !pollingTask.IsCompleted)
            return;

        isPolling = true;
        FlagReset();

        pollingCts?.Cancel(); // 기존 폴링이 있으면 중단
        pollingCts = new CancellationTokenSource();

        pollingTask = PollLobbyAsync(pollingCts.Token);
    }

    private async Task PollLobbyAsync(CancellationToken token)
    {
        bool isHost;

        while (!token.IsCancellationRequested && isPolling && CurrentLobby != null)
        {
            isHost = CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;
            try
            {
                var updatedLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
                var gameStartingFlag = updatedLobby.Data.ContainsKey("gameStarting") ? updatedLobby.Data["gameStarting"] : null;
                if (updatedLobby != null)
                {
                    CurrentLobby = updatedLobby;
                    if (lobbyPanelManager != null && (gameStartingFlag == null || gameStartingFlag.Value != "true"))
                        lobbyPanelManager.SetLobbyPlayers(CurrentLobby);
                }

                if (!hasEnteredGameScene &&
                    CurrentLobby.Data != null &&
                    gameStartingFlag != null && gameStartingFlag.Value == "true")
                {
                    hasEnteredGameScene = true;
                    Debug.Log("Game is starting! Loading GameScene...");
                    if (loadingPanel != null)
                    {
                        loadingPanel.SetActive(true);
                    }

                    Debug.Log($"Is Host: {isHost}");
                    if (isHost)
                    {
                        var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
                        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
                        NetworkManager.Singleton.StartHost();
                        await networkInitializer.Init();

                        var updateOptions = new UpdateLobbyOptions
                        {
                            Data = new Dictionary<string, DataObject>
                        {
                            { "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
                            { "playClicked", new DataObject(DataObject.VisibilityOptions.Public, "true") }
                        }
                        };
                        await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateOptions);

                    }
                }
                Debug.Log(CurrentLobby.Data["playClicked"].Value);

                if (!hasReadyed && CurrentLobby.Data["playClicked"].Value == "true")
                {
                    hasReadyed = true;
                    StopLobbyPolling();
                    Debug.Log("Play button clicked by host, starting game...");
                    if (!isHost)
                    {
                        // 클라이언트는 호스트가 게임 시작을 클릭하면 씬을 로드함
                        await networkInitializer.Init();
                    }
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to poll lobby: " + e.Message);
                if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                {
                    Debug.LogWarning("Lobby not found, leaving lobby.");
                    LobbyLost();
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Unexpected error while polling lobby: " + ex.Message);
            }

            try
            {
                await Task.Delay(2000, token);
            }
            catch (TaskCanceledException) { break; }
        }
    }

    public async void ResetLobbySetting()
    {
        if (CurrentLobby == null) return;

        var updateOptions = new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
                {
                    { "relayJoinCode", new DataObject(CurrentLobby.Data["relayJoinCode"].Visibility, null) },
                    { "gameStarting", new DataObject(CurrentLobby.Data["gameStarting"].Visibility, "false") },
                    { "playClicked", new DataObject(CurrentLobby.Data["playClicked"].Visibility, "false") }
                }
        };
        await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateOptions);
    }

    public void StopLobbyPolling()
    {
        isPolling = false;
        pollingCts?.Cancel();
        heartbeatCts?.Cancel();
    }

    public async Task<List<Lobby>> GetLobbyList()
    {
        var result = await LobbyService.Instance.QueryLobbiesAsync();
        return result.Results;
    }

    public async Task LeaveLobby()
    {
        if (CurrentLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
            }
        }

        CurrentLobby = null;
        StopLobbyPolling();
    }

    public void LobbyLost()
    {
        CurrentLobby = null;
        StopLobbyPolling();
        lobbyPanelManager.ClearLobbyPlayers();
        mainMenuUI.OpenMainMenuPanel();
    }

    public async Task DeleteCurrentLobby()
    {
        if (CurrentLobby == null) return;
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
        CurrentLobby = null;
        StopLobbyPolling();
    }
}