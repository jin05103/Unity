using UnityEngine;

/// <summary>
/// BGMManager 프리팹을 한 번만 생성하는 스포너
/// - 중복 생성 방지
/// - 실제 싱글톤은 프리팹 내부에 존재
/// </summary>
public class BGMManagerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bgmManagerPrefab;

    private static bool hasSpawned = false;

    private void Awake()
    {
        if (hasSpawned) return;

        GameObject bgm = Instantiate(bgmManagerPrefab);
        DontDestroyOnLoad(bgm);
        hasSpawned = true;
    }
}
