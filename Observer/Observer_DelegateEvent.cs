using UnityEngine;

// Subject
public class Health : MonoBehaviour {
    public delegate void OnHealthChanged(int newHealth);
    public event OnHealthChanged onHealthChanged;

    private int health = 100;

    public void TakeDamage(int damage) {
        health -= damage;
        Debug.Log("Health changed to: " + health);
        onHealthChanged?.Invoke(health);
    }
}

// Observer
public class HealthUI : MonoBehaviour {
    public Health playerHealth;

    void OnEnable() {
        playerHealth.onHealthChanged += UpdateUI;
    }

    void OnDisable() {
        playerHealth.onHealthChanged -= UpdateUI;
    }

    void UpdateUI(int newHealth) {
        Debug.Log("UI updated to: " + newHealth);
    }
}
