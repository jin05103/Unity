using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public override void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    [SerializeField] private GameObject playerPrefab;

    // 2D 좌표계(XY 평면)로 정의
    private static readonly Vector2[] spawnPositions2D = new Vector2[]
    {
        new Vector2( 0f,  0f),
        new Vector2( 0f, 60f),
        new Vector2(60f,  0f),
        new Vector2(60f, 60f)
    };

    private Dictionary<ulong, GameObject> playerObjs = new();
    private Dictionary<ulong, int> playerSpawnIndices = new();
    private List<int> availableIndices;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        availableIndices = new List<int> { 0, 1, 2, 3 };
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnForClient;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnForClient(clientId);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= SpawnForClient;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void SpawnForClient(ulong clientId)
    {
        if (playerObjs.ContainsKey(clientId))
        {
            Debug.LogWarning($"Client {clientId} already has a player object!");
            return;
        }

        if (availableIndices.Count == 0)
        {
            Debug.LogWarning("남은 스폰 위치가 없습니다!");
            return;
        }

        int poolIdx = Random.Range(0, availableIndices.Count);
        int spawnIdx = availableIndices[poolIdx];
        availableIndices.RemoveAt(poolIdx);

        Vector2 pos2 = spawnPositions2D[spawnIdx];
        Vector3 spawnPos = new Vector3(pos2.x, pos2.y, 0f);

        var go = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        go.GetComponent<NetworkObject>()
          .SpawnAsPlayerObject(clientId, destroyWithScene: false);

        playerObjs[clientId] = go;
        playerSpawnIndices[clientId] = spawnIdx;
        Debug.Log($"Client {clientId} spawned at {spawnPos} (index {spawnIdx})");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (playerObjs.TryGetValue(clientId, out var go))
        {
            if (go != null && go.TryGetComponent<NetworkObject>(out var netObj))
            {
                if (netObj.IsSpawned)
                    netObj.Despawn();
            }
            // Destroy(go);
            playerObjs.Remove(clientId);
        }
        if (playerSpawnIndices.TryGetValue(clientId, out var idx))
        {
            availableIndices.Add(idx);
            playerSpawnIndices.Remove(clientId);
        }
    }

    public GameObject GetPlayerObject(ulong clientId)
    {
        playerObjs.TryGetValue(clientId, out var go);
        return go;
    }

    public GameObject[] GetAllPlayers()
    {
        if (playerObjs.Count == 0) return null;
        GameObject[] allPlayers = new GameObject[playerObjs.Count];
        int index = 0;
        foreach (var kvp in playerObjs)
        {
            if (kvp.Value != null)
            {
                allPlayers[index++] = kvp.Value;
            }
        }
        return allPlayers;
    }

    public ulong GetId(GameObject gameObject)
    {
        foreach (var pair in playerObjs)
        {
            if (pair.Value == gameObject)
            {
                return pair.Key;
            }
        }

        throw new KeyNotFoundException("해당 GameObject에 대한 ID를 찾을 수 없습니다.");
    }

    public int GetPlayerCount()
    {
        return playerObjs.Count;
    }
}
