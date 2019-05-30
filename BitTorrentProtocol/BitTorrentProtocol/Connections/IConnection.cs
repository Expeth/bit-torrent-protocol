using BitTorrentProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Connections
{
    public interface IConnection
    {
        void Connect(IPEndPoint peer);
        void Disconnect();
        void SendMessage(INetworkData message);
        Task<INetworkData> AcceptMessageTask();
        INetworkData AcceptMessage();
    }
}
