using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Teleporter
{
    public string Class = "Teleporter";

    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");
    }

}
