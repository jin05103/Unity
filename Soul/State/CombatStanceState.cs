using UnityEngine;

public class CombatStanceState : State
{
    public AttackState attackState;
    public PursueTargetState pursueTargetState;

    public override State Tick(EnemyManager enemyManager, EnemyStats enemyStats, EnemyAnimatorManager enemyAnimatorManager)
    {
        enemyManager.distanceFromTarget = Vector3.Distance(enemyManager.currentTarget.transform.position, transform.position);

        if (enemyManager.isPerformingAction || enemyAnimatorManager.GetIsInteracting())
        {
            enemyAnimatorManager._animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
            return this;
        }

        Vector3 frontDirection = enemyManager.transform.forward;
        frontDirection.y = 0f;
        Vector3 targetDirection = enemyManager.currentTarget.transform.position - transform.position;
        targetDirection.y = 0f;
        
        float viewableAngle = Vector3.Angle(targetDirection, frontDirection);

        if (enemyManager.distanceFromTarget > enemyManager.maximumAttackRange)
        {
            return pursueTargetState;
        }

        if (viewableAngle > 1f || viewableAngle < -1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(enemyManager.currentTarget.transform.position - enemyManager.transform.position);
            enemyManager.transform.rotation = Quaternion.RotateTowards(enemyManager.transform.rotation, targetRotation, enemyManager.rotationSpeed * Time.deltaTime);
            return this;
        }

        if (enemyManager.currentRecoveryTime <= 0 && enemyManager.distanceFromTarget <= enemyManager.maximumAttackRange)
        {
            return attackState;
        }
        // else if (enemyManager.distanceFromTarget > enemyManager.maximumAttackRange)
        // {
        //     return pursueTargetState;
        // }
        else
        {
            return this;
        }
    }
}
