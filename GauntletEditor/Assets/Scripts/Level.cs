using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : GameScriptable
{
    public string mName;
    public int mRows;
    public int mColumns;
    public float mTime;
    public Level()
    {
        mName = "Untitled Level";
        mColumns = 32;
        mRows = 32;
        mTime = 60;
    }
}
