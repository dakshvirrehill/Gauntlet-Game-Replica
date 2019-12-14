using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct Box
{
    public float height;
    public float width;
}

[System.Serializable]
public class GPolygonCollider
{
    public string Class = "PolygonCollider";
    public bool trigger;
    public Box box;
    public GPolygonCollider(bool pTrigger)
    {
        trigger = pTrigger;
        box = new Box
        {
            width = 64.0f,
            height = 64.0f
        };
    }

    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");
    }

}
