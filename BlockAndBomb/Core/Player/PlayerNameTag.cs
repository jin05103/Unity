using Unity.Netcode;
using TMPro;
using UnityEngine;
using Unity.Collections;

public class PlayerNameTag : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;

    // 서버가 세팅하고, 클라이언트는 읽기만 하는 변수
    public NetworkVariable<FixedString64Bytes> playerName =
        new NetworkVariable<FixedString64Bytes>(
            new FixedString64Bytes(""),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        nameText.text = playerName.Value.ToString();
        playerName.OnValueChanged += (oldName, newName) =>
        {
            nameText.text = newName.ToString();
        };

        if (IsOwner)
        {
            string nick = GameSession.Instance.localPlayerId;
            SetPlayerNameServerRpc(new FixedString64Bytes(nick));
        }
    }

    [ServerRpc(RequireOwnership = true)]
    public void SetPlayerNameServerRpc(FixedString64Bytes newName)
    {
        if (!IsServer) return;
        playerName.Value = newName;
    }
}
