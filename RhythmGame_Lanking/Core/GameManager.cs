using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Firebase.Storage;
using Firebase.Extensions;
using Firebase.Firestore;

public enum NodeType
{
    Short, Long
}

public enum GameState
{
    Ongo, Paused
}

[Serializable]
public class NoteData
{
    public NodeType type;
    public int line;
    public float duration;
    public int score;
    public bool isLongNoteEnd;
}

public class SongInfo
{
    public string id; // 곡 ID
    public string name; // 곡명
    public string path; // 곡 경로 (Firebase Storage URL)
    public int bpm; // BPM
    public int lineCount; // 라인 수
    public float duration; // 곡 길이 (초 단위)

    public SongInfo()
    {
        id = string.Empty;
        name = string.Empty;
        path = string.Empty;
        bpm = 0;
        lineCount = 0;
        duration = 0f;
    }
}

public class SongFileInfo
{
    public SongInfo songInfo;
    public Dictionary<int, List<NoteData>> notes;
    public GameStats gameStats;

    public SongFileInfo()
    {
        songInfo = new SongInfo();
        notes = new Dictionary<int, List<NoteData>>();
        gameStats = new GameStats();
    }
}

public class ABC
{
    public SongFileInfo songFileInfo; // 곡 정보
    public string path; // 곡 파일 경로 (Firebase Storage URL)
    public static string currentSongId; // 현재 곡 ID

    public ABC()
    {
        songFileInfo = new SongFileInfo();
        path = string.Empty;
        currentSongId = string.Empty;
    }
}

public class GameStats
{
    public int maxCombo;
    public float maxPercentage;
}


public class GameManager : MonoBehaviour
{
    private const long MAX_AUDIO_SIZE = 20 * 1024 * 1024;
    public static byte[] songData;

    public static GameManager instance;
    public MapInit mapInit;
    // public NoteLoader noteLoader;
    public NoteManager noteManager;
    public NoteCheck noteCheck;
    public NoteSpawner noteSpawner;
    public UIManager uiManager;
    public ObjectPool objectPool;
    public AudioSource audioSource;

    // public static SongFileInfo songFileInfo;
    public static ABC abc;

    int bpm;
    int lineCount;
    float duration;
    Dictionary<int, List<NoteData>> notes;
    // AudioClip audioClip;
    string songPath;

    float mapWidth = 5.5f;


    int totalScore = 0;
    int currentSore = 0;
    float rate = 0;
    int currentCombo = 0;
    int maxCombo = 0;
    int savedCombo;
    float SavedRate;
    float currentTime;

    public bool isStart;
    public bool isEnd;

    bool isLoaded;

    GameState state;
    public GameStats gameResults;
    private bool isSongPlaying;
    public static string currentSongId;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        uiManager.Init();
        if (abc.songFileInfo != null)
        {
            savedCombo = abc.songFileInfo.gameStats.maxCombo;
            SavedRate = abc.songFileInfo.gameStats.maxPercentage;
            bpm = abc.songFileInfo.songInfo.bpm;
            lineCount = abc.songFileInfo.songInfo.lineCount;
            duration = abc.songFileInfo.songInfo.duration;
            notes = abc.songFileInfo.notes;
            StartCoroutine(LoadAudio());

            float lineWidth = mapWidth / lineCount;
            float noteWidth = lineWidth - 0.1f;

            Vector3[] spawnPoint = new Vector3[lineCount];
            Vector3[] effectPoint = new Vector3[lineCount];

            for (int i = 0; i < lineCount; i++)
            {
                float x = -mapWidth / 2 + lineWidth / 2 + i * lineWidth;
                spawnPoint[i] = new Vector3(x, 5.5f, 0);
                effectPoint[i] = new Vector3(x, -4.5f, 0);
            }

            mapInit.Init(lineCount, lineWidth, spawnPoint);
            noteCheck.Init(lineCount, effectPoint);
            objectPool.Init(noteWidth);
            noteManager.Init(spawnPoint);
            noteSpawner.Init(bpm, notes);
        }
        else
        {
            Debug.LogError("No Data");
        }

    }

    private IEnumerator LoadAudio()
    {
        string relativePath = abc.songFileInfo.songInfo.path; // e.g. "songs/abc123/audio.mp3"
        string localFilePath = Path.Combine(Application.streamingAssetsPath, relativePath);
        string localDir = Path.GetDirectoryName(localFilePath);

        // 1) 로컬 폴더가 없으면 만든다
        if (!Directory.Exists(localDir))
            Directory.CreateDirectory(localDir);

        // 2) 이미 로컬 파일이 있으면 바로 로드
        if (File.Exists(localFilePath))
        {
            yield return StartCoroutine(PlayLocalFile(localFilePath));
            yield break;
        }

        // 3) 로컬에 없으면 Firebase Storage에서 다운로드
        StorageReference songRef = FirebaseManager.Instance
            .GetStorageRef()
            .Child(relativePath);

        var bytesTask = songRef.GetBytesAsync(MAX_AUDIO_SIZE);
        yield return new WaitUntil(() => bytesTask.IsCompleted);

        if (bytesTask.Exception != null)
        {
            Debug.LogError("오디오 다운로드 실패: " + bytesTask.Exception);
            yield break;
        }

        byte[] audioBytes = bytesTask.Result;
        File.WriteAllBytes(localFilePath, audioBytes);

        // 4) 다운로드 후에도 로컬 파일로 재생
        yield return StartCoroutine(PlayLocalFile(localFilePath));
    }

    private IEnumerator PlayLocalFile(string filePath)
    {
        // file:// 스킴을 붙여 UnityWebRequest로 로드
        string fileUrl = "file://" + filePath;
        using (var uwr = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.MPEG))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"오디오 클립 로드 실패: {uwr.error}");
                yield break;
            }

            audioSource.clip = DownloadHandlerAudioClip.GetContent(uwr);
            StartCoroutine(StartGame());
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isStart && !isEnd)
        {
            if (state == GameState.Ongo)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }

        if (isStart && !isEnd)
        {
            currentTime += Time.deltaTime;

            if (currentTime > 2f && !isSongPlaying)
            {
                isSongPlaying = true;
                audioSource.Play();
            }

            if (currentTime > duration + 4.5f)
            {
                EndGame();
            }
        }

        if (isEnd)
        {
            //press any key to exit
            if (Input.anyKey)
            {
                StopGame();
                SceneManager.LoadScene("Main");
            }
        }
    }

    IEnumerator StartGame()
    {
        // uiManager.Init();
        yield return new WaitForSeconds(0.01f);
        isStart = true;
        // yield return new WaitForSeconds(2f);
        // audioSource.Play();
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        if (isSongPlaying)
        {
            audioSource.Pause();
        }
        state = GameState.Paused;
        uiManager.ShowEscPanel();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (isSongPlaying && !isEnd)
        {
            audioSource.Play();
        }
        state = GameState.Ongo;
        uiManager.HideEscPanel();
    }

    public void StopGame()
    {
        state = GameState.Ongo;
        isStart = false;
        isEnd = false;
        uiManager.HideEscPanel();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    private void EndGame()
    {
        isEnd = true;
        uiManager.ShowEndPanel();

        if (rate > SavedRate)
        {
            var db = FirebaseManager.Instance.Firestore;
            var uid = FirebaseManager.Instance.Auth.CurrentUser.UserId;
            var refDoc = db
                .Collection("songs").Document(currentSongId)
                .Collection("rankings")
                .Document(uid);

            var data = new Dictionary<string, object>
            {
                ["nickName"] = FirebaseManager.Instance.Auth.CurrentUser.DisplayName,
                ["rate"] = rate
            };
            refDoc.SetAsync(data, SetOptions.MergeAll);
        }
    }

    public void AddCombo()
    {
        currentCombo++;
        if (maxCombo < currentCombo)
        {
            maxCombo = currentCombo;
        }
        uiManager.ShowComboPanel(currentCombo);
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        uiManager.ShowComboPanel(currentCombo);
    }

    public void UpdateRate(int total, int current)
    {
        totalScore += total;
        currentSore += current;
        Debug.Log("total: " + totalScore + " current: " + currentSore);
        rate = (float)currentSore / totalScore * 100;
        uiManager.ShowRatePanel(rate);
    }
}
