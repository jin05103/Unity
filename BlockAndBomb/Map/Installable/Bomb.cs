using Unity.Netcode;
using UnityEngine;

public class Bomb : Installable
{
    public float damage = 10;
    public int radius = 1;
    ulong ownerClientId;
    public float timer;
    public bool isActive;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Initialize(Vector2Int gridPos)
    {
        this.gridPos = gridPos;
        hp = 2f;
        transform.position = new Vector3(gridPos.x, gridPos.y, 0);
        spriteRenderer.sortingOrder = -2 * gridPos.y;
        gameObject.SetActive(true);
    }

    public void Spawn(Vector2Int gridPos, float bombDamage, int bombRadius, ulong ownerClientId)
    {
        damage = bombDamage;
        radius = bombRadius;
        this.ownerClientId = ownerClientId;
        isActive = true;
        timer = 2.5f;

        if (IsLocalPlayerOnBlock(gridPos, new Vector2(1, 1)))
        {
            boxCollider.enabled = false;
        }
        else
        {
            boxCollider.enabled = true;
        }

        Initialize(gridPos);
    }

    private bool IsLocalPlayerOnBlock(Vector2Int pos, Vector2 blockSize)
    {
        Vector3 blockWorldPosition = new Vector3(pos.x, pos.y, 0);

        ulong clientId = NetworkManager.Singleton.LocalClientId;
        var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()?.gameObject;
        if (localPlayer == null) return false;

        var playerCollider = localPlayer.GetComponent<Collider2D>();
        if (playerCollider == null) return false;

        Bounds blockBounds = new Bounds(blockWorldPosition, blockSize);
        return playerCollider.bounds.Intersects(blockBounds);
    }

    public float Mining(float damage)
    {
        hp -= damage;
        return hp;
    }

    public void Deactivate()
    {
        this.isActive = false;
        gameObject.SetActive(false);
        PlayerSpawner.Instance.GetPlayerObject(ownerClientId)
            .GetComponent<PlayerStatus>().currentBombCount.Value--;
    }
}
