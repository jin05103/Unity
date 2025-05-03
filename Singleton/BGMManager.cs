using UnityEngine;

/// <summary>
/// 프리팹 내부에 존재하는 실제 싱글톤
/// - Instance 제공
/// - 중복 제거 처리 포함
/// </summary>
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void PlayBGM()
    {
        Debug.Log("Playing BGM...");
    }
}