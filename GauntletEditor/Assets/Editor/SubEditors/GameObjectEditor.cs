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
        Enemy,
        SpawnFactory,
        StaticObject,
    }
    static GameObjectEditor mInstance;
    VisualElement mGameObjectEditorUI = null;
    GameObjectType mActiveType = GameObjectType.None;
    GameScriptable mScriptable;
    EnumField mTypeEnum;
    ObjectField mSelectionField;

    VisualElement mCurrentObjectElement;

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
        mInstance.mTypeEnum.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => mInstance.OnTypeChanged((GameObjectType)aEv.newValue));
        mInstance.mSelectionField = mInstance.mGameObjectEditorUI.Q<ObjectField>("scriptable_gobj_field");
        mInstance.mSelectionField.RegisterCallback<ChangeEvent<Object>>((aEv) => mInstance.OnSelectionChanged(aEv.newValue));
        mInstance.mSelectionField.SetEnabled(false);
        return mInstance.mGameObjectEditorUI;
    }

    void SetObjectFieldType()
    {
        mSelectionField.SetValueWithoutNotify(null);
        mSelectionField.SetEnabled(true);
        switch (mActiveType)
        {
            case GameObjectType.None:
                mSelectionField.SetEnabled(false);
                break;
            case GameObjectType.Enemy:
                mSelectionField.objectType = typeof(Enemy);
                break;
            case GameObjectType.Pickable:
                mSelectionField.objectType = typeof(Item);
                break;
            case GameObjectType.Projectile:
                mSelectionField.objectType = typeof(Projectile);
                break;
            case GameObjectType.SpawnFactory:
                mSelectionField.objectType = typeof(SpawnFactory);
                break;
            case GameObjectType.StaticObject:
                mSelectionField.objectType = typeof(StaticObject);
                break;
        }
    }

    void RemoveCurrentObjectVE()
    {
        if(mCurrentObjectElement != null && mGameObjectEditorUI.Contains(mCurrentObjectElement))
        {
            mGameObjectEditorUI.Remove(mCurrentObjectElement);
        }
        mCurrentObjectElement = null;
    }

    void CreateNewObjectVE()
    {
        VisualTreeAsset aAsset;
        switch (mActiveType)
        {
            case GameObjectType.None:
                return;
            case GameObjectType.Pickable:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/PickableEditor.uxml");
                break;
            case GameObjectType.Projectile:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/ProjectileEditor.uxml");
                break;
            case GameObjectType.SpawnFactory:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/SpawnFactoryEditor.uxml");
                break;
            case GameObjectType.Enemy:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/EnemyEditor.uxml");
                break;
            case GameObjectType.StaticObject:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/StaticObjectEditor.uxml");
                break;
            default:
                return;
        }
        if(aAsset == null)
        {
            return;
        }
        mCurrentObjectElement = aAsset.CloneTree();
        mCurrentObjectElement.Q<Button>("gobj_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
        mGameObjectEditorUI.Add(mCurrentObjectElement);
    }

    void OnTypeChanged(GameObjectType pNewType)
    {
        if(mScriptable != null)
        {
            mTypeEnum.value = mActiveType;
            return;
        }
        RemoveCurrentObjectVE();
        mActiveType = pNewType;
        SetObjectFieldType();
        CreateNewObjectVE();
    }

    void OnSelectionChanged(Object pSelectedObject)
    {
        //setup values according to the object type
    }

    void SaveAsScriptableAsset()
    {
        Debug.Log("Save Scriptable Asset Function Called");
        mTypeEnum.value = GameObjectType.None;
    }

}
