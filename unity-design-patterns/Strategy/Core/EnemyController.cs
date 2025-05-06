using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private FireballAttack_SO fireballSO;

    private void Start()
    {
        enemy.SetAttackStrategy(new MeleeAttack());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            enemy.SetAttackStrategy(new MeleeAttack());
            Debug.Log("전략 변경: 근접");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            enemy.SetAttackStrategy(new RangedAttack());
            Debug.Log("전략 변경: 원거리");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            enemy.SetAttackStrategy(fireballSO);
            Debug.Log("전략 변경: Fireball (SO)");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            enemy.PerformAttack();
        }
    }
}
