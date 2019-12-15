#pragma once
#ifndef _PLAYER_H_
#define _PLAYER_H_
#include "Component.h"
class Player : public Component
{
	DECLARE_DYNAMIC_DERIVED_CLASS(Player, Component);
	std::string mProjectileGUID = "";
	STRCODE mProjectileSTRCode = -1;
	float mSpeed = 0;
	float mMaxHealth = 0;
	float mHealth = 0;
	int mPoolCount = 0;
	std::list<GameObject*> mAvailableProjectiles;
	std::list<GameObject*> mUnavailableProjectiles;
protected:
	virtual void initialize() override;
	virtual void load(json::JSON&) override;
	virtual void update(float deltaTime) override;
public:
	void addProjectileToPool(GameObject*);
	Player() = default;
	~Player();
};
#endif