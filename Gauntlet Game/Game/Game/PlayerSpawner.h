#pragma once
#ifndef _PLAYER_SPAWNER_H_
#define _PLAYER_SPAWNER_H_
#include "Component.h"
class PlayerSpawner : public Component
{
	DECLARE_DYNAMIC_DERIVED_CLASS(PlayerSpawner, Component)
	std::string mPlayerPrefabGUID = "";
	STRCODE mPlayerStrCode = -1;
protected:
	virtual void initialize() override;
	virtual void load(json::JSON&) override;
	virtual void update(float deltaTime) override;
public:
	PlayerSpawner() = default;
	~PlayerSpawner() = default;
};

#endif