using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacketAnalyzer.Packets
{
    public struct PacketData
    {
        public string Description;
        public string Type;
        public uint Size;
        public object Data;

        public PacketData(string description, string type, uint size, object data)
        {
            this.Description = description;
            this.Type = type;
            this.Size = size;
            this.Data = data;
        }
    }
}
