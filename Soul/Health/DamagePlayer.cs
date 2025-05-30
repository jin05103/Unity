using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    public int damage = 25;

    private void OnTriggerEnter(Collider other)
    {
        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats != null && !playerStats.CheckImmune())
        {
            playerStats.TakeDamage(damage, true);
        }
    }
}
