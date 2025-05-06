using UnityEngine;

public class MeleeAttack : IAttackStrategy
{
    public void Attack(Transform origin)
    {
        Debug.Log("Melee Attack! (근접)");
        // 예: 주변 범위 판정
    }
}
