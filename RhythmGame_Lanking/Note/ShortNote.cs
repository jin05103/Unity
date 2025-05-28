using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortNote : Note
{
    public void Init(int line)
    {
        this.line = line;
        isPressed = false;
        isSetup = false;
        checkedNote = false;
    }
}
