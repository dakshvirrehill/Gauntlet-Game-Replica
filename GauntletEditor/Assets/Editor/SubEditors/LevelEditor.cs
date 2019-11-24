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
    Vector2 mPlaceableObjectsGUIScrollPos;

    Level mActiveLevel;
    VisualElement mLevelData;
    VisualElement mEditorMain;
    IMGUIContainer mMapContainer;
    IMGUIContainer mPlaceableObjectContainer;

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
        mInstance.SetDataFromUXML();
        return mInstance.mLevelEditorUI;
    }

    void SetDataFromUXML()
    {
        mEditorMain = mLevelEditorUI.Q<VisualElement>("editor_data");
        mLevelData = mEditorMain.Q<VisualElement>("main_data_display");
        mEditorMain.Remove(mLevelData);
        mLevelData.Q<ObjectField>("gameplay_music").objectType = typeof(AudioClip);
        mLevelData.Q<SliderInt>("row_size").RegisterCallback<ChangeEvent<int>>((aEv) => UpdateLevelMapGrid());
        mLevelData.Q<SliderInt>("column_size").RegisterCallback<ChangeEvent<int>>((aEv) => UpdateLevelMapGrid());
        mEditorMain.Q<IMGUIContainer>("level_list").onGUIHandler = LevelListOnGUI;
        mMapContainer = mLevelData.Q<IMGUIContainer>("level_map");
        mPlaceableObjectContainer = mLevelData.Q<IMGUIContainer>("placeable_objects");
        mMapContainer.onGUIHandler = LevelMapOnGUI;
        mMapContainer.RegisterCallback<MouseUpEvent>(ClickGrid);
        mPlaceableObjectContainer.onGUIHandler = PlaceableObjectListOnGUI;
        mPlaceableObjectContainer.RegisterCallback<MouseUpEvent>(ClickPlaceable);

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
    }

    #region IMGUI
    void LevelListOnGUI()
    {
        mLevelListGUIScrollPos = EditorGUILayout.BeginScrollView(mLevelListGUIScrollPos, GUILayout.Width(1050), GUILayout.Height(50));
        mLevelList.DoLayoutList();
        EditorGUILayout.EndScrollView();
    }

    void LevelMapOnGUI()
    {
        mLevelMapGUIScrollPos = EditorGUILayout.BeginScrollView(mLevelMapGUIScrollPos, GUILayout.Width(600), GUILayout.Height(250));
        //draw grid
        EditorGUILayout.EndScrollView();
    }

    void PlaceableObjectListOnGUI()
    {
        mPlaceableObjectsGUIScrollPos = EditorGUILayout.BeginScrollView(mPlaceableObjectsGUIScrollPos, GUILayout.Width(600), GUILayout.Height(250));
        //draw objects
        EditorGUILayout.EndScrollView();
    }
    #endregion
    void SetupNewLevel()
    {
        if (mEditorMain.Contains(mLevelData))
        {
            mEditorMain.Remove(mLevelData);
        }
        mLevelData.Bind(new SerializedObject(mActiveLevel));
        mEditorMain.Add(mLevelData);
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
        SetupNewLevel();
    }

    void SelectLevel(ReorderableList pList)
    {
        mActiveLevel = (Level)pList.list[pList.index];
        SetupNewLevel();
    }

    void UpdateLevelMapGrid()
    {
        //make grid
    }

    void ClickGrid(MouseUpEvent pEvent)
    {
        Debug.Log("Grid Clicked");
    }

    void ClickPlaceable(MouseUpEvent pEvent)
    {
        Debug.Log("Placeable Clicked");
    }

    #endregion

    public IBinding binding { get; set; }
    public string bindingPath { get; set; }


}
