using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacketAnalyzer.Packets.Outgoing
{
    public class Unknown : OutgoingPacket
    {
        public Unknown(byte packetId, byte[] buffer) : base(packetId, buffer)
        {
            this.description = "UNKOWN_PACKET";
        }
    }
}
