using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Messages
{
    public class NetworkDataParser
    {
        public static INetworkData Parse(byte[] msg)
        {
            if (msg.Length == 40)
            {
                string peer_id = BitConverter.ToString(msg, 0, 20).Replace("-", "");
                string info_hash = BitConverter.ToString(msg, 20, 20).Replace("-", "");

                return new Handshake() { RawBytes = msg, InfoHash = info_hash, PeerId = peer_id };
            }
            if (msg.Length == 4)
            {
                return new Message(NetworkDataType.KeepAlive);
            }

            int messageId = Convert.ToInt32(msg[4]);
            return new Message((NetworkDataType)messageId);
        }
    }
}
