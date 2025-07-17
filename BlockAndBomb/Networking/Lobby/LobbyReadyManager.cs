using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;

public class LobbyReadyManager : NetworkBehaviour
{
    [SerializeField] private LobbyManager lobbyManager;
    private int readyCount = 0;
    private bool isReadySent = false;

    public override void OnNetworkSpawn()
    {
        if (!isReadySent)
        {
            isReadySent = true;
            PlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        readyCount++;
        Debug.Log($"[ReadyManager] 플레이어 준비됨: {readyCount} / {LobbyManager.CurrentLobby.Players.Count}");

        if (readyCount == LobbyManager.CurrentLobby.Players.Count)
        {
            if (IsServer)
            {
                lobbyManager.ResetLobbySetting();
            }
            Debug.Log("[ReadyManager] 모두 준비됨! 씬 이동");
            NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
