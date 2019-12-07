using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetMetaData : ScriptableObject
{
    public enum AssetType
    {
        None,
        TextureAsset,
        AudioAsset,
        FontAsset,
        PrefabAsset
    }
    [HideInInspector]
    public AssetType mType;
    [HideInInspector]
    public string mAssetFilePath;
    [HideInInspector]
    public string mGUID;
}
