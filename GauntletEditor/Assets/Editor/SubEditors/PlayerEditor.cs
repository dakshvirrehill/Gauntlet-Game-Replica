using UnityEditorInternal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class PlayerEditor : IBindable
{
    static PlayerEditor mInstance;
    VisualElement mPlayerEditorUI = null;
    ReorderableList mAnimationList;
    Vector2 mPlayerGUIScrollPos;
    Player mActivePlayer;

    ObjectField mPlayerProjectile;
    ObjectField mSpawnSound;
    ObjectField mAttackSound;
    ObjectField mDeathSound;
    ObjectField mTeleportSound;

    public IBinding binding { get; set; }
    public string bindingPath { get; set; }


    static void CreateInstance()
    {
        if(mInstance == null)
        {
            mInstance = new PlayerEditor();
        }
    }

    public static Player GetActivePlayer()
    {
        return mInstance.mActivePlayer;
    }

    public static VisualElement CreateNewPlayerEditorUI()
    {
        CreateInstance();
        VisualTreeAsset aPlayerEditorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/PlayerEditor.uxml");
        mInstance.mPlayerEditorUI = aPlayerEditorAsset.CloneTree();
        mInstance.SetActivePlayer();
        mInstance.SetUpFromUXML();
        return mInstance.mPlayerEditorUI;
    }

    void SetActivePlayer()
    {
        if(mActivePlayer == null)
        {
            string[] aPlayerGUIDs = AssetDatabase.FindAssets("MainPlayer", new[] { "Assets/ScriptableObjects/Player" });
            if(aPlayerGUIDs.Length > 0)
            {
                for(int aI = 1; aI < aPlayerGUIDs.Length; aI ++)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(aPlayerGUIDs[aI]));
                }
                mActivePlayer = AssetDatabase.LoadAssetAtPath<Player>(AssetDatabase.GUIDToAssetPath(aPlayerGUIDs[0]));
            }
            if(mActivePlayer == null)
            {
                mActivePlayer = ScriptableObject.CreateInstance<Player>();
                mActivePlayer.Init();
                AssetDatabase.CreateAsset(mActivePlayer, "Assets/ScriptableObjects/Player/MainPlayer.asset");
                EditorUtility.SetDirty(mActivePlayer);
            }
        }
    }

    void SetUpFromUXML()
    {
        mPlayerProjectile = mPlayerEditorUI.Q<ObjectField>("player_projectile");
        mPlayerProjectile.objectType = typeof(Projectile);
        mPlayerProjectile.value = mActivePlayer.mProjectile;
        mPlayerProjectile.RegisterCallback<ChangeEvent<Object>>((aEv) => OnProjectileSelection((Projectile)aEv.newValue));
        mSpawnSound = mPlayerEditorUI.Q<ObjectField>("player_spawn_sound");
        mSpawnSound.objectType = typeof(AudioClip);
        mSpawnSound.value = mActivePlayer.mSpawnSound;
        mSpawnSound.RegisterCallback<ChangeEvent<Object>>((aEv) => OnSpawnSoundSelection((AudioClip)aEv.newValue));
        mAttackSound = mPlayerEditorUI.Q<ObjectField>("player_attack_sound");
        mAttackSound.objectType = typeof(AudioClip);
        mAttackSound.value = mActivePlayer.mAttackSound;
        mAttackSound.RegisterCallback<ChangeEvent<Object>>((aEv) => OnAttackSoundSelection((AudioClip)aEv.newValue));
        mDeathSound = mPlayerEditorUI.Q<ObjectField>("player_death_sound");
        mDeathSound.objectType = typeof(AudioClip);
        mDeathSound.value = mActivePlayer.mDeathSound;
        mDeathSound.RegisterCallback<ChangeEvent<Object>>((aEv) => OnDeathSoundSelection((AudioClip)aEv.newValue));
        mTeleportSound = mPlayerEditorUI.Q<ObjectField>("player_teleport_sound");
        mTeleportSound.objectType = typeof(AudioClip);
        mTeleportSound.RegisterCallback<ChangeEvent<Object>>((aEv) => OnTeleportSoundSelection((AudioClip)aEv.newValue));
        mAnimationList = new ReorderableList(mActivePlayer.mAnimationData, typeof(AnimationData));
        mAnimationList.drawHeaderCallback = (Rect aRect) => {
            EditorGUI.LabelField(aRect, "Move Animation Sprites");
        };
        mAnimationList.drawElementCallback = UpdateAnimList;
        mAnimationList.onAddCallback = AddNewAnimation;
        mPlayerEditorUI.Q<IMGUIContainer>("player_animation_sprites").onGUIHandler = PlayerOnGUI;
        mPlayerEditorUI.Q<Button>("player_asset_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
        mPlayerEditorUI.Bind(new SerializedObject(mActivePlayer));
    }

    void PlayerOnGUI()
    {
        mPlayerGUIScrollPos = EditorGUILayout.BeginScrollView(mPlayerGUIScrollPos, GUILayout.Width(1050), GUILayout.Height(350));
        mAnimationList.DoLayoutList();
        EditorGUILayout.EndScrollView();
    }

    void UpdateAnimList(Rect aRect, int aIx, bool aIsActive, bool aIsFocused)
    {
        var aElement = (AnimationData)mAnimationList.list[aIx];
        aRect.y += 2;
        EditorGUI.LabelField(new Rect(aRect.x, aRect.y, 100, EditorGUIUtility.singleLineHeight), aElement.mAnimationName);
    }

    void AddNewAnimation(ReorderableList pList)
    {
        if (!AddAnimationWindow.IsWindowOpen())
        {
            AddAnimationWindow.OpenAnimationWindow(true);
        }
    }

    public static void AddToPlayerAnimation(AnimationData pData)
    {
        mInstance.mActivePlayer.mAnimationData.Add(pData);
        if(mInstance.mActivePlayer.mDisplaySprite == null)
        {
            mInstance.mActivePlayer.mDisplaySprite = pData.mSprites[0];
        }
        EditorUtility.SetDirty(mInstance.mActivePlayer);
    }
    void SaveAsScriptableAsset()
    {
        if(IsDataValid())
        {
            EditorUtility.SetDirty(mInstance.mActivePlayer);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
            EditorUtility.DisplayDialog("Cannot Save Data", "Some Important Player Components are Not Set. Set them to save data.", "Okay");
        }
    }


    #region Helpers
    bool IsDataValid()
    {
        return mActivePlayer != null && !string.IsNullOrEmpty(mActivePlayer.mName)
            && mActivePlayer.mColliderType == GameScriptable.ColliderType.Circle
            && mActivePlayer.mRenderLayer == Level.LayerTypes.Players
            && mActivePlayer.mAnimationData.Count >= 1;
    }

    void OnProjectileSelection(Projectile pProjectile)
    {
        if (pProjectile == null)
        {
            return;
        }
        string[] aAssetFolder = { "Assets/ScriptableObjects/Asset Meta Data" };
        if (!AssetDatabase.IsValidFolder(aAssetFolder[0]))
        {
            GenHelpers.ShowSelectionWarning();
            ResetProjectileSelection();
            return;
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pProjectile.name, aAssetFolder);
        if (aAssetGUIDs.Length > 0)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
            if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(AssetMetaData))
            {
                AssetMetaData aCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
                if (aCurrentAssetData.mType == AssetMetaData.AssetType.PrefabAsset)
                {
                    mActivePlayer.mGUIDProjectile = aCurrentAssetData.mGUID;
                    mActivePlayer.mProjectile = pProjectile;
                }
                else
                {
                    GenHelpers.ShowSelectionWarning();
                    ResetProjectileSelection();
                    return;
                }
            }
            else
            {
                GenHelpers.ShowSelectionWarning();
                ResetProjectileSelection();
                return;
            }
        }
        else
        {
            GenHelpers.ShowSelectionWarning();
            ResetProjectileSelection();
            return;
        }

    }
    void ResetProjectileSelection()
    {
        if (!string.IsNullOrEmpty(mActivePlayer.mGUIDProjectile))
        {
            mPlayerProjectile.SetEnabled(false);
            mPlayerProjectile.value = mActivePlayer.mProjectile;
            mPlayerProjectile.SetEnabled(true);
        }
    }
    void OnSpawnSoundSelection(AudioClip pSpawnSound)
    {
        if (pSpawnSound == null)
        {
            return;
        }
        string[] aAssetFolder = { "Assets/ScriptableObjects/Asset Meta Data" };
        if (!AssetDatabase.IsValidFolder(aAssetFolder[0]))
        {
            GenHelpers.ShowSelectionWarning();
            ResetSpawnSelection();
            return;
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pSpawnSound.name, aAssetFolder);
        if (aAssetGUIDs.Length > 0)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
            if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(AssetMetaData))
            {
                AssetMetaData aCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
                if (aCurrentAssetData.mType == AssetMetaData.AssetType.AudioAsset)
                {
                    mActivePlayer.mGUIDSpawnSound = aCurrentAssetData.mGUID;
                    mActivePlayer.mSpawnSound = pSpawnSound;
                }
                else
                {
                    GenHelpers.ShowSelectionWarning();
                    ResetSpawnSelection();
                    return;
                }
            }
            else
            {
                GenHelpers.ShowSelectionWarning();
                ResetSpawnSelection();
                return;
            }
        }
        else
        {
            GenHelpers.ShowSelectionWarning();
            ResetSpawnSelection();
            return;
        }

    }
    void ResetSpawnSelection()
    {
        if (!string.IsNullOrEmpty(mActivePlayer.mGUIDSpawnSound))
        {
            mSpawnSound.SetEnabled(false);
            mSpawnSound.value = mActivePlayer.mSpawnSound;
            mSpawnSound.SetEnabled(true);
        }
    }
    void OnAttackSoundSelection(AudioClip pAttackSound)
    {
        if (pAttackSound == null)
        {
            return;
        }
        string[] aAssetFolder = { "Assets/ScriptableObjects/Asset Meta Data" };
        if (!AssetDatabase.IsValidFolder(aAssetFolder[0]))
        {
            GenHelpers.ShowSelectionWarning();
            ResetAttackSelection();
            return;
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pAttackSound.name, aAssetFolder);
        if (aAssetGUIDs.Length > 0)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
            if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(AssetMetaData))
            {
                AssetMetaData aCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
                if (aCurrentAssetData.mType == AssetMetaData.AssetType.AudioAsset)
                {
                    mActivePlayer.mGUIDAttackSound = aCurrentAssetData.mGUID;
                    mActivePlayer.mAttackSound = pAttackSound;
                }
                else
                {
                    GenHelpers.ShowSelectionWarning();
                    ResetAttackSelection();
                    return;
                }
            }
            else
            {
                GenHelpers.ShowSelectionWarning();
                ResetAttackSelection();
                return;
            }
        }
        else
        {
            GenHelpers.ShowSelectionWarning();
            ResetAttackSelection();
            return;
        }

    }
    void ResetAttackSelection()
    {
        if (!string.IsNullOrEmpty(mActivePlayer.mGUIDAttackSound))
        {
            mAttackSound.SetEnabled(false);
            mAttackSound.value = mActivePlayer.mAttackSound;
            mAttackSound.SetEnabled(true);
        }
    }
    void OnDeathSoundSelection(AudioClip pDeathSound)
    {
        if (pDeathSound == null)
        {
            return;
        }
        string[] aAssetFolder = { "Assets/ScriptableObjects/Asset Meta Data" };
        if (!AssetDatabase.IsValidFolder(aAssetFolder[0]))
        {
            GenHelpers.ShowSelectionWarning();
            ResetDeathSelection();
            return;
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pDeathSound.name, aAssetFolder);
        if (aAssetGUIDs.Length > 0)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
            if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(AssetMetaData))
            {
                AssetMetaData aCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
                if (aCurrentAssetData.mType == AssetMetaData.AssetType.AudioAsset)
                {
                    mActivePlayer.mGUIDDeathSound = aCurrentAssetData.mGUID;
                    mActivePlayer.mDeathSound = pDeathSound;
                }
                else
                {
                    GenHelpers.ShowSelectionWarning();
                    ResetDeathSelection();
                    return;
                }
            }
            else
            {
                GenHelpers.ShowSelectionWarning();
                ResetDeathSelection();
                return;
            }
        }
        else
        {
            GenHelpers.ShowSelectionWarning();
            ResetDeathSelection();
            return;
        }

    }
    void ResetDeathSelection()
    {
        if (!string.IsNullOrEmpty(mActivePlayer.mGUIDDeathSound))
        {
            mDeathSound.SetEnabled(false);
            mDeathSound.value = mActivePlayer.mDeathSound;
            mDeathSound.SetEnabled(true);
        }
    }
    void OnTeleportSoundSelection(AudioClip pTeleportSound)
    {
        if (pTeleportSound == null)
        {
            return;
        }
        string[] aAssetFolder = { "Assets/ScriptableObjects/Asset Meta Data" };
        if (!AssetDatabase.IsValidFolder(aAssetFolder[0]))
        {
            GenHelpers.ShowSelectionWarning();
            ResetTeleportSelection();
            return;
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pTeleportSound.name, aAssetFolder);
        if (aAssetGUIDs.Length > 0)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
            if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(AssetMetaData))
            {
                AssetMetaData aCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
                if (aCurrentAssetData.mType == AssetMetaData.AssetType.AudioAsset)
                {
                    mActivePlayer.mGUIDTeleportSound = aCurrentAssetData.mGUID;
                    mActivePlayer.mTeleportSound = pTeleportSound;
                }
                else
                {
                    GenHelpers.ShowSelectionWarning();
                    ResetTeleportSelection();
                    return;
                }
            }
            else
            {
                GenHelpers.ShowSelectionWarning();
                ResetTeleportSelection();
                return;
            }
        }
        else
        {
            GenHelpers.ShowSelectionWarning();
            ResetTeleportSelection();
            return;
        }

    }
    void ResetTeleportSelection()
    {
        if (!string.IsNullOrEmpty(mActivePlayer.mGUIDTeleportSound))
        {
            mTeleportSound.SetEnabled(false);
            mTeleportSound.value = mActivePlayer.mTeleportSound;
            mTeleportSound.SetEnabled(true);
        }
    }

    #endregion


}
