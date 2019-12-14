#include "GameCore.h"
#include "Button.h"
#include "Rectangle.h"
#include "UIManager.h"

void UIManager::registerClasses()
{
	REGISTER_DYNAMIC_CLASS(Rectangle)
	REGISTER_DYNAMIC_CLASS(Button)
}

void UIManager::initialize()
{
	registerClasses();

}

void UIManager::update(float deltaTime)
{
}
