using UnityEngine;

public class RangedAttack : IAttackStrategy
{
    public void Attack(Transform origin)
    {
        Debug.Log("Ranged Attack! (원거리)");
        // 예: 총알 Instantiate
    }
}
