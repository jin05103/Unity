using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxStamina(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
    }

    public void SetCurrentStamina(float currentHealth)
    {
        slider.value = currentHealth;
    }

    public void UpdateStamina(float currentStamina, float maxStamina)
    {
        slider.maxValue = maxStamina;
        slider.value = currentStamina;
    }
}
