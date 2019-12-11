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
    public string mSoundGUID;
    public AudioClip mItemCollectSound;

    public override void Init()
    {
        mType = ObjectType.Item;
    }
}
