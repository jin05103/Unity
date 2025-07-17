using System.Collections.Generic;
using UnityEngine;

public class FXPool : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject bombFXPrefab; // FX 프리팹
    public GameObject miningFXPrefab;
    public int poolSize = 20;    // 미리 만들어둘 개수

    private Queue<GameObject> bombPool = new Queue<GameObject>();
    private Queue<GameObject> miningPool = new Queue<GameObject>();

    void Awake()
    {
        // 미리 풀 생성
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bombFX = Instantiate(bombFXPrefab, transform);
            GameObject miningFX = Instantiate(miningFXPrefab, transform);
            bombFX.SetActive(false);
            miningFXPrefab.SetActive(false);
            bombPool.Enqueue(bombFX);
            miningPool.Enqueue(miningFX);
        }
    }

    public GameObject GetBombFX()
    {
        GameObject fx;
        if (bombPool.Count > 0)
        {
            fx = bombPool.Dequeue();
        }
        else
        {
            fx = Instantiate(bombFXPrefab, transform);
        }
        fx.SetActive(true);
        return fx;
    }

    public GameObject GetMiningFX()
    {
        GameObject fx;
        if (miningPool.Count > 0)
        {
            fx = miningPool.Dequeue();
        }
        else
        {
            fx = Instantiate (miningFXPrefab, transform);
        }
        fx .SetActive(true);
        return fx;
    }

    public void ReturnBombFX(GameObject fx)
    {
        fx.SetActive(false);
        bombPool.Enqueue(fx);
    }

    public void ReturnMiningFX(GameObject fx)
    {
        fx.SetActive(false);
        miningPool.Enqueue(fx);
    }
}
