using Assets.HeroEditor4D.Common.Scripts.Common;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : NetworkBehaviour
{
    [SerializeField] PlayerLevelManager playerLevelManager;
    [SerializeField] Animator animator;
    [SerializeField] PlayerStatManager playerStatManager;

    public NetworkVariable<float> miningDamage = new(1f);
    public NetworkVariable<float> miningSpeed = new(1f);
    public NetworkVariable<float> speed = new(3f);
    public NetworkVariable<float> bombDamage = new(3f);
    public NetworkVariable<int> bombRadius = new(1);
    public NetworkVariable<int> maxBombCount = new(1);
    public NetworkVariable<int> currentBombCount = new(0);
    public NetworkVariable<float> maxHp = new(10f);
    public NetworkVariable<float> currentHp = new(10f);

    public NetworkVariable<int> dirtCount = new(0);
    public NetworkVariable<int> stoneCount = new(0);
    public NetworkVariable<int> gemCount = new(0);
    public NetworkVariable<int> dirtMaxCount = new(99);
    public NetworkVariable<int> stoneMaxCount = new(99);

    public NetworkVariable<int> dirtPlaceCount = new(10);
    public NetworkVariable<int> stonePlaceCount = new(10);

    public NetworkVariable<int> level = new(1);
    public NetworkVariable<int> currentExp = new(0);
    public NetworkVariable<int> needExp = new(3);

    public NetworkVariable<int> statPoint = new(0);

    public Image expImage;
    public TMP_Text levelText;

    public bool isDead = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) // 내 PlayerStatus만 UI에 표시
        {
            maxHp.OnValueChanged += OnMaxHpChanged;
            miningSpeed.OnValueChanged += OnMiningSpeedChanged;

            currentBombCount.OnValueChanged += UpdateUI;
            maxBombCount.OnValueChanged += UpdateUI;
            dirtCount.OnValueChanged += UpdateUI;
            stoneCount.OnValueChanged += UpdateUI;

            statPoint.OnValueChanged += OnStatPointChanged;

            UpdateUI(0, 0); // 초기값도 표시
        }
    }

    public override void OnNetworkDespawn()
    {
        maxHp.OnValueChanged -= OnMaxHpChanged;
        miningSpeed.OnValueChanged -= OnMiningSpeedChanged;

        currentBombCount.OnValueChanged -= UpdateUI;
        maxBombCount.OnValueChanged -= UpdateUI;
        dirtCount.OnValueChanged -= UpdateUI;
        stoneCount.OnValueChanged -= UpdateUI;

        statPoint.OnValueChanged -= OnStatPointChanged;

        UnsubscribeExpPanel();
    }

    private void OnMaxHpChanged(float previousValue, float newValue)
    {
        float diff = newValue - previousValue;

        currentHp.Value += diff;
        if (currentHp.Value > newValue)
            currentHp.Value = newValue;
    }

    private void OnMiningSpeedChanged(float previousValue, float newValue)
    {
        animator.SetFloat("AttackSpeed", newValue);
    }

    private void OnStatPointChanged(int previousValue, int newValue)
    {
        playerStatManager.GetStatPanel().GetStatPointText().text = "SP : " + newValue.ToString();
    }

    private void UpdateUI(int a, int b)
    {
        if (UIManager.Instance == null) return;
        UIManager.Instance.UpdateResourceUI(
            currentBombCount.Value,
            maxBombCount.Value,
            dirtCount.Value,
            stoneCount.Value
        );
    }

    public void SetExpPanel(ExpPanel expPanel)
    {
        expImage = expPanel.GetExpImage();
        levelText = expPanel.GetLevelText();
        currentExp.OnValueChanged += OnCurrentExpChanged;
        needExp.OnValueChanged += OnNeedExpChanged;
        level.OnValueChanged += OnLevelChanged;
    }

    public void UnsubscribeExpPanel()
    {
        currentExp.OnValueChanged -= OnCurrentExpChanged;
        needExp.OnValueChanged -= OnNeedExpChanged;
        level.OnValueChanged -= OnLevelChanged;
    }

    private void OnCurrentExpChanged(int oldValue, int newValue)
    {
        UIManager.Instance.UpdateExpImage(newValue, needExp.Value);
        UIManager.Instance.UpdateLevelText(level.Value, newValue, needExp.Value);
    }

    private void OnNeedExpChanged(int oldValue, int newValue)
    {
        UIManager.Instance.UpdateExpImage(currentExp.Value, newValue);
        UIManager.Instance.UpdateLevelText(level.Value, currentExp.Value, newValue);
    }

    private void OnLevelChanged(int oldValue, int newValue)
    {
        UIManager.Instance.UpdateLevelText(newValue, currentExp.Value, needExp.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddExpServerRpc(int value)
    {
        if (!IsServer) return;
        currentExp.Value += value;
        while (currentExp.Value >= needExp.Value)
        {
            currentExp.Value -= needExp.Value;
            needExp.Value++;
            level.Value++;
            playerStatManager.OnLevelUp();
        }
    }

    public void TakeDamage(float damage, ulong clientId, string playerName)
    {
        if (!IsServer || isDead) return;

        currentHp.Value -= damage;
        if (currentHp.Value <= 0)
        {
            currentHp.Value = 0;
            isDead = true;
            PlayerDeadServerRpc(clientId, playerName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddBlockTypePiece_ServerRpc(BlockType blockType, int value)
    {
        switch (blockType)
        {
            case BlockType.Dirt:
                if (dirtCount.Value < dirtMaxCount.Value)
                {
                    dirtCount.Value += value;
                    if (dirtCount.Value > dirtMaxCount.Value)
                    {
                        dirtCount.Value = dirtMaxCount.Value; // 최대치 초과 방지
                    }
                }
                break;
            case BlockType.Stone:
                if (stoneCount.Value < stoneMaxCount.Value)
                {
                    stoneCount.Value += value;
                    if (stoneCount.Value > stoneMaxCount.Value)
                    {
                        stoneCount.Value = stoneMaxCount.Value; // 최대치 초과 방지
                    }
                }
                break;
            case BlockType.Gem:
                gemCount.Value += value;
                // currentExp.Value += value;
                AddExpServerRpc(value);
                break;
        }

        UpdateUI(0, 0);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerDeadServerRpc(ulong playerId, string playerName)
    {
        if (!IsServer) return;

        GameManager.Instance.PlayerDeadEventServerRpc(playerId, playerName);
        var playerObj = PlayerSpawner.Instance.GetPlayerObject(playerId);
        var playerStatus = playerObj.GetComponent<PlayerStatus>();
        if (playerObj != null)
        {
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.isAlive = false;
                PlayerDeadClientRpc(playerId);
                // playerController.Character.AnimationManager.SetState(CharacterState.Death);
            }
        }
        if (playerStatus != null)
        {
            int dirtValue = playerStatus.dirtCount.Value;
            int stoneValue = playerStatus.stoneCount.Value;
            int gemValue = playerStatus.gemCount.Value / 3;

            while (dirtValue > 0)
            {
                if (dirtValue > 10)
                {
                    dirtValue -= 10;
                    MapManager.Instance.CreateDropServerRpc(
                        BlockType.Dirt,
                        playerObj.transform.position,
                        10
                    );
                }
                else
                {
                    MapManager.Instance.CreateDropServerRpc(
                        BlockType.Dirt,
                        playerObj.transform.position,
                        dirtValue
                    );
                    dirtValue = 0;
                }
            }

            while (stoneValue > 0)
            {
                if (stoneValue > 10)
                {
                    stoneValue -= 10;
                    MapManager.Instance.CreateDropServerRpc(
                        BlockType.Stone,
                        playerObj.transform.position,
                        10
                    );
                }
                else
                {
                    MapManager.Instance.CreateDropServerRpc(
                        BlockType.Stone,
                        playerObj.transform.position,
                        stoneValue
                    );
                    stoneValue = 0;
                }
            }

            while (gemValue > 0)
            {
                if (gemValue > 4)
                {
                    gemValue -= 4;
                    MapManager.Instance.CreateDropServerRpc(
                        BlockType.Gem,
                        playerObj.transform.position,
                        4
                    );
                }
                else
                {
                    MapManager.Instance.CreateDropServerRpc(
                        BlockType.Gem,
                        playerObj.transform.position,
                        gemValue
                    );
                    gemValue = 0;
                }
            }
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void PlayerDeadClientRpc(ulong playerId)
    {
        if (IsOwner)
        {
            var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerId)?.gameObject;


            if (playerObj != null)
            {
                PlayerController playerController = playerObj.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    PlayerController.AlivePlayers.Remove(playerController);
                    playerController.isAlive = false;
                    playerController.Character.AnimationManager.SetState(CharacterState.Death);

                    StartCoroutine(HideDeadPlayer(playerController));
                }

                CameraFollow cameraFollow = playerObj.GetComponent<CameraFollow>();
                if (cameraFollow != null)
                {
                    cameraFollow.isDead = true;
                }
            }
        }
    }

    IEnumerator HideDeadPlayer(PlayerController playerController)
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < playerController.transform.childCount; i++)
        {
            playerController.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}