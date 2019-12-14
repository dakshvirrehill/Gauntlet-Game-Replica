using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GEnemy
{
    public string Class = "Enemy";
    public string mProjectileGUID = "NoID";
    public float mSpeed;
    public Enemy.Type mType;
    public float mStopRange;

    public override string ToString()
    {
        string aJson = JsonUtility.ToJson(this);
        return aJson.Replace("Class", "class");
    }

}
