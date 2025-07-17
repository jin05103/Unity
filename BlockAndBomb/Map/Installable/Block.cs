using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Block : Installable
{
    public BlockType type;
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;

    public void Initialize(BlockType newType)
    {
        type = newType;
        gameObject.SetActive(true);
        // boxCollider.enabled = true;

        switch (type)
        {
            case BlockType.Dirt:
                hp = 2;
                spriteRenderer.sprite = MapManager.Instance.soilSprite;
                break;
            case BlockType.Stone:
                hp = 4;
                spriteRenderer.sprite = MapManager.Instance.stoneSprite;
                break;
            case BlockType.Gem:
                hp = 6;
                spriteRenderer.sprite = MapManager.Instance.gemSprite;
                break;
        }
    }

    public void Respawn(BlockType newType)
    {
        if (IsLocalPlayerOnBlock(gridPos, new Vector2(1, 1)))
        {
            boxCollider.enabled = false;
            Debug.Log($"Block at {gridPos} is occupied by local player, disabling collider.");
        }
        else
        {
            boxCollider.enabled = true;
            Debug.Log($"Block at {gridPos} is not occupied by local player, enabling collider.");
        }
        

        Initialize(newType);
    }

    private bool IsLocalPlayerOnBlock(Vector2Int pos, Vector2 blockSize)
    {
        Debug.Log($"Checking if local player is on block at {pos} with size {blockSize}");
        Vector3 blockWorldPosition = new Vector3(pos.x, pos.y, 0);

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
}
