#include "GameCore.h"
#include "GameObject.h"
#include "PrefabAsset.h"
#include "Projectile.h"
#include "Player.h"

IMPLEMENT_DYNAMIC_CLASS(Player)

void Player::initialize()
{
	if (!isEnabled())
	{
		return;
	}
	Component::initialize();
	if (mProjectileSTRCode > 0)
	{
		PrefabAsset* aPrefabAsset = static_cast<PrefabAsset*>(AssetManager::instance().GetAssetBySTRCODE(mProjectileSTRCode));
		if (aPrefabAsset != nullptr)
		{
			GameObject* aGameObject = new GameObject();
			aGameObject->load(aPrefabAsset->getPrefab());
			aGameObject->setEnabled(false);
			Projectile* aProjectile = static_cast<Projectile*>(aGameObject->getComponent("Projectile"));
			mPoolCount = aProjectile->getPoolCount();
			mAvailableProjectiles.push_back(aGameObject);
			aProjectile->setPlayer(this);
			for (int aI = mAvailableProjectiles.size(); aI < mPoolCount; aI++)
			{
				GameObject* aGameObject = new GameObject();
				aGameObject->load(aPrefabAsset->getPrefab());
				aGameObject->setEnabled(false);
				Projectile* aProjectile = static_cast<Projectile*>(aGameObject->getComponent("Projectile"));
				aProjectile->setPlayer(this);
			}
		}
	}
}

void Player::load(json::JSON& pPlayerNode)
{
	Component::load(pPlayerNode);
	if (pPlayerNode.hasKey("mProjectileGUID"))
	{
		mProjectileGUID = pPlayerNode["mProjectileGUID"].ToString();
		mProjectileSTRCode = GUIDToSTRCODE(mProjectileGUID);
	}
	if (pPlayerNode.hasKey("mSpeed"))
	{
		mSpeed = pPlayerNode["mSpeed"].ToFloat();
	}
	if (pPlayerNode.hasKey("mHealth"))
	{
		mHealth = pPlayerNode["mHealth"].ToFloat();
	}
}

void Player::update(float deltaTime)
{
	if (!getGameObject()->isEnabled() || !isEnabled())
	{
		return;
	}
	sf::Vector2f aMovementVector(0.f, 0.f);
	if (InputManager::instance().getKeyState(sf::Keyboard::W) == InputManager::PushState::Held)
	{
		aMovementVector.y = 1;
	}
	if (InputManager::instance().getKeyState(sf::Keyboard::S) == InputManager::PushState::Held)
	{
		aMovementVector.y == -1;
	}
	if (InputManager::instance().getKeyState(sf::Keyboard::A) == InputManager::PushState::Held)
	{
		aMovementVector.x = -1;
	}
	if (InputManager::instance().getKeyState(sf::Keyboard::D) == InputManager::PushState::Held)
	{
		aMovementVector.x = 1;
	}
	getGameObject()->getTransform()->translate(aMovementVector * mSpeed * deltaTime);
	if (mAvailableProjectiles.size() <= 0)
	{
		return;
	}
	if (InputManager::instance().getMouseButtonState(sf::Mouse::Left) == InputManager::PushState::Up)
	{
		GameObject* aProjectile = mAvailableProjectiles.back();
		mAvailableProjectiles.pop_back();
		mUnavailableProjectiles.push_back(aProjectile);
		sf::Vector2f aMousePosition = InputManager::instance().getMousePosition();
		aMousePosition = RenderSystem::instance().getRenderWindow()->mapPixelToCoords(sf::Vector2i((int)aMousePosition.x, (int)aMousePosition.y));
		sf::Vector2f aPlayerPosition = getGameObject()->getTransform()->getPosition();
		Projectile* aProj = static_cast<Projectile*>(aProjectile->getComponent("Projectile"));
		aProj->setMovePosition(aMousePosition - aPlayerPosition);
		aProjectile->getTransform()->setPosition(aPlayerPosition);
		aProjectile->setEnabled(true);
	}
}

void Player::addProjectileToPool(GameObject* pProjectile)
{
	if (pProjectile == nullptr)
	{
		return;
	}
	if (mUnavailableProjectiles.size() <= 0)
	{
		GameObjectManager::instance().removeGameObject(pProjectile);
		return;
	}
	pProjectile->setEnabled(false);
	mUnavailableProjectiles.remove(pProjectile);
	mAvailableProjectiles.push_back(pProjectile);
}

Player::~Player()
{
	mAvailableProjectiles.clear();
	mUnavailableProjectiles.clear();
}
