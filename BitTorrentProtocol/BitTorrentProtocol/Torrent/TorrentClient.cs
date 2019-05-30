using BitTorrentProtocol.Bencode;
using BitTorrentProtocol.Encrypting;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Torrent
{
    public class TorrentClient
    {
        private RestClient _restClient;
        private BencodeParser _bencodeParser;
        private TorrentInfo _torrentFile;
        private IEncryptor _encryptor;
        private long _uploadedBytes;
        private long _downloadedBytes;
        private long _leftBytes;
        private long _interval;
        private long _minInterval;
        private List<IPEndPoint> _peers;
        private string _peerId;

        public TorrentClient(string torrentFile, IEncryptor encryptor)
        {
            _peerId = GeneratePeedId().ToUpper();
            _uploadedBytes = _downloadedBytes = _leftBytes = 0;
            _bencodeParser = new BencodeParser(torrentFile);
            _restClient = new RestClient();
            _encryptor = encryptor;
            _torrentFile = _bencodeParser.GetTorrentInfo();
            _torrentFile.InfoHash = _encryptor.GetUrlEncodedHash(_bencodeParser.ReadInfoValue());
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

        public List<IPEndPoint> GetPeers()
        {
            return _peers;
        }

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
