using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using System.Text;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;

public class NetworkInitializer : MonoBehaviour
{
    [SerializeField] LobbyReadyManager lobbyReadyManager;

    public async Task Init()
    {
        string myId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
        var lobby = LobbyManager.CurrentLobby;
        string hostId = lobby.HostId;

        if (lobby.Data.TryGetValue("seed", out var seedData))
        {
            int.TryParse(seedData.Value, out int seed);
            GameSession.Instance.seed = seed;
        }

        GameSession.Instance.localPlayerId = lobby.Players.Find(p => p.Id == myId)?.Data["nickname"].Value ?? "Unknown";
        GameSession.Instance.playerCount = lobby.Players.Count;

        // JoinCode 세팅
        // string joinCode = lobby.Data["joinCode"].Value;

        if (myId == hostId)
        {
            // 호스트 시작
            // NetworkManager.Singleton.StartHost();
            // lobbyReadyManager.PlayerReadyServerRpc();
        }
        else
        {
            // 클라이언트 → Relay Join
            Debug.Log("networking init");
            var relayJoinCode = lobby.Data["relayJoinCode"].Value;
            var joinAlloc = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAlloc, "dtls"));
            NetworkManager.Singleton.StartClient();

            // lobbyReadyManager.PlayerReadyServerRpc();
        }
    }
}
