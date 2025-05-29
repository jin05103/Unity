using UnityEngine;

public class Rest : Interactable
{
    public EnemySpawner enemySpawner;

    public override void Interact(GameObject player)
    {
        base.Interact(player);
        RestPlayer(player);
    }

    private void RestPlayer(GameObject player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.Rest();
        enemySpawner.ResetEnemies(); // 적 리셋
    }
}
