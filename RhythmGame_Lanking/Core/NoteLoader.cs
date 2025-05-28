using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteLoader : MonoBehaviour
{
    public MapInit mapInit;
    public NoteSpawner noteSpawner;
    public static Dictionary<int, List<NoteData>> notesDictionary;
    public static int lineCount;
    public static int BPM;
    public static float time;
    public static int combo;
    public static float percentage;
    public static string path;

    private void Start()
    {
        // mapInit.InitMap(lineCount);
        // noteSpawner.Init(BPM);
        // GameManager.instance.Init(time, combo, percentage, path);
    }

    public void Reset()
    {
        notesDictionary = new Dictionary<int, List<NoteData>>();
        lineCount = 0;
        BPM = 0;
        time = 0;
        combo = 0;
        percentage = 0;
        path = "";
    }
}