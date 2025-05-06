using UnityEngine;

public class Enemy : MonoBehaviour
{
    private IAttackStrategy currentStrategy;

    public void SetAttackStrategy(IAttackStrategy strategy)
    {
        currentStrategy = strategy;
    }

    public void PerformAttack()
    {
        currentStrategy?.Attack(transform);
    }
}
