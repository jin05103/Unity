using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongNote : Note
{
    public bool isReseted = false;
    public bool startTick = false;
    // public bool initialTickDone = false;

    public void Init(int line)
    {
        this.line = line;
        isPressed = false;
        isSetup = false;
        checkedNote = false;
        this.isReseted = false;
        this.startTick = false;
    }
}
