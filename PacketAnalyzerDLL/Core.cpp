#include "Core.h"
#include "global.h"
#include "Addresses.h"
#include "Packet.h"

Core::Core(){
	::core = this;
	this->baseAddress = (DWORD)GetModuleHandle(0);
	this->initializePipe();
	this->hookOutgoingPackets();

}

Core::~Core(){

}

void Core::initializePipe(){
	this->pipeHandle = CreateNamedPipeA("\\\\.\\pipe\\packetanalyzer", PIPE_ACCESS_DUPLEX, PIPE_TYPE_BYTE, 1, 2048, 2048, 5000, 0);

	if (pipeHandle == INVALID_HANDLE_VALUE){
		MessageBoxA(0, "asd", 0, 0);
	}
}

DWORD Core::alignAddress(DWORD address){

	return address - 0x400000 + this->baseAddress;
}

void Core::hookOutgoingPackets(){

	DWORD old;
	BYTE* jmpAddress = (BYTE*)(core->alignAddress(Addresses::OUTGOING_HOOK_ADDRESS));
	DWORD hookAddress = (DWORD)&this->outgoingHook;
	VirtualProtect(jmpAddress, 6, PAGE_EXECUTE_READWRITE, &old);
	DWORD newAddress = (DWORD)(hookAddress - (DWORD)jmpAddress) - 5;

	*jmpAddress = 0xE9; //JMP
	*((DWORD*)(jmpAddress + 0x1)) = newAddress;
	*(jmpAddress + 0x5) = 0x90; //NOP

	VirtualProtect(jmpAddress, 6, old, 0);
}

DWORD jmpBackAddress = Addresses::OUTGOING_HOOK_ADDRESS - 0x400000 + 0x6 + (DWORD)GetModuleHandle(0);

void __declspec(naked) Core::outgoingHook(){
	__asm{
		PUSHAD
		PUSHFD
	}

	core->handleOutgoingPackets();

	__asm{
		POPFD
		POPAD
		MOV EAX, DWORD PTR FS:[0]
		MOV EDX, jmpBackAddress
		JMP EDX
	}
}

void Core::handleOutgoingPackets(){
	Packet* packet = new Packet();
	DWORD timeStamp = GetTickCount();
	packet->size = *(DWORD*)core->alignAddress(Addresses::OUTGOING_LEN);
	memcpy(packet->buffer, (void*)core->alignAddress(Addresses::OUTGOING_DATA_STREAM), packet->size);

	BYTE *buffer = new BYTE[packet->size + 1 + 4 + 4];
	memset(buffer, 0, packet->size + 1 + 4 + 4);
	*buffer = 1; //1 for outgoing packet
	memcpy((buffer + 1), &timeStamp, 4);
	memcpy((buffer + 5), &packet->size, 4);
	memcpy((buffer + 9), packet->buffer, packet->size);

	DWORD n = 0;
	WriteFile(this->pipeHandle, buffer, packet->size + 1 + 4 + 4, &n, 0);

	delete packet;
}