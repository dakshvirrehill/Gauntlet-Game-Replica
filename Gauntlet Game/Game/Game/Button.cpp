#include "GameCore.h"
#include "Transform.h"
#include "GameObject.h"
#include "Button.h"

IMPLEMENT_DYNAMIC_CLASS(Button)

void Button::initialize()
{
	if (!isEnabled())
	{
		return;
	}
	Component::initialize();
}

void Button::load(json::JSON& pButtonNode)
{
	Component::load(pButtonNode);
	if (pButtonNode.hasKey("Size"))
	{
		mSize = sf::Vector2f(pButtonNode["Size"]["width"].ToFloat(), pButtonNode["Size"]["height"].ToFloat());
	}
	if (pButtonNode.hasKey("offset"))
	{
		mOffset = sf::Vector2f(pButtonNode["offset"]["X"].ToFloat(), pButtonNode["offset"]["Y"].ToFloat());
	}
}

void Button::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !enabled)
	{
		return;
	}
	if (InputManager::instance().getMouseButtonState(sf::Mouse::Left) == InputManager::PushState::Up)
	{
		sf::Vector2f aMousePosition = InputManager::instance().getMousePosition();
		sf::Vector2f aTopLeftPos = getGameObject()->getTransform()->getPosition() + mOffset;
		sf::Vector2f aBottomRightPos = aTopLeftPos + mSize;
		aMousePosition = RenderSystem::instance().getRenderWindow()->mapPixelToCoords(sf::Vector2i((int)aMousePosition.x, (int)aMousePosition.y));
		if (aMousePosition.x >= aTopLeftPos.x && aMousePosition.x <= aBottomRightPos.x)
		{
			if (aMousePosition.y >= aTopLeftPos.y && aMousePosition.y <= aBottomRightPos.y)
			{
				mClickEvent();
			}
		}
	}
}
