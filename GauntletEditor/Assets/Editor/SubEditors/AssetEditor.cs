using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class AssetEditor
{
    static AssetEditor mInstance;
    VisualElement mAssetEditorUI = null;
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
        return mInstance.mAssetEditorUI;
    }

}
