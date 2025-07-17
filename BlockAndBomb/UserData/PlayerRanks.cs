using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerRanks : NetworkBehaviour
{
    public static PlayerRanks instance;
    // public NetworkVariable<string>[] playerNames;
    // public NetworkList<string> playerNames = new NetworkList<string>();
    public NetworkList<FixedString64Bytes> playerNames = new NetworkList<FixedString64Bytes>();

    private void Awake()
    {
        instance = this;

        int playerCount = GameSession.Instance.playerCount;

        // playerNames = new NetworkVariable<string>[playerCount];

        // for (int i = 0; i < playerCount; i++)
        // {
        //     playerNames[i] = new NetworkVariable<string>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        // }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 이 부분은 서버에서만 채운다!
        if (IsServer)
        {
            // playerNames = new NetworkList<FixedString64Bytes>();

            playerNames.Clear();
            int playerCount = GameSession.Instance.playerCount;
            for (int i = 0; i < playerCount; i++)
            {
                playerNames.Add(new FixedString64Bytes(""));
            }
        }
    }

    public void SetResults(Dictionary<string, int> playerRanks)
    {
        // Debug.Log($"SetResults called! playerNames.Length={playerNames?.Length}");
        // foreach (var playerRank in playerRanks)
        // {
        //     int idx = playerRank.Value - 1;
        //     Debug.Log($"Trying to set playerNames[{idx}] = {playerRank.Key}");
        //     if (idx < 0 || idx >= playerNames.Length)
        //     {
        //         Debug.LogError($"playerNames 인덱스 오류: idx={idx}, Length={playerNames.Length}");
        //         continue;
        //     }
        //     if (playerNames[idx] == null)
        //     {
        //         Debug.LogError($"playerNames[{idx}]가 null입니다!");
        //         continue;
        //     }
        //     playerNames[idx].Value = playerRank.Key;
        // }
        Debug.Log($"SetResults called! playerNames.Count={playerNames?.Count}");
        foreach (var playerRank in playerRanks)
        {
            int idx = playerRank.Value - 1;
            Debug.Log($"Trying to set playerNames[{idx}] = {playerRank.Key}");
            if (idx < 0 || idx >= playerNames.Count)
            {
                Debug.LogError($"playerNames 인덱스 오류: idx={idx}, Count={playerNames.Count}");
                continue;
            }
            playerNames[idx] = new FixedString64Bytes(playerRank.Key);
        }
    }

    public int GetRank(string playerName)
    {
        // for (int i = 0; i < playerNames.Length; i++)
        // {
        //     if (playerNames[i].Value == playerName)
        //     {
        //         return i + 1; // Rank is index + 1
        //     }
        // }
        for (int i = 0; i < playerNames.Count; i++)
        {
            if (playerNames[i].ToString() == playerName)
            {
                return i + 1; // Rank is index + 1
            }
        }
        return -1; // Not found
    }
}
