using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

// 스폰 정보를 담을 데이터 클래스 정의
[System.Serializable]
public class SpawnData
{
    public Transform spawnPoint;      // 적을 스폰할 위치
    public GameObject enemyPrefab;    // 스폰할 적 프리팹
    public GameObject enemyObject;    // 해당 위치에 생성된 적 인스턴스
    public GameObject wall;
}

public class EnemySpawner : MonoBehaviour
{
    // 인스펙터에서 설정할 수 있도록 리스트 공개
    public List<SpawnData> spawnList = new List<SpawnData>();
    public PlayerController playerController;



    private void Start()
    {
        ResetEnemies(); // 게임 시작 시 적 리셋
    }

    // 휴식 이벤트 후 적 리셋 예시
    public void ResetEnemies()
    {
        foreach (SpawnData spawnData in spawnList)
        {
            EnemyManager enemyManager;
            if (spawnData.enemyObject != null)
            {
                if (!spawnData.enemyObject.activeSelf && spawnData.enemyObject.GetComponent<EnemyStats>().enemyType == EnemyType.Boss)
                {
                    spawnData.wall.SetActive(false);
                    return;
                }
                else if (spawnData.enemyObject.GetComponent<EnemyStats>().enemyType == EnemyType.Boss)
                {
                    spawnData.enemyObject.GetComponent<BossWall>().wall = spawnData.wall;
                    spawnData.wall.SetActive(true);
                    spawnData.wall.GetComponent<NavMeshObstacle>().enabled = true;
                }
                spawnData.enemyObject.SetActive(true);
                enemyManager = spawnData.enemyObject.GetComponent<EnemyManager>();
                enemyManager.playerController = playerController;
                enemyManager.SpawnEnemy(spawnData.spawnPoint);
            }
            else
            {
                spawnData.enemyObject = Instantiate(spawnData.enemyPrefab);
                if (spawnData.enemyObject.GetComponent<EnemyStats>().enemyType == EnemyType.Boss)
                {
                    spawnData.enemyObject.GetComponent<BossWall>().wall = spawnData.wall;
                    spawnData.wall.SetActive(true);
                    spawnData.wall.GetComponent<NavMeshObstacle>().enabled = true;
                }
                enemyManager = spawnData.enemyObject.GetComponent<EnemyManager>();
                enemyManager.playerController = playerController;
                enemyManager.SpawnEnemy(spawnData.spawnPoint);
            }
        }
    }
}