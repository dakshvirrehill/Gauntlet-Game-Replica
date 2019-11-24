using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.ShortcutManagement;


public class GauntletEditorMain : EditorWindow, IBindable
{

    public enum EditorType
    {
        LevelEditor,
        GameObjectEditor,
        PlayerEditor,
        AssetEditor
    }


    static GauntletEditorMain mWindow;
    #region Menu Variables
    VisualElement mMainMenu;
    Button mLevelButton;
    Button mPlayerButton;
    Button mAssetButton;
    Button mGObjButton;
    #endregion
    #region Current Editor Variables
    EditorType mActiveEditor = EditorType.LevelEditor;
    VisualElement mCurrentEditor;
    #endregion
    StyleSheet mMainStyle;

    [Shortcut("Refresh Gauntlet Editor", KeyCode.F9)]
    [MenuItem("Gauntlet Editor/Main Window")]
    public static void OpenMainWindow()
    {
        if(mWindow != null)
        {
            mWindow.Close();
            mWindow = null;
        }
        mWindow = GetWindow<GauntletEditorMain>();
        mWindow.minSize = new Vector2(1100, 800);
        mWindow.titleContent = new GUIContent("Gauntlet Game Editor Main");
    }

    public void OnEnable()
    {
        mMainStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/StyleSheets/GauntletEditorMain.uss");
        rootVisualElement.styleSheets.Add(mMainStyle);
        Label aMainLabel = new Label("Gauntlet Game Editor");
        aMainLabel.AddToClassList("mainHeader");
        rootVisualElement.Add(aMainLabel);
        CreateMainMenu();
        mCurrentEditor = LevelEditor.CreateNewLevelEditorUI();
        mActiveEditor = EditorType.LevelEditor;
        rootVisualElement.Add(mCurrentEditor);
    }

    void CreateMainMenu()
    {
        if(mMainMenu != null)
        {
            rootVisualElement.Remove(mMainMenu);
        }
        VisualTreeAsset aMainMenuTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UXML Files/MainMenu.uxml");
        mMainMenu = aMainMenuTree.CloneTree();
        rootVisualElement.Add(mMainMenu);
        mLevelButton = mMainMenu.Q<Button>("display_level_editor");
        mLevelButton.RegisterCallback<MouseUpEvent>((aEv) => ChangeCurrentEditor(EditorType.LevelEditor, aEv));
        mAssetButton = mMainMenu.Q<Button>("display_asset_editor");
        mAssetButton.RegisterCallback<MouseUpEvent>((aEv) => ChangeCurrentEditor(EditorType.AssetEditor, aEv));
        mGObjButton = mMainMenu.Q<Button>("display_gameobject_editor");
        mGObjButton.RegisterCallback<MouseUpEvent>((aEv) => ChangeCurrentEditor(EditorType.GameObjectEditor, aEv));
        mPlayerButton = mMainMenu.Q<Button>("display_player_editor");
        mPlayerButton.RegisterCallback<MouseUpEvent>((aEv) => ChangeCurrentEditor(EditorType.PlayerEditor, aEv));
    }


    void RemoveCurrentEditorVE()
    {
        if (mCurrentEditor != null)
        {
            rootVisualElement.Remove(mCurrentEditor);
            SetAllButtonsClass("unselected","selected");
        }
    }

    void SetAllButtonsClass(string pClassToChoose, string pClassToRemove)
    {
        switch (mActiveEditor)
        {
            case EditorType.AssetEditor:
                mAssetButton.parent.RemoveFromClassList(pClassToRemove);
                mAssetButton.parent.AddToClassList(pClassToChoose);
                break;
            case EditorType.LevelEditor:
                mLevelButton.parent.RemoveFromClassList(pClassToRemove);
                mLevelButton.parent.AddToClassList(pClassToChoose);
                break;
            case EditorType.GameObjectEditor:
                mGObjButton.parent.RemoveFromClassList(pClassToRemove);
                mGObjButton.parent.AddToClassList(pClassToChoose);
                break;
            case EditorType.PlayerEditor:
                mPlayerButton.parent.RemoveFromClassList(pClassToRemove);
                mPlayerButton.parent.AddToClassList(pClassToChoose);
                break;
        }
    }

    void CreateCurrentEditor()
    {
        switch(mActiveEditor)
        {
            case EditorType.AssetEditor:
                mCurrentEditor = AssetEditor.CreateNewAssetEditorUI();
                break;
            case EditorType.GameObjectEditor:
                mCurrentEditor = GameObjectEditor.CreateNewGameObjectEditorUI();
                break;
            case EditorType.LevelEditor:
                mCurrentEditor = LevelEditor.CreateNewLevelEditorUI();
                break;
            case EditorType.PlayerEditor:
                mCurrentEditor = PlayerEditor.CreateNewPlayerEditorUI();
                break;
        }
    }

    void ChangeCurrentEditor(EditorType pSelectedEditorType, MouseUpEvent aEv)
    {
        RemoveCurrentEditorVE();
        mActiveEditor = pSelectedEditorType;
        CreateCurrentEditor();
        rootVisualElement.Add(mCurrentEditor);
        SetAllButtonsClass("selected", "unselected");
    }

    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

}