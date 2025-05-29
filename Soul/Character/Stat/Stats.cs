using UnityEngine;

public enum StatType
{
    Health,
    Stamina,
    Strength,
    Agility
}

[CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Stats", order = 1)]
public class Stats : ScriptableObject
{
    [Header("Currency")]
    public int currency;

    [Header("Stats")]
    public int health;
    public int stamina;
    public int strength;
    public int agility;

    // 플레이어 레벨은 네 스탯의 합으로 정의
    public int Level
    {
        get { return health - 10 + stamina - 10 + strength - 1 + agility; }
    }

    // 업그레이드 비용 계산 (레벨 기반)
    public int GetUpgradeCost()
    {
        int baseCost = 10;
        float costMultiplier = 1.1f;
        return (int)(baseCost * Mathf.Pow(costMultiplier, Level - 1));
    }

    public void LevelUp(StatType statType)
    {
        switch (statType)
        {
            case StatType.Health:
                health += 1;
                break;
            case StatType.Stamina:
                stamina += 1;
                break;
            case StatType.Strength:
                strength += 1;
                break;
            case StatType.Agility:
                agility += 1;
                break;
        }
    }
}
