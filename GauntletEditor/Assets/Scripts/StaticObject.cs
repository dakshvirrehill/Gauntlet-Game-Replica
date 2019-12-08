using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObject : GameScriptable
{
    [HideInInspector]
    public string mTextureGUID;
    [HideInInspector]
    public int mSpriteIndex;
    [HideInInspector]
    public Rect mDimensions;

    public override void Init()
    {
        mType = ObjectType.StaticObject;
    }
}
