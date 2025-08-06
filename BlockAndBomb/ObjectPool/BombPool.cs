using UnityEngine;
using System.Collections.Generic;

public class BombPool : MonoBehaviour
{
    [Header("Pool Settings")]
    public Bomb bombPrefab;              // Bomb 프리팹
    public int poolSize = 20;            // 미리 만들어둘 개수

    private Queue<Bomb> pool = new Queue<Bomb>();

    void Awake()
    {
        // 미리 풀 생성
        for (int i = 0; i < poolSize; i++)
        {
            Bomb bomb = Instantiate(bombPrefab, transform);
            bomb.gameObject.SetActive(false);
            pool.Enqueue(bomb);
        }
    }


    public Bomb Get()
    {
        Bomb bomb;
        if (pool.Count > 0)
        {
            bomb = pool.Dequeue();
        }
        else
        {
            bomb = Instantiate(bombPrefab, transform);
        }
        return bomb;
    }

    public void Return(Bomb bomb)
    {
        bomb.gameObject.SetActive(false);
        pool.Enqueue(bomb);
    }
}
