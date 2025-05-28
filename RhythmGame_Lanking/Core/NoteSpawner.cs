using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public NoteManager noteManager;
    float unitTime;
    float timer = 0f;
    int currentTNum = 0;

    Dictionary<int, List<NoteData>> chart;
    // Dictionary<int, List<ChartEntry>> chart;

    public void Init(int bpm, Dictionary<int, List<NoteData>> chart)
    {
        unitTime = 60f / bpm / 4;
        currentTNum = 0;
        this.chart = chart;
        // chart = NoteLoader.notesDictionary;
    }

    void Update()
    {
        if (GameManager.instance.isStart)
        {
            timer += Time.deltaTime;
            if (timer >= unitTime)
            {
                timer -= unitTime;
                // 현재 tNum에 해당하는 노트가 있으면 소환
                if (chart != null && chart.ContainsKey(currentTNum))
                {
                    foreach (NoteData note in chart[currentTNum])
                    {
                        float duration = note.duration / unitTime;
                        if (duration < 1.01f)
                        {
                            // 짧은 노트 소환
                            noteManager.SpawnShortNote(note.line);
                        }
                        else
                        {
                            duration = duration / 10;
                            // 긴 노트 소환: note.duration * unitTime 은 실제 지속 시간(초) 계산
                            // noteManager.SpawnLongNote(note.line, (note.duration - 0.1f) * 10 * unitTime);
                            noteManager.SpawnLongNote(note.line, (duration - 0.1f) * 10 * unitTime);
                        }
                    }
                }
                // 다음 tNum으로 이동 (실제 게임에서는 노트 데이터의 키 범위에 맞게 처리)
                currentTNum++;
            }
        }
    }
}