#include "GameCore.h"
#include "GameObject.h"
#include "GauntletEngine.h"
#include "PrefabAsset.h"
#include "Projectile.h"
#include "Enemy.h"
IMPLEMENT_DYNAMIC_CLASS(Enemy)
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
				Projectile* aProjectile = static_cast<Projectile*>(aGameObject->getComponent("Projectile"));
				mPoolCount = aProjectile->getPoolCount();
				mAvailableProjectiles.push_back(aGameObject);
				aProjectile->setEnemy(this);
				for (int aI = mAvailableProjectiles.size(); aI < mPoolCount; aI++)
				{
					GameObject* aGameObject = new GameObject();
					aGameObject->load(aPrefabAsset->getPrefab());
					aGameObject->setEnabled(false);
					Projectile* aProjectile = static_cast<Projectile*>(aGameObject->getComponent("Projectile"));
					aProjectile->setEnemy(this);
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
			mStopRange = pEnemy["mStopRange"].ToFloat();
		}
	}
	if (pEnemy.hasKey("mSpeed"))
	{
		mSpeed = pEnemy["mSpeed"].ToFloat();
		mFireTime = mSpeed * 0.5f;
	}
}

void Enemy::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !isEnabled())
	{
		return;
	}
	sf::Vector2f aMovementVector = GauntletEngine::instance().getPlayerPosition() - getGameObject()->getTransform()->getPosition();
	if (mType == Enemy::Type::ProjectileThrower)
	{
		mFireTime -= deltaTime;
		float aLen = sqrt(aMovementVector.x * aMovementVector.x + aMovementVector.y * aMovementVector.y);
		aMovementVector = aMovementVector / aLen;
		sf::Vector2f aFinalPos = aMovementVector + getGameObject()->getTransform()->getPosition();
		aFinalPos = GauntletEngine::instance().getPlayerPosition() - aFinalPos;
		if ((aFinalPos.x * aFinalPos.x + aFinalPos.y * aFinalPos.y) <= (mStopRange * mStopRange))
		{
			if (mFireTime <= 0)
			{
				mFireTime = mSpeed * 0.5f;
				GameObject* aProjectile = mAvailableProjectiles.back();
				mAvailableProjectiles.pop_back();
				mUnavailableProjectiles.push_back(aProjectile);
				Projectile* aProj = static_cast<Projectile*>(aProjectile->getComponent("Projectile"));
				aProj->setMovePosition(GauntletEngine::instance().getPlayerPosition() - getGameObject()->getTransform()->getPosition());
				aProjectile->getTransform()->setPosition(getGameObject()->getTransform()->getPosition());
				aProjectile->setEnabled(true);
			}
			return;
		}
	}
	getGameObject()->getTransform()->translate(aMovementVector * mSpeed * deltaTime);
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
		GameObjectManager::instance().removeGameObject(pProjectile);
		return;
	}
	pProjectile->setEnabled(false);
	mUnavailableProjectiles.remove(pProjectile);
	mAvailableProjectiles.push_back(pProjectile);
}

