using UnityEngine;

[CreateAssetMenu(fileName = "StatData", menuName = "Game/StatData")]
public class StatData : ScriptableObject
{
    public StatType statType;
    public string statName;
    [TextArea]
    public string description;
    public Sprite icon;
    public float incrementValue;
}

public enum StatType
{
    MiningDamage,
    MiningSpeed,
    Speed,
    BombDamage,
    BombRadius,
    MaxBombCount,
    MaxHp
}