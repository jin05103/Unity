using UnityEngine;

public abstract class State : MonoBehaviour
{
    public abstract State Tick(EnemyManager enemyManager, EnemyStats enemyStats, EnemyAnimatorManager enemyAnimatorManager);
}
