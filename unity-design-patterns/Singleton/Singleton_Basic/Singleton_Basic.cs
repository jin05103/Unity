using UnityEngine;

/// <summary>
/// 가장 기본적인 전역 싱글톤 패턴
/// - Instance 제공
/// - DontDestroyOnLoad로 씬 유지
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
