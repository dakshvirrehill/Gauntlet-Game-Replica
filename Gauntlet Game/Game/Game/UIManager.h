#pragma once
#include "ISystem.h"
class UIManager : public ISystem
{
	DECLARE_SINGLETON(UIManager)
	friend class GauntletEngine;

protected:
	// Inherited via ISystem
	virtual void initialize() override;

	virtual void update(float deltaTime) override;

};

