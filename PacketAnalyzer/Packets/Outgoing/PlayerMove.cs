using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacketAnalyzer.Packets.Outgoing
{
    public class PlayerMove : OutgoingPacket
    {

        public PlayerMove(byte packetId, byte[] buffer) : base(packetId, buffer)
        {
            switch (this.packetId)
            {
                case 0x65:
                    this.description = "PLAYER_MOVE_NORTH";
                    break;
                case 0x66:
                    this.description = "PLAYER_MOVE_EAST";
                    break;
                case 0x67:
                    this.description = "PLAYER_MOVE_SOUTH";
                    break;
                case 0x68:
                    this.description = "PLAYER_MOVE_WEST";
                    break;
                case 0x6A:
                    this.description = "PLAYER_MOVE_NORTH_EAST";
                    break;
                case 0x6B:
                    this.description = "PLAYER_MOVE_SOUTH_EAST";
                    break;
                case 0x6C:
                    this.description = "PLAYER_MOVE_SOUTH_WEST";
                    break;
                case 0x6D:
                    this.description = "PLAYER_MOVE_NORTH_WEST";
                    break;
            }
        }
    }
}
