#pragma once
#ifndef _UI_MANAGER_H_
#define _UI_MANAGER_H_
#include "ISystem.h"
#include "GauntletEngine.h"
class Component;
class Text;
class UIManager : public ISystem
{
	DECLARE_SINGLETON(UIManager)
	friend class GauntletEngine;
	void registerClasses();
	std::map<GauntletEngine::State, std::map<std::string,Component*>> mUIMap;
	GauntletEngine::State mPrevState = GauntletEngine::State::None;
	Text* mTimer = nullptr;
protected:
	// Inherited via ISystem
	virtual void initialize() override;
	void printHUD(int& pHScore,int& pKills, float& pHealth);
	virtual void update(float deltaTime) override;

};
#endif