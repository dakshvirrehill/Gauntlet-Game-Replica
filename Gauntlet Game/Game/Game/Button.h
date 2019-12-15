#pragma once
#ifndef _BUTTON_H_
#define _BUTTON_H_
#include "Component.h"
class Button : public Component
{
	friend class UIManager;
	DECLARE_DYNAMIC_DERIVED_CLASS(Button,Component)
	sf::Vector2f mSize;
	sf::Vector2f mOffset;
protected:
	std::function<void()> mClickEvent;
	virtual void initialize() override;
	virtual void update(float deltaTime) override;
public:
	virtual void load(json::JSON&) override;
};

#endif