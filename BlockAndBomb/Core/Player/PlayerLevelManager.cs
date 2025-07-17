using Unity.Netcode;
using UnityEngine;

public class PlayerLevelManager : NetworkBehaviour
{
    [SerializeField] PlayerStatus playerStatus;
    [SerializeField] PlayerStatManager playerStatManager;

    [ServerRpc(RequireOwnership = false)]
    public void OnLevelChangedServerRpc(int pre)
    {
        playerStatus.currentExp.Value -= playerStatus.needExp.Value;
        playerStatus.needExp.Value++;
        playerStatus.level.Value++;
        playerStatManager.OnLevelUp();
    }
}
