#include "GameCore.h"
#include "GameObject.h"
#include "GauntletEngine.h"
#include "PrefabAsset.h"
#include "Projectile.h"
#include "Enemy.h"
#include "ICollidable.h"
#include "SpawnFactory.h"
IMPLEMENT_DYNAMIC_CLASS(Enemy)
void Enemy::onTriggerEnter(const Collision* const collisionData)
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
			if (mFactory != nullptr)
			{
				mFactory->addEnemyToPool(getGameObject());
				GauntletEngine::instance().addScore((int)(mType + 1) * 2);
			}
		}
	}
}
void Enemy::onCollisionEnter(const Collision* const collisionData)
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
	if (mType == Type::Collider)
	{
		if (aOther->getComponent("Player") != nullptr)
		{
			if (mFactory != nullptr)
			{
				mFactory->addEnemyToPool(getGameObject());
			}
		}
	}
}
void Enemy::initialize()
{
	if (!isEnabled())
	{
		return;
	}
	Component::initialize();
	if (mType == Enemy::Type::ProjectileThrower)
	{
		if (mProjectileSTRCode > 0)
		{
			PrefabAsset* aPrefabAsset = static_cast<PrefabAsset*>(AssetManager::instance().GetAssetBySTRCODE(mProjectileSTRCode));
			if (aPrefabAsset != nullptr)
			{
				GameObject* aGameObject = new GameObject();
				aGameObject->load(aPrefabAsset->getPrefab());
				aGameObject->setEnabled(false);
				GameObjectManager::instance().addGameObject(aGameObject);
				Projectile* aProjectile = static_cast<Projectile*>(aGameObject->getComponent("Projectile"));
				mPoolCount = aProjectile->getPoolCount();
				mAvailableProjectiles.push_back(aGameObject);
				aProjectile->setEnemy(this);
				for (int aI = mAvailableProjectiles.size(); aI < mPoolCount; aI++)
				{
					GameObject* aGameObject = new GameObject();
					aGameObject->load(aPrefabAsset->getPrefab());
					aGameObject->setEnabled(false);
					GameObjectManager::instance().addGameObject(aGameObject);
					Projectile* aProjectile = static_cast<Projectile*>(aGameObject->getComponent("Projectile"));
					aProjectile->setEnemy(this);
					mAvailableProjectiles.push_back(aGameObject);
				}
			}
		}
	}
}

void Enemy::load(json::JSON& pEnemy)
{
	Component::load(pEnemy);
	if (pEnemy.hasKey("mType"))
	{
		mType = (Enemy::Type) pEnemy["mType"].ToInt();
	}
	if (mType == Enemy::Type::ProjectileThrower)
	{
		if (pEnemy.hasKey("mProjectileGUID"))
		{
			mProjectileGUID = pEnemy["mProjectileGUID"].ToString();
			mProjectileSTRCode = GUIDToSTRCODE(mProjectileGUID);
		}
		if (pEnemy.hasKey("mStopRange"))
		{
			mStopRange = pEnemy["mStopRange"].ToFloat() * 10;
		}
	}
	if (pEnemy.hasKey("mSpeed"))
	{
		mSpeed = pEnemy["mSpeed"].ToFloat() * 2.f;
		mFireTime = 1.f;
	}
}

void Enemy::update(float deltaTime)
{
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}

	if (!getGameObject()->isEnabled() || !isEnabled())
	{
		return;
	}
	sf::Vector2f aMovementVector = GauntletEngine::instance().getPlayerPosition() - getGameObject()->getTransform()->getPosition();
	float aLen = sqrt(aMovementVector.x * aMovementVector.x + aMovementVector.y * aMovementVector.y);
	aMovementVector = aMovementVector / aLen;
	aMovementVector *= mSpeed * deltaTime;
	getGameObject()->getTransform()->translate(aMovementVector);
	if (mType == Enemy::Type::ProjectileThrower)
	{
		mFireTime -= deltaTime;
		sf::Vector2f aFinalPos = getGameObject()->getTransform()->getPosition();
		aFinalPos = GauntletEngine::instance().getPlayerPosition() - aFinalPos;
		if ((aFinalPos.x * aFinalPos.x + aFinalPos.y * aFinalPos.y) <= (mStopRange * mStopRange))
		{
			if (mFireTime <= 0)
			{
				mFireTime = 1.5f;
				if (mAvailableProjectiles.size() <= 0)
				{
					return;
				}
				GameObject* aProjectile = mAvailableProjectiles.back();
				mAvailableProjectiles.pop_back();
				mUnavailableProjectiles.push_back(aProjectile);
				Projectile* aProj = static_cast<Projectile*>(aProjectile->getComponent("Projectile"));
				aProj->setMovePosition(GauntletEngine::instance().getPlayerPosition() - getGameObject()->getTransform()->getPosition());
				aProjectile->getTransform()->setPosition(getGameObject()->getTransform()->getPosition());
				aProjectile->setEnabled(true);
				for (auto& aComps : aProjectile->getAllComponents())
				{
					aComps.second->updatePosition();
				}
			}
			aFinalPos = getGameObject()->getTransform()->getPosition() - aMovementVector;
			getGameObject()->getTransform()->setPosition(aFinalPos);
		}
	}

}

Enemy::~Enemy()
{

}

void Enemy::addProjectileToPool(GameObject* pProjectile)
{
	if (pProjectile == nullptr)
	{
		return;
	}
	if (mUnavailableProjectiles.size() <= 0)
	{
		pProjectile->setEnabled(false);
		return;
	}
	pProjectile->setEnabled(false);
	mUnavailableProjectiles.remove(pProjectile);
	mAvailableProjectiles.push_back(pProjectile);
}

