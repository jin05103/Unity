using UnityEngine;

public class ResetInteracting : StateMachineBehaviour
{
   // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
   override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   {
      animator.applyRootMotion = true;
      animator.SetFloat("Speed", 0);
   }

   // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
   override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   {
      animator.SetBool("IsInteracting", true);  
   }

   // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
   override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   {
      AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(layerIndex);
      if (!nextState.IsName("Hit_F_1")) // 또는 여러 Hit 계열이면 조건 확장
    {
        animator.SetBool("IsInteracting", false);
        animator.applyRootMotion = false;
    }
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
