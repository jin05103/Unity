using UnityEngine;
using UnityEngine.AI;

public class PursueTargetState : State
{
    public CombatStanceState combatStanceState;
    public ReturnState returnState;

    public override State Tick(EnemyManager enemyManager, EnemyStats enemyStats, EnemyAnimatorManager enemyAnimatorManager)
    {
        if (enemyManager.isPerformingAction || enemyAnimatorManager.GetIsInteracting())
        {
            enemyAnimatorManager._animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
            //enemyManager.navMeshAgent.enabled = false;
            enemyManager.navMeshAgent.isStopped = true;
            enemyManager.navMeshAgent.updatePosition = false;
            return this;
        }

        Vector3 targetDirection = enemyManager.currentTarget.transform.position - transform.position;
        enemyManager.distanceFromTarget = Vector3.Distance(enemyManager.currentTarget.transform.position, transform.position);
        float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

        if (!CanReachTarget(enemyManager))
        {
            enemyManager.currentTarget = null;
            enemyManager.isReturning = false;
            return returnState;
        }

        if (enemyManager.distanceFromTarget > enemyManager.maxChaseDistance)
        {
            enemyManager.currentTarget = null;
            enemyManager.isReturning = false;
            return returnState;
        }

        if (enemyManager.distanceFromTarget > enemyManager.maximumAttackRange)
        {
            enemyAnimatorManager._animator.SetFloat("Vertical", 1, 0.1f, Time.deltaTime);
            //
            enemyManager.navMeshAgent.enabled = true;
            enemyManager.navMeshAgent.isStopped = false;
            enemyManager.navMeshAgent.updatePosition = true;
            enemyManager.navMeshAgent.SetDestination(enemyManager.currentTarget.transform.position);
            //
        }
        else if (enemyManager.distanceFromTarget <= enemyManager.maximumAttackRange)
        {
            enemyAnimatorManager._animator.SetFloat("Vertical", 0f);
            //
            //enemyManager.navMeshAgent.enabled = false;
            enemyManager.navMeshAgent.isStopped = true;
            enemyManager.navMeshAgent.updatePosition = false;
            //
        }


        HandleRotateTowardTarget(enemyManager);

        if (enemyManager.distanceFromTarget <= enemyManager.maximumAttackRange)
        {
            enemyAnimatorManager._animator.SetFloat("Vertical", 0f);
            //enemyManager.navMeshAgent.enabled = false;
            enemyManager.navMeshAgent.isStopped = true;
            enemyManager.navMeshAgent.updatePosition = false;
            return combatStanceState;
        }
        else
        {
            return this;
        }
    }
    private void HandleRotateTowardTarget(EnemyManager enemyManager)
    {
        if (enemyManager.isPerformingAction)
        {
            Vector3 direction = enemyManager.currentTarget.transform.position - enemyManager.transform.position;
            direction.y = 0;
            direction.Normalize();

            if (direction == Vector3.zero)
            {
                direction = transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            enemyManager.transform.rotation = Quaternion.Slerp(enemyManager.transform.rotation, targetRotation, enemyManager.rotationSpeed / Time.deltaTime);
        }
        else
        {
            Vector3 relativeDirection = transform.InverseTransformDirection(enemyManager.navMeshAgent.desiredVelocity);
            Vector3 targetVelocity = enemyManager.enemyRigidBody.linearVelocity;

            enemyManager.navMeshAgent.enabled = true;
            enemyManager.navMeshAgent.isStopped = false;
            enemyManager.navMeshAgent.updatePosition = true;
            enemyManager.navMeshAgent.SetDestination(enemyManager.currentTarget.transform.position);
            enemyManager.enemyRigidBody.linearVelocity = targetVelocity;
            enemyManager.transform.rotation = Quaternion.Slerp(enemyManager.transform.rotation, enemyManager.navMeshAgent.transform.rotation, enemyManager.rotationSpeed / Time.deltaTime);
        }
    }

    private bool CanReachTarget(EnemyManager enemyManager)
    {
        if (enemyManager.navMeshAgent == null || enemyManager.currentTarget == null)
            return false;

        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(enemyManager.transform.position, enemyManager.currentTarget.transform.position, NavMesh.AllAreas, path))
            return false;

        // 경로가 유효하지 않거나, 길이 0 이면 도달 불가로 판단
        if (path.status != NavMeshPathStatus.PathComplete || path.corners.Length <= 1)
            return false;

        return true;
    }
}
