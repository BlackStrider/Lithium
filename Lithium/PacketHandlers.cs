using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lithium
{
    class PacketHandlers : Packets //for future use
    {
        private byte Id;

        public PacketHandlers(byte Id)
        {
            this.Id = Id;
        }

        PacketHandlers GetHandlerById(byte Id)
        {
            switch (Id)
            {
                case 0:
                    return MessageHandler();
                case 1:
                    return PingPongHandler();
                case 2:
                    return ServerMessageHandler();
                default :
                    return null;
            }
        }

        private PacketHandlers MessageHandler()
        {
            return new PacketHandlers(1);
        }

        private PacketHandlers PingPongHandler()
        {
            return new PacketHandlers(1);
        }

        private PacketHandlers ServerMessageHandler()
        {
            return new PacketHandlers(1);
        }
    }
}
