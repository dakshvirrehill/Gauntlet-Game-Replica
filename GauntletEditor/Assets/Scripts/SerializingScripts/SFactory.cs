using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SFactory
{
    public string Class = "SpawnFactory";
    public int mPoolCount;
    public float mMinSpawnTime;
    public float mMaxSpawnTime;
    public string mEnemyGUID;
    public SFactory(string pEnemyGUID, int pPoolCount, Vector2 pSpawnTime)
    {
        mPoolCount = pPoolCount;
        mEnemyGUID = pEnemyGUID;
        mMinSpawnTime = pSpawnTime.x;
        mMaxSpawnTime = pSpawnTime.y;
    }

    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");
    }

}
