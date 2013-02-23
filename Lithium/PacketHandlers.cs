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
        
        private PacketHandlers(byte ID, Int32 Header, string Nickname, string Message)
        {
            this.Id = ID;
            this.Header = Header;
            this.Nickname = Nickname;
            this.Message = Message;
        }

        public PacketHandlers GetPacketStructure(MemoryStream memostr)
        {
            PacketReader = new BinaryReader(memostr);
            Id = PacketReader.ReadByte();
            Header = PacketReader.ReadInt32();
            Nickname = PacketReader.ReadString();
            Message = PacketReader.ReadString();
            return new PacketHandlers(Id, Header, Nickname, Message);
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
