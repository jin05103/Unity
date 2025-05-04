using System;
using UnityEngine;

public class ActionHealth : MonoBehaviour {
    public Action<int> onHealthChanged;
    private int health = 100;

    public void TakeDamage(int damage) {
        health -= damage;
        Debug.Log("Health changed to: " + health);
        onHealthChanged?.Invoke(health);
    }
}

public class ActionHealthUI : MonoBehaviour {
    public ActionHealth playerHealth;

    void OnEnable() {
        playerHealth.onHealthChanged += UpdateUI;
    }

    void OnDisable() {
        playerHealth.onHealthChanged -= UpdateUI;
    }

    void UpdateUI(int newHealth) {
        Debug.Log("Action UI: " + newHealth);
    }
}
