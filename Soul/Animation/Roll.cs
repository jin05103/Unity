using UnityEngine;

public class Roll : StateMachineBehaviour
{
   // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
   override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   {
      animator.applyRootMotion = true;
      animator.SetBool("Immune", true);
      animator.SetFloat("Speed", 0);
      // animator.GetComponent<CapsuleCollider>().enabled = false;
   }

   // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
   //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   //{
   //    
   //}

   // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
   override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   {
      animator.SetBool("IsInteracting", false);
      animator.SetBool("Immune", false);
      animator.applyRootMotion = false;
      // animator.GetComponent<CapsuleCollider>().enabled = true;
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
