#pragma once
#include "Component.h"
class Teleporter : public Component
{
	DECLARE_DYNAMIC_DERIVED_CLASS(Teleporter, Component)
	void onTriggerEnter(const Collision* const collisionData) override;
protected:
	virtual void initialize() override;
	virtual void load(json::JSON&) override;
	virtual void update(float deltaTime) override;

public:
	Teleporter() = default;
	~Teleporter() = default;
};

