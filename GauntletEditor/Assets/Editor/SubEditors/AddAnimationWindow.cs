using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AddAnimationWindow : EditorWindow
{
    static AddAnimationWindow mWindow;

    public static void OpenAnimationWindow()
    {
        if (mWindow != null)
        {
            mWindow.Close();
            mWindow = null;
        }
        mWindow = GetWindow<AddAnimationWindow>();
        mWindow.minSize = new Vector2(1100, 700);
        mWindow.titleContent = new GUIContent("Add New Animation");
    }

    public static bool IsWindowOpen()
    {
        return mWindow != null;
    }
}
