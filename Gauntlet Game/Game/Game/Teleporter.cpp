#include "GameCore.h"
#include "GameObject.h"
#include "ICollidable.h"
#include "Player.h"
#include "Teleporter.h"
#include "GauntletEngine.h"

IMPLEMENT_DYNAMIC_CLASS(Teleporter)
void Teleporter::onTriggerEnter(const Collision* const collisionData)
{
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}

	int otherColliderIx = 1;
	otherColliderIx = 1;
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

	Player* aPlayer = dynamic_cast<Player*>(collisionData->colliders[otherColliderIx]->getGameObject()->getComponent("Player"));
	if (aPlayer != nullptr)
	{
		getGameObject()->setEnabled(false);
		GauntletEngine::instance().completeLevel();
	}
}
void Teleporter::initialize()
{
	if (!isEnabled())
	{
		return;
	}
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
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}

}
