#include "GameCore.h"
#include "CameraManager.h"
#include "Player.h"
#include "GauntletEngine.h"

void GauntletEngine::registerClasses()
{
	REGISTER_DYNAMIC_CLASS(Player)
	REGISTER_DYNAMIC_CLASS(CameraManager)
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
