#pragma once
#ifndef _ENEMY_H_
#define _ENEMY_H_
#include "Component.h"
class Enemy : public Component
{
	DECLARE_DYNAMIC_DERIVED_CLASS(Enemy,Component)
public:
	enum Type {
		Collider,
		CloseRange,
		ProjectileThrower
	};
private:
	std::string mProjectileGUID = "";
	STRCODE mProjectileSTRCode = -1;
	int mPoolCount = 0;
	float mSpeed = 0;
	float mStopRange = 0;
	float mFireTime = 0;
	Type mType = Type::Collider;
	std::list<GameObject*> mAvailableProjectiles;
	std::list<GameObject*> mUnavailableProjectiles;
protected:
	virtual void initialize() override;
	virtual void load(json::JSON&) override;
	virtual void update(float deltaTime) override;
public:
	void addProjectileToPool(GameObject*);
	Enemy() = default;
	~Enemy();
};
#endif