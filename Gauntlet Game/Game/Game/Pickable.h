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
	Pickable() = default;
	~Pickable() = default;
private:
	Type mType = Type::HealthBooster;
protected:
	virtual void initialize() override;
	virtual void load(json::JSON&) override;
	virtual void update(float deltaTime) override;
};
#endif