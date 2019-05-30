using BitTorrentProtocol.Bencode;
using BitTorrentProtocol.Connections;
using BitTorrentProtocol.Encrypting;
using BitTorrentProtocol.Messages;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Torrent
{
    /// <summary>
    /// Class to work with torrent file.
    /// </summary>
    public class TorrentClient
    {
        /// <summary>
        /// Class to make HTTP requests.
        /// </summary>
        private RestClient _restClient;
        /// <summary>
        /// Parser of Bencode text.
        /// </summary>
        private BencodeParser _bencodeParser;
        /// <summary>
        /// Information about torrent file.
        /// </summary>
        private TorrentInfo _torrentFile;
        /// <summary>
        /// Computing hash of info dictionary from .torrent file.
        /// </summary>
        private IEncryptor _encryptor;
        private byte[] _infoHash;
        private long _uploadedBytes;
        private long _downloadedBytes;
        private long _leftBytes;
        private long _interval;
        private long _minInterval;
        /// <summary>
        /// IP and port for available peers.
        /// </summary>
        private List<IPEndPoint> _peers;
        /// <summary>
        /// Identificator for local client.
        /// </summary>
        private string _peerId;
        /// <summary>
        /// Connection with peer.
        /// </summary>
        private IConnection _connection;

       /// <summary>
       /// Creates an instance.
       /// </summary>
       /// <param name="torrentFile">Local path to torrent file.</param>
       /// <param name="encryptor">Enctypror for computing info_hash. It's desirable to use Sha1Enctyptor</param>
        public TorrentClient(string torrentFile, IEncryptor encryptor, IConnection connection)
        {
            _connection = connection;
            _peerId = GeneratePeedId().ToUpper();
            _uploadedBytes = _downloadedBytes = _leftBytes = 0;
            _bencodeParser = new BencodeParser(torrentFile);
            _restClient = new RestClient();
            _encryptor = encryptor;
            _torrentFile = _bencodeParser.GetTorrentInfo();
            _torrentFile.InfoHash = _encryptor.GetUrlEncodedHash(_bencodeParser.ReadInfoValue());
            _infoHash = _encryptor.GetHashBytes(_bencodeParser.ReadInfoValue());
        }

        public Dictionary<object, object> GetResponseFromTracker()
        {
            string requestUrl = TrackerRequestUrl.GetRequestUrl(_torrentFile.Announce, _torrentFile.InfoHash,
                                                                _peerId, 12187, _downloadedBytes, _leftBytes,
                                                                _uploadedBytes);

            IRestRequest requestToTracker = new RestRequest(requestUrl);
            IRestResponse responseFromTracker = _restClient.Get(requestToTracker);

            return new BencodeParser(responseFromTracker.RawBytes).GetParsedBencode();
        }

        public Task<Dictionary<object, object>> GetResponseFromTrackerTask()
        {
            return Task.Factory.StartNew<Dictionary<object, object>>(() =>
            {
                return GetResponseFromTracker();
            });
        }

        /// <summary>
        /// Sends request to tracker to get peers list and other info.
        /// </summary>
        public void SendRequestToTracker()
        {
            Dictionary<object, object> responseDictionary = GetResponseFromTracker();
            ParseResponseFromTracker(responseDictionary);
        }

        public async void SendRequestToTrackerAsync()
        {
            Dictionary<object, object> responseDictionary = await GetResponseFromTrackerTask();
            ParseResponseFromTracker(responseDictionary);
        }
        
        private void ParseResponseFromTracker(Dictionary<object, object> dictionary)
        {
            _interval = (int)dictionary["interval"];
            _minInterval = (int)dictionary["min interval"];

            if (dictionary["peers"] is byte[])
                _peers = PeerParser.ParsePeers(dictionary["peers"] as byte[]);
            else
                _peers = PeerParser.ParsePeers(dictionary["peers"] as List<Dictionary<object, object>>);
        }

        /// <summary>
        /// Returns a list of peers.
        /// </summary>
        /// <returns></returns>
        public List<IPEndPoint> GetPeers()
        {
            return _peers;
        }

        /// <summary>
        /// Makes connection with peer.
        /// </summary>
        public void ConnectToPeer()
        {
            foreach (var i in _peers)
            {
                try
                {
                    _connection.Connect(i);
                    break;
                }
                catch (Exception) { }
            }

            INetworkData handshake = new Handshake(_infoHash, _peerId);
            _connection.SendMessage(handshake);
        }

        /// <summary>
        /// Generates an unique ID for local client.
        /// </summary>
        /// <returns></returns>
        private string GeneratePeedId()
        {
            Random rand = new Random();
            StringBuilder peerId = new StringBuilder();

            for (int i = 0; i < 20; i++)
                peerId.Append(rand.Next(0, 9));

            return Convert.ToString(peerId);
        }
    }
}
