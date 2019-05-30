using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Messages
{
    /// <summary>
    /// First message that we send to make connection.
    /// </summary>
    public class Handshake : INetworkData
    {
        public byte[] RawBytes { get; set; }
        public NetworkDataType DataType { get; set; }
        public string InfoHash { get; set; }
        public string PeerId { get; set; }

        public Handshake()
        {
            DataType = NetworkDataType.Handshake;
        }

        public Handshake(byte[] infoHash, string peerId)
        {
            InfoHash = BitConverter.ToString(infoHash);
            PeerId = peerId;
            
            byte[] arr = new byte[] { Convert.ToByte(19) };
            arr = arr.Concat(Encoding.UTF8.GetBytes("BitTorrent protocol")).ToArray();
            arr = arr.Concat(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }).ToArray();
            arr = arr.Concat(infoHash).ToArray();
            arr = arr.Concat(Encoding.UTF8.GetBytes(peerId)).ToArray();

            DataType = NetworkDataType.Handshake;
            RawBytes = arr;
        }
    }
}
