#include "GameCore.h"
#include "GameObject.h"
#include "Enemy.h"
#include "Player.h"
#include "ICollidable.h"
#include "Projectile.h"
#include "GauntletEngine.h"
IMPLEMENT_DYNAMIC_CLASS(Projectile)
void Projectile::onTriggerEnter(const Collision* const collisionData)
{
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}

	int otherColliderIx = 1;
	otherColliderIx = 1;
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
	if (mPlayer != nullptr)
	{
		if (aOther == mPlayer->getGameObject())
		{
			return;
		}
		mPlayer->addProjectileToPool(getGameObject());
	}
	else if (mEnemy != nullptr)
	{
		if (collisionData->colliders[otherColliderIx]->getGameObject() == mEnemy->getGameObject())
		{
			return;
		}
		mEnemy->addProjectileToPool(getGameObject());
	}
	else
	{
		GameObjectManager::instance().removeGameObject(getGameObject());
	}
}
void Projectile::initialize()
{
	if (!isEnabled())
	{
		return;
	}
	Component::initialize();
}

void Projectile::load(json::JSON& pProjectileNode)
{
	Component::load(pProjectileNode);
	if (pProjectileNode.hasKey("mSpeed"))
	{
		mSpeed = pProjectileNode["mSpeed"].ToFloat() * 3.f;
	}
	if (pProjectileNode.hasKey("mPoolCount"))
	{
		mPoolCount = pProjectileNode["mPoolCount"].ToInt();
	}
}

void Projectile::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !isEnabled())
	{
		return;
	}
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}

	getGameObject()->getTransform()->translate(mMovementVector * mSpeed * deltaTime);
}

Projectile::~Projectile()
{

}


void Projectile::setMovePosition(sf::Vector2f pMoveVector)
{
	float aLen = sqrtf(pMoveVector.x * pMoveVector.x + pMoveVector.y * pMoveVector.y);
	mMovementVector = sf::Vector2f(pMoveVector.x / aLen, pMoveVector.y / aLen);
}
