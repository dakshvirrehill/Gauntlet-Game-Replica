using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public struct AnimationData
{
    public string mAnimationName;
    public List<Sprite> mSprites;
}

public class AddAnimationWindow : EditorWindow
{
    static AddAnimationWindow mWindow;
    ReorderableList mAnimations;
    AnimationData mAnimationData;
    int mObjectPickerId = -1;
    static bool mIsPlayer;
    Vector2 mAnimationScroll;
    public static void OpenAnimationWindow(bool pIsPlayer = false)
    {
        if (mWindow != null)
        {
            mWindow.Close();
            mWindow = null;
        }
        mWindow = GetWindow<AddAnimationWindow>();
        mWindow.minSize = new Vector2(500, 500);
        mWindow.titleContent = new GUIContent("Add New Animation");
        mIsPlayer = pIsPlayer;
        mWindow.mAnimationData.mSprites = new List<Sprite>();
        mWindow.mAnimations = new ReorderableList(mWindow.mAnimationData.mSprites, typeof(Sprite));
        mWindow.mAnimations.drawHeaderCallback = (Rect aRect) => {
            EditorGUI.LabelField(aRect, "Animation Sprites");
        };
        mWindow.mAnimations.drawElementCallback = mWindow.UpdateAnimationList;
        mWindow.mAnimations.onAddCallback = mWindow.AddNewAnimation;
    }

    void OnGUI()
    {
        mAnimationScroll = EditorGUILayout.BeginScrollView(mAnimationScroll, GUILayout.Width(500), GUILayout.Height(450));
        EditorGUILayout.BeginHorizontal();
        mAnimationData.mAnimationName = EditorGUILayout.TextField("Name: ", mAnimationData.mAnimationName);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginVertical();
        if (Event.current.commandName == "ObjectSelectorUpdated" && mObjectPickerId == EditorGUIUtility.GetObjectPickerControlID())
        {
            Sprite a = (Sprite)EditorGUIUtility.GetObjectPickerObject();
            mObjectPickerId = -1;
            if (a != null)
            {
                mAnimationData.mSprites.Add(a);
            }
        }
        mAnimations.DoLayoutList();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Save Animation"))
        {
            if(!mIsPlayer)
            {
                GameObjectEditor.AddToCurrentAnimationList(mAnimationData);
                mWindow.Close();
            }
            else
            {
                PlayerEditor.AddToPlayerAnimation(mAnimationData);
                mWindow.Close();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    void UpdateAnimationList(Rect aRect, int aIx, bool aIsActive, bool aIsFocused)
    {
        Sprite aElement = (Sprite)mAnimations.list[aIx];
        aRect.y += 2;
        EditorGUI.LabelField(new Rect(aRect.x, aRect.y, 100, EditorGUIUtility.singleLineHeight), aElement.name);
    }

    void AddNewAnimation(ReorderableList pList)
    {
        mObjectPickerId = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
        EditorGUIUtility.ShowObjectPicker<Sprite>(null, false, "", mObjectPickerId);
    }

    public static bool IsWindowOpen()
    {
        return mWindow != null;
    }
}
