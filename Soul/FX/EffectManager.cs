using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [System.Serializable]
    public class FxPool
    {
        public GameObject prefab;
        public int poolSize = 10;
        [HideInInspector] public Queue<GameObject> pool = new Queue<GameObject>();
    }

    public FxPool fxPool;

    void Awake()
    {
        Instance = this;
        InitPools();
    }

    void InitPools()
    {
        for (int i = 0; i < fxPool.poolSize; i++)
        {
            GameObject obj = Instantiate(fxPool.prefab);
            obj.SetActive(false);
            fxPool.pool.Enqueue(obj);
        }
    }

    public void PlayFx(Vector3 position, Quaternion rotation)
    {
        GameObject obj;
        if (fxPool.pool.Count > 0)
        {
            obj = fxPool.pool.Dequeue();
        }
        else
        {
            obj = Instantiate(fxPool.prefab);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        // 이펙트가 자동으로 꺼지는 시간이 필요하면, 아래처럼
        float duration = 2f;
        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            duration = ps.main.duration + ps.main.startLifetime.constantMax;
        }

        StartCoroutine(ReturnToPool(obj, fxPool, duration));
    }

    public void PlayFx(Vector3 position)
    {
        PlayFx(position, Quaternion.identity);
    }

    IEnumerator ReturnToPool(GameObject obj, FxPool fxPool, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        fxPool.pool.Enqueue(obj);
    }
}