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
	if (GauntletEngine::instance().mState == State::GamePlay)
	{
		mTimer -= deltaTime;
		if (mTimer <= 0.0f)
		{
			gameOver();
		}
		if (InputManager::instance().getKeyState(sf::Keyboard::Tilde) == InputManager::PushState::Down)
		{
			pauseGame();
		}
	}
	UIManager::instance().update(deltaTime);
}

void GauntletEngine::StartGame()
{
	if (GauntletEngine::instance().mState == State::GamePlay)
	{
		return;
	}
	GauntletEngine::instance().mState = State::GamePlay;
	json::JSON aTimerNode = FileSystem::instance().load(GauntletEngine::instance().mLevels[GauntletEngine::instance().mCurrentLevel], true);
	if (aTimerNode.hasKey("Timer"))
	{
		GauntletEngine::instance().mTimer = aTimerNode["Timer"].ToFloat();
	}
}

void GauntletEngine::LoadGame()
{

}

void GauntletEngine::ExitGame()
{
	RenderSystem::instance().closeWindow();
}

void GauntletEngine::ContinueGame()
{
	GauntletEngine::instance().mState = State::GamePlay;
}

void GauntletEngine::SaveGame()
{

}

void GauntletEngine::ExitToMenu()
{
	instance().gameOver();
}

void GauntletEngine::pauseGame()
{
	mState = State::Paused;
}

void GauntletEngine::removeFactory()
{
	mActiveSpawnFactoryCount--;
	if (mActiveSpawnFactoryCount <= 0)
	{
		GameObject* aGOBJ = GameObjectManager::instance().getGameObjectWithComponent("Teleporter");
		if (aGOBJ != nullptr)
		{
			aGOBJ->setEnabled(true);
		}
	}
}

STRCODE GauntletEngine::getRandomItemGUID()
{
	return mItemIDs.at(Random.IRandom(0, mItemIDs.size() - 1));
}

void GauntletEngine::gameOver()
{
	FileSystem::instance().unload(mLevels[mCurrentLevel]);
	mState = State::MainMenu;
	mCurrentLevel = 0;
}

void GauntletEngine::addScore(int pScore)
{
	mKills += 1;
	mScore += mScoreMultiplier * pScore; 
	mHighScore = mHighScore < mScore ? mScore : mHighScore;
	UIManager::instance().printHUD(mHighScore, mKills, mMainPlayer->mHealth);
}

void GauntletEngine::setPlayer(Player* pPlayer)
{
	mMainPlayer = pPlayer;
	if (mMainPlayer != nullptr)
	{
		UIManager::instance().printHUD(
			GauntletEngine::instance().mHighScore,
			GauntletEngine::instance().mKills,
			GauntletEngine::instance().mMainPlayer->mHealth);
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

void GauntletEngine::completeLevel()
{
	FileSystem::instance().unload(mLevels[mCurrentLevel]);
	mCurrentLevel++;
	mScoreMultiplier = 1;
	if (mLevels.count(mCurrentLevel) == 0)
	{
		mState = State::MainMenu;
		mCurrentLevel = 0;
		return;
	}
	json::JSON aTimerNode = FileSystem::instance().load(mLevels[mCurrentLevel], true);
	if (aTimerNode.hasKey("Timer"))
	{
		GauntletEngine::instance().mTimer = aTimerNode["Timer"].ToFloat();
	}
}

std::string GauntletEngine::getTime()
{
	int aMins = (int)(mTimer / 60.f);
	int aSecs = (int)(mTimer - aMins * 60.f);
	std::string aTime = std::to_string(aMins) + " : " + std::to_string(aSecs);
	return aTime;
}
