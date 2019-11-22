using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class GauntletEditorMain : EditorWindow, IBindable
{
    static GauntletEditorMain mWindow;
    VisualElement mMainMenu;
    StyleSheet mMainStyle;
    [MenuItem("Gauntlet Editor/Main Window")]
    public static void OpenMainWindow()
    {
        if(mWindow != null)
        {
            mWindow.Close();
            mWindow = null;
        }
        mWindow = GetWindow<GauntletEditorMain>();
        mWindow.minSize = new Vector2(1100, 700);
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
        //if(LevelEditor.GetCurrentLevelEditorUI() != null)
        //{
        //    rootVisualElement.Add(LevelEditor.GetCurrentLevelEditorUI());
        //}
        //else
        //{
            rootVisualElement.Add(LevelEditor.CreateNewLevelEditorUI());
        //}
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
    }

    public IBinding binding { get; set; }
    public string bindingPath { get; set; }

}