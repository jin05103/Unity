using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    public WeaponItem weaponItem;
    public GameObject root;
    Collider damageCollider;
    public int currentWeaponDamage = 25;
    public bool HeavyAttack;

    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
        damageCollider.gameObject.SetActive(true);
        damageCollider.isTrigger = true;
        damageCollider.enabled = false;
    }

    // private void OnEnable()
    // {
    //     root = gameObject.transform.root.gameObject;
    // }

    public void EnableDamageCollider()
    {
        damageCollider.enabled = true;
    }

    public void DisableDamageCollider()
    {
        damageCollider.enabled = false;
        HeavyAttack = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && root.tag == "Enemy")
        {
            damageCollider.enabled = false;
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            PlayerController playerController = other.GetComponent<PlayerController>();
            AnimationController animationController = other.GetComponent<AnimationController>();

            if (playerStats != null)
            {
                if (playerController.isBlocking)
                {
                    float angle = AngleCheck(other);

                    if (angle <= 30)
                    {
                        // Debug.Log("방어 성공!");
                        playerController.Blocked(currentWeaponDamage, weaponItem.baseStamina2);
                        return;
                    }
                }
                else if (playerController.isParrying)
                {
                    float angle = AngleCheck(other);

                    if (angle <= 30)
                    {
                        playerController.ParrySuccess();
                        root.GetComponent<EnemyAnimatorManager>().PlayTargetAnimation("Hit_F_6", true);
                        return;
                    }
                }

                if (!playerStats.CheckImmune())
                {
                    playerStats.TakeDamage(currentWeaponDamage, true);
                    playerController.currentStaminaRegenDelay = playerStats.staminaRegenDelay;
                }
            }
        }

        if (other.tag == "Enemy" && root.tag == "Player")
        {
            damageCollider.enabled = false;
            EnemyStats enemyStats = other.GetComponent<EnemyStats>();
            PlayerStats playerStats = root.GetComponent<PlayerStats>();
            bool isDead = false;
            if (enemyStats != null)
            {
                int damage = currentWeaponDamage +
                        playerStats.stats.strength * weaponItem.strengthModifier +
                        playerStats.stats.agility * weaponItem.agilityModifier;
                if (HeavyAttack)
                {
                    damage = (int)(damage * 1.5f);
                }

                isDead = enemyStats.TakeDamage(damage, Vector3.zero);
                
            }
            if (isDead)
            {
                root.GetComponent<PlayerStats>().AddCurrency(enemyStats.enemyDropTable.currency);
                root.GetComponent<ItemDrop>().DropItem(enemyStats.enemyDropTable.dropTable, other.transform.position);
            }
        }
    }

    private float AngleCheck(Collider other)
    {
        Vector3 playerForward = other.transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();
        Vector3 playerToEnemyDirection = root.transform.position - other.transform.position;
        playerToEnemyDirection.y = 0;
        playerToEnemyDirection.Normalize();

        float angle = Vector3.Angle(playerForward, playerToEnemyDirection);

        return angle;
    }
}
