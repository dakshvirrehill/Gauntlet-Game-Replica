using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class GameObjectEditor
{
    public enum GameObjectType
    {
        None,
        Projectile,
        Pickable,
        SpawnFactory,
        StaticObject,
    }
    static GameObjectEditor mInstance;
    VisualElement mGameObjectEditorUI = null;
    GameScriptable mScriptable;
    EnumField mTypeEnum;
    static void CreateInstance()
    {
        if(mInstance == null)
        {
            mInstance = new GameObjectEditor();
        }
    }

    public static VisualElement CreateNewGameObjectEditorUI()
    {
        CreateInstance();
        VisualTreeAsset aGameObjectEditorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/GameObjectEditor.uxml");
        mInstance.mGameObjectEditorUI = aGameObjectEditorAsset.CloneTree();
        mInstance.mTypeEnum = mInstance.mGameObjectEditorUI.Q<EnumField>("gobj_type");
        mInstance.mTypeEnum.Init(GameObjectType.None);
        return mInstance.mGameObjectEditorUI;
    }

}
