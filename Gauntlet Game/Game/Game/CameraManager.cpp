#include "GameCore.h"
#include "GameObject.h"
#include "Transform.h"
#include "Player.h"
#include "CameraManager.h"
IMPLEMENT_DYNAMIC_CLASS(CameraManager)
void CameraManager::initialize()
{
	if (!isEnabled())
	{
		return;
	}
	Component::initialize();
}

void CameraManager::load(json::JSON& pCameraNode)
{
	Component::load(pCameraNode);
}

void CameraManager::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !isEnabled())
	{
		return;
	}
	updatePosition();

}

void CameraManager::updatePosition()
{
	if (mPlayer == nullptr)
	{
		return;
	}
	getGameObject()->getTransform()->setPosition(mPlayer->getGameObject()->getTransform()->getPosition());
}
