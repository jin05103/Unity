using UnityEngine;
using UnityEngine.Pool;

public class LinkedPoolExample : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    private LinkedPool<GameObject> pool;

    void Awake()
    {
        pool = new LinkedPool<GameObject>(
            createFunc: () => Instantiate(prefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj)
        );
    }

    public GameObject Get() => pool.Get();

    public void Release(GameObject obj) => pool.Release(obj);
}
