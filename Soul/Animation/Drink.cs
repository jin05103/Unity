using UnityEngine;

public class Drink : StateMachineBehaviour
{
    private float timer = 0f;
    PlayerStats health;
    float healthAmount;
    bool checkAmount;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsInteracting", true);
        checkAmount = false;
        timer = 0f;
        healthAmount = 0f;
        health = animator.GetComponent<PlayerStats>();
        // animator.SetFloat("Speed", 0);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetBool("Drink"))
        {
            timer += Time.deltaTime; // 누적 시간 업데이트

            if (timer >= 0.5f && timer < 1.5f)
            {
                // Health 컴포넌트가 Animator가 붙은 게임 오브젝트에 있다고 가정
                if (health != null)
                {
                    float healTick = health.healPotionAmount * Time.deltaTime;
                    healthAmount += healTick;
                    health.currentHealth += healTick;
                    if (health.currentHealth > health.maxHealth)
                    {
                        health.currentHealth = health.maxHealth;
                    }
                    health.healthBar.SetCurrentHealth(health.currentHealth);
                }
            }
            else if(timer >= 1.5f)
            {
                if (checkAmount == false)
                {
                    checkAmount = true;
                    if (healthAmount < health.healPotionAmount)
                    {
                        health.currentHealth += health.healPotionAmount - healthAmount;
                        if (health.currentHealth > health.maxHealth)
                        {
                            health.currentHealth = health.maxHealth;
                        }
                        health.healthBar.SetCurrentHealth(health.currentHealth);
                    }
                }
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;

        animator.SetBool("IsInteracting", false);
        animator.SetBool("Drink", false);
        animator.SetBool("CannotDrink", false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
