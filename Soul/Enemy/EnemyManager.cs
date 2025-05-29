using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public PlayerController playerController;
    public EnemyStats enemyStats;
    public EnemyAnimatorManager enemyAnimatorManager;
    public EnemyLocomotionManager enemyLocomotionManager;
    public Rigidbody enemyRigidBody;

    [SerializeField] GameObject colliderObject;

    public State currentState;
    public CharacterStats currentTarget;
    public float detectionRadius = 20f;
    public float backDetectionRadius = 5f;
    public float minimumDetectionAngle = 45f;
    public float maximumDetectionAngle = 135f;

    public bool isAttacking;
    public bool isInteracting;

    public bool isDead;
    public float distanceFromTarget;
    public float maxChaseDistance = 35f;
    public bool isPerformingAction;
    public float currentRecoveryTime;
    public float maximumAttackRange;

    public NavMeshAgent navMeshAgent;

    public float rotationSpeed;
    public float viewableAngle;

    public Vector3 originPosition;
    public Vector3 originDirection;
    public Vector3 lastHitDirection;

    public bool isReturning;

    public float actionCoolTime = 1f;
    public float currentCooltime;

    public AlertState alertState;
    public IdleState idleState;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
        enemyRigidBody = GetComponent<Rigidbody>();
        enemyAnimatorManager = GetComponent<EnemyAnimatorManager>();
        enemyLocomotionManager = GetComponent<EnemyLocomotionManager>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.enabled = false;
        enemyRigidBody.isKinematic = false;

        originPosition = transform.position;
        originDirection = transform.forward;
        originDirection.y = 0f;
    }

    private void Start()
    {
        // enemyAnimatorManager.PlayTargetAnimation("Idle", true, false);
    }

    private void Update()
    {
        if (isDead) return;

        if (currentCooltime > 0)
        {
            currentCooltime -= Time.deltaTime;
        }
        HandleRecoveryTime();

    }

    private void FixedUpdate()
    {
        if (isDead) return;

        HandleCurrentAction();
    }

    private void HandleCurrentAction()
    {
        if (currentState != null)
        {
            State nextState = currentState.Tick(this, enemyStats, enemyAnimatorManager);

            if (nextState != null)
            {
                SwitchToNextState(nextState);
            }
        }
    }

    private void SwitchToNextState(State nextState)
    {
        currentState = nextState;
    }

    private void HandleRecoveryTime()
    {
        if (currentRecoveryTime > 0)
        {
            currentRecoveryTime = currentRecoveryTime - Time.deltaTime;
        }
        else
        {
            currentRecoveryTime = 0;
            isPerformingAction = false;
        }
    }

    public void Dead()
    {
        isDead = true;
        colliderObject.SetActive(false);
        playerController.LockOnCancel();
        StartCoroutine(DisableEnemy());
        enemyLocomotionManager.enabled = false;
        enemyRigidBody.isKinematic = true;
        // enemyStats.enabled = false;
        enemyAnimatorManager.enabled = false;
    }

    IEnumerator DisableEnemy()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
    }

    public void OnHit(Vector3 hitDirection)
    {
        if (currentState is IdleState || currentState is ReturnState)
        {
            State lastState = currentState;
            lastHitDirection = hitDirection;
            currentState = alertState;
            alertState.lastState = lastState;
            alertState.ResetAlertTimer();
        }
    }

    public void SpawnEnemy(Transform tran)
    {
        StopAllCoroutines();

        

        enemyStats.ResetEnemyStats();
        transform.position = tran.position;
        transform.rotation = tran.rotation;
        originPosition = tran.position;
        originDirection = tran.forward;
        currentState = idleState;
        enemyLocomotionManager.enabled = true;
        navMeshAgent.enabled = true;
        colliderObject.SetActive(true);
        isDead = false;
    }
}