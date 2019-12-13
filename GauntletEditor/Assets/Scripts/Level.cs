using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : ScriptableObject
{
    public enum LayerTypes
    {
        Environment,
        StaticObjects,
        Enemies,
        Players
    }

    [System.Serializable]
    public struct GamePositions
    {
        public Vector2Int mWorldPosition;
        public string mDisplayName;
        public Sprite mDisplaySprite;
    }
    [HideInInspector]
    public string mGUIDGameplayMusic;
    [HideInInspector]
    public AudioClip mGameplayMusic;
    [HideInInspector]
    public string mName;
    [HideInInspector]
    public int mRows;
    [HideInInspector]
    public int mColumns;
    [HideInInspector]
    public float mTime;
    [HideInInspector]
    public GamePositions mStartPosition;
    [HideInInspector]
    public GamePositions mEndPosition;
    public Dictionary<Vector2Int, List<LayerTypes>> mLevelData;
    public Dictionary<Vector2Int, List<GameScriptable>> mLevelDataScriptable;
    public void Init()
    {
        mName = "Untitled Level";
        mColumns = 32;
        mRows = 32;
        mTime = 60;
        mLevelData = new Dictionary<Vector2Int, List<LayerTypes>>();
        mLevelDataScriptable = new Dictionary<Vector2Int, List<GameScriptable>>();
    }
}
