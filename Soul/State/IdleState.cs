using UnityEngine;

public class IdleState : State
{
    public PursueTargetState pursueTargetState;
    public LayerMask detectionLayer;
    public LayerMask obstacleLayerMask;

    public override State Tick(EnemyManager enemyManager, EnemyStats enemyStats, EnemyAnimatorManager enemyAnimatorManager)
    {

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

        float distanceToOrigin = Vector3.Distance(transform.position, enemyManager.originPosition);
        Vector3 frontDirection = enemyManager.transform.forward;
        frontDirection.y = 0f;
        if (distanceToOrigin < 0.3f)
        {
            enemyAnimatorManager._animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
        }
        if (Vector3.Angle(enemyManager.originDirection, frontDirection) > 5f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(enemyManager.originDirection);
            enemyManager.transform.rotation = Quaternion.RotateTowards(enemyManager.transform.rotation, targetRotation, enemyManager.rotationSpeed * Time.deltaTime);
        }


        enemyAnimatorManager._animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);

        if (enemyManager.currentTarget != null)
        {
            return pursueTargetState;
        }
        else
        {
            return this;
        }
    }
}
