using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : GameScriptable
{
    public enum Type
    {
        Collider,
        CloseRangeAttacker,
        ProjectileThrower
    }
    [HideInInspector]
    [SerializeField]
    public List<AnimationData> mEnemyAnimations;
    [HideInInspector]
    public float mStopRange;
    [HideInInspector]
    public float mSpeed;
    [HideInInspector]
    public Type mEnemyType;
    [HideInInspector]
    public string mProjectileGUID;
    [HideInInspector]
    public Projectile mProjectile;
    [HideInInspector]
    public string mAttackSoundGUID;
    [HideInInspector]
    public AudioClip mAttackSound;
    [HideInInspector]
    public string mDeathSoundGUID;
    [HideInInspector]
    public AudioClip mDeathSound;
    public override void Init()
    {
        mType = ObjectType.Enemy;
        mEnemyAnimations = new List<AnimationData>();
        mSpeed = 4;
        mColliderType = ColliderType.Circle;
        mIsTrigger = false;
    }

}
