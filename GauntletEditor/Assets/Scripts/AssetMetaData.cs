using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetMetaData : GameScriptable
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
    public string mAssetFilePath;
    public string mGUID;
}
