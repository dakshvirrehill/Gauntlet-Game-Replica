#pragma once
#ifndef _SPAWN_FACTORY_H_
#define _SPAWN_FACTORY_H_
#include "Component.h"
class SpawnFactory : public Component
{
	DECLARE_DYNAMIC_DERIVED_CLASS(SpawnFactory, Component)
	int mPoolCount = 0;
	std::string mEnemyGUID = "";
	STRCODE mEnemyStrCode = -1;
	float mMinSpawnTime = 0;
	float mMaxSpawnTime = 0;
	float mSpawnTime = 0;
	std::list<GameObject*> mAvailableEnemies;
	std::list<GameObject*> mUnavailableEnemies;
protected:
	virtual void initialize() override;
	virtual void load(json::JSON&) override;
	virtual void update(float deltaTime) override;
public:
	void addEnemyToPool(GameObject*);
	SpawnFactory() = default;
	~SpawnFactory() = default;

};

#endif