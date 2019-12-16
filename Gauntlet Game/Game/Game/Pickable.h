#pragma once
#ifndef _PICKABLE_H_
#define _PICKABLE_H_
#include "Component.h"
class Pickable : public Component
{
	DECLARE_DYNAMIC_DERIVED_CLASS(Pickable,Component)
public:
	enum Type {
		HealthBooster,
		ScoreMultiplier,
		Invincibility
	};
	Type mType = Type::HealthBooster;
	Pickable() = default;
	~Pickable() = default;
private:
	virtual void onTriggerEnter(const Collision* const collisionData);
protected:
	virtual void initialize() override;
	virtual void load(json::JSON&) override;
	virtual void update(float deltaTime) override;
};
#endif