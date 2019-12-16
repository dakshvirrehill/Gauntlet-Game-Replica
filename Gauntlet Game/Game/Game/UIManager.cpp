#include "GameCore.h"
#include "Button.h"
#include "Rectangle.h"
#include "Component.h"
#include "GameObject.h"
#include "UIManager.h"
#include "Text.h"
void UIManager::registerClasses()
{
	REGISTER_DYNAMIC_CLASS(Rectangle)
	REGISTER_DYNAMIC_CLASS(Button)
}

void UIManager::initialize()
{
	registerClasses();
	std::string aUIPath = "../Assets/UI/UI.json";
	json::JSON aUINode = FileSystem::instance().load(aUIPath,false);
	if (aUINode.hasKey("UIComponents"))
	{
		for (auto& aUIType : aUINode["UIComponents"].ObjectRange())
		{
			GauntletEngine::State aState = GauntletEngine::State::None;
			if (aUIType.first == "HUD")
			{
				aState = GauntletEngine::State::GamePlay;
			}
			else if (aUIType.first == "MainMenu")
			{
				aState = GauntletEngine::State::MainMenu;
			}
			else if (aUIType.first == "PauseMenu")
			{
				aState = GauntletEngine::State::Paused;
			}

			if (mUIMap.count(aState) == 0)
			{
				mUIMap.emplace(aState, std::map<std::string, Component*>());
			}
			if (aState == GauntletEngine::State::None)
			{
				continue;
			}
			for (auto& aComponents : aUIType.second.ArrayRange())
			{
				if (!aComponents.hasKey("class"))
				{
					continue;
				}
				Component* aComponent = static_cast<Component*>(CRtti::constructObject(aComponents["class"].ToString().c_str()));
				aComponent->load(aComponents);
				mUIMap[aState].emplace(aComponents["name"].ToString(), aComponent);
			}

		}
	}
	Button* aNewGameButton = static_cast<Button*>(mUIMap[GauntletEngine::State::MainMenu]["NewGameButton"]);
	aNewGameButton->mClickEvent = &GauntletEngine::StartGame;
	Button* aLoadGameButton = static_cast<Button*>(mUIMap[GauntletEngine::State::MainMenu]["LoadGameButton"]);
	aLoadGameButton->mClickEvent = &GauntletEngine::LoadGame;
	Button* aExitButton = static_cast<Button*>(mUIMap[GauntletEngine::State::MainMenu]["ExitButton"]);
	aExitButton->mClickEvent = &GauntletEngine::ExitGame;
	Button* aContinueButton = static_cast<Button*>(mUIMap[GauntletEngine::State::Paused]["ContinueGameButton"]);
	aContinueButton->mClickEvent = &GauntletEngine::ContinueGame;
	Button* aSaveButton = static_cast<Button*>(mUIMap[GauntletEngine::State::Paused]["SaveGameButton"]);
	aSaveButton->mClickEvent = &GauntletEngine::SaveGame;
	Button* aExitMENU = static_cast<Button*>(mUIMap[GauntletEngine::State::Paused]["MainMenuButton"]);
	aExitMENU->mClickEvent = &GauntletEngine::ExitGame;
	mTimer = static_cast<Text*>(mUIMap[GauntletEngine::State::GamePlay]["Timer"]);
}

void UIManager::printHUD( int& pHScore, int& pKills, float& pHealth)
{
	Text* aLives = static_cast<Text*>(mUIMap[GauntletEngine::State::GamePlay]["Lives"]);
	aLives->setText("Lives : " + std::to_string((int)pHealth));
	Text* aScore = static_cast<Text*>(mUIMap[GauntletEngine::State::GamePlay]["HighScore"]);
	aScore->setText("HighScore : " + std::to_string(pHScore));
	Text* aKills = static_cast<Text*>(mUIMap[GauntletEngine::State::GamePlay]["Kills"]);
	aKills->setText("Kills : " + std::to_string(pKills));
}

void UIManager::update(float deltaTime)
{
	if (mPrevState != GauntletEngine::instance().GetState())
	{
		GameObject* aCameraObject = GameObjectManager::instance().getGameObjectWithComponent("CameraManager");
		if (mUIMap.count(mPrevState) != 0)
		{
			for (auto& components : mUIMap[mPrevState])
			{
				components.second->setEnabled(false);
				aCameraObject->removeComponent(components.second->getID());
			}
		}
		mPrevState = GauntletEngine::instance().GetState();
		if (mUIMap.count(mPrevState) != 0)
		{
			for (auto& components : mUIMap[mPrevState])
			{
				aCameraObject->addComponent(components.second);
				components.second->setEnabled(true);
			}
		}
		
	}
	mTimer->setText("Time Left : " + GauntletEngine::instance().getTime());
}
