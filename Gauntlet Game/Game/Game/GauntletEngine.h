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
	std::list<STRCODE> mItemIDs;
	std::map<int, std::string> mLevels;
	Player* mMainPlayer = nullptr;
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
	inline void SetState(State pState) { mState = pState; }

public:
	inline void setPlayer(Player* pPlayer) { mMainPlayer = pPlayer; }
	const sf::Vector2f& getPlayerPosition();
	inline const State& GetState() { return mState; }
};

#endif