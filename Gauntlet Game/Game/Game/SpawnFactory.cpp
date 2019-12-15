#include "GameCore.h"
#include "GameObject.h"
#include "PrefabAsset.h"
#include "Enemy.h"
#include "SpawnFactory.h"

IMPLEMENT_DYNAMIC_CLASS(SpawnFactory)

void SpawnFactory::initialize()
{
	if (!isEnabled())
	{
		return;
	}
	Component::initialize();

	if (mEnemyStrCode > 0)
	{
		PrefabAsset* aPrefabAsset = static_cast<PrefabAsset*>(AssetManager::instance().GetAssetBySTRCODE(mEnemyStrCode));
		if (aPrefabAsset != nullptr)
		{
			for (int aI = mAvailableEnemies.size(); aI < mPoolCount; aI++)
			{
				GameObject* aGameObject = new GameObject();
				aGameObject->load(aPrefabAsset->getPrefab());
				aGameObject->setEnabled(false);
				mAvailableEnemies.push_back(aGameObject);
			}
		}
	}


}

void SpawnFactory::load(json::JSON& pSpawnFactoryNode)
{
	Component::load(pSpawnFactoryNode);
}

void SpawnFactory::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !isEnabled())
	{
		return;
	}
	mSpawnTime -= deltaTime;
	if (mSpawnTime <= 0.f)
	{
		float aPercentage = Random.Random();
		mSpawnTime = (((aPercentage - 0) * (mMaxSpawnTime - 0)) / (1 - 0)) + mMinSpawnTime;
		//see distance if issue
		GameObject* aEnemy = mAvailableEnemies.back();
		mAvailableEnemies.pop_back();
		mUnavailableEnemies.push_back(aEnemy);
		aEnemy->getTransform()->setPosition(getGameObject()->getTransform()->getPosition() + sf::Vector2f(Random.Random() * 20, Random.Random() * 20));
		aEnemy->setEnabled(true);
	}
}

void SpawnFactory::addEnemyToPool(GameObject* pEnemy)
{
	if (pEnemy == nullptr)
	{
		return;
	}
	if (mUnavailableEnemies.size() <= 0)
	{
		GameObjectManager::instance().removeGameObject(pEnemy);
		return;
	}
	pEnemy->setEnabled(false);
	mUnavailableEnemies.remove(pEnemy);
	mAvailableEnemies.push_back(pEnemy);
}


