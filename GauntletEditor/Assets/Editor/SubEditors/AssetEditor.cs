using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class AssetEditor
{
    static AssetEditor mInstance;
    VisualElement mAssetEditorUI = null;
    VisualElement mAssetEditorData = null;
    ObjectField mSelectionField = null;
    EnumField mTypeOfAsset = null;
    Object mSelectedObject;
    VisualElement mObjectData = null;
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
        mInstance.mTypeOfAsset.Init(AssetMetaData.AssetType.None);
        mInstance.mTypeOfAsset.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => mInstance.ActivateObjectFieldOfType((AssetMetaData.AssetType)aEv.newValue));
        mInstance.mSelectionField.RegisterCallback<ChangeEvent<Object>>((aEv) => mInstance.SetSelectedObjectData(aEv.newValue));
        mInstance.mSelectionField.SetEnabled(false);
        mInstance.mAssetEditorData = mInstance.mAssetEditorUI.Q<VisualElement>("asset_editor_data");
        mInstance.mObjectData = mInstance.mAssetEditorData.Q<VisualElement>("asset_data");
        mInstance.mObjectData.Q<Button>("asset_meta_data").RegisterCallback<MouseUpEvent>(mInstance.SaveAsScriptableAsset);
        mInstance.mAssetEditorData.Remove(mInstance.mObjectData);
        return mInstance.mAssetEditorUI;
    }

    void ActivateObjectFieldOfType(AssetMetaData.AssetType pType)
    {
        if(mAssetEditorData.Contains(mObjectData))
        {
            mAssetEditorData.Remove(mObjectData);
        }
        mSelectionField.value = null;
        mSelectionField.SetEnabled(false);
        switch(pType)
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

    void SetSelectedObjectData(Object pObject)
    {
        switch((AssetMetaData.AssetType)mTypeOfAsset.value)
        {
            case AssetMetaData.AssetType.None:
                return;
            case AssetMetaData.AssetType.TextureAsset:
                break;
            case AssetMetaData.AssetType.AudioAsset:
                break;
            case AssetMetaData.AssetType.FontAsset:
                break;
            case AssetMetaData.AssetType.PrefabAsset:
                break;
        }
        //Find scriptable object from database that matches asset name.
        //If no asset then create a scriptable object
        //bind scriptable object to object data
        //add object data to visual tree
        mAssetEditorData.Add(mObjectData);
    }

    void SaveAsScriptableAsset(MouseUpEvent aEv)
    {
        Debug.Log("Save Scriptable Asset Function Called");
        mTypeOfAsset.value = AssetMetaData.AssetType.None;
    }

}
