using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lithium
{
    class Packets
    {
        private byte                    Id;                 // First byte is packet Id
        private Int32                   Header;             // First 32 bits is length of message + length of nickname
        private string                  Nickname;           // Sender nickname
        private string                  Message;            // Message
        private MemoryStream            memstr;

        public Packets() { }

        public Packets(byte Id, string Nickname, string Message)
        {
            this.Id = Id;
            this.Header = Nickname.Length + Message.Length;
            this.Nickname = Nickname;
            this.Message = Message;
        }

        public byte[] PrepareMessageToSending()
        {
            memstr = new MemoryStream();
            BinaryWriter PacketWriter = new BinaryWriter(memstr);
            PacketWriter.Write((byte)0);
            PacketWriter.Write(Header);
            PacketWriter.Write(Nickname);
            PacketWriter.Write(Message);
            PacketWriter.Flush();
            PacketWriter.Close();
            byte[] newData = new byte[memstr.GetBuffer().Length];
            Array.Copy(memstr.GetBuffer(), 0, newData, 0, memstr.GetBuffer().Length);
            return newData;
        }

        public Packets HandleMessagePacket(int bytesRead, MemoryStream memostr)
        {
            BinaryReader PacketReader = new BinaryReader(memostr);
            Id = PacketReader.ReadByte();
            Header = PacketReader.ReadInt32();
            if (bytesRead >= (1 + 4 + Header))
            {
                Nickname = PacketReader.ReadString();
                Message = PacketReader.ReadString();
                return new Packets(this.Id, this.Nickname, this.Message);
            }
            else
                return null;
        }

        public Int32 GetHeader
        {
            get
            {
                return Header;
            }
        }

        public string GetNickname
        {
            get
            {
                return Nickname;
            }
        }

        public string GetMessage
        {
            get
            {
                return Message;
            }
        }

    }
}
