using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Newtonsoft.Json;

public enum NodeType
{
    Short, Long
}

public enum GameState
{
    Ongo, Paused
}

public class GameResults
{
    public int maxCombo;
    public float maxPercentage;

    // 게임 종료 후 결과를 저장
    public void UpdateResults(int combo, float percentage)
    {
        maxCombo = Mathf.Max(maxCombo, combo); // 최대 콤보 갱신
        maxPercentage = Mathf.Max(maxPercentage, percentage); // 최대 퍼센티지 갱신
    }
}

public class GameManager : MonoBehaviour
{
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
    public GameResults gameResults;
    private bool isSongPlaying;

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

    IEnumerator LoadAudio()
    {
        // UnityWebRequest를 사용해 mp3 파일 로드 (mp3는 AudioType.MPEG 사용)
        songPath = Path.Combine(Application.streamingAssetsPath, abc.songFileInfo.songInfo.path);
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(songPath, AudioType.MPEG);
        yield return request.SendWebRequest();

        Debug.Log(request.result);

        // 오류 확인
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("오디오 클립 로드 실패: " + request.error);
        }
        else
        {
            // 다운로드한 AudioClip 가져오기
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            audioSource.clip = clip;

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
                SceneManager.LoadScene("Select");
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
        SceneManager.LoadScene("Select");
    }

    private void EndGame()
    {
        isEnd = true;
        uiManager.ShowEndPanel();

        if ((rate > SavedRate) || (rate == SavedRate && maxCombo > savedCombo))
        {
            abc.songFileInfo.gameStats.maxCombo = maxCombo;
            abc.songFileInfo.gameStats.maxPercentage = rate;
            string updatedJson = JsonConvert.SerializeObject(abc.songFileInfo, Formatting.Indented);
            File.WriteAllText(abc.path, updatedJson);
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
