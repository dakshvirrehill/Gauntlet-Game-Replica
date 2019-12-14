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

    public override string ToString()
    {
        System.Text.StringBuilder aJSON = new System.Text.StringBuilder("{\n");
        aJSON.Append("\"class\" : \"" + mType.ToString("g") + "\",\n");
        aJSON.Append("\"guid\" : \"" + mGUID + "\",\n");
        aJSON.Append("\"path\" : \"" + mAssetFilePath + "\",\n");
        aJSON.Append("\n}");
        return aJSON.ToString();
    }

}
