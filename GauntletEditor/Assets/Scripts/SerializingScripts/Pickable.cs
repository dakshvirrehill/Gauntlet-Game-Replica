using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Pickable
{
    public string Class = "Pickable";
    public Item.Type mType;


    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");
    }

}
