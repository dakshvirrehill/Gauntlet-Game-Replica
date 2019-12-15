#include "GameCore.h"
#include "GameObject.h"
#include "Pickable.h"
IMPLEMENT_DYNAMIC_CLASS(Pickable)

void Pickable::initialize()
{
	if (!isEnabled())
	{
		return;
	}

	Component::initialize();

}

void Pickable::load(json::JSON& pPickableNode)
{
	Component::load(pPickableNode);
	if (pPickableNode.hasKey("mType"))
	{
		mType = (Type)pPickableNode["mType"].ToInt();
	}
}

void Pickable::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !isEnabled())
	{
		return;
	}

}
