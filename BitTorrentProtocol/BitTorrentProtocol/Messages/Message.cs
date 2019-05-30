using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Messages
{
    public class Message : INetworkData
    {
        public byte[] RawBytes { get; set; }
        public NetworkDataType DataType { get; set; }

        public Message(NetworkDataType type, byte[] payload = null)
        {
            DataType = type;
            switch (DataType)
            {
                case NetworkDataType.KeepAlive:
                    RawBytes = BitConverter.GetBytes(0);
                    return;
                case NetworkDataType.Choke:
                case NetworkDataType.Unchoke:
                case NetworkDataType.Interested:
                case NetworkDataType.NotInterested:
                    RawBytes = BitConverter.GetBytes(1);
                    RawBytes.Append(Convert.ToByte((int)DataType));
                    return;
                case NetworkDataType.Bitfield:
                case NetworkDataType.Have:
                case NetworkDataType.Request:
                default:
                    return;
            }
        }
    }
}
