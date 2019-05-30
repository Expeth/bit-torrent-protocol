using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Messages
{
    public enum NetworkDataType
    {
        Handshake = -2, KeepAlive, Choke, Unchoke, Interested, NotInterested, Have, Bitfield, Request
    }
}
