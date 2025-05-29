using System.Collections;
using UnityEngine;

public class ReturnState : State
{
    public IdleState idleState;
    public PursueTargetState pursueTargetState;

    public LayerMask detectionLayer;
    public LayerMask obstacleLayerMask;

    public float directReturnSpeed = 5f;

    public override State Tick(EnemyManager enemyManager, EnemyStats enemyStats, EnemyAnimatorManager enemyAnimatorManager)
    {
        if (enemyManager.isPerformingAction || enemyAnimatorManager.GetIsInteracting())
        {
            enemyAnimatorManager._animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
            return this;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, enemyManager.detectionRadius, detectionLayer);

        for (int i = 0; i < colliders.Length; i++)
        {
            CharacterStats characterStats = colliders[i].transform.GetComponent<CharacterStats>();
            float distance = Vector3.Distance(transform.position, characterStats.transform.position);

            if (characterStats != null)
            {
                Vector3 targetDirection = characterStats.transform.position - transform.position;
                float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

                if (Physics.Linecast(transform.position, characterStats.transform.position, out RaycastHit hit, obstacleLayerMask))
                {
                    if (hit.transform != characterStats.transform)
                        continue;
                }

                if (viewableAngle > enemyManager.minimumDetectionAngle && viewableAngle < enemyManager.maximumDetectionAngle)
                {
                    enemyManager.currentTarget = characterStats;
                    return pursueTargetState;
                }
                else if (distance < enemyManager.backDetectionRadius)
                {
                    enemyManager.currentTarget = characterStats;
                    return pursueTargetState;
                }
            }
        }

        if (enemyManager.currentTarget != null)
        {
            return pursueTargetState;
        }
        else
        {
            if (!enemyManager.isReturning)
            {
                enemyManager.isReturning = true;
                enemyManager.navMeshAgent.enabled = true;
                enemyManager.navMeshAgent.SetDestination(enemyManager.originPosition);
                enemyAnimatorManager._animator.SetFloat("Vertical", 1, 0.1f, Time.deltaTime);
            }

            float distanceToOrigin = Vector3.Distance(transform.position, enemyManager.originPosition);
            
            if (distanceToOrigin < 2f && distanceToOrigin >= 0.3f)
            {
                enemyManager.navMeshAgent.enabled = false;
                enemyManager.transform.position = Vector3.MoveTowards(enemyManager.transform.position, enemyManager.originPosition, directReturnSpeed * Time.deltaTime);
                enemyAnimatorManager._animator.SetFloat("Vertical", 1, 0.1f, Time.deltaTime);

                return this;
            }
            else if (distanceToOrigin < 0.3f)
            {
                enemyManager.isReturning = false;
                enemyManager.navMeshAgent.enabled = false;
                enemyAnimatorManager._animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
                return idleState;
            }

            return this;
        }
    }
}
