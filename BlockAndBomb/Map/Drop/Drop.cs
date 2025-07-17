using Unity.Netcode;
using UnityEngine;

public class Drop : NetworkBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    private BlockType dropType;
    private int value;

    public void Init(BlockType type, int value)
    {
        if (!IsServer) return;

        InitClientRpc(type, value, transform.position.y);
    }

    [ClientRpc]
    private void InitClientRpc(BlockType type, int value, float y)
    {
        this.value = value;
        dropType = type;
        SetSprite(type, y);
    }

    private void SetSprite(BlockType type, float y = 0)
    {
        spriteRenderer.sortingOrder = -Mathf.RoundToInt(y == 0 ? transform.position.y : y) * 2;
        switch (type)
        {
            case BlockType.Dirt:
                spriteRenderer.sprite = MapManager.Instance.soilPieceSprite;
                break;
            case BlockType.Stone:
                spriteRenderer.sprite = MapManager.Instance.stonePieceSprite;
                break;
            case BlockType.Gem:
                spriteRenderer.sprite = MapManager.Instance.gemPieceSprite;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.gameObject.tag != "Player") return;

        var playerStatus = other.GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            if (playerStatus.isDead) return;

            playerStatus.AddBlockTypePiece_ServerRpc(dropType, value);
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
            }
        }
    }
}