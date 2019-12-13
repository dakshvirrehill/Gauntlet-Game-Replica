using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOrder : ScriptableObject
{
    [HideInInspector]
    public List<Level> mAllLevels;

    public void Init() { mAllLevels = new List<Level>(); }

}
