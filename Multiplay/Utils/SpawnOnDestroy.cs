using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    void OnDestroy()
    {
        Instantiate(prefab, transform.position, Quaternion.identity);    
    }
}
