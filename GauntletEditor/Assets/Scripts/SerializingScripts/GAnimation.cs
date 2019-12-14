using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GAnimation
{
    public string textureAssetGUID;
    public string Class = "Animation";
    public string Name;
    public float Speed;
    public bool Loopable = true;
    public dimensions[] Frames;

    public GAnimation(AnimationData pData)
    {
        Name = pData.mAnimationName;
        textureAssetGUID = pData.mTextureAssetGUID;
        Speed = pData.mAnimSpeed;
        Frames = new dimensions[pData.mSprites.Count];
        int aI = 0;
        foreach(Sprite aSprite in pData.mSprites)
        {
            GSprite aSp = new GSprite(aSprite.rect, textureAssetGUID, aSprite.texture.height, 0);
            Frames[aI] = aSp.Dimensions;
        }
    }

    public override string ToString()
    {
        string aJSON = JsonUtility.ToJson(this);
        return aJSON.Replace("Class", "class");




    }

}
