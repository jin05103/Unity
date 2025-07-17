using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private PlayerStatus playerStatus;

    [SerializeField] private Image healthFillImage;

    public override void OnNetworkSpawn()
    {
        if (playerStatus == null) return;

        // 초기 값 세팅
        UpdateHealthBar(playerStatus.currentHp.Value, playerStatus.maxHp.Value);

        // 값이 바뀔 때마다 UI 업데이트
        playerStatus.currentHp.OnValueChanged += (oldHp, newHp) =>
            UpdateHealthBar(newHp, playerStatus.maxHp.Value);
        playerStatus.maxHp.OnValueChanged += (oldMax, newMax) =>
            UpdateHealthBar(playerStatus.currentHp.Value, newMax);
    }

    private void UpdateHealthBar(float hp, float maxHp)
    {
        if (healthFillImage == null || maxHp <= 0) return;
        healthFillImage.fillAmount = hp / (float)maxHp;
    }
}
