using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class NoteCheck : MonoBehaviour
{
    [SerializeField] ParticleSystem particlePrefab;
    [SerializeField] ParticleSystem[] pushEffect;
    ParticleSystem.MainModule[] mainModule;
    public float judgeLineY = -4.5f;
    public float perfectRange = 0.03f;
    public float greatRange = 0.7f;
    public float goodRange = 0.15f;
    // public float badRange = 0.4f;

    public float longNoteTickTime = 0.2f;
    // public int longNoteTickScore = 1;

    public bool[] canPress;
    public bool[] canHold;
    public ShortNote[] shortNotes;
    public LongNote[] longNotes;

    float[] tickTimers;

    int lineCount;
    KeyCode[] keyCodes;

    public void Init(int _lineCount, Vector3[] points)
    {
        pushEffect = new ParticleSystem[_lineCount];
        mainModule = new ParticleSystem.MainModule[_lineCount];
        lineCount = _lineCount;
        keyCodes = new KeyCode[_lineCount];
        canPress = new bool[_lineCount];
        canHold = new bool[_lineCount];
        shortNotes = new ShortNote[_lineCount];
        longNotes = new LongNote[_lineCount];
        // tickCount = new float[lineCount];
        tickTimers = new float[_lineCount];
        for (int i = 0; i < _lineCount; i++)
        {
            pushEffect[i] = Instantiate(particlePrefab, points[i], Quaternion.identity);
            mainModule[i] = pushEffect[i].main;
            tickTimers[i] = 0f;
            keyCodes[i] = KeyCode.Alpha1 + i;
            canPress[i] = false;
            canHold[i] = false;
            shortNotes[i] = null;
            longNotes[i] = null;
        }

    }

    void Update()
    {
        for (int i = 0; i < lineCount; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                // pushEffect[i].Play();
                if (canPress[i])
                {
                    if (shortNotes[i] != null && !shortNotes[i].isPressed)
                    {
                        pushEffect[i].Play();
                        DisposeShort(i);
                    }
                    else if (longNotes[i] != null && !longNotes[i].isPressed)
                    {
                        pushEffect[i].Play();
                        DisposeLong(i);
                    }
                }
            }
        }

        for (int i = 0; i < lineCount; i++)
        {
            if (longNotes[i] == null)
            {
                continue;
            }

            if (longNotes[i].startTick)
            {
                tickTimers[i] += Time.deltaTime;
            }

            if (Input.GetKeyUp(keyCodes[i]))
            {
                canHold[i] = false;
            }

            if (tickTimers[i] >= longNoteTickTime)
            {
                if (canHold[i])
                {
                    if (Input.GetKey(keyCodes[i]))
                    {
                        mainModule[i].startSize = 0.6f;
                        pushEffect[i].Play();
                        GameManager.instance.AddCombo();
                        GameManager.instance.UpdateRate(1, 1);
                        GameManager.instance.uiManager.ShowVerdictPanel("Perfect");
                        mainModule[i].startSize = 1.0f;
                        pushEffect[i].Play();
                    }
                }
                else
                {
                    GameManager.instance.UpdateRate(1, 0);
                }

                tickTimers[i] -= longNoteTickTime;
            }
        }
    }

    public bool IsNull(int line)
    {
        if (shortNotes[line] != null)
        {
            return false;
        }
        if (longNotes[line] != null)
        {
            return false;
        }
        return true;
    }

    public void SetShortNote(ShortNote note)
    {
        canPress[note.line] = true;
        shortNotes[note.line] = note;
    }

    public void ResetShortNote(ShortNote note)
    {
        if (shortNotes[note.line] == note)
        {
            canPress[note.line] = false;
            shortNotes[note.line] = null;
        }
    }

    public void SetLongNote(LongNote note)
    {
        canHold[note.line] = false;
        canPress[note.line] = true;
        longNotes[note.line] = note;
        tickTimers[note.line] = 0f;
    }

    public void ResetLongNote(Note note)
    {
        if (longNotes[note.line] == note)
        {
            canHold[note.line] = false;
            canPress[note.line] = false;
            longNotes[note.line] = null;
            tickTimers[note.line] = 0f;
        }
    }

    public void ResetNote(int line)
    {
        canPress[line] = false;
        shortNotes[line] = null;
        canHold[line] = false;
        longNotes[line] = null;
        tickTimers[line] = 0f;
    }

    void DisposeShort(int line)
    {
        ShortNote note = shortNotes[line];
        float y = note.transform.position.y;
        float dif = Mathf.Abs(judgeLineY - y);
        if (dif < perfectRange)
        {
            GameManager.instance.AddCombo();
            GameManager.instance.UpdateRate(1, 1);
            GameManager.instance.uiManager.ShowVerdictPanel("Perfect");
            mainModule[line].startSize = 1.1f;
            pushEffect[line].Play();
        }
        else if (dif < greatRange)
        {
            GameManager.instance.AddCombo();
            GameManager.instance.UpdateRate(1, 1);
            GameManager.instance.uiManager.ShowVerdictPanel("Great");
            mainModule[line].startSize = 0.8f;
            pushEffect[line].Play();
        }
        else if (dif < goodRange)
        {
            GameManager.instance.AddCombo();
            GameManager.instance.UpdateRate(1, 1);
            GameManager.instance.uiManager.ShowVerdictPanel("Good");
            mainModule[line].startSize = 0.5f;
            pushEffect[line].Play();
        }
        else
        {
            GameManager.instance.ResetCombo();
            GameManager.instance.UpdateRate(1, 0);
            GameManager.instance.uiManager.ShowVerdictPanel("Bad");
            mainModule[line].startSize = 0.3f;
            pushEffect[line].Play();
        }

        note.checkedNote = true;
        note.isPressed = true;
        ResetShortNote(note);
    }

    void DisposeLong(int line)
    {
        LongNote note = longNotes[line];
        float y = note.transform.position.y;
        float dif = Mathf.Abs(judgeLineY - y);
        if (dif < perfectRange)
        {
            GameManager.instance.AddCombo();
            GameManager.instance.UpdateRate(1, 1);
            GameManager.instance.uiManager.ShowVerdictPanel("Great");
            mainModule[line].startSize = 1.1f;
            pushEffect[line].Play();
        }
        else if (dif < greatRange)
        {
            GameManager.instance.AddCombo();
            GameManager.instance.UpdateRate(1, 1);
            GameManager.instance.uiManager.ShowVerdictPanel("Great");
            mainModule[line].startSize = 0.8f;
            pushEffect[line].Play();
        }
        else if (dif < goodRange)
        {
            GameManager.instance.AddCombo();
            GameManager.instance.UpdateRate(1, 1);
            GameManager.instance.uiManager.ShowVerdictPanel("Good");
            mainModule[line].startSize = 0.5f;
            pushEffect[line].Play();
        }
        else
        {
            GameManager.instance.ResetCombo();
            GameManager.instance.UpdateRate(1, 0);
            GameManager.instance.uiManager.ShowVerdictPanel("Bad");
            mainModule[line].startSize = 0.3f;
            pushEffect[line].Play();
        }

        note.checkedNote = true;
        note.isPressed = true;
        canHold[line] = true;

    }
}