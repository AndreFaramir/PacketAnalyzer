#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include "global.h";

DWORD WINAPI DLLThread(LPVOID lpParam);

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpReserved){
	switch(fdwReason){
	case DLL_PROCESS_ATTACH:
		CreateThread(0, 0, DLLThread, 0, 0, 0);
		break;
	}

	return TRUE;
}

DWORD WINAPI DLLThread(LPVOID lpParam){
	new Core();
	return 0;
}