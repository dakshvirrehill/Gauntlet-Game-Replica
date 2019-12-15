#include "GameCore.h"
#include "Transform.h"
#include "GameObject.h"
#include "CameraManager.h"
#include "Player.h"
#include "Projectile.h"
#include "Enemy.h"
#include "Pickable.h"
#include "PlayerSpawner.h"
#include "Teleporter.h"
#include "SpawnFactory.h"
#include "UIManager.h"
#include "GauntletEngine.h"

void GauntletEngine::registerClasses()
{
	REGISTER_DYNAMIC_CLASS(Player)
	REGISTER_DYNAMIC_CLASS(CameraManager)
	REGISTER_DYNAMIC_CLASS(Projectile)
	REGISTER_DYNAMIC_CLASS(Enemy)
	REGISTER_DYNAMIC_CLASS(Pickable)
	REGISTER_DYNAMIC_CLASS(PlayerSpawner)
	REGISTER_DYNAMIC_CLASS(Teleporter)
	REGISTER_DYNAMIC_CLASS(SpawnFactory)
}

void GauntletEngine::initialize()
{
	registerClasses();
	std::string aAssetPath = "../Assets/GauntletGame.json";
	json::JSON aGauntletGameNode = FileSystem::instance().load(aAssetPath, false);
	if (aGauntletGameNode.hasKey("Levels"))
	{
		for (auto& aLevelObj : aGauntletGameNode["Levels"].ObjectRange())
		{
			mLevels.emplace(std::stoi(aLevelObj.first), aLevelObj.second.ToString());
		}
	}
	if (aGauntletGameNode.hasKey("mItemGUIDs"))
	{
		for (auto& aItemId : aGauntletGameNode["mItemGUIDs"].ArrayRange())
		{
			std::string aGUID = aItemId.ToString();
			mItemIDs.push_back(GUIDToSTRCODE(aGUID));
		}
	}
	mState = State::MainMenu;
	UIManager::instance().initialize();
}

void GauntletEngine::update(float deltaTime)
{
	UIManager::instance().update(deltaTime);
}

void GauntletEngine::StartGame()
{
	if (GauntletEngine::instance().mState == State::GamePlay)
	{
		return;
	}
	GauntletEngine::instance().mState = State::GamePlay;
	FileSystem::instance().load(GauntletEngine::instance().mLevels[GauntletEngine::instance().mCurrentLevel], true);
}

void GauntletEngine::LoadGame()
{

}

void GauntletEngine::ExitGame()
{

}

void GauntletEngine::ContinueGame()
{

}

void GauntletEngine::SaveGame()
{

}

void GauntletEngine::ExitToMenu()
{

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
