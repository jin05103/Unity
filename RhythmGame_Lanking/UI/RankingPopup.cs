using System.Threading.Tasks;
using System.Collections;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RankingPopup : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] Button closeButton;
    [SerializeField] Button playButton;
    [SerializeField] Transform listContent;
    [SerializeField] GameObject rankingItemPrefab;
    [SerializeField] TMP_Text myRankingText;
    [SerializeField] TMP_Text myRateText;
    [SerializeField] TMP_Text myNickNameText;

    FirebaseFirestore db;

    string songUrl;
    string chartUrl;
    string currentUserId;
    string currentSongId;
    float mySavedRate;

    private const long MAX_JSON_SIZE = 10 * 1024 * 1024;

    private void Awake()
    {
        db = FirebaseManager.Instance.Firestore;
        // closeButton.onClick.AddListener(() => this.gameObject.SetActive(false));
    }

    public async void Show(ChartItem chartItem, string userId)
    {
        titleText.text = chartItem.songName;
        currentSongId = chartItem.songId;
        currentUserId = userId;
        songUrl = chartItem.songUrl;
        chartUrl = chartItem.chartUrl;
        ClearList();
        myRankingText.text = "-";
        myRateText.text = "-";
        myNickNameText.text = "-";

        await LoadTop10Rankings();

        await LoadMyRanking();

        playButton.interactable = true;
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() =>
        {
            playButton.interactable = false;
            StartCoroutine(LoadSongAndStartGame(chartItem));
        });
    }

    private async Task LoadTop10Rankings()
    {
        var snap = await db
            .Collection("songs").Document(currentSongId)
            .Collection("rankings")
            .OrderByDescending("rate")
            .Limit(10)
            .GetSnapshotAsync();

        int rank = 1;
        foreach (var doc in snap.Documents)
        {
            var go   = Instantiate(rankingItemPrefab, listContent);
            var item = go.GetComponent<RankingItem>();

            string nick = doc.GetValue<string>("nickName");
            float rate   = doc.GetValue<float>("rate");
            bool isMe   = doc.Id == currentUserId;

            item.SetData(rank, nick, rate, isMe);
            rank++;
        }
    }

     private async Task LoadMyRanking()
    {
        var myDoc = await db
            .Collection("songs").Document(currentSongId)
            .Collection("rankings")
            .Document(currentUserId)
            .GetSnapshotAsync();

        if (!myDoc.Exists)
        {
            myNickNameText.text = FirebaseManager.Instance.Auth.CurrentUser.DisplayName;
            return;
        }

        mySavedRate      = myDoc.GetValue<float>("rate");
        myRateText.text  = mySavedRate.ToString("F2");
        myNickNameText.text = myDoc.GetValue<string>("nickName");

        // 내 rate 보다 높게 기록된 사용자 수 + 1 = 내 등수
        var higherSnap = await db
            .Collection("songs").Document(currentSongId)
            .Collection("rankings")
            .WhereGreaterThan("rate", mySavedRate)
            .GetSnapshotAsync();

        myRankingText.text = (higherSnap.Count + 1).ToString();
    }

    private IEnumerator LoadSongAndStartGame(ChartItem chartItem)
    {
        int bpm = chartItem.bpm;
        int lineCount = chartItem.lineCount;
        float duration = chartItem.duration;
        string chartGs = chartItem.chartUrl;
        string songGs = chartItem.songUrl;

        var chartRef = FirebaseManager.Instance.GetStorageRef();
        chartRef = chartRef.Child(chartGs);
        var jsonTask = chartRef.GetBytesAsync(MAX_JSON_SIZE);
        yield return new WaitUntil(() => jsonTask.IsCompleted);

        if (jsonTask.Exception != null)
        {
            Debug.LogError("차트 JSON 다운로드 실패: " + jsonTask.Exception);
            yield break;
        }

        byte[] jsonBytes = jsonTask.Result;
        string jsonText = System.Text.Encoding.UTF8.GetString(jsonBytes);

        Dictionary<int, List<NoteData>> notes =
            JsonConvert.DeserializeObject<Dictionary<int, List<NoteData>>>(jsonText);
        GameStats gameStats = new GameStats
        {
            maxPercentage = mySavedRate,
        };
        SongFileInfo songFileInfo = new SongFileInfo
        {
            songInfo = new SongInfo
            {
                bpm = bpm,
                lineCount = lineCount,
                duration = duration,
                path = songGs
            },
            gameStats = gameStats,
            notes = notes
        };

        ABC abc = new ABC
        {
            songFileInfo = songFileInfo,
            path = songGs
        };
        GameManager.abc = abc;
        GameManager.currentSongId = currentSongId;

        SceneManager.LoadScene("Game 2");
    }

    public void ClearList()
    {
        for (int i = listContent.childCount - 1; i >= 0; i--)
        {
            Destroy(listContent.GetChild(i).gameObject);
        }
    }
}