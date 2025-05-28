using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapInit : MonoBehaviour
{
    [SerializeField] GameObject wallPrefab;

    public void Init(int lineCount, float lineWidth, Vector3[] spawnPoint)
    {
        for (int i = 0; i < lineCount - 1; i++)
        {
            Vector3 wallPosition = new Vector3(spawnPoint[i].x + lineWidth / 2, 0, 0);
            Instantiate(wallPrefab, wallPosition, Quaternion.identity);
        }
    }
}