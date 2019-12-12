using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class GenHelpers
{
    public static void OnCollectSoundSelection(AudioClip pClip, ObjectField pField)
    {
        if (pClip == null)
        {
            return;
        }
        string[] aAssetFolder = { "Assets/ScriptableObjects/Asset Meta Data" };
        if (!AssetDatabase.IsValidFolder(aAssetFolder[0]))
        {
            ShowSelectionWarning();
            ResetCollectSelection(pField);
            return;
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pClip.name, aAssetFolder);
        if (aAssetGUIDs.Length > 0)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
            if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(AssetMetaData))
            {
                AssetMetaData aCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
                if (aCurrentAssetData.mType == AssetMetaData.AssetType.AudioAsset)
                {
                    Item aTempObj = (Item)GameObjectEditor.GetCurrentScriptable();
                    aTempObj.mSoundGUID = aCurrentAssetData.mGUID;
                    aTempObj.mItemCollectSound = pClip;
                }
                else
                {
                    ShowSelectionWarning();
                    ResetCollectSelection(pField);
                    return;
                }
            }
            else
            {
                ShowSelectionWarning();
                ResetCollectSelection(pField);
                return;
            }
        }
        else
        {
            ShowSelectionWarning();
            ResetCollectSelection(pField);
            return;
        }

    }

    public static void OnSpriteSelection(Sprite pSelectedSprite, ObjectField pField)
    {
        if (pSelectedSprite == null)
        {
            return;
        }
        string[] aAssetFolder = { "Assets/ScriptableObjects/Asset Meta Data" };
        if (!AssetDatabase.IsValidFolder(aAssetFolder[0]))
        {
            ShowSelectionWarning();
            ResetSpriteSelection(pField);
            return;
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pSelectedSprite.texture.name, aAssetFolder);
        if (aAssetGUIDs.Length > 0)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
            if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(AssetMetaData))
            {
                AssetMetaData aCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
                if (aCurrentAssetData.mType == AssetMetaData.AssetType.TextureAsset)
                {
                    switch (GameObjectEditor.GetCurrentScriptable().mType)
                    {
                        case GameScriptable.ObjectType.SpawnFactory:
                            SpawnFactory aTempObj = (SpawnFactory)GameObjectEditor.GetCurrentScriptable();
                            aTempObj.mTextureGUID = aCurrentAssetData.mGUID;
                            aTempObj.mDisplaySprite = pSelectedSprite;
                            break;
                        case GameScriptable.ObjectType.StaticObject:
                            StaticObject aTObj = (StaticObject)GameObjectEditor.GetCurrentScriptable();
                            aTObj.mTextureGUID = aCurrentAssetData.mGUID;
                            aTObj.mDisplaySprite = pSelectedSprite;
                            break;
                        case GameScriptable.ObjectType.Item:
                            Item aTmpObj = (Item)GameObjectEditor.GetCurrentScriptable();
                            aTmpObj.mTextureGUID = aCurrentAssetData.mGUID;
                            aTmpObj.mDisplaySprite = pSelectedSprite;
                            break;
                        default:
                            ShowSelectionWarning();
                            ResetSpriteSelection(pField);
                            break;
                    }
                }
                else
                {
                    ShowSelectionWarning();
                    ResetSpriteSelection(pField);
                    return;
                }
            }
            else
            {
                ShowSelectionWarning();
                ResetSpriteSelection(pField);
                return;
            }
        }
        else
        {
            ShowSelectionWarning();
            ResetSpriteSelection(pField);
            return;
        }
    }
    static void ShowSelectionWarning()
    {
        EditorUtility.DisplayDialog("Warning Asset GUID Not Set", "Use the Asset Meta Data Editor to Generate Game Metas before assigning to a Game Object", "Okay");
    }
    public static void ResetCollectSelection(ObjectField pField)
    {
        Item aItem = (Item)GameObjectEditor.GetCurrentScriptable();
        if(!string.IsNullOrEmpty(aItem.mSoundGUID))
        {
            pField.SetEnabled(false);
            pField.value = aItem.mItemCollectSound;
            pField.SetEnabled(true);
        }
    }
    public static void ResetSpriteSelection(ObjectField pField)
    {
        switch (GameObjectEditor.GetCurrentScriptable().mType)
        {
            case GameScriptable.ObjectType.SpawnFactory:
                SpawnFactory aTempObj = (SpawnFactory)GameObjectEditor.GetCurrentScriptable();
                if (!string.IsNullOrEmpty(aTempObj.mTextureGUID))
                {
                    pField.SetEnabled(false);
                    pField.value = aTempObj.mDisplaySprite;
                    pField.SetEnabled(true);
                }
                break;
            case GameScriptable.ObjectType.StaticObject:
                StaticObject aTObj = (StaticObject)GameObjectEditor.GetCurrentScriptable();
                if (!string.IsNullOrEmpty(aTObj.mTextureGUID))
                {
                    pField.SetEnabled(false);
                    pField.value = aTObj.mDisplaySprite;
                    pField.SetEnabled(true);
                }
                break;
            case GameScriptable.ObjectType.Item:
                Item aTmpObj = (Item)GameObjectEditor.GetCurrentScriptable();
                if (!string.IsNullOrEmpty(aTmpObj.mTextureGUID))
                {
                    pField.SetEnabled(false);
                    pField.value = aTmpObj.mDisplaySprite;
                    pField.SetEnabled(true);
                }
                break;
        }

    }


}
