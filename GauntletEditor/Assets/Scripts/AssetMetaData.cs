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
    public AssetType mType;
    public string mAssetUnityGUID;
    public string mAssetFilePath;
    public string mGUID;
}
