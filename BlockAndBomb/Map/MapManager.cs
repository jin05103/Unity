using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Assets.HeroEditor4D.Common.Scripts.Common;
using System.Collections;
using System.Linq;

public enum BlockType { Dirt, Stone, Gem }
public enum InstallableType { Block_Dirt, Block_Stone, Bomb }

public class MapManager : NetworkBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Map Settings")]
    public int mapWidth = 61;
    public int mapHeight = 61;

    private Dictionary<Vector2Int, Block> blockMap = new();
    private Dictionary<Vector2Int, Bomb> bombMap = new();
    [SerializeField] NetworkedAnnulusMesh safeZoneMesh;
    [SerializeField] GameObject dummyBlockParent;
    [SerializeField] BombPool bombPool;
    [SerializeField] FXPool fxPool;
    // [SerializeField] DropPool dropPool;
    [SerializeField] NetworkDropPool dropPool;
    [SerializeField] float safeZoneMultiplier = 0.5f;
    [SerializeField] float poisonGasDamage = 0.03f;

    public Sprite soilSprite;
    public Sprite stoneSprite;
    public Sprite gemSprite;

    public GameObject dropPrefab;

    public Sprite soilPieceSprite;
    public Sprite stonePieceSprite;
    public Sprite gemPieceSprite;

    public bool isGameStarted = false;
    public bool isGameOver = false;
    private List<Drop> activeDrops = new List<Drop>();

    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, 1),  // Up
        new Vector2Int(0, -1), // Down
        new Vector2Int(-1, 0), // Left
        new Vector2Int(1, 0)   // Right
    };

    private static readonly Vector2Int[] spawnPoints =
    {
        new Vector2Int(0, 0),
        new Vector2Int(60, 0),
        new Vector2Int(0, 60),
        new Vector2Int(60, 60)
    };

    private static readonly Vector2Int[] safeZoneTiles =
    {
        new(0, 0), new(1, 0), new(0, 1), new(1, 1),

        new(59, 0), new(60, 0), new(59, 1), new(60, 1),

        new(0, 59), new(1, 59), new(0, 60), new(1, 60),

        new(59, 59), new(60, 59), new(59, 60), new(60, 60)
    };


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (!IsServer || isGameOver || !isGameStarted) return;

        List<Bomb> toExplode = new();

        foreach (var kvp in bombMap)
        {
            Bomb bomb = kvp.Value;
            if (!bomb.isActive) continue;

            bomb.timer -= Time.deltaTime;
            if (bomb.timer <= 0f)
            {
                toExplode.Add(bomb);

            }
        }

        foreach (Bomb bomb in toExplode)
        {
            // ExplodeBomb(bomb);
            if (bomb.coroutine != null)
            {
                StopCoroutine(bomb.coroutine);
            }
            bomb.coroutine = StartCoroutine(ExplodeBombCoroutine(bomb));
            //StartCoroutine(ExplodeBombCoroutine(bomb));
            RemoveBomb(bomb.gridPos);
        }

        // Update safe zone mesh
        if (safeZoneMesh != null && safeZoneMesh.innerRadius.Value > 0)
        {
            safeZoneMesh.innerRadius.Value -= Time.deltaTime * safeZoneMultiplier;
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        foreach (var player in PlayerSpawner.Instance.GetAllPlayers())
        {
            if (player == null) continue;
            if (Vector2.Distance(player.transform.position, safeZoneMesh.zoneCenter.Value) > safeZoneMesh.innerRadius.Value)
            {
                string playerName = player.GetComponent<PlayerNameTag>().playerName.Value.ToString();
                player.GetComponent<PlayerStatus>().TakeDamage(poisonGasDamage, PlayerSpawner.Instance.GetId(player), playerName);
            }
        }
    }

    public void GameStart()
    {
        isGameStarted = true;
    }

    public void Init()
    {
        foreach (Transform child in dummyBlockParent.transform)
        {
            GameObject dummyBlock = child.gameObject;
            Block block = dummyBlock.GetComponent<Block>();
            Vector2Int pos = block.gridPos;
            RegisterBlock(pos, block);
        }

        InitAllBlocks(GameSession.Instance.seed);
    }

    public void RegisterBlock(Vector2Int pos, Block block)
    {
        blockMap.TryAdd(pos, block);

    }

    public void RegisterBomb(Vector2Int pos, Bomb bomb)
    {
        bombMap.TryAdd(pos, bomb);
    }

    public Block GetBlockAt(Vector2Int pos)
    {
        blockMap.TryGetValue(pos, out var block);
        return block;
    }

    public Bomb GetBombAt(Vector2Int pos)
    {
        bombMap.TryGetValue(pos, out var bomb);
        return bomb;
    }

    public void RemoveBlock(Vector2Int pos)
    {
        if (blockMap.TryGetValue(pos, out var block))
        {
            block.SetActive(false);
        }
    }

    public void RemoveBomb(Vector2Int pos)
    {
        if (bombMap.TryGetValue(pos, out var bomb))
        {
            bombMap[pos].Deactivate(IsServer);
            bombMap.Remove(pos);
        }
    }

    public bool IsInsideMap(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight;
    }

    public void InitAllBlocks(int seed)
    {
        System.Random rng = new(seed);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2Int pos = new(x, y);

                Block block = GetBlockAt(pos);
                if (block == null) continue;

                float roll = (float)rng.NextDouble();
                BlockType type;

                if (roll < 0.6f) type = BlockType.Dirt;
                else if (roll < 0.9f) type = BlockType.Stone;
                else type = BlockType.Gem;

                block.Initialize(type);
            }
        }

        foreach (var pos in safeZoneTiles)
        {
            if (!IsInsideMap(pos)) continue;

            Block block = GetBlockAt(pos);
            if (block != null)
            {
                block.SetActive(false);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    // public void CanPlaceServerRpc(Vector2Int pos, InstallableType installableType, ulong clientId)
    public void CanPlaceServerRpc(Vector2Int pos, int installableType, ulong clientId)
    {
        bool canPlace = CheckCanPlace(pos);
        bool canPlace2 = false;
        PlayerStatus playerStatus = PlayerSpawner.Instance.GetPlayerObject(clientId)
            .GetComponent<PlayerStatus>();
        if (canPlace)
        {
            switch ((InstallableType)installableType)
            {
                case InstallableType.Block_Dirt:
                    // 해당 플레이어의 재화가 충분한지 확인
                    if (playerStatus.dirtCount.Value >= 10)
                    {
                        playerStatus.dirtCount.Value -= 10;
                        canPlace2 = true;
                    }
                    break;
                case InstallableType.Block_Stone:
                    //해당 플레이어의 재화가 충분한지 확인
                    if (playerStatus.stoneCount.Value >= 10)
                    {
                        playerStatus.stoneCount.Value -= 10;
                        canPlace2 = true;
                    }
                    break;
                case InstallableType.Bomb:
                    //해당 플레이어의 남은 설치 가능한 폭탄 개수 확인
                    if (playerStatus.currentBombCount.Value < playerStatus.maxBombCount.Value)
                    {
                        playerStatus.currentBombCount.Value++;
                        canPlace2 = true;
                        Bomb bomb = bombPool.Get();
                        RegisterBomb(pos, bomb);
                    }
                    break;
            }
            if (canPlace2)
            {
                PlaceBlockClientRpc(pos, installableType, clientId);
            }
        }
    }

    public bool CheckCanPlace(Vector2Int pos)
    {
        if (!IsInsideMap(pos)) return false;

        Block block = GetBlockAt(pos);
        // Bomb bomb = GetBombAt(pos);
        bombMap.TryGetValue(pos, out var bomb);

        if (block.gameObject.activeSelf) return false;
        if (bomb != null && bomb.gameObject.activeSelf) return false;

        return true;
    }

    [ClientRpc]
    private void PlaceBlockClientRpc(Vector2Int pos, int installableType, ulong clientId)
    {
        Block block;
        InstallableType installableTypeI = (InstallableType)installableType;
        switch (installableTypeI)
        {
            case InstallableType.Block_Dirt:
                block = GetBlockAt(pos);
                block.Respawn(BlockType.Dirt);
                break;
            case InstallableType.Block_Stone:
                block = GetBlockAt(pos);
                block.Respawn(BlockType.Stone);
                break;
            case InstallableType.Bomb:
                Bomb bomb;
                if (!bombMap.TryGetValue(pos, out bomb))
                {
                    bomb = bombPool.Get();
                    RegisterBomb(pos, bomb);
                }
                if (IsServer)
                {
                    PlayerStatus playerStatus = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId).gameObject
                        .GetComponent<PlayerStatus>();
                    bomb.Spawn(pos, playerStatus.bombDamage.Value, playerStatus.bombRadius.Value, clientId);
                }
                else
                {
                    bomb.Spawn(pos, 0, 0, clientId);
                }
                break;
        }
    }

    public void Mining(Vector2Int pos, float miningDamage)
    {
        if (!IsInsideMap(pos)) return;

        Block block = GetBlockAt(pos);
        bombMap.TryGetValue(pos, out var bomb);

        if (block != null && block.gameObject.activeSelf)
        {
            float remainingHp = block.Mining(miningDamage);
            MiningFXClientRpc(pos);
            if (remainingHp <= 0)
            {
                RemoveBlock(pos);
                RemoveBlockClientRpc(pos);
                DropBlockPieces(block.type, new Vector3(pos.x, pos.y, 0));
            }
            else
            {
                DamageBlockClientRpc(pos, remainingHp);
            }
        }
        else if (bomb != null && bomb.gameObject.activeSelf)
        {
            float remainingHp = bomb.Mining(miningDamage);
            MiningFXClientRpc(pos);
            if (remainingHp <= 0)
            {
                bomb.Deactivate(IsServer);
                RemoveBombClientRpc(pos);
                RemoveBomb(pos);
            }
            else
            {
                DamageBombClientRpc(pos, remainingHp);
            }
        }
    }

    [ClientRpc]
    private void MiningFXClientRpc(Vector2Int pos)
    {
        GameObject fxObject = fxPool.GetMiningFX();
        ParticleSystem effect = fxObject.GetComponent<ParticleSystem>();
        effect.transform.position = new Vector3(pos.x, pos.y + 0.3f, 0);
        fxObject.SetActive(true);
        effect.Play();
        StartCoroutine(ReturnMiningFXToPool(effect, fxObject));

        Block block = GetBlockAt(pos);
        if (block.gameObject.activeSelf)
        {
            block.ColorChange();
        }
    }

    private IEnumerator ReturnMiningFXToPool(ParticleSystem effect, GameObject fxObject)
    {
        yield return new WaitForSeconds(effect.main.duration);
        if (fxObject == null) yield break; // 이미 Destroy됐으면 종료
        if (!fxObject) yield break; // Unity 오브젝트 특수 null 체크
        fxObject.SetActive(false);
        fxPool.ReturnMiningFX(fxObject);
    }

    [ClientRpc]
    private void RemoveBlockClientRpc(Vector2Int pos)
    {
        if (blockMap.TryGetValue(pos, out var block))
        {
            block.gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void DamageBlockClientRpc(Vector2Int pos, float remainingHp)
    {
        if (blockMap.TryGetValue(pos, out var block))
        {
            block.hp = remainingHp;
        }
    }

    [ClientRpc]
    private void RemoveBombClientRpc(Vector2Int pos)
    {
        if (bombMap.TryGetValue(pos, out var bomb))
        {
            // 
            RemoveBomb(pos);
            bombPool.Return(bomb);
        }
    }

    [ClientRpc]
    private void DamageBombClientRpc(Vector2Int pos, float remainingHp)
    {
        if (bombMap.TryGetValue(pos, out var bomb))
        {
            bomb.hp = remainingHp;
        }
    }

    private IEnumerator ExplodeBombCoroutine(Bomb bomb)
    {
        if (bomb == null || !bomb.isActive) yield break;

        bomb.Deactivate(IsServer);
        RemoveBombClientRpc(bomb.gridPos);
        ExplodeBombClientRpc(bomb.gridPos);

        Dictionary<ulong, bool> isGetDamaged = new();
        foreach (var player in PlayerSpawner.Instance.GetAllPlayers())
        {
            if (player == null || !player.gameObject.activeSelf) continue;
            ulong clientId = player.GetComponent<NetworkObject>().OwnerClientId;
            isGetDamaged[clientId] = false;
        }

        ulong[] getDamagedClientIds = CheckPlayerOnExplosion(bomb.gridPos);
        foreach (var clientId in getDamagedClientIds)
        {
            if (!isGetDamaged[clientId])
            {
                GameObject playerObject = PlayerSpawner.Instance.GetPlayerObject(clientId);
                string playerName = playerObject.GetComponent<PlayerNameTag>().playerName.Value.ToString();
                isGetDamaged[clientId] = true;
                PlayerSpawner.Instance.GetPlayerObject(clientId)
                    .GetComponent<PlayerStatus>().TakeDamage(bomb.damage, clientId, playerName);
            }
        }

        Dictionary<Vector2Int, bool> explosionDirections = new();
        foreach (var dir in directions)
        {
            explosionDirections[dir] = true;
        }

        for (int i = 1; i <= bomb.radius; i++)
        {
            yield return new WaitForSeconds(0.1f); // **여기서 딜레이**

            foreach (var dir in directions)
            {
                if (!explosionDirections[dir]) continue;

                Vector2Int checkPos = bomb.gridPos + dir * i;
                if (!IsInsideMap(checkPos))
                {
                    explosionDirections[dir] = false;
                    continue;
                }

                getDamagedClientIds = CheckPlayerOnExplosion(checkPos);
                foreach (var clientId in getDamagedClientIds)
                {
                    if (!isGetDamaged[clientId])
                    {
                        isGetDamaged[clientId] = true;
                        GameObject playerObject = PlayerSpawner.Instance.GetPlayerObject(clientId);
                        string playerName = playerObject.GetComponent<PlayerNameTag>().playerName.Value.ToString();
                        PlayerSpawner.Instance.GetPlayerObject(clientId)
                            .GetComponent<PlayerStatus>().TakeDamage(bomb.damage, clientId, playerName);
                    }
                }

                RemoveDropsAtPosition(checkPos);

                Block block = GetBlockAt(checkPos);
                if (block != null && block.gameObject.activeSelf)
                {
                    float remainingHp = block.Mining(bomb.damage);
                    if (remainingHp <= 0)
                    {
                        RemoveBlock(checkPos);
                        RemoveBlockClientRpc(checkPos);
                        DropBlockPieces(block.type, new Vector3(checkPos.x, checkPos.y, 0));
                    }
                    else
                    {
                        DamageBlockClientRpc(checkPos, remainingHp);
                    }

                    explosionDirections[dir] = false;
                    continue;
                }

                if (bombMap.TryGetValue(checkPos, out var otherBomb) && otherBomb.isActive)
                {
                    otherBomb.timer = 0f; // 즉시 터짐
                    explosionDirections[dir] = false;
                    continue;
                }

                ExplodeBombClientRpc(checkPos);
            }
        }
    }

    [ClientRpc]
    private void ExplodeBombClientRpc(Vector2Int pos)
    {
        GameObject fxObject = fxPool.GetBombFX();
        ParticleSystem effect = fxObject.GetComponent<ParticleSystem>();
        effect.transform.position = new Vector3(pos.x, pos.y, 0);
        fxObject.SetActive(true);
        effect.Play();
        StartCoroutine(ReturnEffectToPool(effect, fxObject));
    }

    private IEnumerator ReturnEffectToPool(ParticleSystem effect, GameObject fxObject)
    {
        yield return new WaitForSeconds(effect.main.duration);
        if (fxObject == null) yield break; // 이미 Destroy됐으면 종료
        if (!fxObject) yield break; // Unity 오브젝트 특수 null 체크
        fxObject.SetActive(false);
        fxPool.ReturnBombFX(fxObject);
    }

    private void RemoveDropsAtPosition(Vector2Int pos)
    {
        Vector2 explosionCenter = new Vector2(pos.x + 0.5f, pos.y + 0.5f);
        List<Drop> toRemove = new List<Drop>();

        foreach (var drop in activeDrops)
        {
            if (drop == null || !drop.gameObject.activeSelf) continue;

            Vector2 dropCenter = drop.transform.position;
            if (new Bounds(new Vector3(pos.x, pos.y, 0), new Vector3(1, 1, 0)).Contains(dropCenter))
            {
                toRemove.Add(drop);
            }
        }

        foreach (var drop in toRemove)
        {
            activeDrops.Remove(drop);
            drop.DespawnDrop();
        }
    }

    private ulong[] CheckPlayerOnExplosion(Vector2Int pos)
    {
        Vector2 explosionCenter = new Vector2(pos.x + 0.5f, pos.y + 0.5f); // 바운더리 중앙
        List<(ulong clientId, float distance)> candidates = new();

        foreach (var player in PlayerSpawner.Instance.GetAllPlayers())
        {
            if (player == null || !player.gameObject.activeSelf) continue;

            var collider = player.GetComponent<Collider2D>();
            if (collider == null) continue;

            if (new Bounds(new Vector3(pos.x, pos.y, 0), new Vector3(1, 1, 0)).Intersects(collider.bounds))
            {
                Vector2 playerCenter = collider.bounds.center;
                float dist = Vector2.Distance(explosionCenter, playerCenter);
                ulong clientId = player.GetComponent<NetworkObject>().OwnerClientId;
                candidates.Add((clientId, dist));
            }
        }

        // 거리 기준 오름차순 정렬 후 clientId만 배열로 반환
        return candidates.OrderBy(c => c.distance).Select(c => c.clientId).ToArray();
    }


    public void DropBlockPieces(BlockType type, Vector3 blockPos)
    {
        for (int i = 0; i < 4; i++)
        {
            float a = Random.Range(0f, 0.3f);
            float b = Random.Range(0f, 360f);
            float x = a * Mathf.Cos(b * Mathf.Deg2Rad);
            float y = a * Mathf.Sin(b * Mathf.Deg2Rad);
            Vector3 dropPos = blockPos + new Vector3(x, y, 0);
            CreateDropServerRpc(type, dropPos, 1);
        }
    }

    [ServerRpc]
    public void CreateDropServerRpc(BlockType type, Vector3 pos, int value)
    {
        var dropObj = dropPool.Instantiate(0, pos, Quaternion.identity);
        dropObj.transform.position = pos;
        dropObj.GetComponent<NetworkObject>().Spawn();
        dropObj.GetComponent<Drop>().Init(type, value);
        activeDrops.Add(dropObj.GetComponent<Drop>());
    }

    public void RemoveDrop(Drop drop)
    {
        if (drop != null && activeDrops.Contains(drop))
        {
            activeDrops.Remove(drop);
        }
    }
}