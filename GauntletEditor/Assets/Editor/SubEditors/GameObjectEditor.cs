using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditorInternal;

public class GameObjectEditor : IBindable
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
    EnumField mLayerType;

    #region Static Object Elements
    ObjectField mSObjSprite;
    EnumField mColliderType;
    #endregion

    #region Projectile Elements
    ReorderableList mProjectileAnimList;
    Vector2 mProjectileGUIScrollPos;
    #endregion

    #region Item Elements
    ObjectField mItemSprite;
    ObjectField mItemSound;
    EnumField mItemType;
    #endregion

    #region Spawn Factory Elements
    ObjectField mSpawnEnemy;
    ObjectField mSpawnSound;
    ObjectField mFactorySprite;
    #endregion

    #region Enemy Elements
    ReorderableList mEnemyAnimList;
    Vector2 mEnemyGUIScrollPos;
    ObjectField mEnAttackSound;
    ObjectField mEnDeathSound;
    EnumField mEnemyType;
    ObjectField mEnProjectile;
    #endregion

    public IBinding binding { get; set; }
    public string bindingPath { get; set; }


    static void CreateInstance()
    {
        if(mInstance == null)
        {
            mInstance = new GameObjectEditor();
        }
    }

    public static GameScriptable GetCurrentScriptable()
    {
        return mInstance.mActiveGameObjectAsset;
    }

    public static VisualElement CreateNewGameObjectEditorUI()
    {
        CreateInstance();
        VisualTreeAsset aGameObjectEditorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/GameObjectEditor.uxml");
        mInstance.mGameObjectEditorUI = aGameObjectEditorAsset.CloneTree();
        mInstance.mEditoryBlock = mInstance.mGameObjectEditorUI.Q<VisualElement>("gobj_editor_data");
        mInstance.mTypeEnum = mInstance.mEditoryBlock.Q<EnumField>("gobj_type");
        mInstance.mTypeEnum.Init(GameObjectType.None);
        if(mInstance.mActiveGameObjectAsset != null)
        {
            mInstance.mTypeEnum.value = mInstance.mActiveType;
        }
        mInstance.mTypeEnum.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => mInstance.OnTypeChanged((GameObjectType)aEv.newValue));
        mInstance.mSelectionField = mInstance.mEditoryBlock.Q<ObjectField>("scriptable_gobj_field");
        mInstance.mSelectionField.RegisterCallback<ChangeEvent<Object>>((aEv) => mInstance.OnSelectionChanged(aEv.newValue));
        if(mInstance.mActiveGameObjectAsset == null)
        {
            mInstance.mSelectionField.SetEnabled(false);
        }
        mInstance.mCreateNewButton = mInstance.mEditoryBlock.Q<Button>("gobj_create_new");
        mInstance.mCreateNewButton.RegisterCallback<MouseUpEvent>((aEv) => mInstance.OnCreateNewObject());
        if(mInstance.mActiveGameObjectAsset == null)
        {
            mInstance.mCreateNewButton.SetEnabled(false);
        }
        mInstance.mNameField = mInstance.mEditoryBlock.Q<TextField>("gobj_name");
        if(mInstance.mActiveGameObjectAsset != null)
        {
            mInstance.mSelectionField.value = mInstance.mActiveGameObjectAsset;
            mInstance.OnSelectionChanged(mInstance.mActiveGameObjectAsset);
        }
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
    #region Setup Region
    VisualTreeAsset SetUpPickable()
    {
        VisualTreeAsset aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/PickableEditor.uxml");
        mCurrentObjectElement = aAsset.CloneTree();
        mItemType = mCurrentObjectElement.Q<EnumField>("item_type");
        mItemType.Init(Item.Type.HealthBoost);
        mItemType.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => OnItemTypeChanged((Item.Type)aEv.newValue));
        mItemSound = mCurrentObjectElement.Q<ObjectField>("item_collect_sound");
        mItemSound.objectType = typeof(AudioClip);
        mItemSound.RegisterCallback<ChangeEvent<Object>>((aEv) => GenHelpers.OnCollectSoundSelection((AudioClip)aEv.newValue, mItemSound));
        mItemSprite = mCurrentObjectElement.Q<ObjectField>("item_idle_sprite");
        mItemSprite.objectType = typeof(Sprite);
        mItemSprite.RegisterCallback<ChangeEvent<Object>>((aEv) => GenHelpers.OnSpriteSelection((Sprite)aEv.newValue, mItemSprite));
        return aAsset;
    }

    VisualTreeAsset SetUpProjectile()
    {
        VisualTreeAsset aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/ProjectileEditor.uxml");
        mCurrentObjectElement = aAsset.CloneTree();
        mProjectileAnimList = new ReorderableList(new List<AnimationData>(), typeof(AnimationData));
        mProjectileAnimList.drawHeaderCallback = (Rect aRect) => {
            EditorGUI.LabelField(aRect, "Move Animation Sprites");
        };
        mProjectileAnimList.drawElementCallback = UpdateAnimList;
        mProjectileAnimList.onAddCallback = AddNewAnimation;
        mCurrentObjectElement.Q<IMGUIContainer>("projectile_animation_sprites").onGUIHandler = ProjectileOnGUI;
        return aAsset;
    }

    VisualTreeAsset SetUpSpawnFactory()
    {
        VisualTreeAsset aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/SpawnFactoryEditor.uxml");
        mCurrentObjectElement = aAsset.CloneTree();
        mSpawnEnemy = mCurrentObjectElement.Q<ObjectField>("spawn_enemy");
        mSpawnEnemy.objectType = typeof(Enemy);
        mSpawnEnemy.RegisterCallback<ChangeEvent<Object>>((aEv) => GenHelpers.OnSpawnEnemySelection((Enemy)aEv.newValue, mSpawnEnemy));
        mSpawnSound = mCurrentObjectElement.Q<ObjectField>("spawn_sound");
        mSpawnSound.objectType = typeof(AudioClip);
        mSpawnSound.RegisterCallback<ChangeEvent<Object>>((aEv) => GenHelpers.OnCollectSoundSelection((AudioClip)aEv.newValue, mSpawnSound));
        mFactorySprite = mCurrentObjectElement.Q<ObjectField>("factory_sprite");
        mFactorySprite.objectType = typeof(Sprite);
        mFactorySprite.RegisterCallback<ChangeEvent<Object>>((aEv) => GenHelpers.OnSpriteSelection((Sprite)aEv.newValue, mFactorySprite));
        return aAsset;
    }

    VisualTreeAsset SetUpEnemy()
    {
        VisualTreeAsset aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/EnemyEditor.uxml");
        mCurrentObjectElement = aAsset.CloneTree();
        mEnAttackSound = mCurrentObjectElement.Q<ObjectField>("enemy_attack_sound");
        mEnAttackSound.objectType = typeof(AudioClip);
        mEnAttackSound.RegisterCallback<ChangeEvent<Object>>((aEv) => GenHelpers.OnAttackSoundSelection((AudioClip)aEv.newValue, mEnAttackSound));
        mEnDeathSound = mCurrentObjectElement.Q<ObjectField>("enemy_death_sound");
        mEnDeathSound.objectType = typeof(AudioClip);
        mEnDeathSound.RegisterCallback<ChangeEvent<Object>>((aEv) => GenHelpers.OnDeathSoundSelection((AudioClip)aEv.newValue, mEnDeathSound));
        mEnemyType = mCurrentObjectElement.Q<EnumField>("enemy_type");
        mEnemyType.Init(Enemy.Type.Collider);
        mEnemyType.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => OnEnemyTypeChanged((Enemy.Type)aEv.newValue));
        mEnProjectile = mCurrentObjectElement.Q<ObjectField>("enemy_projectile");
        mEnProjectile.objectType = typeof(Projectile);
        mEnProjectile.RegisterCallback<ChangeEvent<Object>>((aEv) => GenHelpers.OnEnemyProjectileSelection((Projectile)aEv.newValue, mEnProjectile));
        mEnemyAnimList = new ReorderableList(new List<AnimationData>(), typeof(AnimationData));
        mEnemyAnimList.drawHeaderCallback = (Rect aRect) =>
        {
            EditorGUI.LabelField(aRect, "Move Animation Sprites");
        };
        mEnemyAnimList.drawElementCallback = UpdateEnemyAnimationList;
        mEnemyAnimList.onAddCallback = AddNewAnimation;
        mCurrentObjectElement.Q<IMGUIContainer>("enemy_animation_sprites").onGUIHandler = EnemyOnGUI;
        return aAsset;
    }

    VisualTreeAsset SetUpStaticObject()
    {
        VisualTreeAsset aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/StaticObjectEditor.uxml");
        mCurrentObjectElement = aAsset.CloneTree();
        mSObjSprite = mCurrentObjectElement.Q<ObjectField>("static_object_sprite");
        mSObjSprite.objectType = typeof(Sprite);
        mSObjSprite.RegisterCallback<ChangeEvent<Object>>((aEv) => GenHelpers.OnSpriteSelection((Sprite)aEv.newValue, mSObjSprite));
        mColliderType = mCurrentObjectElement.Q<EnumField>("static_object_collider");
        mColliderType.Init(GameScriptable.ColliderType.None);
        mColliderType.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => OnColliderTypeChanged((GameScriptable.ColliderType)aEv.newValue));
        return aAsset;
    }
    void CreateNewObjectVE()
    {
        VisualTreeAsset aAsset;
        switch (mActiveType)
        {
            case GameObjectType.None:
                return;
            case GameObjectType.Pickable:
                aAsset = SetUpPickable();
                break;
            case GameObjectType.Projectile:
                aAsset = SetUpProjectile();
                break;
            case GameObjectType.SpawnFactory:
                aAsset = SetUpSpawnFactory();
                break;
            case GameObjectType.Enemy:
                aAsset = SetUpEnemy();
                break;
            case GameObjectType.StaticObject:
                aAsset = SetUpStaticObject();
                break;
            default:
                return;
        }
        if(aAsset == null)
        {
            return;
        }
        mLayerType = mCurrentObjectElement.Q<EnumField>("gobj_layer");
        mLayerType.Init(Level.LayerTypes.Environment);
        mLayerType.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => OnLayerChanged((Level.LayerTypes) aEv.newValue));
        mCurrentObjectElement.Q<Button>("gobj_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
        mEditoryBlock.Add(mCurrentObjectElement);
    }
    #endregion

    #region Selection Changed Setup
    void SelectEnemy(Enemy pEnemy)
    {
        mLayerType.value = pEnemy.mRenderLayer;
        mEnemyType.value = pEnemy.mEnemyType;
        GenHelpers.ResetDeathSoundSelection(mEnDeathSound);
        GenHelpers.ResetAttackSoundSelection(mEnAttackSound);
        GenHelpers.ResetProjectileSelection(mEnProjectile);
        if (pEnemy.mEnemyAnimations == null)
        {
            pEnemy.mEnemyAnimations = new List<AnimationData>();
        }
        mEnemyAnimList.list = pEnemy.mEnemyAnimations;
        mEditoryBlock.Bind(new SerializedObject(pEnemy));
    }
    void SelectPickable(Item pItem)
    {
        GenHelpers.ResetSpriteSelection(mItemSprite);
        GenHelpers.ResetCollectSelection(mItemSound);
        mLayerType.value = pItem.mRenderLayer;
        mItemType.value = pItem.mItemType;
        mEditoryBlock.Bind(new SerializedObject(pItem));
    }
    void SelectProjectile(Projectile pProj)
    {
        mLayerType.value = pProj.mRenderLayer;
        if (pProj.mProjectileAnimation == null)
        {
            pProj.mProjectileAnimation = new List<AnimationData>();
        }
        mProjectileAnimList.list = pProj.mProjectileAnimation;
        mEditoryBlock.Bind(new SerializedObject(pProj));
    }
    void SelectStaticObject(StaticObject pObject)
    {
        GenHelpers.ResetSpriteSelection(mSObjSprite);
        mLayerType.value = pObject.mRenderLayer;
        mColliderType.value = pObject.mColliderType;
        mEditoryBlock.Bind(new SerializedObject(pObject));
    }
    void SelectSpawnFactory(SpawnFactory pSpawnFactory)
    {
        mLayerType.value = pSpawnFactory.mRenderLayer;
        GenHelpers.ResetSpawnEnemySelection(mSpawnEnemy);
        GenHelpers.ResetSpriteSelection(mFactorySprite);
        GenHelpers.ResetCollectSelection(mSpawnSound);
        mEditoryBlock.Bind(new SerializedObject(pSpawnFactory));
    }
    void OnSelectionChanged(Object pSelectedObject)
    {
        SaveCurrentActiveAsset();
        RemoveCurrentObjectVE();
        if (pSelectedObject == null)
        {
            return;
        }
        CreateNewObjectVE();
        mActiveGameObjectAsset = (GameScriptable)pSelectedObject;
        switch(mActiveType)
        {
            case GameObjectType.Enemy:
                SelectEnemy((Enemy)mActiveGameObjectAsset);
                break;
            case GameObjectType.Pickable:
                SelectPickable((Item)mActiveGameObjectAsset);
                break;
            case GameObjectType.Projectile:
                SelectProjectile((Projectile)mActiveGameObjectAsset);
                break;
            case GameObjectType.SpawnFactory:
                SelectSpawnFactory((SpawnFactory)mActiveGameObjectAsset);
                break;
            case GameObjectType.StaticObject:
                SelectStaticObject((StaticObject)mActiveGameObjectAsset);
                break;
        }
        EditorUtility.SetDirty(mActiveGameObjectAsset);
    }
    #endregion
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
        mActiveGameObjectAsset.Init();
        mActiveGameObjectAsset.mName = mNameField.value;
        AssetDatabase.CreateAsset(mActiveGameObjectAsset, aAssetFolder[0] + "/" + mNameField.value + ".asset");
        mSelectionField.value = mActiveGameObjectAsset;
    }

    #region Type Change Callbacks
    void OnTypeChanged(GameObjectType pNewType)
    {
        SaveCurrentActiveAsset();
        RemoveCurrentObjectVE();
        mActiveType = pNewType;
        SetObjectFieldType();
    }
    void OnLayerChanged(Level.LayerTypes pType)
    {
        if (mActiveGameObjectAsset == null)
        {
            return;
        }
        if (pType == mActiveGameObjectAsset.mRenderLayer)
        {
            return;
        }
        mActiveGameObjectAsset.mRenderLayer = pType;
    }
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
    void OnItemTypeChanged(Item.Type pType)
    {
        if (mActiveGameObjectAsset == null)
        {
            return;
        }
        Item aItem = (Item)mActiveGameObjectAsset;
        if(aItem == null)
        {
            return;
        }
        if (pType == aItem.mItemType)
        {
            return;
        }
        aItem.mItemType = pType;
    }
    void OnEnemyTypeChanged(Enemy.Type pType)
    {
        if (mActiveGameObjectAsset == null)
        {
            return;
        }
        Enemy aEnemy = (Enemy)mActiveGameObjectAsset;
        if (aEnemy == null)
        {
            return;
        }
        if (pType == aEnemy.mEnemyType)
        {
            return;
        }
        aEnemy.mEnemyType = pType;

    }
    #endregion

    void SaveCurrentActiveAsset()
    {
        if (mActiveGameObjectAsset != null)
        {
            EditorUtility.SetDirty(mActiveGameObjectAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            mEditoryBlock.Unbind();
            mActiveGameObjectAsset = null;
            mNameField.value = "";
        }
    }

    bool IsDataValid()
    {
        if(mActiveGameObjectAsset == null)
        {
            return true;
        }



        return true;
    }

    void SaveAsScriptableAsset()
    {
        if (IsDataValid())
        {
            SaveCurrentActiveAsset();
            mTypeEnum.value = GameObjectType.None;
        }
        else
        {
            if(!EditorUtility.DisplayDialog("Some Required Values Are Missing",mActiveType.ToString() + " requires a few fields that you haven't filled. Please fill them before saving, or delete the asset.","Keep Editing","Delete Asset"))
            {
                //delete asset
            }
        }
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
                Enemy aEnemy = (Enemy)mInstance.mActiveGameObjectAsset;
                aEnemy.mEnemyAnimations.Add(pData);
                mInstance.mActiveGameObjectAsset.mDisplaySprite = pData.mSprites[0];
                EditorUtility.SetDirty(aEnemy);
                break;
            case GameObjectType.Projectile:
                Projectile aProjectile = (Projectile)mInstance.mActiveGameObjectAsset;
                aProjectile.mProjectileAnimation.Add(pData);
                mInstance.mActiveGameObjectAsset.mDisplaySprite = pData.mSprites[0];
                EditorUtility.SetDirty(aProjectile);
                break;
        }
    }

    #endregion

}
