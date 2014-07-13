using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacketAnalyzer.Packets
{
    public class OutgoingPacket
    {
        protected int position;
        protected byte[] buffer;
        protected byte packetId;
        protected List<PacketData> processedData;
        protected string description;

        public OutgoingPacket(byte packetId, byte[] buffer)
        {
            this.position = 18;
            this.buffer = buffer;
            this.packetId = packetId;
            this.processedData = new List<PacketData>();
            this.description = "";
        }

        public List<PacketData> ProcessedData
        {
            get { return processedData; }
        }

        public string Description
        {
            get { return description; }
        }

        protected byte ReadByte()
        {
            return buffer[position++];
        }

        protected byte[] ReadBytes(int length)
        {
            byte[] ret = new byte[length];
            Array.Copy(buffer, ret, position += length);
            return ret;
        }

        protected short ReadInt16()
        {
            return  BitConverter.ToInt16(buffer, (position += 2) - 2);
        }

        protected ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(buffer, (position += 2) - 2);
        }

        protected int ReadInt32()
        {
            return BitConverter.ToInt32(buffer, (position += 4) - 4);
        }

        protected uint ReadUInt32()
        {
            return BitConverter.ToUInt32(buffer, (position += 4) - 4);
        }

        protected string ReadString(out ushort length)
        {
            length = ReadUInt16();
            return Encoding.ASCII.GetString(buffer, (position += length) - length, length);
        }
    }
}
