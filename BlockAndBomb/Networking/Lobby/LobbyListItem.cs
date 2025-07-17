using TMPro;
using UnityEngine;

public class LobbyListItem : MonoBehaviour
{
    [SerializeField] TMP_Text LobbyNameText;
    [SerializeField] TMP_Text PlayerCountText;
    string lobbyId;

    public void SetLobbyInfo(string lobbyName, int currentPlayers, int maxPlayers, string lobbyId)
    {
        LobbyNameText.text = $"{lobbyName}";
        PlayerCountText.text = $"{currentPlayers}/{maxPlayers}";
        this.lobbyId = lobbyId;
    }

    public void OnJoinButtonClicked()
    {
        if (string.IsNullOrEmpty(lobbyId))
        {
            Debug.Log(lobbyId);
            Debug.LogError("Lobby code is not set.");
            return;
        }

        GetComponentInParent<LobbyListParent>().Join(lobbyId);
    }
}
