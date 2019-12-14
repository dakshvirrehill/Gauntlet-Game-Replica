using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[System.Serializable]
public struct position
{
    public float X;
    public float Y;
    public position(float x, float y)
    {
        X = x;
        Y = y;
    }
}
[System.Serializable]
public class GTransform
{
    public string Class;
    public position Position;

    public GTransform()
    {
        Class = "Transform";
    }

    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");
    }
}
