using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject lobbiesListPanel;
    [SerializeField] TMP_InputField lobbyCodeInputField;
    [SerializeField] LobbyListUI lobbyListUI;

    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] Button lobbyListButton;
    [SerializeField] Button logoutButton;

    private void Start()
    {
        if (LobbyManager.CurrentLobby != null)
        {
            OpenLobbyPanel();
        }
        else
        {
            OpenMainMenuPanel();
        }
        // OpenMainMenuPanel();
    }

    public async void OnHostButtonClicked()
    {
        DisableButtons();

        string name = FirebaseManager.Instance.CurrentUserData.Nickname;
        string lobbyName = $"{name}'s Lobby";
        var lobby = await LobbyManager.Instance.CreateLobby(lobbyName);

        EnableButtons();

        if (lobby != null)
        {
            Debug.Log($"Lobby created: {lobby.Name} ({lobby.LobbyCode})");
            OpenLobbyPanel();
        }
    }

    public async void OnJoinButtonClicked()
    {
        DisableButtons();

        string lobbyCode = lobbyCodeInputField.text;
        if (string.IsNullOrEmpty(lobbyCode))
        {
            Debug.LogError("Lobby code is empty.");
            EnableButtons();
            return;
        }
        var lobby = await LobbyManager.Instance.JoinLobbyByCode(lobbyCode);

        EnableButtons();

        if (lobby != null)
        {
            lobbyCodeInputField.text = string.Empty;
            Debug.Log($"Joined lobby: {lobby.Name} ({lobby.LobbyCode})");
            OpenLobbyPanel();
        }
        else
        {
            Debug.LogError("Failed to join lobby. Check the code.");
        }
    }

    public async void JoinLobby(string lobbyId)
    {

        if (string.IsNullOrEmpty(lobbyId))
        {
            Debug.LogError("Lobby code is empty.");
            return;
        }
        var lobby = await LobbyManager.Instance.JoinLobbyById(lobbyId);

        if (lobby != null)
        {
            Debug.Log($"Joined lobby: {lobby.Name} ({lobby.LobbyCode})");
            OpenLobbyPanel();
            return;
        }
        else
        {
            Debug.LogError("Failed to join lobby. Check the code.");
        }
    }

    public async void OnLobbyListButtonClicked()
    {
        DisableButtons();

        var lobbies = await LobbyManager.Instance.GetLobbyList();

        EnableButtons();

        OpenLobbiesListPanel();
        lobbyListUI.ShowLobbies(lobbies);
    }

    public void OpenMainMenuPanel()
    {
        mainMenuPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        lobbiesListPanel.SetActive(false);
    }

    public void OpenLobbyPanel()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        lobbiesListPanel.SetActive(false);
    }

    public void OpenLobbiesListPanel()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        lobbiesListPanel.SetActive(true);
    }

    public void EnableButtons()
    {
        hostButton.interactable = true;
        joinButton.interactable = true;
        lobbyListButton.interactable = true;
        logoutButton.interactable = true;
    }

    public void DisableButtons()
    {
        hostButton.interactable = false;
        joinButton.interactable = false;
        lobbyListButton.interactable = false;
        logoutButton.interactable = false;
    }
}
