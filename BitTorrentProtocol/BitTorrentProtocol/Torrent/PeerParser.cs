using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Torrent
{
    public static class PeerParser
    {
        public static List<IPEndPoint> ParsePeers(byte[] peers)
        {
            List<IPEndPoint> peersList = new List<IPEndPoint>();

            for (int i = 0; i < peers.Length; i += 6)
            {
                int port = (UInt16)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(peers, i + 4));
                peersList.Add(new IPEndPoint(IPAddress.Parse($"{peers[i]}.{peers[i + 1]}.{peers[i + 2]}.{peers[i + 3]}"), port));
            }

            return peersList;
        }

        public static List<IPEndPoint> ParsePeers(List<Dictionary<object, object>> peers)
        {
            List<IPEndPoint> peersList = new List<IPEndPoint>();

            foreach (Dictionary<object, object> peer in peers)
            {
                peersList.Add(new IPEndPoint(IPAddress.Parse(peer["ip"] as string), (int)peer["port"]));
            }

            return peersList;
        }
    }
}
