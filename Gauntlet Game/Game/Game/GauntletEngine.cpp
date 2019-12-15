#include "GameCore.h"
#include "Transform.h"
#include "GameObject.h"
#include "CameraManager.h"
#include "Player.h"
#include "Projectile.h"
#include "Enemy.h"
#include "GauntletEngine.h"

void GauntletEngine::registerClasses()
{
	REGISTER_DYNAMIC_CLASS(Player)
	REGISTER_DYNAMIC_CLASS(CameraManager)
	REGISTER_DYNAMIC_CLASS(Projectile)
	REGISTER_DYNAMIC_CLASS(Enemy)
}

void GauntletEngine::initialize()
{
	registerClasses();

}

void GauntletEngine::update(float deltaTime)
{

}

void GauntletEngine::StartGame(bool pLoadMode)
{
	if (pLoadMode)
	{

	}
	else
	{

	}
}

const sf::Vector2f& GauntletEngine::getPlayerPosition()
{
	if (mMainPlayer == nullptr)
	{
		return sf::Vector2f();
	}
	else
	{
		return mMainPlayer->getGameObject()->getTransform()->getPosition();
	}
}
