using UnityEditorInternal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class PlayerEditor
{
    static PlayerEditor mInstance;
    VisualElement mPlayerEditorUI = null;
    List<AnimationData> mAnimationData;
    ReorderableList mAnimationList;
    Vector2 mPlayerGUIScrollPos;

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
        mInstance.SetUpFromUXML();
        return mInstance.mPlayerEditorUI;
    }

    void SetUpFromUXML()
    {
        mPlayerEditorUI.Q<ObjectField>("player_projectile").objectType = typeof(Projectile);
        mPlayerEditorUI.Q<ObjectField>("player_spawn_sound").objectType = typeof(AudioClip);
        mPlayerEditorUI.Q<ObjectField>("player_attack_sound").objectType = typeof(AudioClip);
        mPlayerEditorUI.Q<ObjectField>("player_death_sound").objectType = typeof(AudioClip);
        mPlayerEditorUI.Q<ObjectField>("player_teleport_sound").objectType = typeof(AudioClip);
        mAnimationData = new List<AnimationData>();
        mAnimationList = new ReorderableList(mAnimationData, typeof(AnimationData));
        mAnimationList.drawHeaderCallback = (Rect aRect) => {
            EditorGUI.LabelField(aRect, "Move Animation Sprites");
        };
        mAnimationList.drawElementCallback = UpdateAnimList;
        mAnimationList.onAddCallback = AddNewAnimation;
        mPlayerEditorUI.Q<IMGUIContainer>("player_animation_sprites").onGUIHandler = PlayerOnGUI;
        mPlayerEditorUI.Q<Button>("player_asset_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
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
        mInstance.mAnimationData.Add(pData);
    }
    void SaveAsScriptableAsset()
    {
        Debug.Log("Save Scriptable Asset Function Called");
    }
}
