#pragma once
#ifndef _RECTANGLE_H_
#define _RECTANGLE_H_
#include "Component.h"
#include "IRenderable.h"
class Rectangle : public Component, public IRenderable
{
	DECLARE_DYNAMIC_DERIVED_CLASS(Rectangle,Component)
	sf::RectangleShape mRectangle;
	sf::Vector2f mSize;
	sf::Vector2f mOffset;
	sf::Color mColor;
protected:
	virtual void initialize() override;
	virtual void update(float deltaTime) override;
	virtual void render(sf::RenderWindow* _window) override;
public:
	void updatePosition();
	virtual void load(json::JSON&) override;
};

#endif