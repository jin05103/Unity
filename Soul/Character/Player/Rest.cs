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
        playerController.Rest(transform); // 플레이어 휴식 메소드 호출
        enemySpawner.ResetEnemies(); // 적 리셋
    }
}
