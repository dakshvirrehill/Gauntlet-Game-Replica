using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


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
        mWindow.mAnimationData.mSprites = new SpriteList();
        mWindow.mAnimations = new ReorderableList(mWindow.mAnimationData.mSprites, typeof(Sprite));
        mWindow.mAnimations.drawHeaderCallback = (Rect aRect) => {
            EditorGUI.LabelField(aRect, "Animation Sprites");
        };
        mWindow.mAnimations.drawElementCallback = mWindow.UpdateAnimationList;
        mWindow.mAnimations.onAddCallback = mWindow.AddNewAnimation;
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        mAnimationData.mAnimationName = EditorGUILayout.TextField("Name: ", mAnimationData.mAnimationName);
        mAnimationData.mAnimSpeed = EditorGUILayout.Slider("Animation Speed: ", mAnimationData.mAnimSpeed, 0, 50);
        mAnimationScroll = EditorGUILayout.BeginScrollView(mAnimationScroll, GUILayout.Width(500), GUILayout.Height(450));
        if (Event.current.commandName == "ObjectSelectorUpdated" && mObjectPickerId == EditorGUIUtility.GetObjectPickerControlID())
        {
            Sprite a = (Sprite)EditorGUIUtility.GetObjectPickerObject();
            mObjectPickerId = -1;
            if (a != null)
            {
                if(mAnimationData.mSprites.Count > 0)
                {
                    if(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a.texture)) != mAnimationData.mTextureAssetGUID)
                    {
                        EditorUtility.DisplayDialog("Cannot Add Sprite", "Sprite Not Part of The Same Texture Asset", "Okay");
                    }
                    else
                    {
                        mAnimationData.mSprites.Add(a);
                        mWindow.Repaint();
                    }
                }
                else
                {
                    mAnimationData.mTextureAssetGUID = DoesAssetExists(a);
                    if (mAnimationData.mTextureAssetGUID != null)
                    {
                        mAnimationData.mSprites.Add(a);
                        mWindow.Repaint();
                    }
                }
            }
        }
        mAnimations.DoLayoutList();
        EditorGUILayout.EndScrollView();
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
        EditorGUILayout.EndVertical();
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

    string DoesAssetExists(Sprite pNewSprite)
    {
        string[] aAssetFolder = { "Assets/ScriptableObjects/Asset Meta Data" };
        if (!AssetDatabase.IsValidFolder(aAssetFolder[0]))
        {
            return null;
        }
        string[] aAssetGUIDs = AssetDatabase.FindAssets(pNewSprite.texture.name, aAssetFolder);
        if (aAssetGUIDs.Length <= 0)
        {
            return null;
        }
        string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUIDs[0]);
        if (AssetDatabase.GetMainAssetTypeAtPath(aPath) != typeof(AssetMetaData))
        {
            return null;
        }

        AssetMetaData aCurrentAssetData = (AssetMetaData)AssetDatabase.LoadAssetAtPath(aPath, typeof(AssetMetaData));
        if(aCurrentAssetData.mType != AssetMetaData.AssetType.TextureAsset)
        {
            return null;
        }

        return aCurrentAssetData.mGUID;
    }

    public static bool IsWindowOpen()
    {
        return mWindow != null;
    }
}
