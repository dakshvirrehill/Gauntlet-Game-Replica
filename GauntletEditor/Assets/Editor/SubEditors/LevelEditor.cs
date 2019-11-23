using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class LevelEditor
{
    static LevelEditor mInstance;
    VisualElement mLevelEditorUI = null;
    static void CreateInstance()
    {
        if(mInstance == null)
        {
            mInstance = new LevelEditor();
        }
    }

    public static VisualElement CreateNewLevelEditorUI()
    {
        CreateInstance();
        VisualTreeAsset aLevelEditorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/LevelEditor.uxml");
        mInstance.mLevelEditorUI = aLevelEditorAsset.CloneTree();
        return mInstance.mLevelEditorUI;
    }

}
