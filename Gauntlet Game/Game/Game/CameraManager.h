#pragma once
#ifndef _CAMERA_MANAGER_H_
#define _CAMERA_MANAGER_H_
#include "Component.h"
class Player;
class CameraManager : public Component
{
	DECLARE_DYNAMIC_DERIVED_CLASS(CameraManager, Component)
	Player* mPlayer = nullptr;
protected:
	virtual void initialize() override;
	virtual void load(json::JSON&) override;
	virtual void update(float deltaTime) override;
public:
	void setPlayer(Player* pPlayer) { mPlayer = pPlayer; }
	void updatePosition();
};
#endif