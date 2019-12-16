using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct dimensions
{
    public int Left;
    public int Top;
    public int Width;
    public int Height;
}
[System.Serializable]
public class GSprite
{
    public string Class;
    public string textureAssetGUID;
    public dimensions Dimensions;
    public int layer;
    public GSprite(Rect pSpriteRect, string pGUID, int pHeight,int pLayer)
    {
        Class = "Sprite";
        textureAssetGUID = pGUID;
        Dimensions = new dimensions
        {
            Left = (int)pSpriteRect.x,
            Top = pHeight - (int)pSpriteRect.height - (int)pSpriteRect.y,
            Width = (int)pSpriteRect.width,
            Height = (int)pSpriteRect.height
        };
        layer = pLayer;
    }

    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");
    }

}
