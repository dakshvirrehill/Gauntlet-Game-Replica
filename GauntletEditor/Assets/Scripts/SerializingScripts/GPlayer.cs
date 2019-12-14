using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GPlayer
{
    public string Class = "Player";
    public string mProjectileGUID;
    public float mSpeed;
    public float mHealth;

    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");
    }
}
