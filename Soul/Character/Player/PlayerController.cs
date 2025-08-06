using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;
    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;
    public float blockSpeed = 1.0f;
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    // public AudioClip LandingAudioClip;
    // public AudioClip[] FootstepAudioClips;
    // [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the collider")]
    public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // Cinemachine
    float _cinemachineTargetYaw;
    public float _cinemachineTargetPitch;

    // Player
    float _speed;
    float _animationBlend;
    float _targetRotation = 0.0f;
    float _rotationVelocity;
    float _verticalVelocity;
    float _terminalVelocity = 53.0f;

    // Timeout delta
    float _jumpTimeoutDelta;
    float _fallTimeoutDelta;

    // private Animator _animator;
    AnimationController animationController;
    Rigidbody _rigidbody;
    InputsHandler _input;
    PlayerAttacker playerAttacker;
    PlayerInventory playerInventory;
    PlayerStats playerStats;
    [SerializeField] UIManager uIManager;
    [SerializeField] InteractableUI interactableUI;
    GameObject _mainCamera;
    const float _threshold = 0.01f;
    float _fallDuration = 0f;
    Vector3 groundNormal;

    List<Interactable> interactableObjects = new List<Interactable>();
    int interactableObjectIndex = 0;
    int lastInteractableObjectCount;
    Interactable interactableObject;
    [SerializeField] GameObject interactableObjectPanel;
    [SerializeField] GameObject getItemPanel;

    public float lockOnDistance = 25.0f;
    bool lockOn;
    Transform lockOnTarget;
    [SerializeField] GameObject lockOnIndicator;
    bool combo;

    public GameObject shieldSlot;
    public GameObject potionSlot;

    public bool isDead;

    public bool twoHanded;
    public bool isBlocking;
    public float currentStaminaRegenDelay = 0f;
    public float rollStaminaCost = 6f;
    public float jumpStaminaCost = 4f;
    public float blockStaminaCost = 20f;
    public float sprintStaminaCost = 5f;
    public float parryStaminaCost = 15f;
    public float blockingStaminaGenRate = 0.4f;
    public bool isParrying;
    public bool isDrinking;

    public int restPotionCount = 0;
    public int maxRestPotionCount = 4;

    public LayerMask interactableLayerMask;

    public Transform respawnPoint;
    public GameObject blackScreen;
    public EnemySpawner enemySpawner;

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        playerAttacker = GetComponent<PlayerAttacker>();
        playerInventory = GetComponent<PlayerInventory>();
        playerStats = GetComponent<PlayerStats>();

    }

    private void Start()
    {
        lockOnIndicator.SetActive(false);
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _rigidbody = GetComponent<Rigidbody>();
        animationController = GetComponent<AnimationController>();

        _input = GetComponent<InputsHandler>();

        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        groundNormal = Vector3.up;

        restPotionCount = maxRestPotionCount;
        uIManager.restPotionText.GetComponent<TMP_Text>().text = restPotionCount.ToString();
    }

    private void Update()
    {
        if (isDead) return;

        LockOn();
        CheckLockOnDistance();
        CheckInteract();
        SelectInteract();
        if (lockOn && lockOnTarget != null)
        {
            // Vector3 worldPosition = new Vector3(lockOnTarget.position.x, lockOnTarget.position.y + 1.5f, lockOnTarget.position.z);
            Vector3 worldPosition = lockOnTarget.Find("LockOnIndicator").position;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            lockOnIndicator.transform.position = screenPosition;
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
            if (!Grounded)
            {
                Fall();

                return;
            }
            return;
        }

        if (getItemPanel.activeSelf && _input.interact)
        {
            _input.interact = false;
            getItemPanel.SetActive(false);
        }

        if (animationController.GetInteractingState())
        {
            if (!animationController.GetBool("Blocked") && !animationController.GetBool("Parry"))
            {
                BlockCancel();
            }
            currentStaminaRegenDelay = playerStats.staminaRegenDelay;
            // _animationBlend = 0f;
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
            if (!Grounded)
            {
                Fall();

                return;
            }

            if (_input.leftClick && animationController.GetBool("CanDoCombo"))
            {
                animationController.SetBool("DoComboAttack", true);
            }

            if (isDrinking)
            {
                Move();
            }

            return;
        }
        else
        {
            if (currentStaminaRegenDelay <= 0f)
            {
                if (isBlocking)
                {
                    playerStats.currentStamina += playerStats.staminaRegenRate * Time.deltaTime * blockingStaminaGenRate;
                }
                else
                {
                    playerStats.currentStamina += playerStats.staminaRegenRate * Time.deltaTime;
                }

                if (playerStats.currentStamina > playerStats.maxStamina)
                {
                    playerStats.currentStamina = playerStats.maxStamina;
                }
                playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);
            }
            else
            {
                currentStaminaRegenDelay -= Time.deltaTime;
            }

            if (isDrinking)
            {
                isDrinking = false;
                animationController.PotionAnim(false);
                WeaponHide(false);
            }
        }

        if (animationController.GetBool("DoComboAttack"))
        {
            animationController.SetBool("DoComboAttack", false);
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0.0f, _targetRotation, 0.0f);
            playerAttacker.HandleWeaponCombo(playerInventory.rightWeapon);

            _input.leftClick = false;

            return;
        }

        if (!animationController.GetBool("JumpEnd"))
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _animationBlend = 0f;
            animationController.SetFloat("Speed", 0f);
            animationController.SetFloat("MotionSpeed", 0f);
            return;
        }

        JumpAndGravity();
        GroundedCheck();
        // Fall();

        Move();
        Roll();
        Attack();
        Interact();
        WeaponChange();
        Block();
        Parry();
        ItemUse();

    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void LockOn()
    {
        if (_input.lockOn)
        {
            _input.lockOn = false;
            if (lockOn)
            {
                lockOn = false;
                lockOnTarget = null;
                lockOnIndicator.SetActive(false);
                animationController.SetBool("LockOn", false);
            }
            else
            {
                // 메인 카메라 컴포넌트 가져오기
                Camera cam = _mainCamera.GetComponent<Camera>();
                Vector3 origin = cam.transform.position;
                Vector3 direction = cam.transform.forward;

                // 인식 가능 거리와 스피어 반경 설정 (이 값들은 상황에 맞게 조절하거나 Inspector에서 관리할 수 있음)
                float maxDistance = lockOnDistance; // 락온 최대 거리
                float sphereRadius = 5.0f; // 스피어캐스트 반경

                // 카메라 전방으로 스피어캐스트 실행
                RaycastHit[] hits = Physics.SphereCastAll(origin, sphereRadius, direction, maxDistance);

                GameObject bestTarget = null;
                float bestScreenDistance = float.MaxValue;
                Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);

                foreach (RaycastHit hit in hits)
                {
                    // 적 태그를 가진 대상만 처리 (적이 다른 태그나 Layer를 사용할 경우 해당 조건으로 변경)
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        // 적의 위치를 화면 좌표로 변환
                        Vector3 screenPos = cam.WorldToScreenPoint(hit.collider.transform.position);
                        // 화면 앞쪽(카메라에 보이는 영역)인 경우만 고려
                        if (screenPos.z > 0)
                        {
                            // 화면 중앙과의 거리를 계산하여 가장 가까운 적을 선택
                            float distance = Vector2.Distance(new Vector2(screenPos.x, screenPos.y),
                                                              new Vector2(screenCenter.x, screenCenter.y));
                            if (distance < bestScreenDistance)
                            {
                                bestScreenDistance = distance;
                                bestTarget = hit.collider.gameObject;
                            }
                        }
                    }
                }

                if (bestTarget != null)
                {
                    // 가장 적합한 적이 발견되면 해당 타겟을 락온 상태로 설정
                    lockOnTarget = bestTarget.transform;
                    lockOn = true;
                    lockOnIndicator.SetActive(true);
                    animationController.SetBool("LockOn", true);
                }
                else
                {
                    // 타겟팅 가능한 적이 없으면 락온 해제
                    lockOnTarget = null;
                    lockOn = false;
                    lockOnIndicator.SetActive(false);
                    animationController.SetBool("LockOn", false);
                }
            }
        }
    }

    private void CheckLockOnDistance()
    {
        if (lockOnTarget != null)
        {
            float distance = Vector3.Distance(transform.position, lockOnTarget.position);
            if (distance > lockOnDistance)
            {
                LockOnCancel();
            }
        }
    }

    public void LockOnCancel()
    {
        if (lockOn)
        {
            lockOn = false;
            lockOnTarget = null;
            lockOnIndicator.SetActive(false);
            animationController.SetBool("LockOn", false);
        }
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        if (!Grounded)
        {
            animationController.SetFloat("FallBlend", _fallDuration);
        }

        animationController.SetBool("Grounded", Grounded);
    }

    private void Fall()
    {
        _fallDuration += Time.deltaTime;
        // animationController.SetFloat("FallBlend", _fallDuration);
        _verticalVelocity += Gravity * Time.deltaTime;
        _rigidbody.linearVelocity += new Vector3(0.0f, _verticalVelocity, 0.0f);
    }

    private void CameraRotation()
    {
        if (!lockOn)
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = 1.0f;
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }
        else
        {
            _input.look = Vector2.zero;
            Vector3 direction = lockOnTarget.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _cinemachineTargetPitch = targetRotation.eulerAngles.x;
            _cinemachineTargetYaw = targetRotation.eulerAngles.y;

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }
    }

    private void JumpAndGravity()
    {
        if (animationController.GetInteractingState())
        {
            return;
        }

        if (Grounded)
        {
            _fallDuration = 0f;
            _fallTimeoutDelta = FallTimeout;

            animationController.SetBool("Jump", false);
            animationController.SetBool("FreeFall", false);

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            if (_input.jump)
            {
                if (playerStats.currentStamina < jumpStaminaCost)
                {
                    return;
                }
                else
                {
                    playerStats.currentStamina -= jumpStaminaCost;
                    playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);
                }

                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                animationController.SetBool("Jump", true);
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _fallDuration += Time.deltaTime;

            animationController.SetFloat("FallBlend", _fallDuration);

            _jumpTimeoutDelta = JumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                animationController.SetBool("FreeFall", true);
            }

            // Prevent jumping while falling
            _input.jump = false;
        }

        // Apply custom gravity
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void Move()
    {
        if (animationController.GetInteractingState() && !isDrinking)
        {
            return;
        }

        if (!lockOn)
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (playerStats.currentStamina < 1f)
            {
                targetSpeed = MoveSpeed;
            }
            if (isBlocking || isDrinking)
            {
                targetSpeed = blockSpeed;
            }

            if (_input.move == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }

            if (targetSpeed == SprintSpeed)
            {
                currentStaminaRegenDelay = playerStats.staminaRegenDelay;
                playerStats.currentStamina -= sprintStaminaCost * Time.deltaTime;
                playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);
            }

            float currentHorizontalSpeed = new Vector3(_rigidbody.linearVelocity.x, 0.0f, _rigidbody.linearVelocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f)
                _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            RaycastHit hit;
            groundNormal = Vector3.up;
            float rayDistance = 0.4f; // 상황에 맞게 조정
            if (Physics.Raycast(transform.position + new Vector3(0f, 0.2f, 0f), Vector3.down, out hit, rayDistance, GroundLayers))
            {
                groundNormal = hit.normal;
            }

            Vector3 moveDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            Vector3 slopeDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
            Vector3 moveVelocity = slopeDirection * _speed;

            _rigidbody.linearVelocity = moveVelocity;
            if (!Grounded || _verticalVelocity > 0f)
            {
                _rigidbody.linearVelocity += new Vector3(0.0f, _verticalVelocity, 0.0f);
            }

            animationController.SetFloat("Speed", _animationBlend);
            animationController.SetFloat("MotionSpeed", inputMagnitude);
        }
        else
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (playerStats.currentStamina < 1f)
            {
                targetSpeed = MoveSpeed;
            }
            if (isBlocking || isDrinking)
            {
                targetSpeed = blockSpeed;
            }
            if (_input.move == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }
            if (targetSpeed == SprintSpeed)
            {
                currentStaminaRegenDelay = playerStats.staminaRegenDelay;
                playerStats.currentStamina -= sprintStaminaCost * Time.deltaTime;
                playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);
            }

            float currentHorizontalSpeed = new Vector3(_rigidbody.linearVelocity.x, 0.0f, _rigidbody.linearVelocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f)
                _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                Vector3 direction = lockOnTarget.position - transform.position;
                direction.y = 0;
                _targetRotation = Quaternion.LookRotation(direction).eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            RaycastHit hit;
            groundNormal = Vector3.up;
            float rayDistance = 0.4f; // 상황에 맞게 조정
            if (Physics.Raycast(transform.position + new Vector3(0f, 0.2f, 0f), Vector3.down, out hit, rayDistance, GroundLayers))
            {
                groundNormal = hit.normal;
            }

            Vector3 moveDirection = Vector3.forward * _input.move.y + Vector3.right * _input.move.x;
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);
            Vector3 slopeDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * slopeDirection;
            Vector3 moveVelocity = targetDirection * _speed;

            _rigidbody.linearVelocity = moveVelocity;
            if (!Grounded || _verticalVelocity > 0f)
            {
                _rigidbody.linearVelocity += new Vector3(0.0f, _verticalVelocity, 0.0f);
            }

            Vector3 a = new Vector3(_input.move.x, 0, _input.move.y);
            // a.Normalize();
            a = a * _speed / SprintSpeed;


            animationController.SetFloat("X", a.x);
            animationController.SetFloat("Y", a.z);

            animationController.SetFloat("Speed", _animationBlend);
            animationController.SetFloat("MotionSpeed", inputMagnitude);
        }
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        angle = (angle + 360f) % 360f; // 0~360
        if (angle > 180f)
            angle -= 360f; // -180~180

        return Mathf.Clamp(angle, min, max);
    }

    public void Roll()
    {
        if (animationController.GetInteractingState())
        {
            return;
        }
        if (playerStats.currentStamina < rollStaminaCost)
        {
            return;
        }

        if (!lockOn)
        {
            if (_input.roll)
            {
                playerStats.currentStamina -= rollStaminaCost;
                playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);

                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                // _rigidbody.useGravity = true;
                // animationController.SetFloat("Speed", 0f);
                transform.rotation = Quaternion.Euler(0.0f, _targetRotation, 0.0f);
                animationController.PlayTargetAnimation("Rolling", true);
            }
        }
        else
        {
            if (_input.roll)
            {
                playerStats.currentStamina -= rollStaminaCost;
                playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);

                Vector3 inputDirection;

                if (_input.move.y != 0 && _input.move.x != 0)
                {
                    inputDirection = new Vector3(_input.move.x, 0.0f, 0.0f).normalized;
                }
                else if (_input.move == Vector2.zero)
                {
                    inputDirection = new Vector3(0.0f, 0.0f, 1.0f);
                }
                else
                {
                    inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
                }

                animationController.SetFloat("X", inputDirection.x);
                animationController.SetFloat("Y", inputDirection.z);
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                animationController.PlayTargetAnimation("Roll Blend", true);
            }
        }
    }

    private void Attack()
    {
        if (animationController.GetInteractingState())
        {
            return;
        }

        if (playerInventory.rightWeapon.weaponType == WeaponType.none || (playerInventory.rightWeapon.weaponType == WeaponType.bow && !twoHanded))
        {
            return;
        }

        if (_input.sprint && _input.leftClick)
        {
            if (playerInventory.GetWeaponType(false) == WeaponType.bow)
            {
                return;
            }

            if (playerStats.currentStamina < playerAttacker.StaminaCost(playerInventory.rightWeapon, "Heavy"))
            {
                return;
            }

            playerStats.currentStamina -= playerAttacker.StaminaCost(playerInventory.rightWeapon, "Heavy");
            playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);

            playerInventory.weaponSlotManager.rightHandSlot.GetComponentInChildren<DamageCollider>().HeavyAttack = true;

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0.0f, _targetRotation, 0.0f);
            playerAttacker.HandleHeavyAttack(playerInventory.rightWeapon);

            _input.leftClick = false;
        }
        else if (_input.leftClick)
        {
            if (playerStats.currentStamina < playerAttacker.StaminaCost(playerInventory.rightWeapon, "Light"))
            {
                return;
            }

            if (playerInventory.GetWeaponType(false) == WeaponType.bow && !twoHanded)
            {
                return;
            }

            playerStats.currentStamina -= playerAttacker.StaminaCost(playerInventory.rightWeapon, "Light");
            playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0.0f, _targetRotation, 0.0f);
            playerAttacker.HandleLightAttack(playerInventory.rightWeapon);

            _input.leftClick = false;
        }
        // else if (_input.rightClick)
        // {
        //     playerAttacker.HandleHeavyAttack(playerInventory.rightWeapon);
        // }
    }

    private void CheckInteract()
    {
        // 주어진 반경 내의 Collider들을 가져옴
        // Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f, interactableLayerMask);

        // 이미 감지된 interactableObjects를 추적할 리스트
        List<Interactable> detectedInteractables = new List<Interactable>();

        // 감지된 Collider들 중 Interactable 오브젝트만 리스트에 추가
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Interactable"))
            {
                Interactable interactable = hitCollider.GetComponent<Interactable>();
                if (interactable != null)
                {
                    detectedInteractables.Add(interactable);
                }
            }
        }

        // 기존의 interactableObjects와 비교하여 추가되거나 제거된 오브젝트들만 갱신
        // 추가된 오브젝트가 있으면 리스트에 추가
        foreach (var newInteractable in detectedInteractables)
        {
            if (!interactableObjects.Contains(newInteractable))
            {
                interactableObjects.Add(newInteractable);
            }
        }

        // 제거된 오브젝트가 있으면 리스트에서 제거
        for (int i = interactableObjects.Count - 1; i >= 0; i--)
        {
            if (!detectedInteractables.Contains(interactableObjects[i]))
            {
                interactableObjects.RemoveAt(i);
            }
        }
    }

    private void SelectInteract()
    {
        if (interactableObjects.Count == 0)
        {
            interactableObjectIndex = 0;
            interactableObject = null;
            interactableUI.interactableText.text = "";
            interactableObjectPanel.SetActive(false);
            return;
        }

        if (interactableObjects.Count == 1)
        {
            interactableObjectIndex = 0;
            interactableObject = interactableObjects[interactableObjectIndex];
        }
        else if (interactableObjects.Count > 1)
        {
            if (interactableObjectIndex >= interactableObjects.Count)
            {
                interactableObjectIndex = 0;
            }

            if (_input.interactLeft && interactableObjectIndex > 0)
            {
                _input.interactLeft = false;
                interactableObjectIndex--;
            }
            else if (_input.interactRight && interactableObjectIndex < interactableObjects.Count)
            {
                _input.interactRight = false;
                interactableObjectIndex++;
            }

            interactableObject = interactableObjects[interactableObjectIndex];
        }
        interactableUI.interactableText.text = interactableObject.interactableText;
        interactableObjectPanel.SetActive(true);
    }

    private void Interact()
    {
        if (animationController.GetInteractingState())
        {
            return;
        }

        if (interactableObject != null && _input.interact)
        {
            _input.interact = false;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            interactableObject.Interact(gameObject);
        }
    }

    public void GetItem(Item weapon)
    {
        interactableUI.itemText.text = weapon.itemName;
        interactableUI.itemImage.texture = weapon.itemIcon.texture;
        getItemPanel.SetActive(true);
    }

    public void WeaponChange()
    {
        if (animationController.GetInteractingState())
        {
            return;
        }

        BlockCancel();
        // 잡기 변경, 오른손 무기 변경, 왼손 무기 변경 로직을 Update나 관련 함수 내에 작성합니다.
        if (_input.handChange)
        {
            _input.handChange = false;
            // 우수(오른손)에 무기가 있는지 확인 (없으면 아무것도 안 함)
            var rightWeapon = playerInventory.GetWeaponType(false);
            if (rightWeapon == WeaponType.none)
            {
                return;
            }
            else
            {
                // 현재 상태가 한손잡이 상태라면 → 양손잡이 모션으로 전환
                if (!twoHanded)
                {
                    // 오른손 무기의 타입에 따라 양손잡이 모션 적용 (두번째 파라미터 true: 양손 모션)
                    animationController.ChangeWeaponLayer(rightWeapon, true, false);
                    twoHanded = true;
                    if (playerInventory.GetWeaponType(false) == WeaponType.bow)
                    {
                        //활 왼손으로 이동
                        playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.parent
                        = playerInventory.weaponSlotManager.rightHandSlot.parentOverride2;
                        playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.localPosition = Vector3.zero;
                        playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.localScale = Vector3.one;
                    }
                    if (playerInventory.GetWeaponType(true) == WeaponType.shield)
                    {
                        //방패 off
                        shieldSlot.SetActive(false);
                    }
                }
                else // 현재 양손잡이 상태일 경우
                {
                    // 왼손에 방패가 있는지 확인
                    var leftWeapon = playerInventory.GetWeaponType(true);
                    if (leftWeapon == WeaponType.shield)
                    {
                        // 방패가 있을 경우, 오른손 무기를 한손 모션 + 방패 오버라이드 (두번째 파라미터 false, 세번째 true)
                        animationController.ChangeWeaponLayer(rightWeapon, false, true);
                        //방패 on
                        shieldSlot.SetActive(true);
                        twoHanded = false;
                    }
                    else
                    {
                        // 왼손에 방패가 없으면 단순히 한손 모션으로 전환
                        animationController.ChangeWeaponLayer(rightWeapon, false, false);
                        twoHanded = false;
                    }

                    if (playerInventory.GetWeaponType(false) == WeaponType.bow)
                    {
                        //활 오른손으로 이동
                        playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.parent
                        = playerInventory.weaponSlotManager.rightHandSlot.parentOverride;
                        playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.localPosition = Vector3.zero;
                        playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.localScale = Vector3.one;
                    }
                }
            }
        }
        else if (_input.dRight)
        {
            // 오른손 무기 변경 시
            _input.dRight = false;
            playerInventory.ChangeRightWeapon();
            var newRightWeapon = playerInventory.GetWeaponType(false);
            if (!shieldSlot.activeSelf)
            {
                shieldSlot.SetActive(true);
            }

            if (twoHanded)
            {
                // 현재 양손잡이 상태라면, 왼손에 방패가 있는지 확인
                var leftWeapon = playerInventory.GetWeaponType(true);
                if (leftWeapon == WeaponType.shield)
                {
                    // 방패가 있을 경우 → 한손 모션 + 방패 오버라이드
                    animationController.ChangeWeaponLayer(newRightWeapon, false, true);
                }
                else
                {
                    // 방패가 없으면 → 한손 모션으로만 변경
                    animationController.ChangeWeaponLayer(newRightWeapon, false, false);
                }
                twoHanded = false;
            }
            else
            {
                // 한손잡이 상태라면 오른손 무기만 한손 모션으로 적용
                animationController.ChangeWeaponLayer(newRightWeapon, false, false);
                twoHanded = false;
            }
        }
        else if (_input.dLeft)
        {
            // 왼손 무기 변경 시
            _input.dLeft = false;
            playerInventory.ChangeLeftWeapon();
            var newLeftWeapon = playerInventory.GetWeaponType(true);
            if (!shieldSlot.activeSelf)
            {
                shieldSlot.SetActive(true);
            }

            if (twoHanded)
            {
                // 양손잡이 상태라면, 기존 오른손 무기를 한손 모션으로 전환한 후 왼손 무기로 오버라이드
                var currentRightWeapon = playerInventory.GetWeaponType(false);
                // 왼손: 변경된 무기로 오버라이드 (세번째 파라미터 true로 방패 오버라이드 처리)
                if (newLeftWeapon == WeaponType.shield)
                {
                    animationController.ChangeWeaponLayer(currentRightWeapon, false, true);
                }
                else
                {
                    animationController.ChangeWeaponLayer(currentRightWeapon, false, false);
                }

                if (playerInventory.GetWeaponType(false) == WeaponType.bow)
                {
                    //활 오른손으로 이동
                    playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.parent
                    = playerInventory.weaponSlotManager.rightHandSlot.parentOverride;
                    playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.localPosition = Vector3.zero;
                    playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    playerInventory.weaponSlotManager.rightHandSlot.currentWeaponModel.transform.localScale = Vector3.one;
                }
                twoHanded = false;
            }
            else
            {
                var currentRightWeapon = playerInventory.GetWeaponType(false);
                if (newLeftWeapon == WeaponType.shield)
                {
                    animationController.ChangeWeaponLayer(currentRightWeapon, false, true);
                }
                else
                {
                    animationController.ChangeWeaponLayer(currentRightWeapon, false, false);
                }
            }
        }
    }

    public void Block()
    {
        if (animationController.GetInteractingState())
        {
            return;
        }

        if (_input.rightClick)
        {
            if (isBlocking)
            {
                return;
            }

            if (!twoHanded && playerInventory.GetWeaponType(true) == WeaponType.shield)
            {
                isBlocking = true;
                animationController.ShieldAnim(true);
                animationController.SetBool("Block", true);
            }
            else if (twoHanded)
            {
                if (playerInventory.GetWeaponType(false) == WeaponType.bow)
                {
                    return;
                }
                else
                {
                    isBlocking = true;
                    animationController.TwoHandShieldAnim(true);
                    animationController.SetBool("Block", true);
                }
            }
            else
            {
                return;
            }
        }
        else
        {
            if (isBlocking)
            {
                BlockCancel();
            }
            // animationController.SetBool("Block", false);
            // isBlocking = false;
        }
    }

    public void BlockCancel()
    {
        if (!twoHanded && playerInventory.GetWeaponType(true) == WeaponType.shield)
        {
            animationController.ShieldAnim(false);
        }
        else if (twoHanded)
        {
            animationController.TwoHandShieldAnim(false);
        }
        animationController.SetBool("TwoHandBlock", false);
        animationController.SetBool("Block", false);
        isBlocking = false;
    }

    public void Blocked(int currentWeaponDamage, float guardStaminaCost)
    {
        currentStaminaRegenDelay = playerStats.staminaRegenDelay;

        if (playerStats.currentStamina >= guardStaminaCost)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            playerStats.currentStamina -= guardStaminaCost;
            playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);

            animationController.SetBool("Blocked", true);
            int damage;
            if (twoHanded)
            {
                damage = (int)(currentWeaponDamage * (100 - playerInventory.rightWeapon.guardRate) / 100);
            }
            else
            {
                damage = (int)(currentWeaponDamage * (100 - playerInventory.leftWeapon.guardRate) / 100);
            }

            playerStats.TakeDamage(damage, false);
        }
        else
        {
            playerStats.currentStamina = 0;
            playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);
            playerStats.TakeDamage(currentWeaponDamage, true);
        }


    }

    public void Parry()
    {
        if (animationController.GetInteractingState())
        {
            return;
        }

        if (_input.parry && playerInventory.leftWeapon.weaponType == WeaponType.shield && !twoHanded)
        {
            _input.parry = false;

            if (playerStats.currentStamina >= parryStaminaCost)
            {
                playerStats.currentStamina -= parryStaminaCost;
                playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);
                // isParrying = true;
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                animationController.ShieldAnim(true);
                animationController.SetBool("Parry", true);
            }
        }
    }

    public void ParrySuccess()
    {
        playerInventory.weaponSlotManager.leftHandSlot.GetComponentInChildren<ParticleSystem>().Play();
    }

    public void ItemUse()
    {
        if (animationController.GetInteractingState())
        {
            return;
        }

        if (_input.itemUse)
        {
            _input.itemUse = false;

            isDrinking = true;
            animationController.SetBool("IsInteracting", true);

            // _rigidbody.linearVelocity = Vector3.zero;
            // _rigidbody.angularVelocity = Vector3.zero;
            if (restPotionCount > 0)
            {
                WeaponHide(true);
                restPotionCount--;
                uIManager.PotionTextUpdate(restPotionCount);

                animationController.SetBool("Drink", true);
                animationController.PotionAnim(true);
            }
            else
            {
                WeaponHide(true);
                animationController.SetBool("CannotDrink", true);
                animationController.PotionAnim(true);
            }
        }
    }

    public void WeaponHide(bool hide)
    {
        if (hide)
        {
            playerInventory.weaponSlotManager.leftHandSlot.gameObject.SetActive(false);
            playerInventory.weaponSlotManager.rightHandSlot.gameObject.SetActive(false);
            potionSlot.gameObject.SetActive(true);
        }
        else
        {
            playerInventory.weaponSlotManager.leftHandSlot.gameObject.SetActive(true);
            playerInventory.weaponSlotManager.rightHandSlot.gameObject.SetActive(true);
            potionSlot.gameObject.SetActive(false);
        }
    }

    public void Dead()
    {
        isDead = true;

        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        blackScreen.SetActive(true);

        yield return new WaitForSeconds(1f);

        while (blackScreen.GetComponent<Image>().color.a < 1f)
        {
            blackScreen.GetComponent<Image>().color += new Color(0, 0, 0, Time.deltaTime * 0.3f);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        transform.position = respawnPoint.position;

        restPotionCount = maxRestPotionCount;
        playerStats.currentHealth = playerStats.maxHealth;
        playerStats.currentStamina = playerStats.maxStamina;

        playerStats.healthBar.SetCurrentHealth(playerStats.currentHealth);
        playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);
        uIManager.PotionTextUpdate(restPotionCount);

        playerStats.stats.currency = 0;
        uIManager.CurrencyTextUpdate(playerStats.stats.currency);
        enemySpawner.ResetEnemies();

        animationController.SetBool("Dead", false);

        while (blackScreen.GetComponent<Image>().color.a > 0f)
        {
            blackScreen.GetComponent<Image>().color -= new Color(0, 0, 0, Time.deltaTime * 0.3f);
            yield return null;
        }
        blackScreen.SetActive(false);

        isDead = false;
    }

    public void Rest(Transform restPoint)
    {
        restPotionCount = maxRestPotionCount;
        playerStats.currentHealth = playerStats.maxHealth;
        playerStats.currentStamina = playerStats.maxStamina;

        animationController.PlayTargetAnimation("Item_Give", true);

        playerStats.healthBar.SetCurrentHealth(playerStats.currentHealth);
        playerStats.staminaBar.SetCurrentStamina(playerStats.currentStamina);
        uIManager.PotionTextUpdate(restPotionCount);

        respawnPoint = restPoint;
        transform.position = respawnPoint.position;
    }
}