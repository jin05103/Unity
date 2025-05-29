using UnityEngine;

public class EnemyAnimatorManager : AnimatorManager
{
    EnemyManager enemyManager;

    public void PlayTargetAnimation(string targetAnim, bool isInteracting)
    {
        _animator.applyRootMotion = isInteracting;
        _animator.SetBool("IsInteracting", isInteracting);
        _animator.CrossFade(targetAnim, 0.2f);
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        enemyManager = GetComponent<EnemyManager>();
    }

    private void OnAnimatorMove()
    {
        if (enemyManager.isDead) return;
        float delta = Time.deltaTime;
        enemyManager.enemyRigidBody.linearDamping = 0;
        Vector3 deltaPosition = _animator.deltaPosition;
        deltaPosition.y = 0;
        Vector3 velocity = deltaPosition / delta;
        enemyManager.enemyRigidBody.linearVelocity = velocity;
    }

    public bool GetIsInteracting()
    {
        return _animator.GetBool("IsInteracting");
    }
}
