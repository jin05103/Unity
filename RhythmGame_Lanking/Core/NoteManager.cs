using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    [SerializeField] ObjectPool notePool;
    [SerializeField] NoteCheck noteCheck;
    [SerializeField] Vector3[] spawnPoints;

    public List<ShortNote> activeShortNotes = new List<ShortNote>();
    public List<LongNote> activeLongNotes = new List<LongNote>();
    public float judgeLineStartY = -4.2f;
    public float judgeLineEndY = -4.8f;
    public float deleteLineY = -5.5f;

    // [SerializeField] float baseSpeed = 3;
    [SerializeField] float speed = 5f;

    public void Init(Vector3[] _spawnPoints)
    {
        spawnPoints = _spawnPoints;
    }

    void Update()
    {
        for (int i = activeShortNotes.Count - 1; i >= 0; i--)
        {
            ShortNote note = activeShortNotes[i];
            note.transform.position -= new Vector3(0, speed * Time.deltaTime, 0);

            if (!note.isSetup && note.transform.position.y < judgeLineStartY)
            {
                if (!noteCheck.IsNull(note.line))
                {
                    noteCheck.ResetNote(note.line);
                }

                noteCheck.SetShortNote(note);
                note.isSetup = true;
            }

            if (!note.checkedNote && note.transform.position.y < judgeLineEndY)
            {
                GameManager.instance.ResetCombo();
                GameManager.instance.UpdateRate(1, 0);
                GameManager.instance.uiManager.ShowVerdictPanel("Miss");
                note.checkedNote = true;
            }

            if (note.transform.position.y < deleteLineY)
            {
                noteCheck.ResetShortNote(note);
                DespawnNote(note, NoteType.Short);
                activeShortNotes.RemoveAt(i);
            }
        }

        for (int i = activeLongNotes.Count - 1; i >= 0; i--)
        {
            LongNote note = activeLongNotes[i];
            note.transform.position -= new Vector3(0, speed * Time.deltaTime, 0);

            if (!note.isSetup && note.transform.position.y < judgeLineStartY)
            {
                if (!noteCheck.IsNull(note.line))
                {
                    noteCheck.ResetNote(note.line);
                }

                noteCheck.SetLongNote(note);
                note.isSetup = true;
            }

            if (note.transform.position.y < -4.6f)
            {
                note.startTick = true;
            }

            if (!note.checkedNote && note.transform.position.y < judgeLineEndY)
            {
                GameManager.instance.ResetCombo();
                GameManager.instance.UpdateRate(1, 0);
                GameManager.instance.uiManager.ShowVerdictPanel("Miss");

                noteCheck.canPress[note.line] = false;

                note.checkedNote = true;
            }

            if (!note.isReseted && note.transform.GetChild(2).position.y < -4.5f)
            {
                noteCheck.ResetLongNote(note);
                note.isReseted = true;
            }

            if (note.transform.GetChild(2).position.y < deleteLineY)
            {
                DespawnNote(note, NoteType.Long);
                activeLongNotes.RemoveAt(i);
            }
        }
    }

    public void SpawnShortNote(int line)
    {
        GameObject obj = notePool.GetObject(NoteType.Short);
        ShortNote note = obj.GetComponent<ShortNote>();
        note.Init(line);
        note.transform.position = spawnPoints[line];

        activeShortNotes.Add(note);
    }

    public void SpawnLongNote(int line, float _length)
    {
        float length = speed * _length;

        GameObject obj = notePool.GetObject(NoteType.Long);
        LongNote note = obj.GetComponent<LongNote>();
        note.Init(line);
        // note.transform.GetChild(1).localScale = new Vector3(1, length - 0.2f, 1);
        // note.transform.GetChild(1).localPosition = new Vector3(0, length / 2 - 0.05f, 0);
        // note.transform.GetChild(2).localPosition = new Vector3(0, length - 0.1f, 0);
        note.transform.GetChild(1).localScale = new Vector3(1, length + 0.1f, 1);
        note.transform.GetChild(1).localPosition = new Vector3(0, length / 2, 0);
        note.transform.GetChild(2).localPosition = new Vector3(0, length, 0);
        note.transform.position = spawnPoints[line];

        activeLongNotes.Add(note);
    }

    public void DespawnNote(Note note, NoteType type)
    {
        notePool.ReturnObject(note.gameObject, type);
    }
}
