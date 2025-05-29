using UnityEngine;

public class Arrow : MonoBehaviour
{
    public GameObject root;
    Rigidbody rb;
    Collider col;
    [SerializeField] float speed = 20f;
    [SerializeField] float lifeTime = 5f;
    [SerializeField] LayerMask[] hitLayerMask;

    public int currentWeaponDamage = 10;
    public WeaponItem weaponItem;

    Vector3 arrowDirection;

    public void Shoot(Vector3 forward, GameObject root)
    {
        this.root = root;

        arrowDirection = forward.normalized + Vector3.up * 0.06f;
        arrowDirection.Normalize();
        gameObject.transform.forward = arrowDirection;

        col = GetComponent<Collider>();
        col.enabled = true;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(arrowDirection * speed, ForceMode.Impulse);
        rb.AddTorque(transform.localRotation * Vector3.right * 0.4f, ForceMode.Impulse);

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && root.CompareTag("Player"))
        {
            EnemyStats enemyStats = other.GetComponent<EnemyStats>();
            PlayerStats playerStats = root.GetComponent<PlayerStats>();

            bool isDead = false;
            if (enemyStats != null)
            {
                int damage = currentWeaponDamage +
                        playerStats.stats.strength * weaponItem.strengthModifier +
                        playerStats.stats.agility * weaponItem.agilityModifier;

                // Vector3 hitDirection = transform.position;
                // hitDirection.y = 0f;

                // Vector3 hitDirection = transform.position - other.transform.position;
                // hitDirection.y = 0f;
                Vector3 hitDirection = -arrowDirection;
                hitDirection.y = 0f;
                hitDirection.Normalize();

                // isDead = enemyStats.TakeDamage(damage, transform.position);
                isDead = enemyStats.TakeDamage(damage, hitDirection);
            }
            if (isDead)
            {
                root.GetComponent<PlayerStats>().AddCurrency(enemyStats.enemyDropTable.currency);
                root.GetComponent<ItemDrop>().DropItem(enemyStats.enemyDropTable.dropTable, other.transform.position);
            }

            Destroy(gameObject); // Destroy the arrow on hit
        }
        else if (other.CompareTag("Player") && root.CompareTag("Enemy"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerStats != null)
            {
                if (playerController.isBlocking)
                {
                    float angle = AngleCheck(other);

                    if (angle <= 30)
                    {
                        playerController.Blocked(currentWeaponDamage, weaponItem.baseStamina2);
                        Destroy(gameObject);
                        return;
                    }
                }

                if (!playerStats.CheckImmune())
                {
                    playerStats.TakeDamage(currentWeaponDamage, true);
                    playerController.currentStaminaRegenDelay = playerStats.staminaRegenDelay;
                }
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
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
