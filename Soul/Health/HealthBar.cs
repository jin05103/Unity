using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxHealth(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
    }

    public void SetCurrentHealth(float currentHealth)
    {
        slider.value = currentHealth;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
    }
}
