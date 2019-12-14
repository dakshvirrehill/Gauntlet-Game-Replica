#pragma once
#ifndef _PLAYER_H_
#define _PLAYER_H_
#include "Component.h"
class Player : public Component
{
	DECLARE_DYNAMIC_DERIVED_CLASS(Player, Component);
protected:
	virtual void initialize() override;
	virtual void load(json::JSON&) override;
	virtual void update(float deltaTime) override;
};
#endif