﻿using System.Collections.Generic;
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
    EnumField mTypeEnum;
    ObjectField mSelectionField;
    Button mCreateNewButton;
    TextField mNameField;
    VisualElement mCurrentObjectElement;
    GameScriptable mActiveGameObjectAsset;

    #region Static Object Elements
    ObjectField mSObjSprite;
    EnumField mColliderType;
    #endregion

    #region Projectile Elements
    ReorderableList mProjectileAnimList;
    List<AnimationData> mSelectedProjAnimData;
    Vector2 mProjectileGUIScrollPos;
    #endregion

    #region Enemy Elements
    ReorderableList mEnemyAnimList;
    List<AnimationData> mEnemyAnimations;
    Vector2 mEnemyGUIScrollPos;
    #endregion



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
        mInstance.mEditoryBlock = mInstance.mGameObjectEditorUI.Q<VisualElement>("gobj_editor_data");
        mInstance.mTypeEnum = mInstance.mEditoryBlock.Q<EnumField>("gobj_type");
        mInstance.mTypeEnum.Init(GameObjectType.None);
        mInstance.mTypeEnum.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => mInstance.OnTypeChanged((GameObjectType)aEv.newValue));
        mInstance.mSelectionField = mInstance.mEditoryBlock.Q<ObjectField>("scriptable_gobj_field");
        mInstance.mSelectionField.RegisterCallback<ChangeEvent<Object>>((aEv) => mInstance.OnSelectionChanged(aEv.newValue));
        mInstance.mSelectionField.SetEnabled(false);
        mInstance.mCreateNewButton = mInstance.mEditoryBlock.Q<Button>("gobj_create_new");
        mInstance.mCreateNewButton.RegisterCallback<MouseUpEvent>((aEv) => mInstance.OnCreateNewObject());
        mInstance.mCreateNewButton.SetEnabled(false);
        mInstance.mNameField = mInstance.mEditoryBlock.Q<TextField>("gobj_name");
        return mInstance.mGameObjectEditorUI;
    }

    void SetObjectFieldType()
    {
        mSelectionField.value = null;
        mSelectionField.SetEnabled(true);
        mCreateNewButton.SetEnabled(true);
        switch (mActiveType)
        {
            case GameObjectType.None:
                mSelectionField.SetEnabled(false);
                mCreateNewButton.SetEnabled(false);
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
                mCurrentObjectElement.Q<Button>("gobj_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
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
                mCurrentObjectElement.Q<Button>("gobj_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
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
                mCurrentObjectElement.Q<Button>("gobj_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
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
                mCurrentObjectElement.Q<Button>("gobj_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
                break;
            case GameObjectType.StaticObject:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/StaticObjectEditor.uxml");
                mCurrentObjectElement = aAsset.CloneTree();
                mSObjSprite = mCurrentObjectElement.Q<ObjectField>("static_object_sprite");
                mSObjSprite.objectType = typeof(Sprite);
                mSObjSprite.RegisterCallback<ChangeEvent<Object>>((aEv) => OnStaticSpriteSelection((Sprite)aEv.newValue));
                mColliderType = mCurrentObjectElement.Q<EnumField>("static_object_collider");
                mColliderType.Init(GameScriptable.ColliderType.None);
                mColliderType.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => OnColliderTypeChanged((GameScriptable.ColliderType) aEv.newValue));
                break;
            default:
                return;
        }
        if(aAsset == null)
        {
            return;
        }
        mEditoryBlock.Add(mCurrentObjectElement);
    }

    void SaveCurrentActiveAsset()
    {
        if (mActiveGameObjectAsset != null)
        {
            EditorUtility.SetDirty(mActiveGameObjectAsset);
            AssetDatabase.SaveAssets();
            mActiveGameObjectAsset = null;
        }
    }

    void OnTypeChanged(GameObjectType pNewType)
    {
        SaveCurrentActiveAsset();
        RemoveCurrentObjectVE();
        mActiveType = pNewType;
        SetObjectFieldType();
    }

    void OnSelectionChanged(Object pSelectedObject)
    {
        if(pSelectedObject == null)
        {
            return;
        }
        SaveCurrentActiveAsset();
        RemoveCurrentObjectVE();
        CreateNewObjectVE();
        mActiveGameObjectAsset = (GameScriptable)pSelectedObject;
        switch(mActiveType)
        {
            case GameObjectType.Enemy:
                Enemy aEnemy = (Enemy)pSelectedObject;
                //do enemy specific things like setting up the anim values
                mCurrentObjectElement.Bind(new SerializedObject(aEnemy));
                break;
            case GameObjectType.Pickable:
                Item aItem = (Item)pSelectedObject;
                //do item specific things
                mCurrentObjectElement.Bind(new SerializedObject(aItem));
                break;
            case GameObjectType.Projectile:
                Projectile aProjectile = (Projectile)pSelectedObject;
                //do projectile specific things
                mCurrentObjectElement.Bind(new SerializedObject(aProjectile));
                break;
            case GameObjectType.SpawnFactory:
                SpawnFactory aFactory = (SpawnFactory)pSelectedObject;
                //do spawn factory specific things
                mCurrentObjectElement.Bind(new SerializedObject(aFactory));
                break;
            case GameObjectType.StaticObject:
                StaticObject aStObj = (StaticObject)pSelectedObject;
                if(!string.IsNullOrEmpty(aStObj.mTextureGUID))
                {
                    Sprite[] aAllSprites = (Sprite[]) AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(aStObj.mTextureGUID));
                    mSObjSprite.SetEnabled(false);
                    mSObjSprite.value = aAllSprites[aStObj.mSpriteIndex];
                    mSObjSprite.SetEnabled(true);
                }
                mColliderType.value = aStObj.mColliderType;
                mCurrentObjectElement.Bind(new SerializedObject(aStObj));
                break;
        }
        EditorUtility.SetDirty(mActiveGameObjectAsset);
    }

    void OnCreateNewObject()
    {
        if(mActiveType == GameObjectType.None)
        {
            return;
        }
        if(string.IsNullOrEmpty(mNameField.value))
        {
            return;
        }
        string[] aAssetFolder = { "Assets/ScriptableObjects/GameObjects/" };
        System.Type aAssetType = null;
        switch(mActiveType)
        {
            case GameObjectType.Enemy:
                aAssetFolder[0] = aAssetFolder[0] + "Enemy";
                aAssetType = typeof(Enemy);
                break;
            case GameObjectType.Pickable:
                aAssetFolder[0] = aAssetFolder[0] + "Item";
                aAssetType = typeof(Item);
                break;
            case GameObjectType.Projectile:
                aAssetFolder[0] = aAssetFolder[0] + "Projectile";
                aAssetType = typeof(Projectile);
                break;
            case GameObjectType.SpawnFactory:
                aAssetFolder[0] = aAssetFolder[0] + "Spawn Factory";
                aAssetType = typeof(SpawnFactory);
                break;
            case GameObjectType.StaticObject:
                aAssetFolder[0] = aAssetFolder[0] + "Static Object";
                aAssetType = typeof(StaticObject);
                break;
        }

        string[] aAssetGUIDs = AssetDatabase.FindAssets(mNameField.value, aAssetFolder);
        if(aAssetGUIDs.Length > 0)
        {
            foreach(string aAssetGUID in aAssetGUIDs)
            {
                string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUID);
                if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == aAssetType)
                {
                    GameScriptable aTempAsset = (GameScriptable)AssetDatabase.LoadAssetAtPath(aPath, aAssetType);
                    if(aTempAsset.mName == mNameField.value)
                    {
                        mSelectionField.value = aTempAsset;
                        return;
                    }
                }
            }
        }

        SaveCurrentActiveAsset();

        mActiveGameObjectAsset = (GameScriptable)ScriptableObject.CreateInstance(aAssetType);
        mActiveGameObjectAsset.mName = mNameField.value;
        AssetDatabase.CreateAsset(mActiveGameObjectAsset, aAssetFolder[0] + "/" + mNameField.value + ".asset");
        mSelectionField.value = mActiveGameObjectAsset;
    }

    #region Static Objects Callbacks
    void OnColliderTypeChanged(GameScriptable.ColliderType pType)
    {
        if(mActiveGameObjectAsset == null)
        {
            return;
        }
        if (pType == mActiveGameObjectAsset.mColliderType)
        {
            return;
        }
        mActiveGameObjectAsset.mColliderType = pType;
    }

    void OnStaticSpriteSelection(Sprite pNewSprite)
    {
        if(pNewSprite == null)
        {
            return;
        }
        string[] aAssetFolder = { "Assets/ScriptableObjects/Asset Meta Data" };
        if (!AssetDatabase.IsValidFolder(aAssetFolder[0]))
        {
            //launch warning and set sprite selection to null
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pNewSprite.texture.name, aAssetFolder);
        if (aAssetGUIDs.Length > 0)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
            if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(AssetMetaData))
            {
                AssetMetaData aCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
                
            }
            else
            {
                //launch warning and set sprite selection to null
            }
        }
        else
        {
            //launch warning and set sprite selection to null
        }
    }

    #endregion

    void SaveAsScriptableAsset()
    {
        SaveCurrentActiveAsset();
        mTypeEnum.value = GameObjectType.None;
    }


    #region IMGUI OnGUIs
    void ProjectileOnGUI()
    {
        mProjectileGUIScrollPos = EditorGUILayout.BeginScrollView(mProjectileGUIScrollPos,GUILayout.Width(1050),GUILayout.Height(350));
        mProjectileAnimList.DoLayoutList();
        EditorGUILayout.EndScrollView();
    }
    void EnemyOnGUI()
    {
        mEnemyGUIScrollPos = EditorGUILayout.BeginScrollView(mEnemyGUIScrollPos, GUILayout.Width(1050), GUILayout.Height(350));
        mEnemyAnimList.DoLayoutList();
        EditorGUILayout.EndScrollView();
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
