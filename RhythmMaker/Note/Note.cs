using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType
{
    Short,
    Long
}

public class Note : MonoBehaviour
{
    public int line;
    public float duration;
    public NoteType type;
    public bool isPressed = false;
    public bool isSetup = false;
    public bool checkedNote = false;
}
