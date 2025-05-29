using UnityEngine;

public class AlertState : State
{
    public PursueTargetState pursueTargetState;
    public IdleState idleState;
    public ReturnState returnState;
    public LayerMask detectionLayer;

    public float alertDuration = 3f; // 경계 상태에서 적을 감지하는 제한 시간
    private float currentAlertTime = 0f;

    public State lastState;

    public override State Tick(EnemyManager enemyManager, EnemyStats enemyStats, EnemyAnimatorManager enemyAnimatorManager)
    {
        // 경계 상태에 진입한 후 타이머 증가
        currentAlertTime += Time.deltaTime;

        // 피격 당한 방향이 있다면 그쪽을 바라봄
        // if (enemyManager.lastHitDirection != Vector3.zero)
        // {
        //     Quaternion targetRotation = Quaternion.LookRotation(-enemyManager.lastHitDirection);
        //     enemyManager.transform.rotation = Quaternion.Slerp(enemyManager.transform.rotation, targetRotation, enemyManager.rotationSpeed * Time.deltaTime);
        // }

        enemyAnimatorManager._animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(enemyManager.lastHitDirection);
        enemyManager.transform.rotation = Quaternion.RotateTowards(enemyManager.transform.rotation, targetRotation, enemyManager.rotationSpeed * Time.deltaTime);


        // 주변 적 감지 (탐지 레이어로 감지)
        Collider[] colliders = Physics.OverlapSphere(enemyManager.transform.position, enemyManager.detectionRadius, detectionLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            CharacterStats characterStats = colliders[i].transform.GetComponent<CharacterStats>();
            float distance = Vector3.Distance(transform.position, characterStats.transform.position);

            if (characterStats != null)
            {
                Vector3 targetDirection = characterStats.transform.position - transform.position;
                float viewableAngle = Vector3.Angle(targetDirection, transform.forward);

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

        // 제한 시간(alertDuration) 내에 적을 발견하지 못하면 원래 자리 여부에 따라 Idle 또는 Return 상태로 복귀
        if (currentAlertTime >= alertDuration)
        {
            currentAlertTime = 0f;
            enemyManager.lastHitDirection = Vector3.zero;
            return lastState;
        }

        return this;
    }

    // 경계 상태에 새로 진입할 때 alert 타이머를 리셋할 필요가 있다면 별도 메서드로 제공할 수 있음.
    public void ResetAlertTimer()
    {
        currentAlertTime = 0f;
    }
}