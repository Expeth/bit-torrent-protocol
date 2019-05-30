using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Messages
{
    public interface INetworkData
    {
        byte[] RawBytes { get; set; }
        NetworkDataType DataType { get; set; }
    }
}
