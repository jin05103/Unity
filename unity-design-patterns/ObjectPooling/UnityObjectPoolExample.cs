using UnityEngine;
using UnityEngine.Pool;

public class UnityObjectPoolExample : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    private ObjectPool<GameObject> pool;

    void Awake()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 50
        );
    }

    public GameObject Get() => pool.Get();

    public void Release(GameObject obj) => pool.Release(obj);
}
