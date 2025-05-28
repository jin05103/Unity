using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject shortPrefab;
    public GameObject longPrefab;
    public int poolSize;
    public Queue<GameObject> shortPool = new Queue<GameObject>();
    public Queue<GameObject> longPool = new Queue<GameObject>();
    float noteWidth;

    public void Init(float width)
    {
        noteWidth = width;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(shortPrefab);
            obj.transform.localScale = new Vector3(noteWidth, 0.1f, 1);
            obj.SetActive(false);
            shortPool.Enqueue(obj);
        }
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(longPrefab);
            obj.transform.localScale = new Vector3(noteWidth, 1, 1);
            obj.SetActive(false);
            longPool.Enqueue(obj);
        }
    }

    public GameObject GetObject(NoteType type)
    {
        if (type == NoteType.Short)
        {
            if (shortPool.Count > 0)
            {
                GameObject obj = shortPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                GameObject obj = Instantiate(shortPrefab);
                obj.transform.localScale = new Vector3(noteWidth, 0.1f, 1);
                obj.SetActive(true);
                return obj;
            }
        }
        else
        {
            if (longPool.Count > 0)
            {
                GameObject obj = longPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                GameObject obj = Instantiate(longPrefab);
                obj.transform.localScale = new Vector3(noteWidth, 1, 1);
                obj.SetActive(true);
                return obj;
            }
        }
    }

    public void ReturnObject(GameObject obj, NoteType type)
    {
        obj.SetActive(false);
        if (type == NoteType.Short)
        {
            shortPool.Enqueue(obj);
        }
        else
        {
            longPool.Enqueue(obj);
        }
    }
}
