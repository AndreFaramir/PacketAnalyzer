using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacketAnalyzer.Packets.Outgoing
{
    public class PlayerSpeech : OutgoingPacket
    {
        public PlayerSpeech(byte packetId, byte[] buffer) : base(packetId, buffer)
        {
            description = "PLAYER_SPEECH";

            processedData.Add(new PacketData("SPEECH_TYPE", "BYTE", 1, (SpeechType)(ReadByte())));

            switch ((byte)processedData[0].Data)
            {
                case 0x1:
                case 0x2:
                case 0x3:
                    break;
                case 0x5:
                    ushort length;
                    string reciever = ReadString(out length);
                    processedData.Add(new PacketData("RECIEVER", "STRING", length, reciever));
                    break;
            }

             ushort msgLength;
             string msg = ReadString(out msgLength);
             processedData.Add(new PacketData("MESSAGE", "STRING", msgLength, msg));
        }

        private enum SpeechType : byte
        {
            SAY = 1,
            WHISPER = 2,
            YELL = 3,
            PRIVATE = 5,
        }
    }
}
