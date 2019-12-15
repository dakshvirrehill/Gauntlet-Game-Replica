#include "GameCore.h"
#include "GameObject.h"
#include "Enemy.h"
#include "Projectile.h"
IMPLEMENT_DYNAMIC_CLASS(Projectile)
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
		mSpeed = pProjectileNode["mSpeed"].ToFloat();
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
