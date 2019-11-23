using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class PlayerEditor
{
    static PlayerEditor mInstance;
    VisualElement mPlayerEditorUI = null;
    static void CreateInstance()
    {
        if(mInstance == null)
        {
            mInstance = new PlayerEditor();
        }
    }

    public static VisualElement CreateNewPlayerEditorUI()
    {
        CreateInstance();
        VisualTreeAsset aPlayerEditorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/PlayerEditor.uxml");
        mInstance.mPlayerEditorUI = aPlayerEditorAsset.CloneTree();
        return mInstance.mPlayerEditorUI;
    }

}
