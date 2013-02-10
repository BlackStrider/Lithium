using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lithium
{
    class PacketHandlers : Packets //for future use
    {
        private byte Id;
        private Int32 Header;             
        private Int32 bytesRead;
        private string Nickname;          
        private string Message;           

        public PacketHandlers()
        {
        }

        public Packets GetPacket(int bytesRead, MemoryStream memostr)
        {
            this.bytesRead = bytesRead;
            Nickname = Message = "";
            BinaryReader PacketReader = new BinaryReader(memostr);
            Id = PacketReader.ReadByte();
            switch (Id)
            {
                case 0:
                    return MessageHandler(PacketReader);
                case 1:
                    return PingPongHandler(PacketReader);
                case 2:
                    return ServerMessageHandler(PacketReader);
                default :
                    return null;
            }
        }

        private Packets MessageHandler(BinaryReader PacketReader)
        {
            Header = PacketReader.ReadInt32();
            if (bytesRead >= (1 + 4 + Header))
            {
                Nickname = PacketReader.ReadString();
                Message = PacketReader.ReadString();
                return new Packets(Id, Nickname, Message);
            }
            else
                return null;
        }

        private Packets PingPongHandler(BinaryReader PacketReader)
        {
            Header = PacketReader.ReadInt32();
            if (bytesRead >= (1 + 4 + Header))
            {
                Message = "Test";
                return new Packets(Id, Nickname, Message);
            }
            else
                return null;
        }

        private Packets ServerMessageHandler(BinaryReader PacketReader)
        {
            Header = PacketReader.ReadInt32();
            if (bytesRead >= (1 + 4 + Header))
            {
                Message = PacketReader.ReadString();
                return new Packets(Id, Nickname, Message);
            }
            else
                return null;
        }
    }
}
