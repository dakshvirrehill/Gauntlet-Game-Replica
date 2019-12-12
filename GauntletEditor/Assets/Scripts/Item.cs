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
    [HideInInspector]
    public string mTextureGUID;
    [HideInInspector]
    public Type mItemType;
    [HideInInspector]
    public string mSoundGUID;
    [HideInInspector]
    public AudioClip mItemCollectSound;

    public override void Init()
    {
        mType = ObjectType.Item;
    }
}
