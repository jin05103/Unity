using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntEvent : UnityEvent<int> { }

public class UnityEventHealth : MonoBehaviour {
    public IntEvent onHealthChanged;
    private int health = 100;

    public void TakeDamage(int damage) {
        health -= damage;
        Debug.Log("Health changed to: " + health);
        onHealthChanged?.Invoke(health);
    }
}
