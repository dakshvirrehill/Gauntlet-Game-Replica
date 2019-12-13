using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : GameScriptable
{
    public override void Init()
    {
        mType = ObjectType.Player;
        mColliderType = ColliderType.Circle;
        mRenderLayer = Level.LayerTypes.Players;
        mAnimationData = new List<AnimationData>();
        mSpeed = 4;
        mHealth = 3;
    }

    [HideInInspector]
    public List<AnimationData> mAnimationData;
    [HideInInspector]
    public float mHealth;
    [HideInInspector]
    public float mSpeed;
    [HideInInspector]
    public string mGUIDProjectile;
    [HideInInspector]
    public Projectile mProjectile;
    [HideInInspector]
    public string mGUIDSpawnSound;
    [HideInInspector]
    public AudioClip mSpawnSound;
    [HideInInspector]
    public string mGUIDAttackSound;
    [HideInInspector]
    public AudioClip mAttackSound;
    [HideInInspector]
    public string mGUIDDeathSound;
    [HideInInspector]
    public AudioClip mDeathSound;
    [HideInInspector]
    public string mGUIDTeleportSound;
    [HideInInspector]
    public AudioClip mTeleportSound;

}
