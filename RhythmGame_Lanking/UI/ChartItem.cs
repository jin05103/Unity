using Firebase.Firestore;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChartItem : MonoBehaviour
{
    [SerializeField] TMP_Text songNameText;    // 곡명 표시 텍스트
    [SerializeField] Button btn;
    public string songUrl { get; private set; }
    public string chartUrl { get; private set; }
    public string songId { get; private set; }
    public string songName { get; private set; }
    public int bpm { get; private set; }
    public int lineCount { get; private set; }
    public float duration { get; private set; }
    public Button Button => btn;

    public void SetData(DocumentSnapshot data)
    {
        songId = data.Id;
        songName = data.GetValue<string>("songName");
        songNameText.text = songName;
        songUrl = data.GetValue<string>("songUrl");
        chartUrl = data.GetValue<string>("chartUrl");
        bpm = data.GetValue<int>("bpm");
        lineCount = data.GetValue<int>("lineCount");
        duration = data.GetValue<float>("duration");

    }
}
