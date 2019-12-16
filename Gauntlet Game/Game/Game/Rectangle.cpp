#include "GameCore.h"
#include "Transform.h"
#include "GameObject.h"
#include "Rectangle.h"

IMPLEMENT_DYNAMIC_CLASS(Rectangle)

void Rectangle::load(json::JSON& pRectNode)
{
	Component::load(pRectNode);
	if (pRectNode.hasKey("Size"))
	{
		mSize = sf::Vector2f(pRectNode["Size"]["width"].ToFloat(), pRectNode["Size"]["height"].ToFloat());
	}
	if (pRectNode.hasKey("offset"))
	{
		mOffset = sf::Vector2f(pRectNode["offset"]["X"].ToFloat(), pRectNode["offset"]["Y"].ToFloat());
	}
	if (pRectNode.hasKey("color"))
	{
		mColor = sf::Color(pRectNode["color"]["r"].ToInt(), pRectNode["color"]["g"].ToInt()
				, pRectNode["color"]["b"].ToInt(), pRectNode["color"]["a"].ToInt());
	}
	IRenderable::loadLayer(pRectNode);
}

void Rectangle::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !enabled)
	{
		return;
	}
	updatePosition();
}

void Rectangle::updatePosition()
{
	mRectangle.setPosition(mOffset + getGameObject()->getTransform()->getPosition());
}

void Rectangle::render(sf::RenderWindow* _window)
{
	if (getGameObject() == nullptr)
	{
		return;
	}
	if (!getGameObject()->isEnabled() || !enabled)
	{
		return;
	}

	if (_window != nullptr)
	{
		_window->draw(mRectangle);
	}
}

void Rectangle::initialize()
{
	if (!isEnabled())
	{
		return;
	}
	Component::initialize();
	mRectangle.setFillColor(mColor);
	mRectangle.setSize(mSize);
	if (getGameObject() != nullptr)
	{
		mRectangle.setPosition(mOffset + getGameObject()->getTransform()->getPosition());
	}
	else
	{
		mRectangle.setPosition(sf::Vector2f());
	}
}
