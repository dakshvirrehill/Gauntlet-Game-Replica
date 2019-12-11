using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : GameScriptable
{
    public enum Type
    {
        HealthBoost,
        ScoreMultiplier,
        Invincibility
    }

    public string mTextureGUID;
    public Type mItemType;


    public override void Init()
    {
        mType = ObjectType.Item;
    }
}
