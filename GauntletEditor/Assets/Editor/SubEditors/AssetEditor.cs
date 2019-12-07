using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;

public class AssetEditor : IBindable
{
    static AssetEditor mInstance;
    VisualElement mAssetEditorUI = null;
    VisualElement mAssetEditorData = null;
    ObjectField mSelectionField = null;
    EnumField mTypeOfAsset = null;
    Object mSelectedObject;
    VisualElement mObjectData = null;

    AssetMetaData mCurrentAssetData = null;


    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

    static void CreateInstance()
    {
        if(mInstance == null)
        {
            mInstance = new AssetEditor();
        }
    }

    public static VisualElement CreateNewAssetEditorUI()
    {
        CreateInstance();
        VisualTreeAsset aAssetEditorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/AssetEditor.uxml");
        mInstance.mAssetEditorUI = aAssetEditorAsset.CloneTree();
        mInstance.mSelectionField = mInstance.mAssetEditorUI.Q<ObjectField>("asset_field");
        mInstance.mTypeOfAsset = mInstance.mAssetEditorUI.Q<EnumField>("asset_type");
        if(mInstance.mCurrentAssetData == null)
        {
            mInstance.mTypeOfAsset.Init(AssetMetaData.AssetType.None);
        }
        else
        {
            mInstance.mTypeOfAsset.Init(mInstance.mCurrentAssetData.mType);
        }
        mInstance.mTypeOfAsset.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => mInstance.ActivateObjectFieldOfType((AssetMetaData.AssetType)aEv.newValue));
        if(mInstance.mCurrentAssetData != null)
        {
            mInstance.SetObjectFieldType((AssetMetaData.AssetType)mInstance.mTypeOfAsset.value);
            string aAssetPath = AssetDatabase.GUIDToAssetPath(mInstance.mCurrentAssetData.mGUID);
            mInstance.mSelectionField.value = AssetDatabase.LoadAssetAtPath(aAssetPath, AssetDatabase.GetMainAssetTypeAtPath(aAssetPath));
        }
        else
        {
            mInstance.mSelectionField.SetEnabled(false);
        }
        mInstance.mSelectionField.RegisterCallback<ChangeEvent<Object>>((aEv) => mInstance.SetSelectedObjectData(aEv.newValue));
        mInstance.mAssetEditorData = mInstance.mAssetEditorUI.Q<VisualElement>("asset_editor_data");
        mInstance.mObjectData = mInstance.mAssetEditorData.Q<VisualElement>("asset_data");
        mInstance.mObjectData.Q<Button>("asset_meta_data").RegisterCallback<MouseUpEvent>(mInstance.SaveAsScriptableAsset);
        if(mInstance.mCurrentAssetData != null)
        {
            mInstance.mObjectData.Bind(new SerializedObject(mInstance.mCurrentAssetData));
        }
        else
        {
            mInstance.mAssetEditorData.Remove(mInstance.mObjectData);
        }
        return mInstance.mAssetEditorUI;
    }

    void SetObjectFieldType(AssetMetaData.AssetType pType)
    {
        mSelectionField.SetEnabled(false);
        switch (pType)
        {
            case AssetMetaData.AssetType.None:
                return;
            case AssetMetaData.AssetType.AudioAsset:
                mSelectionField.objectType = typeof(AudioClip);
                break;
            case AssetMetaData.AssetType.FontAsset:
                mSelectionField.objectType = typeof(Font);
                break;
            case AssetMetaData.AssetType.TextureAsset:
                mSelectionField.objectType = typeof(Texture);
                break;
            case AssetMetaData.AssetType.PrefabAsset:
                mSelectionField.objectType = typeof(GameScriptable);
                break;
        }
        mSelectionField.SetEnabled(true);
    }

    void ActivateObjectFieldOfType(AssetMetaData.AssetType pType)
    {
        if(mAssetEditorData.Contains(mObjectData))
        {
            mAssetEditorData.Remove(mObjectData);
        }
        if(mCurrentAssetData != null)
        {
            EditorUtility.SetDirty(mCurrentAssetData);
            AssetDatabase.SaveAssets();
            mCurrentAssetData = null;
        }
        mSelectionField.value = null;
        SetObjectFieldType(pType);
    }

    void SetSelectedObjectData(Object pObject)
    {
        if(pObject == null)
        {
            return;
        }
        string aAssetExtension = pObject.name;
        switch ((AssetMetaData.AssetType)mTypeOfAsset.value)
        {
            case AssetMetaData.AssetType.None:
                return;
            case AssetMetaData.AssetType.TextureAsset:
                aAssetExtension = "Textures/" + aAssetExtension + Path.GetExtension(AssetDatabase.GetAssetPath(pObject));
                break;
            case AssetMetaData.AssetType.AudioAsset:
                aAssetExtension = "Audios/" + aAssetExtension + Path.GetExtension(AssetDatabase.GetAssetPath(pObject));
                break;
            case AssetMetaData.AssetType.FontAsset:
                aAssetExtension = "Fonts/" + aAssetExtension + Path.GetExtension(AssetDatabase.GetAssetPath(pObject));
                break;
            case AssetMetaData.AssetType.PrefabAsset:
                ((GameScriptable)pObject).mIsPrefab = true;
                aAssetExtension = "Prefabs/" + aAssetExtension + ".json";
                break;
        }
        string[] aAssetFolder = { "Assets/ScriptableObjects/Asset Meta Data" };
        if(!AssetDatabase.IsValidFolder(aAssetFolder[0]))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Asset Meta Data");
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pObject.name, aAssetFolder);
        if(aAssetGUIDs.Length > 0)
        {
            for(int i = 1; i < aAssetGUIDs.Length; i++)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(aAssetGUIDs[i]));
            }
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
            if(AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(AssetMetaData))
            {
                mCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
            }
        }

        if (mCurrentAssetData == null)
        {
            mCurrentAssetData = (AssetMetaData)ScriptableObject.CreateInstance(typeof(AssetMetaData));
            mCurrentAssetData.mAssetFilePath = "../Assets/Resources/" + aAssetExtension;
            mCurrentAssetData.mType = (AssetMetaData.AssetType)mTypeOfAsset.value;
            mCurrentAssetData.mGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(pObject));
            AssetDatabase.CreateAsset(mCurrentAssetData, aAssetFolder[0] + "/" + pObject.name + ".asset");
        }
        EditorUtility.SetDirty(mCurrentAssetData);
        mObjectData.Bind(new SerializedObject(mCurrentAssetData));
        mAssetEditorData.Add(mObjectData);
    }

    void SaveAsScriptableAsset(MouseUpEvent aEv)
    {
        EditorUtility.SetDirty(mCurrentAssetData);
        AssetDatabase.SaveAssets();
        mCurrentAssetData = null;
        mTypeOfAsset.value = AssetMetaData.AssetType.None;
    }

}
