using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    // public Image expImage;
    [SerializeField] ExpPanel expPanel;
    [SerializeField] StatPanel StatPanel;
    [SerializeField] GameObject endPanel;

    private Dictionary<string, int> playerRanks = new Dictionary<string, int>();

    public bool isGameStarted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        StartCoroutine(StartGameCoroutine());
    }

    public IEnumerator StartGameCoroutine()
    {
        if (isGameStarted)
        {
            Debug.LogWarning("Game has already started!");
            yield break;
        }

        isGameStarted = true;
        Debug.Log("Game has started!");

        // 조건 충족까지 대기
        Debug.Log(PlayerSpawner.Instance.GetPlayerCount() + " players are ready.");
        Debug.Log(GameSession.Instance.playerCount + " players are required.");
        while (PlayerSpawner.Instance.GetPlayerCount() < GameSession.Instance.playerCount)
        {
            Debug.Log(GameSession.Instance.playerCount);
            yield return new WaitForSeconds(0.1f);
        }

        MapManager.Instance.GameStart();

        StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        GameObject player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().gameObject;
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetGameStarted(true, expPanel, StatPanel);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDeadEventServerRpc(ulong playerId, string playerName)
    {
        if (!IsServer) return;

        if (playerRanks.ContainsKey(playerName)) return; // 이미 죽은 플레이어 예외

        int rank = GameSession.Instance.playerCount - playerRanks.Count;
        playerRanks[playerName] = rank;

        Debug.Log($"Player {playerName} died. Rank: {rank}");

        if (rank == 2)
        {
            GameObject[] players = PlayerSpawner.Instance.GetAllPlayers();
            string[] playerNames = players.Select(p => p.GetComponent<PlayerNameTag>()
                                    .playerName.Value.ToString()).ToArray();
            GameObject[] deadPlayers = new GameObject[GameSession.Instance.playerCount];
            string[] deadPlayerNames = new string[GameSession.Instance.playerCount];

            foreach (string name in playerRanks.Keys)
            {
                deadPlayerNames[playerRanks[name] - 1] = name;
            }

            foreach (string name in playerNames)
            {
                if (deadPlayerNames.Contains(name)) continue;

                playerRanks[name] = 1;

                Debug.Log($"Winner: {name}");
                break;
            }

            EndGame();
        }
    }

    public void EndGame()
    {
        if (!IsServer) return;

        Debug.Log("Ending game...");
        Debug.Log("Player ranks: " + string.Join(", ", playerRanks.Select(kv => $"{kv.Key}: {kv.Value}")));

        PlayerRanks.instance.SetResults(playerRanks);

        StartCoroutine(EndGameCoroutine());
    }

    IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(2f);

        EndGameClientRpc();
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        GameObject player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().gameObject;
        string name = player.GetComponent<PlayerNameTag>().playerName.Value.ToString();

        int rank = PlayerRanks.instance.GetRank(name);

        UserData userData = FirebaseManager.Instance.CurrentUserData;

        if (userData != null)
        {
            if (rank == 1)
            {
                userData.Wins += 1;
            }
            else
            {
                userData.Losses += 1;
            }

            UpdateUserData(userData);
        }
        else
        {
            Debug.LogWarning("UserData is null, cannot update win/loss.");
        }

        UpdatePlayerData();

        endPanel.SetActive(true);
        endPanel.GetComponent<EndPanel>().ShowResults();
        Debug.Log("Game ended. Displaying results.");
    }

    public async void UpdateUserData(UserData userData)
    {
        if (userData == null)
        {
            Debug.LogWarning("UserData is null, cannot update.");
            return;
        }

        await FirebaseManager.Instance.UpdateCurrentUserData(userData);
    }

    public async void UpdatePlayerData()
    {
        var updateOptions = new UpdatePlayerOptions
        {
            Data = GetPlayerData()
        };

        string playerId = AuthenticationService.Instance.PlayerId;
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(LobbyManager.CurrentLobby.Id, playerId, updateOptions);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("UpdatePlayerAsync 실패: " + e.Message);
        }
    }

    public void OnExitButtonClicked()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
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
}
