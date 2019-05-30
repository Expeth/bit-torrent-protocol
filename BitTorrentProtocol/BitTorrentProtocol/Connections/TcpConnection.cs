using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BitTorrentProtocol.Messages;

namespace BitTorrentProtocol.Connections
{
    public class TcpConnection : IConnection
    {
        private TcpClient _client;

        public TcpConnection()
        {
            _client = new TcpClient(AddressFamily.InterNetwork);
        }

        public void Connect(IPEndPoint peer)
        {
            _client.Connect(peer);
        }

        public void SendMessage(INetworkData message)
        {
            _client.GetStream().Write(message.RawBytes, 0, message.RawBytes.Length);
        }

        public Task<INetworkData> AcceptMessageTask()
        {
            return Task.Factory.StartNew<INetworkData>(() => 
            {
                return AcceptMessage();
            });
        }

        public INetworkData AcceptMessage()
        {
            byte[] msg = new byte[256];
            _client.GetStream().Read(msg, 0, msg.Length);

            return NetworkDataParser.Parse(msg);
        }

        public void Disconnect()
        {
            if (_client == null)
                return;
            _client.Client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }

        ~TcpConnection()
        {
            Disconnect();
        }
    }
}
