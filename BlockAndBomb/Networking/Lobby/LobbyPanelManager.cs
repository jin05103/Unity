using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class LobbyPanelManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerSlotUI
    {
        public GameObject slotRoot;
        public TMP_Text nicknameText;
        public TMP_Text winLoseText;
        public GameObject crownImage;
    }

    [SerializeField] List<PlayerSlotUI> playerSlots;
    [SerializeField] MainMenuUI mainMenuUI;
    [SerializeField] TMP_Text lobbyCodeText;
    [SerializeField] Button playButton;

    public void SetLobbyPlayers(Lobby lobby)
    {
        for (int i = 0; i < playerSlots.Count; i++)
        {
            var slot = playerSlots[i];
            slot.nicknameText.text = "";
            slot.winLoseText.text = "";
            slot.crownImage.SetActive(false);
        }

        for (int i = 0; i < lobby.Players.Count && i < playerSlots.Count; i++)
        {
            var player = lobby.Players[i];
            var slot = playerSlots[i];
            slot.slotRoot.SetActive(true);

            string nickname = player.Data != null && player.Data.ContainsKey("nickname") ? player.Data["nickname"].Value : "???";
            string wins = player.Data != null && player.Data.ContainsKey("wins") ? player.Data["wins"].Value : "-";
            string losses = player.Data != null && player.Data.ContainsKey("losses") ? player.Data["losses"].Value : "-";

            slot.nicknameText.text = nickname;
            slot.winLoseText.text = $"W:{wins}  L:{losses}";
            slot.crownImage.SetActive(player.Id == lobby.HostId);
        }

        lobbyCodeText.text = $"Lobby Code: {lobby.LobbyCode}";

        string myPlayerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;

        if (lobby.HostId == myPlayerId)
        {
            playButton.gameObject.SetActive(true); // 호스트는 플레이 버튼 활성화
            playButton.interactable = lobby.Players.Count > 1;
        }
        else
        {
            playButton.gameObject.SetActive(false); // 비호스트는 플레이 버튼 숨김
        }
    }

    public void ClearLobbyPlayers()
    {
        foreach (var slot in playerSlots)
        {
            slot.nicknameText.text = "";
            slot.winLoseText.text = "";
            slot.crownImage.SetActive(false);
        }

        lobbyCodeText.text = "Lobby Code: N/A";
        playButton.gameObject.SetActive(false);
    }

    public async void OnLeaveOrDeleteLobbyButtonClicked()
    {
        var myPlayerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
        var isHost = LobbyManager.CurrentLobby != null &&
                     LobbyManager.CurrentLobby.HostId == myPlayerId;

        if (isHost)
            await LobbyManager.Instance.DeleteCurrentLobby();
        else
            await LobbyManager.Instance.LeaveLobby();

        // UI: 메인메뉴로 전환 등
        ClearLobbyPlayers();
        mainMenuUI.OpenMainMenuPanel();
    }

    public async void OnPlayButtonClicked()
    {
        playButton.interactable = false;

        string myPlayerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
        var currentLobby = LobbyManager.CurrentLobby;

        await LobbyService.Instance.UpdateLobbyAsync(
            currentLobby.Id,
            new UpdateLobbyOptions
            {
                IsLocked = true,
            }
        );

        if (currentLobby == null || currentLobby.HostId != myPlayerId)
        {
            Debug.LogWarning("Only the host can start the game.");
            return;
        }

        int seed = Random.Range(0, 999999);

        var data = new Dictionary<string, DataObject>()
        {
            { "gameStarting", new DataObject(DataObject.VisibilityOptions.Member, "true") },
            { "seed", new DataObject(DataObject.VisibilityOptions.Member, seed.ToString()) },
        };

        try
        {
            var updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
            {
                Data = data
            });

            LobbyManager.Instance.CurrentLobbySetter = updatedLobby;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Start game failed: {e.Message}");
        }
    }
}
