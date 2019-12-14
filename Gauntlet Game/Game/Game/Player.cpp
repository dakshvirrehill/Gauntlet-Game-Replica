#include "GameCore.h"
#include "GameObject.h"
#include "Player.h"
IMPLEMENT_DYNAMIC_CLASS(Player)

void Player::initialize()
{
	if (!isEnabled())
	{
		return;
	}
	Component::initialize();
}

void Player::load(json::JSON& pPlayerNode)
{
	Component::load(pPlayerNode);
}

void Player::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !isEnabled())
	{
		return;
	}

}
