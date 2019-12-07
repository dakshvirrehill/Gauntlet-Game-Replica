using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AnimationData
{
    public string mAnimationName;
    public float mAnimSpeed;
    public string mTextureAssetGUID;
    public List<Sprite> mSprites;
}
public class GameScriptable : ScriptableObject
{
    public enum ColliderType
    {
        None,
        Box,
        Circle
    }
    [HideInInspector]
    public bool mIsPrefab = false;
    [HideInInspector]
    public ColliderType mColliderType = ColliderType.None;
    [HideInInspector]
    public bool mIsTrigger = false;
    [HideInInspector]
    public string mName;
    [HideInInspector]
    public Level.LayerTypes mRenderLayer;
}
