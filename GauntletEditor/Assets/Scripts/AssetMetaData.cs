using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetMetaData : ScriptableObject
{
    public enum AssetType
    {
        TextureAsset,
        AudioAsset,
        FontAsset,
        PrefabAsset
    }
    public string mAssetFilePath;
    public string mGUID;
}
