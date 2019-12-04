using UnityEditorInternal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public enum LayerTypes
{
    Environment,
    StartingObjects,
    Enemies,
    Players
}

public struct LevelObjectsDisplay
{
    public string mObjectName;
    public Sprite mDisplaySprite;
    public GameScriptable mScriptableObject;
}

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
    LayerTypes mActiveLayer;
    bool mShowAll;
    VisualElement mLevelData;
    VisualElement mEditorMain;
    IMGUIContainer mMapContainer;

    List<LevelObjectsDisplay> mAllPlaceableObjects;

    static void CreateInstance()
    {
        if(mInstance == null)
        {
            mInstance = new LevelEditor();
            mInstance.mAllPlaceableObjects = new List<LevelObjectsDisplay>();
            LevelObjectsDisplay aStartPoint = new LevelObjectsDisplay();
            aStartPoint.mObjectName = "Start Point";
            aStartPoint.mDisplaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GameAssets/Asset Files/StartPoint.png");
            mInstance.mAllPlaceableObjects.Add(aStartPoint);
            LevelObjectsDisplay aEndPoint = new LevelObjectsDisplay();
            aEndPoint.mObjectName = "End Point";
            aEndPoint.mDisplaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GameAssets/Asset Files/EndPoint.png");
            mInstance.mAllPlaceableObjects.Add(aEndPoint);
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
        mMapContainer.onGUIHandler = LevelMapOnGUI;
        mMapContainer.RegisterCallback<MouseUpEvent>(ClickGrid);
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
        mLevelMapGUIScrollPos = EditorGUILayout.BeginScrollView(mLevelMapGUIScrollPos,true,true, GUILayout.Width(mLevelMapScrollerSize.x), GUILayout.Height(mLevelMapScrollerSize.y));
        if(mActiveLevel != null)
        {
            EditorGUILayout.LabelField("", GUILayout.Width(mActiveLevel.mColumns * mCellSize + 10), GUILayout.Height(mActiveLevel.mRows * mCellSize + 10));
            for (int aI = 0; aI <= mActiveLevel.mRows; aI ++)
            {
                EditorGUI.DrawRect(new Rect(0, aI * mCellSize, mActiveLevel.mColumns * mCellSize, 2), new Color(0.9245283f, 0.8049799f, 0.6585084f));
            }
            for(int aI = 0; aI <= mActiveLevel.mColumns; aI ++)
            {
                EditorGUI.DrawRect(new Rect(aI * mCellSize, 0, 2, mActiveLevel.mRows * mCellSize), new Color(0.9245283f, 0.8049799f, 0.6585084f));
            }
        }
        EditorGUILayout.EndScrollView();
        GUILayout.Space(20);
        PlaceableObjectListOnGUI();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
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
        mActiveLayer = (LayerTypes)EditorGUILayout.EnumPopup((System.Enum)mActiveLayer);
        mShowAll = EditorGUILayout.Toggle("Show All", mShowAll);
        EditorGUILayout.EndHorizontal();
    }


    void PlaceableObjectListOnGUI()
    {
        mPlaceableObjectsGUIScrollPos = EditorGUILayout.BeginScrollView(mPlaceableObjectsGUIScrollPos, true, true, GUILayout.Width(mPlaceableObjectsScrollerSize.x), GUILayout.Height(mPlaceableObjectsScrollerSize.y));
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Game Objects", GUILayout.Width(mPlaceableObjectsScrollerSize.x));
        EditorGUILayout.Space();
        int aI = 0;
        foreach (LevelObjectsDisplay aDisplayData in mAllPlaceableObjects)
        {
            EditorGUILayout.LabelField(aDisplayData.mObjectName, GUILayout.Width(mPlaceableObjectsScrollerSize.x));
            GUILayout.Space(mCellSize + 5);
            DrawTexturePreview(new Rect(10, aI * (mCellSize + 25) + 45, mCellSize, mCellSize), aDisplayData.mDisplaySprite);
            aI++;
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
        Vector2 fullSize = new Vector2(pSprite.texture.width, pSprite.texture.height);
        Vector2 size = new Vector2(pSprite.textureRect.width, pSprite.textureRect.height);

        Rect coords = pSprite.textureRect;
        coords.x /= fullSize.x;
        coords.width /= fullSize.x;
        coords.y /= fullSize.y;
        coords.height /= fullSize.y;

        Vector2 ratio;
        ratio.x = pPosition.width / size.x;
        ratio.y = pPosition.height / size.y;
        float minRatio = Mathf.Min(ratio.x, ratio.y);

        Vector2 center = pPosition.center;
        pPosition.width = size.x * minRatio;
        pPosition.height = size.y * minRatio;
        pPosition.center = center;

        GUI.DrawTextureWithTexCoords(pPosition, pSprite.texture, coords);
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
        mActiveLevel = new Level();
        mAllLevels.Add(mActiveLevel);
        pList.index = pList.list.IndexOf(mActiveLevel);
        SetupNewLevel();
    }

    void SelectLevel(ReorderableList pList)
    {
        mActiveLevel = (Level)pList.list[pList.index];
        SetupNewLevel();
    }

    void ClickGrid(MouseUpEvent pEvent)
    {
        if(pEvent.localMousePosition.x <= mLevelMapScrollerSize.x - 15)
        {
            if (pEvent.localMousePosition.y >= 20 && pEvent.localMousePosition.y <= (mLevelMapScrollerSize.y + 5))
            {
                Debug.Log("In The Real Map");
            }
        }
        Debug.LogFormat("Grid Clicked \n{0} {1} {2}\n\n", pEvent.localMousePosition, mLevelMapGUIScrollPos, mPlaceableObjectsGUIScrollPos);
    }

    void SaveAsScriptableAsset()
    {
        Debug.Log("Save As Scriptable Asset Clicked");
    }

    #endregion

    public IBinding binding { get; set; }
    public string bindingPath { get; set; }


}
