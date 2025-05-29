using UnityEngine;

public class AttackState : State
{

    public CombatStanceState combatStanceState;

    public EnemyAttackAction[] enemyAttacks;
    public EnemyAttackAction currentAttack;

    public override State Tick(EnemyManager enemyManager, EnemyStats enemyStats, EnemyAnimatorManager enemyAnimatorManager)
    {
        Vector3 targetDirection = enemyManager.currentTarget.transform.position - transform.position;
        float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

        if (enemyManager.isPerformingAction || enemyAnimatorManager.GetIsInteracting())
        {
            return combatStanceState;
        }

        if (enemyManager.currentCooltime > 0)
        {
            return this;
        }

        if (currentAttack != null)
        {
            if (enemyManager.distanceFromTarget < currentAttack.minimumDistanceNeededToAttack)
            {
                return this;
            }
            else if (enemyManager.distanceFromTarget < currentAttack.maximumDistanceNeededToAttack)
            {
                if (viewableAngle <= currentAttack.maximumAttackAngle && viewableAngle >= currentAttack.minimumAttackAngle)
                {
                    if (enemyManager.currentRecoveryTime <= 0 && enemyManager.isPerformingAction == false)
                    {
                        enemyAnimatorManager._animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
                        enemyAnimatorManager._animator.SetFloat("Horizontal", 0, 0.1f, Time.deltaTime);
                        enemyAnimatorManager.PlayTargetAnimation(currentAttack.actionAnimation, true);
                        enemyManager.isPerformingAction = true;
                        enemyManager.currentRecoveryTime = currentAttack.recoveryTime;
                        currentAttack = null;
                        return combatStanceState;
                    }
                }
                else
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    // enemyManager.transform.rotation = Quaternion.RotateTowards(enemyManager.transform.rotation, targetRotation, enemyManager.rotationSpeed * Time.deltaTime);
                    enemyManager.transform.rotation = Quaternion.RotateTowards(enemyManager.transform.rotation, targetRotation, enemyManager.rotationSpeed * Time.deltaTime);
                    return this;
                }
            }
        }
        else
        {
            GetNewAttack(enemyManager);
        }

        return combatStanceState;
    }

    private void GetNewAttack(EnemyManager enemyManager)
    {
        Vector3 targetsDirection = enemyManager.currentTarget.transform.position - transform.position;
        float viewableAngle = Vector3.Angle(targetsDirection, transform.forward);
        enemyManager.distanceFromTarget = Vector3.Distance(enemyManager.currentTarget.transform.position, transform.position);

        int maxScore = 0;

        for (int i = 0; i < enemyAttacks.Length; i++)
        {
            EnemyAttackAction enemyAttackAction = enemyAttacks[i];

            if (enemyManager.distanceFromTarget <= enemyAttackAction.maximumDistanceNeededToAttack && enemyManager.distanceFromTarget >= enemyAttackAction.minimumDistanceNeededToAttack)
            {
                if (viewableAngle <= enemyAttackAction.maximumAttackAngle && viewableAngle >= enemyAttackAction.minimumAttackAngle)
                {
                    maxScore += enemyAttackAction.attackScore;
                }
            }
        }

        int randomValue = Random.Range(0, maxScore);
        int temporaryScore = 0;

        for (int i = 0; i < enemyAttacks.Length; i++)
        {
            EnemyAttackAction enemyAttackAction = enemyAttacks[i];

            if (enemyManager.distanceFromTarget <= enemyAttackAction.maximumDistanceNeededToAttack && enemyManager.distanceFromTarget >= enemyAttackAction.minimumDistanceNeededToAttack)
            {
                if (viewableAngle <= enemyAttackAction.maximumAttackAngle && viewableAngle >= enemyAttackAction.minimumAttackAngle)
                {
                    if (currentAttack != null)
                    {
                        return;
                    }

                    temporaryScore += enemyAttackAction.attackScore;

                    if (temporaryScore > randomValue)
                    {
                        currentAttack = enemyAttackAction;
                    }
                }
            }
        }
    }
}
