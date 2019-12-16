#pragma once
#ifndef _PROJECTILE_H_
#define _PROJECTILE_H_
#include "Component.h"
class Player;
class Enemy;
class Projectile : public Component
{
	DECLARE_DYNAMIC_DERIVED_CLASS(Projectile,Component)
	float mSpeed = 0;
	int mPoolCount = 0;
	sf::Vector2f mMovementVector;
	virtual void onTriggerEnter(const Collision* const collisionData);
protected:
	virtual void initialize() override;
	virtual void load(json::JSON&) override;
	virtual void update(float deltaTime) override;
public:
	Player* mPlayer = nullptr;
	Enemy* mEnemy = nullptr;
	Projectile() = default;
	~Projectile();
	void setMovePosition(sf::Vector2f pMoveVector);
	inline int getPoolCount() { return mPoolCount; }
	inline void setPlayer(Player* pPlayer) { mPlayer = pPlayer; }
	inline void setEnemy(Enemy* pEnemy) { mEnemy = pEnemy; }
};

#endif