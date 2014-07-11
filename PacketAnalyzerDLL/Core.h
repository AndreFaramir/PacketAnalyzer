#ifndef CORE_H
#define CORE_H

#include <Windows.h>

class Core{
public:
	Core();
	~Core();

	DWORD alignAddress(DWORD address);
private:
	DWORD baseAddress;
	HANDLE pipeHandle;

	static void outgoingHook();
	void handleOutgoingPackets();
	void hookOutgoingPackets();

	void initializePipe();
};

#endif