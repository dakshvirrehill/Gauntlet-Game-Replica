using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using System.IO;

public class SerializeToJSON
{
    [Shortcut("Save To Game Data", KeyCode.F10)]
    [MenuItem("Gauntlet Editor/Save To Game Data")]
    public static void SerializeToJson()
    {
        string[] aCurLevelOrder = AssetDatabase.FindAssets("LevelOrder", new[] { "Assets/ScriptableObjects/Level Data" });
        if(aCurLevelOrder.Length <= 0)
        {
            EditorUtility.DisplayDialog("No Data", "Please Create Levels and Scriptable Objects Before Saving", "Okay");
            return;
        }
        LevelOrder aLevelOrderObject = AssetDatabase.LoadAssetAtPath<LevelOrder>(AssetDatabase.GUIDToAssetPath(aCurLevelOrder[0]));
        SaveLevelOrderJSON(aLevelOrderObject);
    }

    static void SaveLevelOrderJSON(LevelOrder pOrder)
    {
        Dictionary<AssetMetaData.AssetType, List<AssetMetaData>> aCurrentAssets = GetAssetData();
        string aLevelOrderJSON = "[";
        foreach(Level aLevel in pOrder.mAllLevels)
        {
            string aResourcesArray = "\"resources\": [";
            string aLevelJSON = "{\"GameObjects\": [";



            aLevelJSON += "]";
            aResourcesArray += "]";
            aLevelJSON += "," + aResourcesArray;
            aLevelJSON += "}";
        }
        aLevelOrderJSON += "]";
    }

    static Dictionary<AssetMetaData.AssetType,List<AssetMetaData>> GetAssetData()
    {
        Dictionary<AssetMetaData.AssetType, List<AssetMetaData>> aAssetData = new Dictionary<AssetMetaData.AssetType, List<AssetMetaData>>();
        aAssetData.Add(AssetMetaData.AssetType.AudioAsset, new List<AssetMetaData>());
        aAssetData.Add(AssetMetaData.AssetType.FontAsset, new List<AssetMetaData>());
        aAssetData.Add(AssetMetaData.AssetType.TextureAsset, new List<AssetMetaData>());
        aAssetData.Add(AssetMetaData.AssetType.PrefabAsset, new List<AssetMetaData>());
        string aBaseFolderPath = "Assets/ScriptableObjects/Asset Meta Data";
        string[] aAssetFiles = Directory.GetFiles(aBaseFolderPath + "/Textures/");
        foreach (string aTexture in aAssetFiles)
        {
            AssetMetaData aAsset = AssetDatabase.LoadAssetAtPath<AssetMetaData>(aTexture);
            if (aAsset != null)
            {
                if (aAsset.mType == AssetMetaData.AssetType.TextureAsset)
                {
                    aAssetData[aAsset.mType].Add(aAsset);
                }
            }
        }
        aAssetFiles = Directory.GetFiles(aBaseFolderPath + "/Audios/");
        foreach(string aAudio in aAssetFiles)
        {
            AssetMetaData aAsset = AssetDatabase.LoadAssetAtPath<AssetMetaData>(aAudio);
            if (aAsset != null)
            {
                if (aAsset.mType == AssetMetaData.AssetType.AudioAsset)
                {
                    aAssetData[aAsset.mType].Add(aAsset);
                }
            }
        }
        aAssetFiles = Directory.GetFiles(aBaseFolderPath + "/Fonts/");
        foreach(string aFont in aAssetFiles)
        {
            AssetMetaData aAsset = AssetDatabase.LoadAssetAtPath<AssetMetaData>(aFont);
            if (aAsset != null)
            {
                if (aAsset.mType == AssetMetaData.AssetType.FontAsset)
                {
                    aAssetData[aAsset.mType].Add(aAsset);
                }
            }

        }
        aAssetFiles = Directory.GetFiles(aBaseFolderPath + "/Prefabs/");
        foreach(string aPrefab in aAssetFiles)
        {
            AssetMetaData aAsset = AssetDatabase.LoadAssetAtPath<AssetMetaData>(aPrefab);
            if (aAsset != null)
            {
                if (aAsset.mType == AssetMetaData.AssetType.FontAsset)
                {
                    aAssetData[aAsset.mType].Add(aAsset);
                }
            }

        }
        return aAssetData;
    }

}
