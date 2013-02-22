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
        private BinaryReader PacketReader;
        private string Nickname;          
        private string Message;           

        public PacketHandlers()
        {
        }

        public Int32 GetPacketByID(MemoryStream memostr)
        {
            PacketReader = new BinaryReader(memostr);
            Id = PacketReader.ReadByte();
            Header = PacketReader.ReadInt32();
            Nickname = PacketReader.ReadString();
            Message = PacketReader.ReadString();
            switch (Id)
            {
                case 0:
                    return 0;
                case 1:
                    return 1;
                case 2:
                    return 2;
                default :
                    return -1;
            }
        }

        public Int32 GetHeader()
        {
            return Header;
        }

        public string GetNickname()
        {
            if (Nickname.Length > 0)
                return Nickname;
            else return null;
        }

        public string GetMessage()
        {
            if (Message.Length > 0)
                return Message;
            else return null;
        }
    }
}
