using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropEntry
{
    public float dropRate;
    public WeaponItem item; // 혹은 원하는 타입
}

public class EnemyDropTable : MonoBehaviour
{
    public int currency = 10;
    public DropEntry dropTable;
}
