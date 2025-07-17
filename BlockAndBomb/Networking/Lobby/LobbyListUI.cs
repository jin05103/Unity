using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections;
using System;

public class LobbyListUI : MonoBehaviour
{
    [SerializeField] MainMenuUI mainMenuUI;
    [SerializeField] Transform listRoot;
    [SerializeField] GameObject lobbyItemPrefab;

    [SerializeField] Button refreshButton;
    [SerializeField] Button leaveButton;

    public void ShowLobbies(List<Lobby> lobbies)
    {
        foreach (Transform child in listRoot) Destroy(child.gameObject);

        foreach (var lobby in lobbies)
        {
            var go = Instantiate(lobbyItemPrefab, listRoot);
            var item = go.GetComponent<LobbyListItem>();
            if (item != null)
            {
                Debug.Log(lobby.LobbyCode);
                item.SetLobbyInfo(lobby.Name, lobby.Players.Count, lobby.MaxPlayers, lobby.Id);
            }
            else
            {
                Debug.LogError("LobbyListItem component not found on the instantiated prefab.");
            }
        }
    }

    public async void OnRefreshButtonClicked()
    {
        if (!refreshButton.interactable)
            return;

        refreshButton.interactable = false;
        try
        {
            var lobbies = await LobbyManager.Instance.GetLobbyList();
            ShowLobbies(lobbies);
        }
        catch (Exception ex)
        {
            Debug.LogError("로비 새로고침 에러: " + ex.Message);
        }
        finally
        {
            await Task.Delay(1500);
            refreshButton.interactable = true;
        }
    }

    public void OnLeaveButtonClicked()
    {
        mainMenuUI.OpenMainMenuPanel();
    }
}
