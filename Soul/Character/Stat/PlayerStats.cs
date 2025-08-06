using UnityEngine;

public class PlayerStats : CharacterStats
{
    public PlayerController playerController;
    public int Level;
    public Stats stats;
    public HealthBar healthBar;
    public StaminaBar staminaBar;
    [SerializeField] UIManager uiManager;

    public float staminaRegenRate = 5f; // Stamina regeneration rate per second
    public float staminaRegenDelay = 0.6f; // Delay before stamina starts regenerating

    AnimationController animationController;

    public int healPotionAmount = 20;

    private void Awake()
    {
        animationController = GetComponent<AnimationController>();
        playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        SetMaxHealthFromHealthLevel();
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        SetMaxStaminaFromStaminaLevel();
        currentStamina = maxStamina;
        staminaBar.SetMaxStamina(maxStamina);

        Level = stats.Level;

        uiManager.CurrencyTextUpdate(stats.currency);
    }

    public void SetMaxHealthFromHealthLevel()
    {
        maxHealth = stats.health * 8;
    }

    public void SetMaxStaminaFromStaminaLevel()
    {
        maxStamina = stats.stamina * 8;
    }

    public bool CheckImmune()
    {
        return animationController.GetBool("Immune");
    }

    public void TakeDamage(int damage, bool hit)
    {
        if (playerController.isDead) return;
        currentHealth -= damage;

        healthBar.SetCurrentHealth(currentHealth);

        if (hit && currentHealth > 0)
        {
            animationController.PlayTargetAnimation("Hit_F_1", true);
        }

        if (currentHealth <= 0)
        {
            playerController.Dead();
            currentHealth = 0;
            animationController.SetBool("Dead", true);
            animationController.PlayTargetAnimation("Hit_F_5_Die", true);
        }
    }

    public void TakeStaminaDamage(int damage)
    {
        currentStamina -= damage;

        staminaBar.SetCurrentStamina(currentStamina);

        if (currentStamina <= 0)
        {
            currentStamina = 0;
        }
    }

    public void AddCurrency(int amount)
    {
        stats.currency += amount;
        uiManager.CurrencyTextUpdate(stats.currency);
    }

    public void HealthUpdate()
    {
        SetMaxHealthFromHealthLevel();
        currentHealth += 8;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        healthBar.UpdateHealth(currentHealth, maxHealth);
    }

    public void StaminaUpdate()
    {
        SetMaxStaminaFromStaminaLevel();
        currentStamina += 8;
        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
        staminaBar.UpdateStamina(currentStamina, maxStamina);
    }
}
