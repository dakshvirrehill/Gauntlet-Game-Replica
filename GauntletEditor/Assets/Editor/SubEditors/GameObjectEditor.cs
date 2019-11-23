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
    VisualElement mEditoryBlock = null;
    GameObjectType mActiveType = GameObjectType.None;
    GameScriptable mScriptable;
    EnumField mTypeEnum;
    ObjectField mSelectionField;

    VisualElement mCurrentObjectElement;

    IMGUIContainer mProjectileImgui;

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
        mInstance.mEditoryBlock = mInstance.mGameObjectEditorUI.Q<VisualElement>("gobj_editor_data");
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
        if(mCurrentObjectElement != null && mEditoryBlock.Contains(mCurrentObjectElement))
        {
            mEditoryBlock.Remove(mCurrentObjectElement);
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
                mCurrentObjectElement = aAsset.CloneTree();
                EnumField aItemType = mCurrentObjectElement.Q<EnumField>("item_type");
                aItemType.Init(Item.Type.TempType1);
                mCurrentObjectElement.Q<ObjectField>("item_collect_sound").objectType = typeof(AudioClip);
                mCurrentObjectElement.Q<ObjectField>("item_idle_sprite").objectType = typeof(Sprite);
                //aItemType.RegisterCallback<>
                break;
            case GameObjectType.Projectile:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/ProjectileEditor.uxml");
                mCurrentObjectElement = aAsset.CloneTree();
                mProjectileImgui = mCurrentObjectElement.Q<IMGUIContainer>("projectile_animation_sprites");
                mProjectileImgui.onGUIHandler = ProjectileOnGUI;
                break;
            case GameObjectType.SpawnFactory:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/SpawnFactoryEditor.uxml");
                mCurrentObjectElement = aAsset.CloneTree();
                break;
            case GameObjectType.Enemy:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/EnemyEditor.uxml");
                mCurrentObjectElement = aAsset.CloneTree();
                break;
            case GameObjectType.StaticObject:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/StaticObjectEditor.uxml");
                mCurrentObjectElement = aAsset.CloneTree();
                break;
            default:
                return;
        }
        if(aAsset == null)
        {
            return;
        }
        mCurrentObjectElement.Q<Button>("gobj_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
        mEditoryBlock.Add(mCurrentObjectElement);
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


    #region IMGUI OnGUIs
    void ProjectileOnGUI()
    {
        EditorGUI.DrawRect(new Rect(0, 0, 1050, 350), Color.white);

    }
    #endregion

}
