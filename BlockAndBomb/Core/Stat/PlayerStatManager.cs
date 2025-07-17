using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class PlayerStatManager : NetworkBehaviour
{
    [SerializeField] PlayerStatus playerStatus;
    [SerializeField] List<StatData> allStats;
    private StatPanel statPanel;

    private List<StatData> currentStatChoices = new();

    public void LinkStatPanel(StatPanel statPanel)
    {
        this.statPanel = statPanel;
    }

    public void OnLevelUp()
    {
        playerStatus.statPoint.Value++;
        if (playerStatus.statPoint.Value == 1)
        {
            DrawAndShowStatPanelClientRpc();
        }
    }

    [ClientRpc]
    void DrawAndShowStatPanelClientRpc()
    {
        currentStatChoices = DrawRandomStats(3, allStats);
        statPanel.ShowStats(currentStatChoices, OnStatSelected);
    }

    public void OnStatSelected(StatType statType)
    {
        RequestUpgradeStatServerRpc(statType, NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc]
    void RequestUpgradeStatServerRpc(StatType statType, ulong clientId)
    {
        var player = PlayerSpawner.Instance.GetPlayerObject(clientId);
        var playerStatus = player.GetComponent<PlayerStatus>();

        StatData stat = allStats.Find(s => s.statType == statType);
        if (stat == null) return;

        if (playerStatus.statPoint.Value > 0)
        {
            switch (statType)
            {
                case StatType.MiningDamage: playerStatus.miningDamage.Value += stat.incrementValue; break;
                case StatType.MiningSpeed: playerStatus.miningSpeed.Value += stat.incrementValue; break;
                case StatType.Speed: playerStatus.speed.Value += stat.incrementValue; break;
                case StatType.BombDamage: playerStatus.bombDamage.Value += stat.incrementValue; break;
                case StatType.BombRadius: playerStatus.bombRadius.Value += (int)stat.incrementValue; break;
                case StatType.MaxBombCount: playerStatus.maxBombCount.Value += (int)stat.incrementValue; break;
                case StatType.MaxHp: playerStatus.maxHp.Value += stat.incrementValue; break;
            }
            playerStatus.statPoint.Value--;
        }

        AfterSelectStatClientRpc(clientId);
    }

    [ClientRpc]
    private void AfterSelectStatClientRpc(ulong clientId)
    {
        if (!IsOwner) return;

        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        if (playerStatus.statPoint.Value >= 1)
        {
            DrawAndShowStatPanelClientRpc();
        }
        else
        {
            statPanel.Hide();
        }

    }

    List<StatData> DrawRandomStats(int count, List<StatData> pool)
    {
        var temp = new List<StatData>(pool);
        var result = new List<StatData>();
        for (int i = 0; i < count && temp.Count > 0; i++)
        {
            int idx = Random.Range(0, temp.Count);
            result.Add(temp[idx]);
            temp.RemoveAt(idx);
        }
        return result;
    }

    public StatPanel GetStatPanel()
    {
        return statPanel;
    }
}