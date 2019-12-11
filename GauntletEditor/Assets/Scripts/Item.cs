using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : GameScriptable
{
    public enum Type
    {
        TempType1,
        TempType2,
        TempType3
    }

    public string mTextureGUID;

    public override void Init()
    {
        mType = ObjectType.Item;
    }
}
