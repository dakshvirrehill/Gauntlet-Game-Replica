#include "GameCore.h"
#include "GameObject.h"
#include "PrefabAsset.h"
#include "Enemy.h"
#include "SpawnFactory.h"
#include "ICollidable.h"
#include "Projectile.h"
#include "GauntletEngine.h"

IMPLEMENT_DYNAMIC_CLASS(SpawnFactory)

void SpawnFactory::increasePool(int pAmount)
{
	PrefabAsset* aPrefabAsset = static_cast<PrefabAsset*>(AssetManager::instance().GetAssetBySTRCODE(mEnemyStrCode));
	if (aPrefabAsset != nullptr)
	{
		mPoolCount += pAmount;
		for (int aI = 0; aI < pAmount; aI ++)
		{
			GameObject* aGameObject = new GameObject();
			aGameObject->load(aPrefabAsset->getPrefab());
			aGameObject->setEnabled(false);
			GameObjectManager::instance().addGameObject(aGameObject);
			mAvailableEnemies.push_back(aGameObject);
			Enemy* aEnemy = static_cast<Enemy*>(aGameObject->getComponent("Enemy"));
			aEnemy->mFactory = this;
		}
	}
}

void SpawnFactory::onTriggerEnter(const Collision* const collisionData)
{
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}

	int otherColliderIx = 1;
	if (collisionData->colliders[otherColliderIx] == nullptr)
	{
		otherColliderIx = 0;
	}
	if (collisionData->colliders[otherColliderIx] == nullptr)
	{
		return;
	}
	if (collisionData->colliders[otherColliderIx]->getGameObject() == getGameObject())
	{
		otherColliderIx = 0;
	}
	GameObject* aOther = collisionData->colliders[otherColliderIx]->getGameObject();
	Projectile* aProj = static_cast<Projectile*>(aOther->getComponent("Projectile"));
	if (aProj != nullptr)
	{
		if (aProj->mPlayer != nullptr)
		{
			getGameObject()->setEnabled(false);
			PrefabAsset* aItem = static_cast<PrefabAsset*>(AssetManager::instance().GetAssetBySTRCODE( 
				GauntletEngine::instance().getRandomItemGUID()
			));
			if (aItem != nullptr)
			{
				GameObject* aGObj = GameObjectManager::instance().instance().instantiatePrefab(aItem);
				aGObj->getTransform()->setPosition(getGameObject()->getTransform()->getPosition());
			}
			GauntletEngine::instance().removeFactory();
			GauntletEngine::instance().addScore(5);
		}
	}
}

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
				GameObjectManager::instance().addGameObject(aGameObject);
				mAvailableEnemies.push_back(aGameObject);
				Enemy* aEnemy = static_cast<Enemy*>(aGameObject->getComponent("Enemy"));
				aEnemy->mFactory = this;
			}
		}
	}


}

void SpawnFactory::load(json::JSON& pSpawnFactoryNode)
{
	Component::load(pSpawnFactoryNode);
	if (pSpawnFactoryNode.hasKey("mPoolCount"))
	{
		mPoolCount = pSpawnFactoryNode["mPoolCount"].ToInt();
	}
	if (pSpawnFactoryNode.hasKey("mMinSpawnTime"))
	{
		mMinSpawnTime = pSpawnFactoryNode["mMinSpawnTime"].ToFloat();
	}
	if (pSpawnFactoryNode.hasKey("mMaxSpawnTime"))
	{
		mMaxSpawnTime = pSpawnFactoryNode["mMaxSpawnTime"].ToFloat();
	}
	if (pSpawnFactoryNode.hasKey("mEnemyGUID"))
	{
		mEnemyGUID = pSpawnFactoryNode["mEnemyGUID"].ToString();
		mEnemyStrCode = GUIDToSTRCODE(mEnemyGUID);
	}
}

void SpawnFactory::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !isEnabled())
	{
		return;
	}
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}
	mSpawnTime -= deltaTime;
	if (mSpawnTime <= 0.f)
	{
		float aPercentage = Random.Random();
		mSpawnTime = (((aPercentage - 0) * (mMaxSpawnTime - 0)) / (1 - 0)) + mMinSpawnTime;
		if (mAvailableEnemies.size() <= 0)
		{
			increasePool(5);
		}
		GameObject* aEnemy = mAvailableEnemies.back();
		mAvailableEnemies.pop_back();
		mUnavailableEnemies.push_back(aEnemy);
		aEnemy->getTransform()->setPosition(getGameObject()->getTransform()->getPosition() + 
			sf::Vector2f(64.f,-64.f));
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
		pEnemy->setEnabled(false);
		return;
	}
	pEnemy->setEnabled(false);
	mUnavailableEnemies.remove(pEnemy);
	mAvailableEnemies.push_back(pEnemy);
}

SpawnFactory::SpawnFactory()
{
	GauntletEngine::instance().addFactory();
}




