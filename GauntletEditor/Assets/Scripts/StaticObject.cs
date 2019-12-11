using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObject : GameScriptable
{
    [HideInInspector]
    public string mTextureGUID;

    public override void Init()
    {
        mType = ObjectType.StaticObject;
    }
}
