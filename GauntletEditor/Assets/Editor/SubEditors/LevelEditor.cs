using UnityEditorInternal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class LevelEditor : IBindable
{
    static LevelEditor mInstance;
    VisualElement mLevelEditorUI = null;

    List<Level> mAllLevels;
    ReorderableList mLevelList;

    Vector2 mLevelListGUIScrollPos;
    Vector2 mLevelMapGUIScrollPos;
    Vector2 mLevelMapScrollerSize = new Vector2(900, 350);
    Vector2 mPlaceableObjectsGUIScrollPos;
    Vector2 mPlaceableObjectsScrollerSize = new Vector2(150, 350);
    int mMaxCellSize = 100;
    int mCellSize = 64;
    int mMinCellSize = 16;
    Level mActiveLevel;
    Level.LayerTypes mActiveLayer;
    bool mShowAll;
    VisualElement mLevelData;
    VisualElement mEditorMain;
    IMGUIContainer mMapContainer;
    Dictionary<Level.LayerTypes, List<GameScriptable>> mBrushes;
    int mActiveBrushesCount = 0;
    int mActiveBrushId = -1;
    static void CreateInstance()
    {
        if(mInstance == null)
        {
            mInstance = new LevelEditor();
        }
    }

    public static VisualElement CreateNewLevelEditorUI()
    {
        CreateInstance();
        VisualTreeAsset aLevelEditorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/LevelEditor.uxml");
        mInstance.mLevelEditorUI = aLevelEditorAsset.CloneTree();
        mInstance.mEditorMain = mInstance.mLevelEditorUI.Q<VisualElement>("editor_data");
        mInstance.CreateReorderableList();
        return mInstance.mLevelEditorUI;
    }

    void CreateReorderableList()
    {
        mAllLevels = new List<Level>();
        string[] aCurLevels = AssetDatabase.FindAssets("Level", new[] { "Assets/ScriptableObjects/Level Data" });
        foreach(string aCurLevelGUID in aCurLevels)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aCurLevelGUID);
            if(AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(Level))
            {
                mAllLevels.Add(AssetDatabase.LoadAssetAtPath<Level>(aPath));
            }
        }
        mLevelList = new ReorderableList(mAllLevels, typeof(Level));
        mLevelList.drawHeaderCallback = (Rect aRect) => {
            EditorGUI.LabelField(aRect, "Current Levels In Order");
        };
        mLevelList.drawElementCallback = UpdateLevelList;
        mLevelList.onAddCallback = AddNewLevel;
        mLevelList.onCanRemoveCallback = (ReorderableList pList) =>
        {
            return pList.count > 1;
        };
        mLevelList.onSelectCallback = SelectLevel;
        mEditorMain.Q<IMGUIContainer>("level_list").onGUIHandler = LevelListOnGUI;
    }

    void SetDataFromUXML()
    {
        if(mEditorMain.Contains(mLevelData))
        {
            mEditorMain.Remove(mLevelData);
        }
        VisualTreeAsset aLevelDataAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/PerLevelEditor.uxml");
        mLevelData = aLevelDataAsset.CloneTree();
        mLevelData.Q<ObjectField>("gameplay_music").objectType = typeof(AudioClip);
        mLevelData.Q<Button>("level_data").RegisterCallback<MouseUpEvent>((aEv) => SaveAsScriptableAsset());
        mMapContainer = mLevelData.Q<IMGUIContainer>("level_map");
        mActiveBrushId = -1;
        GetAllScriptableObjects();
        mMapContainer.onGUIHandler = LevelMapOnGUI;
        mMapContainer.RegisterCallback<MouseUpEvent>(ClickGrid);
    }

    void GetAllScriptableObjects()
    {
        string[] aAllAssetGUIDs = AssetDatabase.FindAssets("", new[] { "Assets/ScriptableObjects/GameObjects" });
        mBrushes = new Dictionary<Level.LayerTypes, List<GameScriptable>>(aAllAssetGUIDs.Length)
        {
            {Level.LayerTypes.Environment, new List<GameScriptable>() },
            {Level.LayerTypes.StaticObjects, new List<GameScriptable>() },
            {Level.LayerTypes.Enemies, new List<GameScriptable>() },
            {Level.LayerTypes.Players, new List<GameScriptable>() }
        };
        foreach (string aAssetGUID in aAllAssetGUIDs)
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aAssetGUID);
            GameScriptable aAsset = AssetDatabase.LoadAssetAtPath<GameScriptable>(aPath);
            if(aAsset != null)
            {
                mBrushes[aAsset.mRenderLayer].Add(aAsset);
            }
        }
    }

    #region IMGUI
    void LevelListOnGUI()
    {
        mLevelListGUIScrollPos = EditorGUILayout.BeginScrollView(mLevelListGUIScrollPos, GUILayout.Width(1050), GUILayout.Height(70));
        mLevelList.DoLayoutList();
        EditorGUILayout.EndScrollView();
    }

    void LevelMapOnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        ZoomInOut();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if(mActiveLevel != null)
        {
            mLevelMapGUIScrollPos = EditorGUILayout.BeginScrollView(mLevelMapGUIScrollPos, true, true, GUILayout.Width(mLevelMapScrollerSize.x), GUILayout.Height(mLevelMapScrollerSize.y));
            EditorGUILayout.LabelField("", GUILayout.Width(mActiveLevel.mColumns * mCellSize + 10), GUILayout.Height(mActiveLevel.mRows * mCellSize + 10));
            for (int aI = 0; aI <= mActiveLevel.mRows; aI ++)
            {
                EditorGUI.DrawRect(new Rect(0, aI * mCellSize, mActiveLevel.mColumns * mCellSize, 2), new Color(0.9245283f, 0.8049799f, 0.6585084f));
            }
            for(int aI = 0; aI <= mActiveLevel.mColumns; aI ++)
            {
                EditorGUI.DrawRect(new Rect(aI * mCellSize, 0, 2, mActiveLevel.mRows * mCellSize), new Color(0.9245283f, 0.8049799f, 0.6585084f));
            }
            if(!mShowAll)
            {
                RenderLayerOnly();
            }
            else
            {
                RenderAll();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.Space(20);
            PlaceableObjectListOnGUI();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    void RenderAll()
    {
        foreach(Level.LayerTypes aLayerTypes in System.Enum.GetValues(typeof(Level.LayerTypes)))
        {
            foreach (KeyValuePair<Vector2Int, List<GameScriptable>> aPlacedObjs in mActiveLevel.mLevelDataScriptable)
            {
                if(!mActiveLevel.mLevelData[aPlacedObjs.Key].Contains(aLayerTypes))
                {
                    continue;
                }
                foreach (GameScriptable aScriptable in aPlacedObjs.Value)
                {
                    if(aScriptable.mRenderLayer == aLayerTypes)
                    {
                        DrawTexturePreview(new Rect(aPlacedObjs.Key.x * mCellSize, aPlacedObjs.Key.y * mCellSize, mCellSize, mCellSize), aScriptable.mDisplaySprite);
                        break;
                    }
                }
            }
        }
    }

    void RenderLayerOnly()
    {
        if (mActiveLayer == Level.LayerTypes.Players)
        {
            if(mActiveLevel.mStartPosition.mWorldPosition.x != -1)
            {
                DrawTexturePreview(new Rect(mActiveLevel.mStartPosition.mWorldPosition.x * mCellSize, mActiveLevel.mStartPosition.mWorldPosition.y * mCellSize, mCellSize, mCellSize), mActiveLevel.mStartPosition.mDisplaySprite);
            }
            if(mActiveLevel.mEndPosition.mWorldPosition.x != -1)
            {
                DrawTexturePreview(new Rect(mActiveLevel.mEndPosition.mWorldPosition.x * mCellSize, mActiveLevel.mEndPosition.mWorldPosition.y * mCellSize, mCellSize, mCellSize), mActiveLevel.mEndPosition.mDisplaySprite);
            }
        }

        foreach (KeyValuePair<Vector2Int, List<GameScriptable>> aPlacedObjs in mActiveLevel.mLevelDataScriptable)
        {
            foreach (GameScriptable aScriptable in aPlacedObjs.Value)
            {
                if (aScriptable.mRenderLayer == mActiveLayer)
                {
                    DrawTexturePreview(new Rect(aPlacedObjs.Key.x * mCellSize, aPlacedObjs.Key.y * mCellSize, mCellSize, mCellSize), aScriptable.mDisplaySprite);
                }
            }
        }
    }

    void ZoomInOut()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Map Zoom: ");
        mCellSize = EditorGUILayout.IntSlider(mCellSize, mMinCellSize, mMaxCellSize);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(100);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Draw Layer: ");
        Level.LayerTypes aLayer = (Level.LayerTypes)EditorGUILayout.EnumPopup(mActiveLayer);
        if(aLayer != mActiveLayer)
        {
            mActiveLayer = aLayer;
            mActiveBrushId = -1;
            mActiveBrushesCount = 0;
        }
        mShowAll = EditorGUILayout.Toggle("Show All", mShowAll);
        EditorGUILayout.EndHorizontal();
    }


    void PlaceableObjectListOnGUI()
    {
        mPlaceableObjectsGUIScrollPos = EditorGUILayout.BeginScrollView(mPlaceableObjectsGUIScrollPos, true, true, GUILayout.Width(mPlaceableObjectsScrollerSize.x), GUILayout.Height(mPlaceableObjectsScrollerSize.y));
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Game Objects", GUILayout.Width(mPlaceableObjectsScrollerSize.x));
        GUILayout.Space(10);
        mActiveBrushesCount = 0;
        if(mActiveLayer == Level.LayerTypes.Players)
        {
            EditorGUILayout.LabelField(mActiveLevel.mStartPosition.mDisplayName, GUILayout.Width(mPlaceableObjectsScrollerSize.x));
            GUILayout.Space(mCellSize + 5);
            Rect aImgRect = new Rect(10, mActiveBrushesCount * (mCellSize + 25) + 45, mCellSize, mCellSize);
            DrawTexturePreview(aImgRect, mActiveLevel.mStartPosition.mDisplaySprite);
            if (mActiveBrushesCount == mActiveBrushId)
            {
                EditorGUI.DrawRect(aImgRect, new Color(Color.green.r, Color.green.g, Color.green.b, 0.5f));
            }
            mActiveBrushesCount++;
            aImgRect = new Rect(10, mActiveBrushesCount * (mCellSize + 25) + 45, mCellSize, mCellSize);
            EditorGUILayout.LabelField(mActiveLevel.mEndPosition.mDisplayName, GUILayout.Width(mPlaceableObjectsScrollerSize.x));
            GUILayout.Space(mCellSize + 5);
            DrawTexturePreview(aImgRect, mActiveLevel.mEndPosition.mDisplaySprite);
            if (mActiveBrushesCount == mActiveBrushId)
            {
                EditorGUI.DrawRect(aImgRect, new Color(Color.green.r, Color.green.g, Color.green.b, 0.5f));
            }
            mActiveBrushesCount++;
        }
        foreach (GameScriptable aBrush in mBrushes[mActiveLayer])
        {
            if(!aBrush.mIsPrefab)
            {
                EditorGUILayout.LabelField(aBrush.mName, GUILayout.Width(mPlaceableObjectsScrollerSize.x));
                GUILayout.Space(mCellSize + 5);
                Rect aImgRect = new Rect(10, mActiveBrushesCount * (mCellSize + 25) + 45, mCellSize, mCellSize);
                DrawTexturePreview(aImgRect, aBrush.mDisplaySprite);
                aBrush.mBrushId = mActiveBrushesCount;
                if(aBrush.mBrushId == mActiveBrushId)
                {
                    EditorGUI.DrawRect(aImgRect, new Color(Color.green.r,Color.green.g,Color.green.b, 0.5f));
                }
                mActiveBrushesCount++;
            }
            else
            {
                aBrush.mBrushId = -1;
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
    #endregion
    void SetupNewLevel()
    {
        SetDataFromUXML();
        mLevelData.Bind(new SerializedObject(mActiveLevel));
        mEditorMain.Add(mLevelData);
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

    #region CallBacks
    void UpdateLevelList(Rect aRect, int aIx, bool aIsActive, bool aIsFocused)
    {
        var aElement = (Level)mLevelList.list[aIx];
        aRect.y += 2;
        EditorGUI.LabelField(new Rect(aRect.x, aRect.y, 100, EditorGUIUtility.singleLineHeight), aElement.mName);
    }

    void AddNewLevel(ReorderableList pList)
    {
        SaveAsScriptableAsset();
        mActiveLevel = ScriptableObject.CreateInstance<Level>();
        mActiveLevel.Init();
        mActiveLevel.mStartPosition.mDisplayName = "Start Point";
        mActiveLevel.mStartPosition.mDisplaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/StartPoint.png");
        mActiveLevel.mStartPosition.mWorldPosition = mActiveLevel.mEndPosition.mWorldPosition = new Vector2Int(-1, -1);
        mActiveLevel.mEndPosition.mDisplayName = "End Point";
        mActiveLevel.mEndPosition.mDisplaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/EndPoint.png");
        string[] aCurLevels = AssetDatabase.FindAssets("Level", new[] { "Assets/ScriptableObjects/Level Data" });
        int aLevelId = aCurLevels.Length + 1;
        AssetDatabase.CreateAsset(mActiveLevel, "Assets/ScriptableObjects/Level Data/Level" + aLevelId + ".asset");
        mAllLevels.Add(mActiveLevel);
        pList.index = pList.list.IndexOf(mActiveLevel);
        SetupNewLevel();
    }

    void SelectLevel(ReorderableList pList)
    {
        SaveAsScriptableAsset();
        mActiveLevel = (Level)pList.list[pList.index];
        SetupNewLevel();
    }

    GameScriptable GetActiveBrushObject()
    {
        foreach (GameScriptable aBrush in mBrushes[mActiveLayer])
        {
            if(aBrush.mBrushId == mActiveBrushId)
            {
                return aBrush;
            }
        }
        return null;
    }

    void ClickGrid(MouseUpEvent pEvent)
    {
        if(pEvent.localMousePosition.x <= (mLevelMapScrollerSize.x - 15))
        {
            if (pEvent.localMousePosition.y >= 20 && pEvent.localMousePosition.y <= (mLevelMapScrollerSize.y + 5))
            {
                Vector2 postion = pEvent.localMousePosition + mLevelMapGUIScrollPos + new Vector2(0, -20);
                Vector2Int actP = new Vector2Int((int)postion.x / mCellSize, (int)postion.y / mCellSize);
                if (actP.x >= mActiveLevel.mColumns || actP.y >= mActiveLevel.mRows)
                {
                    return;
                }

                if(mActiveBrushId == -1)
                {
                    return;
                }
                if(mActiveLayer == Level.LayerTypes.Players && mActiveBrushId <= 1)
                {
                    if(mActiveBrushId == 0)
                    {
                        mActiveLevel.SetResetStartPosition(actP);
                    }
                    else
                    {
                        mActiveLevel.SetResetEndPosition(actP);
                    }
                }
                else
                {
                    GameScriptable aObject = GetActiveBrushObject();
                    if (aObject != null)
                    {
                        if(mActiveLevel.IsScriptablePresent(actP,aObject))
                        {
                            mActiveLevel.RemoveScriptable(actP, aObject);
                        }
                        else
                        {
                            if(!mActiveLevel.IsLayerObjectPresent(actP, mActiveLayer))
                            {
                                mActiveLevel.AddScriptable(actP, aObject);
                            }
                        }
                    }

                }
                GauntletEditorMain.DoRepaint();
            }
        }
        else if(pEvent.localMousePosition.x > (mLevelMapScrollerSize.x + 15) && pEvent.localMousePosition.x < (mLevelMapScrollerSize.x + mPlaceableObjectsScrollerSize.x))
        {
            if (pEvent.localMousePosition.y >= 45 && pEvent.localMousePosition.y <= (mPlaceableObjectsScrollerSize.y + 5))
            {
                Vector2 postion = pEvent.localMousePosition + mPlaceableObjectsGUIScrollPos + new Vector2(0, -45);
                int actP = (int)postion.y / (mCellSize + 25);
                if(actP >= mActiveBrushesCount)
                {
                    return;
                }
                mActiveBrushId = actP;
                GauntletEditorMain.DoRepaint();
            }
        }
    }

    void SaveAsScriptableAsset()
    {
        if(mActiveLevel != null)
        {
            EditorUtility.SetDirty(mActiveLevel);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            mActiveLevel = null;
        }
    }

    #endregion

    public IBinding binding { get; set; }
    public string bindingPath { get; set; }


}
