using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using System.IO;
using System.Text;

public class SerializeToJSON
{

    static Dictionary<AssetMetaData.AssetType, List<AssetMetaData>> mCurrentAssets;

    [Shortcut("Save To Game Data", KeyCode.F10)]
    [MenuItem("Gauntlet Editor/Save To Game Data")]
    public static void SerializeToJson()
    {
        string[] aCurLevelOrder = AssetDatabase.FindAssets("LevelOrder", new[] { "Assets/ScriptableObjects/Level Data" });
        if(aCurLevelOrder.Length <= 0)
        {
            EditorUtility.DisplayDialog("No Data", "Please Create Levels and Scriptable Objects Before Saving", "Okay");
            return;
        }
        LevelOrder aLevelOrderObject = AssetDatabase.LoadAssetAtPath<LevelOrder>(AssetDatabase.GUIDToAssetPath(aCurLevelOrder[0]));
        SaveLevelOrderJSON(aLevelOrderObject);
    }

    static void SaveLevelOrderJSON(LevelOrder pOrder)
    {
        CreateLevelFolder();
        CreateResourcesFolder();
        SetAssetData();
        if (mCurrentAssets[AssetMetaData.AssetType.PrefabAsset].Count <= 0)
        {
            EditorUtility.DisplayDialog("No Data", "Please Create Levels and Scriptable Objects Before Saving", "Okay");
            return;
        }
        AssetMetaData aCurrentPlayer = null;
        foreach(AssetMetaData aPrefab in mCurrentAssets[AssetMetaData.AssetType.PrefabAsset])
        {
            string aPath = AssetDatabase.GUIDToAssetPath(aPrefab.mGUID);
            if (AssetDatabase.GetMainAssetTypeAtPath(aPath) == typeof(Player))
            {
                aCurrentPlayer = aPrefab;
                break;
            }
        }
        if(aCurrentPlayer == null)
        {
            EditorUtility.DisplayDialog("No Data", "Please Create Levels and Scriptable Objects Before Saving", "Okay");
            return;
        }
        StringBuilder aLevelOrderJSON = new StringBuilder("{\n\"Levels\" : {\n");
        int aI = 0;
        foreach(Level aLevel in pOrder.mAllLevels)
        {
            if (aLevel.mStartPosition.mWorldPosition.x < 0 && aLevel.mStartPosition.mWorldPosition.y < 0)
            {
                continue;
            }
            if(aLevel.mEndPosition.mWorldPosition.x < 0 && aLevel.mEndPosition.mWorldPosition.y < 0)
            {
                continue;
            }
            StringBuilder aResourcesArray = new StringBuilder("\"resources\": [");
            StringBuilder aLevelJSON = new StringBuilder("{\"GameObjects\": [");

            aLevelJSON.Append(GetStartPositionJson(aLevel.mStartPosition,ref aResourcesArray, aCurrentPlayer));
            aLevelJSON.Append(",");
            aLevelJSON.Append(GetEndPositionJson(aLevel.mEndPosition,ref aResourcesArray));
            aLevelJSON.Append(",");
            for (int aJ = 0; aJ < aLevel.mLevelData.Count; aJ++)
            {
                foreach (GameScriptable aScriptable in aLevel.mLevelData[aJ].mScriptables)
                {
                    aLevelJSON.Append(GetGameScriptableJson(aLevel.mLevelData[aJ].mPosition, aScriptable,ref aResourcesArray));
                    aLevelJSON.Append(",");
                }
            }
            aLevelJSON.Remove(aLevelJSON.Length - 1, 1);
            aLevelJSON.Append("]");
            aResourcesArray.Remove(aResourcesArray.Length - 1, 1);
            aResourcesArray.Append("]");
            aLevelJSON.Append("," + aResourcesArray.ToString());
            aLevelJSON.Append("," + "\"Timer\" : " + aLevel.mTime);
            aLevelJSON.Append("}");
            SaveToFile(aLevelJSON.ToString(), Application.dataPath + "/Assets/Level Data/" + aLevel.mName + ".json");
            aLevelOrderJSON.Append("\"" + aI + "\" : \"../Assets/Level Data/" + aLevel.mName + ".json\",");
            aI++;
        }
        aLevelOrderJSON.Remove(aLevelOrderJSON.Length - 1, 1);
        aLevelOrderJSON.Append("\n}\n");
        string[] aItems = SaveItemPrefabs();
        aLevelOrderJSON.Append("," + aItems[0] + "\n");
        aLevelOrderJSON.Append("," + aItems[1]);
        aLevelOrderJSON.Append("\n}");
        SaveToFile(aLevelOrderJSON.ToString(), Application.dataPath + "/Assets/GauntletGame.json");
        AssetDatabase.Refresh();
    }
    static void SetAssetData()
    {
        mCurrentAssets = new Dictionary<AssetMetaData.AssetType, List<AssetMetaData>>();
        mCurrentAssets.Add(AssetMetaData.AssetType.AudioAsset, new List<AssetMetaData>());
        mCurrentAssets.Add(AssetMetaData.AssetType.FontAsset, new List<AssetMetaData>());
        mCurrentAssets.Add(AssetMetaData.AssetType.TextureAsset, new List<AssetMetaData>());
        mCurrentAssets.Add(AssetMetaData.AssetType.PrefabAsset, new List<AssetMetaData>());
        string aBaseFolderPath = "Assets/ScriptableObjects/Asset Meta Data/";
        string[] aAssetFiles = Directory.GetFiles(aBaseFolderPath);
        foreach (string aAssets in aAssetFiles)
        {
            AssetMetaData aAsset = AssetDatabase.LoadAssetAtPath<AssetMetaData>(aAssets);
            if (aAsset != null)
            {
               mCurrentAssets[aAsset.mType].Add(aAsset);
            }
        }
    }
    static StringBuilder GetStartPositionJson(Level.GamePositions pStartPosition, ref StringBuilder pResources, AssetMetaData pPlayer)
    {
        StringBuilder aStartPositionGobj =new StringBuilder("{\n");
        aStartPositionGobj.Append("\"name\" : \"Start Position\",\n");
        aStartPositionGobj.Append("\"Components\" : [\n");
        GTransform aTransform = new GTransform();
        aTransform.Position = new position(pStartPosition.mWorldPosition.x * 64.00001f, pStartPosition.mWorldPosition.y * 64.00001f);
        aStartPositionGobj.Append(aTransform.ToString());
        aStartPositionGobj.Append(",\n");
        PlayerSpawner aSpawner = new PlayerSpawner();
        aSpawner.mPlayerPrefabGUID = pPlayer.mGUID;
        SavePlayer(pPlayer,ref pResources);
        aStartPositionGobj.Append(aSpawner.ToString());
        aStartPositionGobj.Append("]\n");
        aStartPositionGobj.Append("}\n");
        return aStartPositionGobj;
    }
    static void SaveAssetData(AssetMetaData pData, ref StringBuilder pResources)
    {
        System.Type aAssetType = typeof(Object);
        string aSaveFolder = "/Assets/Resources/";
        switch (pData.mType)
        {
            case AssetMetaData.AssetType.None:
                return;
            case AssetMetaData.AssetType.PrefabAsset:
                return;
            case AssetMetaData.AssetType.AudioAsset:
                aSaveFolder += "Audios/";
                aAssetType = typeof(AudioClip);
                break;
            case AssetMetaData.AssetType.FontAsset:
                aSaveFolder += "Fonts/";
                aAssetType = typeof(Font);
                break;
            case AssetMetaData.AssetType.TextureAsset:
                aSaveFolder += "Textures/";
                aAssetType = typeof(Texture);
                break;
            default:
                return;
        }
        string aOldPath = AssetDatabase.GUIDToAssetPath(pData.mGUID);
        string aFileName = aOldPath.Substring(aOldPath.LastIndexOf('/') + 1);
        pData.mAssetFilePath = ".." + aSaveFolder + aFileName;
        if (!IsAssetInResources(Application.dataPath + aSaveFolder + aFileName))
        {
            AssetDatabase.CopyAsset(aOldPath, "Assets" + aSaveFolder + aFileName);
            SaveToFile(pData.ToString(), Application.dataPath + aSaveFolder + aFileName + ".json");
        }
        if(!IsPathInResources(pResources , pData.mAssetFilePath + ".json"))
        {
            pResources.Append("\"" + pData.mAssetFilePath + ".json\",");
        }
    }
    static void SavePlayer(AssetMetaData pPlayer,ref StringBuilder pResources)
    {
        Player aPlScriptable = AssetDatabase.LoadAssetAtPath<Player>(AssetDatabase.GUIDToAssetPath(pPlayer.mGUID));
        string aPlayerFilePath = "/Assets/Resources/Prefabs/" + aPlScriptable.mName + ".json";
        pPlayer.mAssetFilePath = ".." + aPlayerFilePath;
        if (!IsAssetInResources(Application.dataPath + aPlayerFilePath))
        {
            GPlayer aPlayer = new GPlayer();
            aPlayer.mHealth = aPlScriptable.mHealth;
            aPlayer.mSpeed = aPlScriptable.mSpeed;
            aPlayer.mProjectileGUID = aPlScriptable.mGUIDProjectile;
            SaveProjectile(GetAssetFromGUID(aPlayer.mProjectileGUID, AssetMetaData.AssetType.PrefabAsset), ref pResources);
            GSprite aSprite = new GSprite(aPlScriptable.mDisplaySprite.rect, aPlScriptable.mAnimationData[0].mTextureAssetGUID,
                aPlScriptable.mDisplaySprite.texture.height, (int)aPlScriptable.mRenderLayer);
            SaveAssetData(GetAssetFromGUID(aPlScriptable.mAnimationData[0].mTextureAssetGUID, AssetMetaData.AssetType.TextureAsset), ref pResources);
            GCircleCollider aCircleCollider = new GCircleCollider(32.00001f, false);
            GRigidbody aRigidBody = new GRigidbody();
            StringBuilder aPlayerObject = new StringBuilder("{\n");
            aPlayerObject.Append("\"name\" : \"" + aPlScriptable.mName + "\",\n");
            aPlayerObject.Append("\"Components\" : [\n");
            aPlayerObject.Append(aPlayer.ToString() + ",\n");
            aPlayerObject.Append(aSprite.ToString() + ",\n");
            aPlayerObject.Append(aRigidBody.ToString() + ",\n");
            aPlayerObject.Append(aCircleCollider.ToString() + ",\n");
            aPlayerObject.Append("{\n\"class\" : \"Animator\",\n\"Name\" : \"" + aPlScriptable.mName + "\"\n},\n");
            foreach (AnimationData aData in aPlScriptable.mAnimationData)
            {
                GAnimation aAnimation = new GAnimation(aData);
                aPlayerObject.Append(aAnimation.ToString() + ",\n");
            }
            aPlayerObject.Remove(aPlayerObject.Length - 2, 2);
            aPlayerObject.Append("]\n");
            aPlayerObject.Append("}\n");
            SaveToFile(aPlayerObject.ToString(), Application.dataPath + aPlayerFilePath);
            SaveToFile(pPlayer.ToString(), Application.dataPath + aPlayerFilePath + ".json");
        }
        if (!IsPathInResources(pResources, pPlayer.mAssetFilePath + ".json"))
        {
            pResources.Append("\"" + pPlayer.mAssetFilePath + ".json\",");
        }
    }   
    static AssetMetaData GetAssetFromGUID(string pGUID, AssetMetaData.AssetType pType)
    {
        foreach (AssetMetaData aData in mCurrentAssets[pType])
        {
            if(aData.mGUID == pGUID)
            {
                return aData;
            }
        }
        return null;
    }
    static void SaveProjectile(AssetMetaData pProjectile, ref StringBuilder pResources)
    {
        if(pProjectile == null)
        {
            return;
        }
        Projectile aProjectile = AssetDatabase.LoadAssetAtPath<Projectile>(AssetDatabase.GUIDToAssetPath(pProjectile.mGUID));
        string aProjectileFilePath = "/Assets/Resources/Prefabs/" + aProjectile.mName + ".json";
        pProjectile.mAssetFilePath = ".." + aProjectileFilePath;
        if (!IsAssetInResources(Application.dataPath + aProjectileFilePath))
        {
            GProjectile aProj = new GProjectile();
            aProj.mPoolCount = aProjectile.mPoolCount;
            aProj.mSpeed = aProjectile.mSpeed;
            GCircleCollider aCollider = new GCircleCollider(32.00001f, true);
            GRigidbody aRigidBody = new GRigidbody();
            GSprite aSprite = new GSprite(aProjectile.mDisplaySprite.rect, aProjectile.mProjectileAnimation[0].mTextureAssetGUID,
                aProjectile.mDisplaySprite.texture.height, (int)aProjectile.mRenderLayer);
            SaveAssetData(GetAssetFromGUID(aProjectile.mProjectileAnimation[0].mTextureAssetGUID, AssetMetaData.AssetType.TextureAsset), ref pResources);
            StringBuilder aProjectileObject = new StringBuilder("{\n");
            aProjectileObject.Append("\"name\" : \"" + aProjectile.mName + "\",\n");
            aProjectileObject.Append("\"Components\" : [\n");
            aProjectileObject.Append(aSprite.ToString() + ",\n");
            aProjectileObject.Append(aProj.ToString() + ",\n");
            aProjectileObject.Append(aCollider.ToString() + ",\n");
            aProjectileObject.Append(aRigidBody.ToString() + ",\n");
            aProjectileObject.Append("{\n\"class\" : \"Animator\",\n\"Name\" : \"" + aProjectile.mName + "\"\n},\n");
            foreach (AnimationData aData in aProjectile.mProjectileAnimation)
            {
                GAnimation aAnimation = new GAnimation(aData);
                aProjectileObject.Append(aAnimation.ToString() + ",\n");
            }
            aProjectileObject.Remove(aProjectileObject.Length - 2, 2);
            aProjectileObject.Append("]\n");
            aProjectileObject.Append("}\n");
            SaveToFile(aProjectileObject.ToString(), Application.dataPath + aProjectileFilePath);
            SaveToFile(pProjectile.ToString(), Application.dataPath + aProjectileFilePath + ".json");
        }
        if(!IsPathInResources(pResources, pProjectile.mAssetFilePath + ".json"))
        {
            pResources.Append("\"" + pProjectile.mAssetFilePath + ".json\",");
        }
    }
    static void SaveEnemyPrefab(AssetMetaData pEnemy, ref StringBuilder pResources)
    {
        if(pEnemy == null)
        {
            return;
        }
        Enemy aEnemy = AssetDatabase.LoadAssetAtPath<Enemy>(AssetDatabase.GUIDToAssetPath(pEnemy.mGUID));
        string aEnemyFilePath = "/Assets/Resources/Prefabs/" + aEnemy.mName + ".json";
        pEnemy.mAssetFilePath = ".." + aEnemyFilePath;
        if(!IsAssetInResources(Application.dataPath + aEnemyFilePath))
        {
            GEnemy aEnemyComp = new GEnemy();
            aEnemyComp.mType = aEnemy.mEnemyType;
            aEnemyComp.mSpeed = aEnemy.mSpeed;
            aEnemyComp.mStopRange = aEnemy.mStopRange;
            if (aEnemyComp.mType == Enemy.Type.ProjectileThrower)
            {
                aEnemyComp.mProjectileGUID = aEnemy.mProjectileGUID;
                SaveProjectile(GetAssetFromGUID(aEnemy.mProjectileGUID, AssetMetaData.AssetType.PrefabAsset), ref pResources);
            }
            GSprite aSprite = new GSprite(aEnemy.mDisplaySprite.rect, aEnemy.mEnemyAnimations[0].mTextureAssetGUID,
                aEnemy.mDisplaySprite.texture.height, (int)aEnemy.mRenderLayer);
            GRigidbody aRigidBody = new GRigidbody();
            GCircleCollider aCollider = new GCircleCollider(32.00001f, false);
            StringBuilder aEnemyObject = new StringBuilder("{\n");
            aEnemyObject.Append("\"name\" : \"" + aEnemy.mName + "\",\n");
            aEnemyObject.Append("\"Components\" : [\n");
            aEnemyObject.Append(aSprite.ToString() + ",\n");
            aEnemyObject.Append(aEnemyComp.ToString() + ",\n");
            aEnemyObject.Append(aCollider.ToString() + ",\n");
            aEnemyObject.Append(aRigidBody.ToString() + ",\n");
            aEnemyObject.Append("{\n\"class\" : \"Animator\",\n\"Name\" : \"" + aEnemy.mName + "\"\n},\n");
            foreach (AnimationData aData in aEnemy.mEnemyAnimations)
            {
                GAnimation aAnimation = new GAnimation(aData);
                aEnemyObject.Append(aAnimation.ToString() + ",\n");
            }
            aEnemyObject.Remove(aEnemyObject.Length - 2, 2);
            aEnemyObject.Append("]\n");
            aEnemyObject.Append("}\n");
            SaveToFile(aEnemyObject.ToString(), Application.dataPath + aEnemyFilePath);
            SaveToFile(pEnemy.ToString(), Application.dataPath + aEnemyFilePath + ".json");
        }
        if(!IsPathInResources(pResources, pEnemy.mAssetFilePath + ".json"))
        {
            pResources.Append("\"" + pEnemy.mAssetFilePath + ".json\",");
        }
    }
    static string[] SaveItemPrefabs()
    {
        StringBuilder aItemsSB = new StringBuilder("\"resources\" : [\n");
        StringBuilder aItemGUIDs = new StringBuilder("\"mItemGUIDs\" : [\n");
        foreach(AssetMetaData aAsset in mCurrentAssets[AssetMetaData.AssetType.PrefabAsset])
        {
            string aAssetPath = AssetDatabase.GUIDToAssetPath(aAsset.mGUID);
            if (AssetDatabase.GetMainAssetTypeAtPath(aAssetPath) != typeof(Item))
            {
                continue;
            }
            StringBuilder aItemJSON = new StringBuilder("{\n");
            Item aItem = AssetDatabase.LoadAssetAtPath<Item>(aAssetPath);
            string aItemFilePath = "/Assets/Resources/Prefabs/" + aItem.mName + ".json";
            aAsset.mAssetFilePath = ".." + aItemFilePath;
            if(!IsAssetInResources(Application.dataPath + aItemFilePath))
            {
                aItemJSON.Append("\"name\" : \"" + aItem.mName + "\",\n");
                Pickable aPickable = new Pickable();
                aPickable.mType = aItem.mItemType;
                GPolygonCollider aCollider = new GPolygonCollider(true);
                GSprite aSprite = new GSprite(aItem.mDisplaySprite.rect, aItem.mTextureGUID,
                    aItem.mDisplaySprite.texture.height, (int)aItem.mRenderLayer);
                SaveAssetData(GetAssetFromGUID(aItem.mTextureGUID, AssetMetaData.AssetType.TextureAsset), ref aItemsSB);
                aItemJSON.Append("\"Components\" : [\n");
                aItemJSON.Append(aSprite.ToString() + ",\n");
                aItemJSON.Append(aPickable.ToString() + ",\n");
                aItemJSON.Append(aCollider.ToString() + "\n");
                aItemJSON.Append("]\n");
                aItemJSON.Append("}\n");
                SaveToFile(aItemJSON.ToString(), Application.dataPath + aItemFilePath);
                SaveToFile(aAsset.ToString(), Application.dataPath + aItemFilePath + ".json");
            }
            if(!IsPathInResources(aItemsSB, aAsset.mAssetFilePath + ".json"))
            {
                aItemsSB.Append("\"" + aAsset.mAssetFilePath + ".json\",");
                aItemGUIDs.Append("\"" + aAsset.mGUID + "\",");
            }
        }
        aItemsSB.Remove(aItemsSB.Length - 1, 1);
        aItemsSB.Append("]\n");
        aItemGUIDs.Remove(aItemGUIDs.Length - 1, 1);
        aItemGUIDs.Append("]\n");
        return new[] { aItemsSB.ToString(), aItemGUIDs.ToString() };
    }
    static StringBuilder GetEndPositionJson(Level.GamePositions pEndPosition, ref StringBuilder pResources)
    {
        StringBuilder aEndPositionGobj = new StringBuilder("{");
        aEndPositionGobj.Append("\"name\" : \"End Position\",\n");
        aEndPositionGobj.Append("\"Components\" : [\n");
        GTransform aTransform = new GTransform();
        aTransform.Position = new position(pEndPosition.mWorldPosition.x * 64.00001f, pEndPosition.mWorldPosition.y * 64.00001f);
        aEndPositionGobj.Append(aTransform.ToString());
        aEndPositionGobj.Append(",\n");
        string aSpGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(pEndPosition.mDisplaySprite));
        GSprite aSprite = new GSprite(pEndPosition.mDisplaySprite.rect, aSpGUID, 
            pEndPosition.mDisplaySprite.texture.height, (int)Level.LayerTypes.Players);
        SaveAssetData(GetAssetFromGUID(aSpGUID, AssetMetaData.AssetType.TextureAsset), ref pResources);
        aEndPositionGobj.Append(aSprite.ToString() + ",\n");
        GPolygonCollider aPolygonCollider = new GPolygonCollider(true);
        aEndPositionGobj.Append(aPolygonCollider.ToString() +",\n");
        Teleporter aTeleporter = new Teleporter();
        aEndPositionGobj.Append(aTeleporter.ToString());
        aEndPositionGobj.Append("]\n");
        aEndPositionGobj.Append("}");
        return aEndPositionGobj;
    }
    #region CreatingGameScriptableJSONs
    static StringBuilder GetGameScriptableJson(Vector2Int pPosition, GameScriptable pScriptable, ref StringBuilder pResources)
    {
        StringBuilder aGameScriptable = new StringBuilder("{\n");
        aGameScriptable.Append("\"name\" : \"" + pScriptable.mName + "\",\n");
        aGameScriptable.Append("\"Components\" : [\n");
        GTransform aTransform = new GTransform();
        aTransform.Position = new position(pPosition.x * 64.00001f, pPosition.y * 64.00001f);
        aGameScriptable.Append(aTransform.ToString() + ",\n");
        switch (pScriptable.mType)
        {
            case GameScriptable.ObjectType.SpawnFactory:
                aGameScriptable.Append(GetSpawnFactoryJson((SpawnFactory)pScriptable, ref pResources));
                break;
            case GameScriptable.ObjectType.StaticObject:
                aGameScriptable.Append(GetStaticObjectJson((StaticObject)pScriptable, ref pResources));
                break;
        }
        aGameScriptable.Append("]\n");
        aGameScriptable.Append("}");
        return aGameScriptable;
    }
    static StringBuilder GetStaticObjectJson(StaticObject pObject, ref StringBuilder pResources)
    {
        StringBuilder aObjectComponents = new StringBuilder();
        GSprite aSprite = new GSprite(pObject.mDisplaySprite.rect, pObject.mTextureGUID, 
            pObject.mDisplaySprite.texture.height, (int)pObject.mRenderLayer);
        SaveAssetData(GetAssetFromGUID(pObject.mTextureGUID, AssetMetaData.AssetType.TextureAsset), ref pResources);
        aObjectComponents.Append(aSprite.ToString());
        if(pObject.mColliderType == GameScriptable.ColliderType.Box)
        {
            GPolygonCollider aCollider = new GPolygonCollider(pObject.mIsTrigger);
            aObjectComponents.Append(",\n" + aCollider.ToString());
        }
        else if(pObject.mColliderType == GameScriptable.ColliderType.Circle)
        {
            GCircleCollider aCollider = new GCircleCollider(32.00001f, pObject.mIsTrigger);
            aObjectComponents.Append(",\n" + aCollider.ToString());
        }
        return aObjectComponents;
    }
    static StringBuilder GetSpawnFactoryJson(SpawnFactory pFactory, ref StringBuilder pResources)
    {
        StringBuilder aFactoryComponents = new StringBuilder();
        GSprite aSprite = new GSprite(pFactory.mDisplaySprite.rect, pFactory.mTextureGUID,
            pFactory.mDisplaySprite.texture.height, (int)pFactory.mRenderLayer);
        SaveAssetData(GetAssetFromGUID(pFactory.mTextureGUID, AssetMetaData.AssetType.TextureAsset), ref pResources);
        aFactoryComponents.Append(aSprite.ToString() + ",\n");
        GPolygonCollider aCollider = new GPolygonCollider(pFactory.mIsTrigger);
        aFactoryComponents.Append(aCollider.ToString() + ",\n");
        SFactory aFactory = new SFactory(pFactory.mEnemyGUID, pFactory.mPoolCount, pFactory.mSpawnTime);
        SaveEnemyPrefab(GetAssetFromGUID(pFactory.mEnemyGUID, AssetMetaData.AssetType.PrefabAsset), ref pResources);
        aFactoryComponents.Append(aFactory.ToString());
        return aFactoryComponents;
    }
    #endregion
    static bool IsAssetInResources(string pAssetPath)
    {
        return File.Exists(pAssetPath);
    }
    static bool IsPathInResources(StringBuilder pResources, string pAssetPath)
    {
        return pResources.ToString().Contains(pAssetPath);
    }
    static void CreateLevelFolder()
    {
        if(!AssetDatabase.IsValidFolder("Assets/Assets"))
        {
            AssetDatabase.CreateFolder("Assets", "Assets");
        }
        if(!AssetDatabase.IsValidFolder("Assets/Assets/Level Data"))
        {
            AssetDatabase.CreateFolder("Assets/Assets", "Level Data");
        }
    }
    static void CreateResourcesFolder()
    {
        if(!AssetDatabase.IsValidFolder("Assets/Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets/Assets", "Resources");
            AssetDatabase.CreateFolder("Assets/Assets/Resources", "Textures");
            AssetDatabase.CreateFolder("Assets/Assets/Resources", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Assets/Resources", "Audios");
            AssetDatabase.CreateFolder("Assets/Assets/Resources", "Fonts");
        }
    }
    static void SaveToFile(string pData, string pPath)
    {
        try
        {
            using(StreamWriter aWriter = new StreamWriter(pPath,false))
            {
                aWriter.WriteLine(pData);
            }
        }
        catch(System.Exception aE)
        {
            Debug.LogError(aE.Message + "\n" + aE.StackTrace);
        }
    }
}
