using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInstallState : NetworkBehaviour
{
    InstallableType currentInstallType;

    private void Start()
    {
        currentInstallType = InstallableType.Bomb;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentInstallType = InstallableType.Bomb;
            UIManager.Instance.UpdateSelection(currentInstallType);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentInstallType = InstallableType.Block_Dirt;
            UIManager.Instance.UpdateSelection(currentInstallType);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentInstallType = InstallableType.Block_Stone;
            UIManager.Instance.UpdateSelection(currentInstallType);
        }
    }

    public InstallableType GetCurrentInstallType()
    {
        return currentInstallType;
    }
}
