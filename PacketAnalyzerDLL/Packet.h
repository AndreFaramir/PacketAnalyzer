#ifndef PACKET_H
#define PACKET_H

#define MAX_PACKET_SIZE 2048

class Packet{

public:
	Packet(){
		this->buffer = new BYTE[MAX_PACKET_SIZE];
		memset(this->buffer, 0, MAX_PACKET_SIZE);
		this->size = 0;
		this->position = 0;
	}
	~Packet(){

	}

	BYTE* buffer;
	DWORD size;
	DWORD position;
};
#endif