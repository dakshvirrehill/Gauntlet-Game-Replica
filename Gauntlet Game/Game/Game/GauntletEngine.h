#pragma once
#ifndef _GAUNTLET_ENGINE_H_
#define _GAUNTLET_ENGINE_H_
#include "ISystem.h"
class GauntletEngine final : public ISystem
{
	DECLARE_SINGLETON(GauntletEngine)
	friend class GameEngine;
	friend class UIManager;
	std::list<STRCODE> mItemIDs;
	std::map<int, std::string> mLevels;
public:
	enum State
	{
		None,
		MainMenu,
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
	void StartGame(bool pLoadMode);


public:
	inline const State& GetState() { return mState; }
};

#endif