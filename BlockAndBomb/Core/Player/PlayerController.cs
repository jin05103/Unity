using UnityEngine;
using Unity.Netcode;
using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using System.Collections.Generic;
using Assets.HeroEditor4D.Common.Scripts.Enums;

[RequireComponent(typeof(Rigidbody2D))]
// [RequireComponent(typeof(ItemHandler))]
public class PlayerController : NetworkBehaviour
{
    public static List<PlayerController> AlivePlayers = new List<PlayerController>();

    public enum Direction { Up, Down, Left, Right }
    public Character4D Character;
    [SerializeField] AnimationEvents animationEvents;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] PlayerStatus playerStatus;
    [SerializeField] PlayerInstallState playerInstallState;
    [SerializeField] PlayerStatManager playerStatManager;

    public bool InitDirection;

    private List<Direction> directionQueue = new List<Direction>();
    private Vector2 dir;
    private NetworkVariable<Vector2> currentDirection = new NetworkVariable<Vector2>(
        Vector2.down, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private bool _moving;

    public GameObject FrontObj, BackObj, LeftObj, RightObj;

    public bool isGameStarted = false;
    public bool isAlive = true;

    private Vector2 _lastSentDirection = Vector2.zero;

    public void Start()
    {
        Character.AnimationManager.SetState(CharacterState.Idle);

        if (InitDirection)
        {
            Character.SetDirection(Vector2.down);
        }
    }

    private void OnEnable()
    {
        AlivePlayers.Add(this);
    }

    private void OnDisable()
    {
        AlivePlayers.Remove(this);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            currentDirection.OnValueChanged += OnDirectionChanged;
        }
        else
        {
            animationEvents.OnEvent += OnAnimationEvent;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
        {
            currentDirection.OnValueChanged -= OnDirectionChanged;
        }
        else
        {
            animationEvents.OnEvent -= OnAnimationEvent;
        }
    }

    private void Update()
    {
        if (!isGameStarted || !isAlive)
        {
            return;
        }

        if (IsOwner)
        {
            SetDirection();
            Move();
            Actions();
        }
    }

    private void OnDirectionChanged(Vector2 previous, Vector2 current)
    {
        UpdateDirectionObjects();
        Character.SetDirection(current);
    }

    public void SetGameStarted(bool started, ExpPanel expPanel, StatPanel statPanel)
    {
        isGameStarted = started;
        playerStatus.SetExpPanel(expPanel);
        playerStatManager.LinkStatPanel(statPanel);
    }

    private void SetDirection()
    {
        CheckKey(KeyCode.UpArrow, Direction.Up);
        CheckKey(KeyCode.DownArrow, Direction.Down);
        CheckKey(KeyCode.LeftArrow, Direction.Left);
        CheckKey(KeyCode.RightArrow, Direction.Right);

        dir = Vector2.zero;
        if (directionQueue.Count > 0)
        {
            var lastDir = directionQueue[directionQueue.Count - 1];
            dir = DirectionToVector(lastDir);
            currentDirection.Value = dir;
        }

        if (dir != Vector2.zero)
        {
            Character.SetDirection(dir);
        }
    }

    void CheckKey(KeyCode key, Direction dir)
    {
        if (Input.GetKeyDown(key))
        {
            directionQueue.Remove(dir); // 중복방지
            directionQueue.Add(dir);
        }
        if (Input.GetKeyUp(key))
        {
            directionQueue.Remove(dir); // 키를 뗄 때 그 키만 제거
        }
    }

    Vector2 DirectionToVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Vector2.up;
            case Direction.Down: return Vector2.down;
            case Direction.Left: return Vector2.left;
            case Direction.Right: return Vector2.right;
            default: return Vector2.zero;
        }
    }

    private void UpdateDirectionObjects()
    {
        if (FrontObj && BackObj && LeftObj && RightObj)
        {
            FrontObj.SetActive(false);
            BackObj.SetActive(false);
            LeftObj.SetActive(false);
            RightObj.SetActive(false);

            if (currentDirection.Value == Vector2.up)
            {
                BackObj.SetActive(true);
            }
            else if (currentDirection.Value == Vector2.down)
            {
                FrontObj.SetActive(true);
            }
            else if (currentDirection.Value == Vector2.left)
            {
                LeftObj.SetActive(true);
            }
            else if (currentDirection.Value == Vector2.right)
            {
                RightObj.SetActive(true);
            }
        }
    }

    private void Move()
    {
        if (dir == Vector2.zero)
        {
            if (_moving)
            {
                SendCharacterStateServerRpc(CharacterState.Idle);

                Character.AnimationManager.SetState(CharacterState.Idle);
                rb.linearVelocity = Vector2.zero;
                _moving = false;
            }
            return;
        }
        else
        {
            if (!_moving)
            {
                SendCharacterStateServerRpc(CharacterState.Run);
            }
            Character.AnimationManager.SetState(CharacterState.Run);

            rb.linearVelocity = dir.normalized * playerStatus.speed.Value;
            _moving = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendCharacterStateServerRpc(CharacterState state)
    {
        UpdateCharacterStateClientRpc(state);
    }

    [ClientRpc]
    void UpdateCharacterStateClientRpc(CharacterState state)
    {
        if (IsOwner) return;
        Character.AnimationManager.SetState(state);
    }

    private void Actions()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !Character.AnimationManager.GetIsSlashing())
        {
            Character.AnimationManager.Slash(twoHanded: false);
            Character.AnimationManager.SetIsSlashing(true);
            SlashServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Install(playerInstallState.GetCurrentInstallType());
        }
    }

    [ServerRpc]
    void SlashServerRpc(ServerRpcParams rpcParams = default)
    {
        PlaySlashClientRpc();
    }

    [ClientRpc]
    void PlaySlashClientRpc()
    {
        if (IsOwner) return;

        Character.AnimationManager.Slash(twoHanded: false);
        // Character.AnimationManager.SetIsSlashing(true);
    }

    void OnAnimationEvent(string eventName)
    {
        if (eventName == "Hit")
        {
            Vector2 hitPos = transform.position + (Vector3)currentDirection.Value * 0.5f;
            Vector2Int hitPosInt = new Vector2Int(Mathf.RoundToInt(hitPos.x), Mathf.RoundToInt(hitPos.y));

            MiningServerRPC(hitPosInt, playerStatus.miningDamage.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MiningServerRPC(Vector2Int pos, float miningDamage)
    {
        MapManager.Instance.Mining(pos, miningDamage);
    }

    private void Install(InstallableType installableType)
    {
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        MapManager.Instance.CanPlaceServerRpc(gridPos, (int)installableType, NetworkManager.Singleton.LocalClientId);
    }
}
