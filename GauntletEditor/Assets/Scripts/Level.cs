using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[System.Serializable]
//public class SerializableDictionary
//{
//    [SerializeField] List<Vector2Int> Keys;
//    [SerializeField] List<List<Level.LayerTypes>> Values;
//    [SerializeField] List<List<GameScriptable>> Values2;

//    public void InitializeKey()
//    {
//        Keys = new List<Vector2Int>();
//    }

//    public void InitializeValue()
//    {
//        Values = new List<List<Level.LayerTypes>>();
//    }

//    public void InitializeValue2()
//    {
//        Values2 = new List<List<GameScriptable>>();
//    }

//    public void Add(Vector2Int pKey, List<Level.LayerTypes> pVal, List<GameScriptable> pVal2)
//    {
//        int aIx = -1;
//        if (Keys.Contains(pKey))
//        {
//            aIx = Keys.IndexOf(pKey);
//        }
//        if (aIx == -1)
//        {
//            Keys.Add(pKey);
//            Values.Add(pVal);
//            Values2.Add(pVal2);
//        }
//        else
//        {
//            Values[aIx] = pVal;
//            Values2[aIx] = pVal2;
//        }
//    }
//    public void Remove(Vector2Int pKey)
//    {
//        if (Keys.Contains(pKey))
//        {
//            int aIx = Keys.IndexOf(pKey);
//            Keys.RemoveAt(aIx);
//            Values.RemoveAt(aIx);
//            Values2.RemoveAt(aIx);
//        }
//    }
//    public int Count()
//    {
//        return Keys.Count;
//    }

//    public bool ContainsKey(Vector2Int pKey)
//    {
//        return Keys.Contains(pKey);
//    }

//    public List<Level.LayerTypes> GetValue(Vector2Int pKey)
//    {
//        if (!Keys.Contains(pKey))
//        {
//            throw new System.Exception();
//        }
//        return Values[Keys.IndexOf(pKey)];
//    }

//    public List<GameScriptable> GetValue2(Vector2Int pKey)
//    {
//        if (!Keys.Contains(pKey))
//        {
//            throw new System.Exception();
//        }
//        return Values2[Keys.IndexOf(pKey)];
//    }

//    public List<Level.LayerTypes> GetValue(int pIndex)
//    {
//        if (pIndex >= Keys.Count)
//        {
//            throw new System.Exception();
//        }
//        return Values[pIndex];
//    }

//    public List<GameScriptable> GetValue2(int pIndex)
//    {
//        if (pIndex >= Keys.Count)
//        {
//            throw new System.Exception();
//        }
//        return Values2[pIndex];
//    }

//    public Vector2Int GetKey(int pIndex)
//    {
//        if (pIndex >= Keys.Count)
//        {
//            throw new System.Exception();
//        }
//        return Keys[pIndex];
//    }

//}


[System.Serializable]
public struct LevelMapData
{
    public Vector2Int mPosition;
    [SerializeField]
    public List<GameScriptable> mScriptables;
    [SerializeField]
    public List<Level.LayerTypes> mLayerTypes;
}

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
    [HideInInspector]
    [SerializeField]
    public List<LevelMapData> mLevelData;
    //[HideInInspector]
    //[SerializeField]
    //public List<Vector2Int> Keys;
    //[HideInInspector]
    //[SerializeField]
    //public List<List<Level.LayerTypes>> Values;
    //[HideInInspector]
    //[SerializeField]
    //public List<List<GameScriptable>> Values2;
    //[SerializeField]
    //public SerializableDictionary mLevelData;
    //public Dictionary<Vector2Int, List<LayerTypes>> mLevelData;
    //public Dictionary<Vector2Int, List<GameScriptable>> mLevelDataScriptable;
    public void Init()
    {
        mName = "Untitled Level";
        mColumns = 32;
        mRows = 32;
        mTime = 60;
    }
}
