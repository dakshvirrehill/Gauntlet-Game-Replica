#include "GameCore.h"
#include "GameObject.h"
#include "PrefabAsset.h"
#include "Projectile.h"
#include "CameraManager.h"
#include "Player.h"
#include "ICollidable.h"
#include "Pickable.h"
#include "Enemy.h"
#include "GauntletEngine.h"

IMPLEMENT_DYNAMIC_CLASS(Player)

void Player::onTriggerEnter(const Collision* const collisionData)
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
	if (aOther->getComponent("Teleporter") != nullptr)
	{
		return;
	}
	Projectile* aProj = static_cast<Projectile*>(aOther->getComponent("Projectile"));
	if (aProj != nullptr)
	{
		if (aProj->mPlayer != nullptr)
		{
			return;
		}
		else
		{
			if (mInvincible)
			{
				return;
			}
			doDamage(1);
		}
	}
	Pickable* aPickable = static_cast<Pickable*>(aOther->getComponent("Pickable"));
	if (aPickable != nullptr)
	{
		switch (aPickable->mType)
		{
		case Pickable::Type::HealthBooster: mHealth += 10;
			break;
		case Pickable::Type::Invincibility: mInvincible = true;
			break;
		case Pickable::Type::ScoreMultiplier: GauntletEngine::instance().doubleMultiplier();
			break;
		}
	}
}

void Player::onCollisionEnter(const Collision* const collisionData)
{
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}

	if (mInvincible)
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
	Enemy* aEnemy = static_cast<Enemy*>(aOther->getComponent("Enemy"));
	if (aEnemy == nullptr)
	{
		return;
	}
	switch (aEnemy->getType())
	{
	case Enemy::Type::Collider: doDamage(2);
		break;
	case Enemy::Type::CloseRange: doDamage(1);
		break;
	}
}

void Player::onCollisionStay(const Collision* const collisionData)
{
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}

	if (mInvincible)
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
	Enemy* aEnemy = static_cast<Enemy*>(aOther->getComponent("Enemy"));
	if (aEnemy == nullptr)
	{
		return;
	}
	if (aEnemy->getType() != Enemy::Type::CloseRange)
	{
		return;
	}
	if (mInAttackTime <= 0)
	{
		doDamage(1);
		mInAttackTime = mSpeed * 0.25f;
	}
}

void Player::doDamage(int pDamage)
{
	mHealth -= pDamage;
	GauntletEngine::instance().addScore(0);
	if (mHealth <= 0)
	{
		GauntletEngine::instance().gameOver();
		getGameObject()->setEnabled(false);
	}
}

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
			GameObjectManager::instance().addGameObject(aGameObject);
			Projectile* aProjectile = static_cast<Projectile*>(aGameObject->getComponent("Projectile"));
			mPoolCount = aProjectile->getPoolCount();
			mAvailableProjectiles.push_back(aGameObject);
			aProjectile->setPlayer(this);
			for (int aI = mAvailableProjectiles.size(); aI < mPoolCount; aI++)
			{
				GameObject* aGameObject = new GameObject();
				aGameObject->load(aPrefabAsset->getPrefab());
				aGameObject->setEnabled(false);
				GameObjectManager::instance().addGameObject(aGameObject);
				Projectile* aProjectile = static_cast<Projectile*>(aGameObject->getComponent("Projectile"));
				aProjectile->setPlayer(this);
				mAvailableProjectiles.push_back(aGameObject);
			}
		}
		GameObject* aGObj = GameObjectManager::instance().getGameObjectWithComponent("CameraManager");
		CameraManager* aCM = static_cast<CameraManager*>(aGObj->getComponent("CameraManager"));
		aCM->setPlayer(this);
		GauntletEngine::instance().setPlayer(this);
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
		mSpeed = pPlayerNode["mSpeed"].ToFloat() * 3.f;
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
	if (GauntletEngine::instance().GetState() != GauntletEngine::State::GamePlay)
	{
		return;
	}

	sf::Vector2f aMovementVector(0.f, 0.f);
	mInAttackTime -= deltaTime;
	if (InputManager::instance().getKeyState(sf::Keyboard::W) == InputManager::PushState::Held)
	{
		aMovementVector.y = -1;
	}
	if (InputManager::instance().getKeyState(sf::Keyboard::S) == InputManager::PushState::Held)
	{
		aMovementVector.y = 1;
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
		aProjectile->getTransform()->setPosition(aPlayerPosition);
		aProj->setMovePosition(aMousePosition - aPlayerPosition);
		aProjectile->setEnabled(true);
		for (auto& aComps : aProjectile->getAllComponents())
		{
			aComps.second->updatePosition();
		}
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
		pProjectile->setEnabled(false);
		return;
	}
	pProjectile->setEnabled(false);
	mUnavailableProjectiles.remove(pProjectile);
	mAvailableProjectiles.push_back(pProjectile);
}

Player::~Player()
{
	GameObject* aGObj = GameObjectManager::instance().getGameObjectWithComponent("CameraManager");
	if (aGObj != nullptr)
	{
		CameraManager* aCM = static_cast<CameraManager*>(aGObj->getComponent("CameraManager"));
		aCM->setPlayer(nullptr);
	}
	GauntletEngine::instance().setPlayer(nullptr);
	mAvailableProjectiles.clear();
	mUnavailableProjectiles.clear();
}
