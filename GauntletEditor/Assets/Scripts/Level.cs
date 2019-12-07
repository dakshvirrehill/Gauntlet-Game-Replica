using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : GameScriptable
{

    public enum LayerTypes
    {
        Environment,
        StartingObjects,
        Enemies,
        Players
    }

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
