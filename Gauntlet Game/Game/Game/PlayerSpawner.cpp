#include "GameCore.h"
#include "PrefabAsset.h"
#include "GameObject.h"
#include "PlayerSpawner.h"
IMPLEMENT_DYNAMIC_CLASS(PlayerSpawner)
void PlayerSpawner::initialize()
{
	if (!isEnabled())
	{
		return;
	}
	Component::initialize();
	if (mPlayerStrCode > 0)
	{
		PrefabAsset* aPlayer = static_cast<PrefabAsset*>(AssetManager::instance().GetAssetBySTRCODE(mPlayerStrCode));
		GameObject* aGobj = GameObjectManager::instance().instantiatePrefab(aPlayer);
		aGobj->getTransform()->setPosition(getGameObject()->getTransform()->getPosition());
		GameObjectManager::instance().removeGameObject(getGameObject());
	}
}

void PlayerSpawner::load(json::JSON& pSpawnerNode)
{
	Component::load(pSpawnerNode);
	if (pSpawnerNode.hasKey("mPlayerPrefabGUID"))
	{
		mPlayerPrefabGUID = pSpawnerNode["mPlayerPrefabGUID"].ToString();
		mPlayerStrCode = GUIDToSTRCODE(mPlayerPrefabGUID);
	}
}

void PlayerSpawner::update(float deltaTime)
{

}
