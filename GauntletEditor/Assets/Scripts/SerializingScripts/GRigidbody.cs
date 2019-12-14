using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GRigidbody
{
    public string Class = "RigidBody";
    public int BodyType = 1;

    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");
    }
}
