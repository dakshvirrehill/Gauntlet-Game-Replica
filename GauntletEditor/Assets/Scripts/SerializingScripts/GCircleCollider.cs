using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GCircleCollider
{
    public string Class;
    public float radius;
    public bool trigger;

    public GCircleCollider(float pRadius, bool pTrigger)
    {
        Class = "CircleCollider";
        radius = pRadius;
        trigger = pTrigger;
    }

    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");
    }
}
