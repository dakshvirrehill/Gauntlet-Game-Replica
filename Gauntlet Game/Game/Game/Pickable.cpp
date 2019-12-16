#include "GameCore.h"
#include "GameObject.h"
#include "Pickable.h"
#include "ICollidable.h"
#include "GauntletEngine.h"
IMPLEMENT_DYNAMIC_CLASS(Pickable)

void Pickable::onTriggerEnter(const Collision* const collisionData)
{
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}
	int otherColliderIx = 1;
	if (collisionData->colliders[otherColliderIx] == nullptr)
	{
		otherColliderIx = 0;
	}
	if (collisionData->colliders[otherColliderIx] == nullptr)
	{
		return;
	}
	if (collisionData->colliders[otherColliderIx]->getGameObject() == getGameObject())
	{
		otherColliderIx = 0;
	}
	GameObject* aOther = collisionData->colliders[otherColliderIx]->getGameObject();
	if (aOther->getComponent("Player") != nullptr)
	{
		getGameObject()->setEnabled(false);
	}
}

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
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}

}
