using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnFactory : GameScriptable
{
    public override void Init()
    {
        mType = ObjectType.SpawnFactory;
        mSpawnTime = new Vector2(4, 10);
    }

    [HideInInspector]
    public string mTextureGUID;
    [HideInInspector]
    public Vector2 mSpawnTime;
    [HideInInspector]
    public int mPoolCount;
    [HideInInspector]
    public string mEnemyGUID;
    [HideInInspector]
    public Enemy mSpawnEnemy;
    [HideInInspector]
    public string mSoundGUID;
    [HideInInspector]
    public AudioClip mSpawnSound;


}
