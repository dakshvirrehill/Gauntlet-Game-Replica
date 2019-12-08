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
    //public Dictionary<LayerTypes, List<GameScriptable>> mLayerVsScriptable;
    public void Init()
    {
        mName = "Untitled Level";
        mColumns = 32;
        mRows = 32;
        mTime = 60;
        mLevelData = new Dictionary<Vector2Int, List<LayerTypes>>();
        mLevelDataScriptable = new Dictionary<Vector2Int, List<GameScriptable>>();
        //mLayerVsScriptable = new Dictionary<LayerTypes, List<GameScriptable>>();
    }

    public bool IsLayerObjectPresent(Vector2Int pPosition, LayerTypes pLayer)
    {
        if(mLevelData == null)
        {
            return false;
        }
        if(!mLevelData.ContainsKey(pPosition))
        {
            return false;
        }
        if(mLevelData[pPosition].Contains(pLayer))
        {
            return true;
        }
        return false;
    }

    public bool IsScriptablePresent(Vector2Int pPosition, GameScriptable pObject)
    {
        if(mLevelDataScriptable == null)
        {
            return false;
        }
        if(!mLevelDataScriptable.ContainsKey(pPosition))
        {
            return false;
        }
        if(mLevelDataScriptable[pPosition].Contains(pObject))
        {
            return true;
        }
        return false;
    }

    public void AddScriptable(Vector2Int pPosition, GameScriptable pObject)
    {
        if(mLevelData == null)
        {
            mLevelData = new Dictionary<Vector2Int, List<LayerTypes>>();
        }
        if(!mLevelData.ContainsKey(pPosition))
        {
            mLevelData.Add(pPosition, new List<LayerTypes>());
        }
        mLevelData[pPosition].Add(pObject.mRenderLayer);
        if(mLevelDataScriptable == null)
        {
            mLevelDataScriptable = new Dictionary<Vector2Int, List<GameScriptable>>();
        }
        if(!mLevelDataScriptable.ContainsKey(pPosition))
        {
            mLevelDataScriptable.Add(pPosition, new List<GameScriptable>());
        }
        mLevelDataScriptable[pPosition].Add(pObject);
        //if(!mLayerVsScriptable.ContainsKey(pObject.mRenderLayer))
        //{
        //    mLayerVsScriptable.Add(pObject.mRenderLayer, new List<GameScriptable>());
        //}
        //mLayerVsScriptable[pObject.mRenderLayer].Add(pObject);
    }

    public void RemoveScriptable(Vector2Int pPosition, GameScriptable pObject)
    {
        if(mLevelData == null || mLevelDataScriptable == null)
        {
            return;
        }
        mLevelData[pPosition].Remove(pObject.mRenderLayer);
        mLevelDataScriptable[pPosition].Remove(pObject);
        //mLayerVsScriptable[pObject.mRenderLayer].Remove(pObject);
    }

    public void SetResetStartPosition(Vector2Int pPosition)
    {
        if(mStartPosition.mWorldPosition == pPosition)
        {
            mStartPosition.mWorldPosition = new Vector2Int(-1, -1);
        }
        else
        {
            mStartPosition.mWorldPosition = pPosition;
        }
    }

    public void SetResetEndPosition(Vector2Int pPosition)
    {
        if (mEndPosition.mWorldPosition == pPosition)
        {
            mEndPosition.mWorldPosition = new Vector2Int(-1, -1);
        }
        else
        {
            mEndPosition.mWorldPosition = pPosition;
        }

    }

}
