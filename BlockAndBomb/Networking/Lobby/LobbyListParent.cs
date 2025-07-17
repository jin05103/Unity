using System.Threading.Tasks;
using UnityEngine;

public class LobbyListParent : MonoBehaviour
{
    [SerializeField] MainMenuUI mainMenuUI;

    public bool isClicked = false;

    public void Join(string lobbyId)
    {
        if (isClicked)
        {
            Debug.LogWarning("Join button already clicked. Ignoring subsequent clicks.");
            return;
        }

        isClicked = true;

        mainMenuUI.JoinLobby(lobbyId);

        isClicked = false;
    }
}
