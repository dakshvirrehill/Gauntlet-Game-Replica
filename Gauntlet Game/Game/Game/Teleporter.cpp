#include "GameCore.h"
#include "GameObject.h"
#include "Teleporter.h"

IMPLEMENT_DYNAMIC_CLASS(Teleporter)
void Teleporter::initialize()
{
	if (!isEnabled())
	{
		return;
	}
	mPlayerGObj = GameObjectManager::instance().getGameObjectWithComponent(std::string("Player"));
}

void Teleporter::load(json::JSON& pNode)
{
	Component::load(pNode);
}

void Teleporter::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !isEnabled())
	{
		return;
	}

}
