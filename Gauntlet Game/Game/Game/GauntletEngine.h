#pragma once
#ifndef _GAUNTLET_ENGINE_H_
#define _GAUNTLET_ENGINE_H_
#include "ISystem.h"
class Player;
class GauntletEngine final : public ISystem
{
	DECLARE_SINGLETON(GauntletEngine)
	friend class GameEngine;
	friend class UIManager;
	int mCurrentLevel = 0;
	std::vector<STRCODE> mItemIDs;
	std::map<int, std::string> mLevels;
	int mKills = 0;
	int mScore = 0;
	int mHighScore = 0;
	int mScoreMultiplier = 1;
	Player* mMainPlayer = nullptr;
	int mActiveSpawnFactoryCount = 0;
	float mTimer;
public:
	enum State
	{
		None,
		MainMenu,
		GamePlay,
		Paused,
		GameOver
	};
private:
	State mState = State::None;
	void registerClasses();
protected:
	// Inherited via ISystem
	virtual void initialize() override;
	virtual void update(float deltaTime) override;
	static void StartGame();
	static void LoadGame();
	static void ExitGame();
	static void ContinueGame();
	static void SaveGame();
	static void ExitToMenu();
	void pauseGame();
	inline void SetState(State pState) { mState = pState; }

public:
	void addFactory() { mActiveSpawnFactoryCount++; }
	void removeFactory();
	STRCODE getRandomItemGUID();
	void gameOver();
	void addScore(int pScore);
	void doubleMultiplier() { mScoreMultiplier *= 2; }
	void setPlayer(Player* pPlayer);
	const sf::Vector2f& getPlayerPosition();
	inline const State& GetState() { return mState; }
	void completeLevel();
	std::string getTime();
};

#endif