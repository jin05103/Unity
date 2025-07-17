using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class NetworkDropPool : MonoBehaviour, INetworkPrefabInstanceHandler
{
    [SerializeField] private GameObject dropPrefab;
    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        // Netcode에 풀 등록
        NetworkManager.Singleton.PrefabHandler.AddHandler(dropPrefab, this);
    }

    void OnDestroy()
    {    
        // Netcode에서 풀 핸들러 제거
        NetworkManager.Singleton.PrefabHandler.RemoveHandler(dropPrefab);
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(dropPrefab);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj.GetComponent<NetworkObject>();
    }

    public void Destroy(NetworkObject networkObject)
    {
        // Destroy 대신 풀에 반환 (SetActive(false)만!)
        networkObject.gameObject.SetActive(false);
        pool.Enqueue(networkObject.gameObject);
    }
}