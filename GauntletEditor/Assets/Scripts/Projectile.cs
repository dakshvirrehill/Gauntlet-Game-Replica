﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : GameScriptable
{
    [HideInInspector]
    public float mSpeed;
    [HideInInspector]
    public int mPoolCount;
    [HideInInspector]
    [SerializeField]
    public List<AnimationData> mProjectileAnimation;

    public override void Init()
    {
        mType = ObjectType.Projectile;
    }
}
