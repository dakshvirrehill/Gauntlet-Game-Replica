using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GProjectile
{
    public string Class = "Projectile";
    public float mSpeed;
    public int mPoolCount;

    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");
    }
}
