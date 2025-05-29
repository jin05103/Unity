using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum EnemyType
{
    Basic,
    Boss
}

public class EnemyStats : CharacterStats
{
    Animator animator;
    EnemyManager enemyManager;
    public EnemyDropTable enemyDropTable;
    public EnemyType enemyType;

    public GameObject healthBarPosition;
    public GameObject healthBarParent;
    public Slider healthBarSlider;
    public TMP_Text damageText;

    IEnumerator hideDamageTextCoroutine;
    IEnumerator hideHealthBarCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyManager = GetComponent<EnemyManager>();
        enemyDropTable = GetComponent<EnemyDropTable>();
    }

    private void Start()
    {
        maxHealth = SetMaxHealthFromHealthLevel();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(healthBarPosition.transform.position);
        healthBarParent.transform.position = screenPos;
    }

    private float SetMaxHealthFromHealthLevel()
    {
        maxHealth = healthLevel * 10;
        return maxHealth;
    }

    public void ResetEnemyStats()
    {
        currentHealth = maxHealth;
        healthBarSlider.gameObject.SetActive(false);
        damageText.gameObject.SetActive(false);
        animator.SetBool("Die", false);
        //애니메이터 Idle 애니메이션으로 이동
        animator.Play("Idle");
        GetComponent<CapsuleCollider>().enabled = true;
    }

    public bool TakeDamage(int damage, Vector3 hitDirection)
    {
        currentHealth = currentHealth - damage;
        ShowHealthBar();
        ShowDamageText(damage);
        if (hitDirection != Vector3.zero)
        {
            enemyManager.OnHit(hitDirection);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            enemyManager.Dead();
            animator.SetBool("Die", true);
            // Die();
            GetComponent<CapsuleCollider>().enabled = false;

            return true;
        }
        else
        {
            if (enemyType == EnemyType.Basic)
            {
                animator.Play("Hit_F_1");
            }
            return false;
        }
    }

    public void ShowDamageText(int damage)
    {
        if (hideDamageTextCoroutine != null)
        {
            StopCoroutine(hideDamageTextCoroutine);
        }
        damageText.gameObject.SetActive(true);
        damageText.text = damage.ToString();
        hideDamageTextCoroutine = HideDamageText();
        StartCoroutine(hideDamageTextCoroutine);
    }

    public void ShowHealthBar()
    {
        if (hideHealthBarCoroutine != null)
        {
            StopCoroutine(hideHealthBarCoroutine);
        }
        healthBarSlider.gameObject.SetActive(true);
        healthBarSlider.value = currentHealth / maxHealth;
        hideHealthBarCoroutine = HideHealthBar();
        StartCoroutine(hideHealthBarCoroutine);
    }

    IEnumerator HideDamageText()
    {
        yield return new WaitForSeconds(1.5f);
        damageText.gameObject.SetActive(false);
    }

    IEnumerator HideHealthBar()
    {
        yield return new WaitForSeconds(3f);
        healthBarSlider.gameObject.SetActive(false);
    }
}
