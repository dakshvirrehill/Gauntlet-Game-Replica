using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditorInternal;

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

    ReorderableList mProjectileAnimList;
    List<AnimationData> mSelectedProjAnimData;
    Vector2 mProjectileGUIScrollPos;

    ObjectField mSObjSprite;

    ReorderableList mEnemyAnimList;
    List<AnimationData> mEnemyAnimations;
    Vector2 mEnemyGUIScrollPos;

    Vector2 mStaticGUIScrollPos;

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
                mSelectedProjAnimData = new List<AnimationData>();
                mProjectileAnimList = new ReorderableList(mSelectedProjAnimData, typeof(AnimationData));
                mProjectileAnimList.drawHeaderCallback = (Rect aRect) => {
                    EditorGUI.LabelField(aRect, "Move Animation Sprites");
                };
                mProjectileAnimList.drawElementCallback = UpdateAnimList;
                mProjectileAnimList.onAddCallback = AddNewAnimation;
                mCurrentObjectElement.Q<IMGUIContainer>("projectile_animation_sprites").onGUIHandler = ProjectileOnGUI;
                break;
            case GameObjectType.SpawnFactory:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/SpawnFactoryEditor.uxml");
                mCurrentObjectElement = aAsset.CloneTree();
                mCurrentObjectElement.Q<ObjectField>("spawn_enemy").objectType = typeof(Enemy);
                mCurrentObjectElement.Q<ObjectField>("spawn_sound").objectType = typeof(AudioClip);
                mCurrentObjectElement.Q<ObjectField>("item_1").objectType = typeof(Item);
                mCurrentObjectElement.Q<ObjectField>("item_2").objectType = typeof(Item);
                mCurrentObjectElement.Q<ObjectField>("item_3").objectType = typeof(Item);
                mCurrentObjectElement.Q<ObjectField>("factory_sprite").objectType = typeof(Sprite);
                break;
            case GameObjectType.Enemy:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/EnemyEditor.uxml");
                mCurrentObjectElement = aAsset.CloneTree();
                mCurrentObjectElement.Q<ObjectField>("enemy_attack_sound").objectType = typeof(AudioClip);
                mCurrentObjectElement.Q<ObjectField>("enemy_death_sound").objectType = typeof(AudioClip);
                EnumField aEnemyType = mCurrentObjectElement.Q<EnumField>("enemy_type");
                mCurrentObjectElement.Q<ObjectField>("enemy_projectile").objectType = typeof(Projectile);                
                aEnemyType.Init(Enemy.Type.Collider);
                mEnemyAnimations = new List<AnimationData>();
                mEnemyAnimList = new ReorderableList(mEnemyAnimations, typeof(AnimationData));
                mEnemyAnimList.drawHeaderCallback = (Rect aRect) =>
                {
                    EditorGUI.LabelField(aRect, "Move Animation Sprites");
                };
                mEnemyAnimList.drawElementCallback = UpdateEnemyAnimationList;
                mEnemyAnimList.onAddCallback = AddNewAnimation;
                mCurrentObjectElement.Q<IMGUIContainer>("enemy_animation_sprites").onGUIHandler = EnemyOnGUI;
                break;
            case GameObjectType.StaticObject:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/StaticObjectEditor.uxml");
                mCurrentObjectElement = aAsset.CloneTree();
                mSObjSprite = mCurrentObjectElement.Q<ObjectField>("static_object_sprite");
                mSObjSprite.objectType = typeof(Sprite);
                mCurrentObjectElement.Q<IMGUIContainer>("static_object_collider").onGUIHandler = StaticObjectOnGUI;
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
        mProjectileGUIScrollPos = EditorGUILayout.BeginScrollView(mProjectileGUIScrollPos,GUILayout.Width(1050),GUILayout.Height(350));
        mProjectileAnimList.DoLayoutList();
        EditorGUILayout.EndScrollView();
    }
    void StaticObjectOnGUI()
    {
        if(mSObjSprite.value != null)
        {
            mStaticGUIScrollPos = EditorGUILayout.BeginScrollView(mStaticGUIScrollPos, GUILayout.Width(1050), GUILayout.Height(350));
            Sprite aSprite = (Sprite)mSObjSprite.value;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(new Rect(100, 0, 2, aSprite.rect.height), Color.white);
            EditorGUI.DrawRect(new Rect(100 + aSprite.rect.width, 0, 2, aSprite.rect.height), Color.white);
            EditorGUI.DrawRect(new Rect(100, 0, aSprite.rect.width, 2), Color.white);
            EditorGUI.DrawRect(new Rect(100, aSprite.rect.height, aSprite.rect.width, 2), Color.white);
            DrawTexturePreview(new Rect(100,0, aSprite.rect.width,aSprite.rect.height), aSprite);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(50);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            if(GUILayout.Button("Create Collider"))
            {
                Debug.Log("Collider Created");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(50);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }
    }
    void EnemyOnGUI()
    {
        mEnemyGUIScrollPos = EditorGUILayout.BeginScrollView(mEnemyGUIScrollPos, GUILayout.Width(1050), GUILayout.Height(350));
        mEnemyAnimList.DoLayoutList();
        EditorGUILayout.EndScrollView();
    }

    void DrawTexturePreview(Rect pPosition, Sprite pSprite)
    {
        Vector2 aFullSize = new Vector2(pSprite.texture.width, pSprite.texture.height);
        Vector2 aSize = new Vector2(pSprite.textureRect.width, pSprite.textureRect.height);

        Rect aCoords = pSprite.textureRect;
        aCoords.x /= aFullSize.x;
        aCoords.width /= aFullSize.x;
        aCoords.y /= aFullSize.y;
        aCoords.height /= aFullSize.y;

        Vector2 ratio;
        ratio.x = pPosition.width / aSize.x;
        ratio.y = pPosition.height / aSize.y;
        float minRatio = Mathf.Min(ratio.x, ratio.y);

        Vector2 center = pPosition.center;
        pPosition.width = aSize.x * minRatio;
        pPosition.height = aSize.y * minRatio;
        pPosition.center = center;

        GUI.DrawTextureWithTexCoords(pPosition, pSprite.texture, aCoords);
    }

    #endregion
    #region Reorderable Helpers
    void UpdateAnimList(Rect aRect, int aIx, bool aIsActive, bool aIsFocused)
    {
        var aElement = (AnimationData)mProjectileAnimList.list[aIx];
        aRect.y += 2;
        EditorGUI.LabelField(new Rect(aRect.x, aRect.y, 100, EditorGUIUtility.singleLineHeight), aElement.mAnimationName);
    }

    void UpdateEnemyAnimationList(Rect aRect, int aIx, bool aIsActive, bool aIsFocused)
    {
        AnimationData aElement = (AnimationData)mEnemyAnimList.list[aIx];
        aRect.y += 2;
        EditorGUI.LabelField(new Rect(aRect.x, aRect.y, 100, EditorGUIUtility.singleLineHeight), aElement.mAnimationName);
    }

    void AddNewAnimation(ReorderableList pList)
    {
        if(!AddAnimationWindow.IsWindowOpen())
        {
            AddAnimationWindow.OpenAnimationWindow();
        }
    }

    public static void AddToCurrentAnimationList(AnimationData pData)
    {
        switch (mInstance.mActiveType)
        {
            case GameObjectType.Enemy:
                mInstance.mEnemyAnimations.Add(pData);
                break;
            case GameObjectType.Projectile:
                mInstance.mSelectedProjAnimData.Add(pData);
                break;
        }
    }

    #endregion

}
