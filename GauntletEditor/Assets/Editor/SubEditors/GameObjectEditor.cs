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

    #region Enemy Elements
    ReorderableList mEnemyAnimList;
    List<AnimationData> mEnemyAnimations;
    Vector2 mEnemyGUIScrollPos;
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
                break;
            case GameObjectType.Projectile:
                aAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/ProjectileEditor.uxml");
                mCurrentObjectElement = aAsset.CloneTree();
                
                mProjectileAnimList = new ReorderableList(new List<AnimationData>(), typeof(AnimationData));
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
        mLayerType = mCurrentObjectElement.Q<EnumField>("gobj_layer");
        mLayerType.Init(Level.LayerTypes.Environment);
        mLayerType.RegisterCallback<ChangeEvent<System.Enum>>((aEv) => OnLayerChanged((Level.LayerTypes) aEv.newValue));
        mCurrentObjectElement.Q<Button>("gobj_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
        mEditoryBlock.Add(mCurrentObjectElement);
    }

    void SaveCurrentActiveAsset()
    {
        if (mActiveGameObjectAsset != null)
        {
            EditorUtility.SetDirty(mActiveGameObjectAsset);
            AssetDatabase.SaveAssets();
            mActiveGameObjectAsset = null;
            mNameField.value = "";
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
                mLayerType.value = aProjectile.mRenderLayer;
                if (aProjectile.mProjectileAnimation == null)
                {
                    aProjectile.mProjectileAnimation = new List<AnimationData>();
                }
                mProjectileAnimList.list = aProjectile.mProjectileAnimation;
                mCurrentObjectElement.Bind(new SerializedObject(aProjectile));
                break;
            case GameObjectType.SpawnFactory:
                SpawnFactory aFactory = (SpawnFactory)pSelectedObject;
                //do spawn factory specific things
                mCurrentObjectElement.Bind(new SerializedObject(aFactory));
                break;
            case GameObjectType.StaticObject:
                StaticObject aStObj = (StaticObject)pSelectedObject;
                ResetSpriteSelection(aStObj);
                mLayerType.value = aStObj.mRenderLayer;
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
        mActiveGameObjectAsset.Init();
        mActiveGameObjectAsset.mName = mNameField.value;
        AssetDatabase.CreateAsset(mActiveGameObjectAsset, aAssetFolder[0] + "/" + mNameField.value + ".asset");
        mSelectionField.value = mActiveGameObjectAsset;
    }

    void OnLayerChanged(Level.LayerTypes pType)
    {
        if(mActiveGameObjectAsset == null)
        {
            return;
        }
        if(pType == mActiveGameObjectAsset.mRenderLayer)
        {
            return;
        }
        mActiveGameObjectAsset.mRenderLayer = pType;
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
            ResetSpriteSelectionWithWarning();
            return;
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pNewSprite.texture.name, aAssetFolder);
        if (aAssetGUIDs.Length > 0)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
            if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(AssetMetaData))
            {
                AssetMetaData aCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
                if(aCurrentAssetData.mType == AssetMetaData.AssetType.TextureAsset)
                {
                    StaticObject aTempObj = (StaticObject)mActiveGameObjectAsset;
                    aTempObj.mTextureGUID = aCurrentAssetData.mGUID;
                    aTempObj.mDimensions = new Rect(pNewSprite.rect.x, (pNewSprite.texture.height - pNewSprite.rect.y), pNewSprite.rect.width, pNewSprite.rect.height);
                    aTempObj.mDisplaySprite = pNewSprite;
                    Object[] aAllSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(pNewSprite.texture));
                    for(int aI = 0; aI < aAllSprites.Length; aI ++)
                    {
                        if(pNewSprite.GetInstanceID() == aAllSprites[aI].GetInstanceID())
                        {
                            aTempObj.mSpriteIndex = aI;
                            break;
                        }
                    }
                }
                else
                {
                    ResetSpriteSelectionWithWarning();
                    return;
                }
            }
            else
            {
                ResetSpriteSelectionWithWarning();
                return;
            }
        }
        else
        {
            ResetSpriteSelectionWithWarning();
            return;
        }
    }

    void ResetSpriteSelectionWithWarning()
    {
        EditorUtility.DisplayDialog("Warning Sprite Asset GUID Not Set", "Use the Asset Meta Data Editor to Generate Game Metas before assigning the sprite to a Game Object", "Okay");
        ResetSpriteSelection((StaticObject)mActiveGameObjectAsset);
    }

    void ResetSpriteSelection(StaticObject pStObj)
    {
        if (!string.IsNullOrEmpty(pStObj.mTextureGUID))
        {
            Object[] aAllSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(pStObj.mTextureGUID));
            mSObjSprite.SetEnabled(false);
            mSObjSprite.value = aAllSprites[pStObj.mSpriteIndex];
            mSObjSprite.SetEnabled(true);
        }
        else
        {
            mSObjSprite.SetEnabled(false);
            mSObjSprite.value = null;
            mSObjSprite.SetEnabled(true);
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
                EditorUtility.SetDirty(mInstance.mActiveGameObjectAsset);
                AssetDatabase.SaveAssets();
                break;
            case GameObjectType.Projectile:
                ((Projectile)mInstance.mActiveGameObjectAsset).mProjectileAnimation.Add(pData);
                mInstance.mActiveGameObjectAsset.mDisplaySprite = pData.mSprites[0];
                EditorUtility.SetDirty(mInstance.mActiveGameObjectAsset);
                AssetDatabase.SaveAssets();
                break;
        }
    }

    #endregion

}
