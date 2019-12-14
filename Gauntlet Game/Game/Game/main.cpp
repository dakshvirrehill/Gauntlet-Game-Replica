#include "GameCore.h"
#include "GameEngine.h"
#include "GauntletEngine.h"

int main()
{
	GameEngine::instance().initialize(&GauntletEngine::instance());
	GameEngine::instance().gameLoop();
	return 0;
}