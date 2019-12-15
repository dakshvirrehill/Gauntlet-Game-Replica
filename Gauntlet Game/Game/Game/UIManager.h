#pragma once
#ifndef _UI_MANAGER_H_
#define _UI_MANAGER_H_
#include "ISystem.h"
#include "GauntletEngine.h"
class Component;
class UIManager : public ISystem
{
	DECLARE_SINGLETON(UIManager)
	friend class GauntletEngine;
	void registerClasses();
	std::map<GauntletEngine::State, std::map<std::string,Component*>> mUIMap;
	GauntletEngine::State mPrevState = GauntletEngine::State::None;
protected:
	// Inherited via ISystem
	virtual void initialize() override;

	virtual void update(float deltaTime) override;

};
#endif