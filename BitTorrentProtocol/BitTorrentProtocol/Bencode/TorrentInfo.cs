using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Bencode
{
    public class TorrentInfo
    {
        /// <summary>
        /// Hash of info dictionary in URL-Encoded format.
        /// </summary>
        public string InfoHash { get; set; } 
        public string Announce { get; set; }
        public string Comment { get; set; }
        public int PieceLength { get; set; }
        public List<string> AnnounceList { get; set; }
        public List<(string, long)> Files { get; set; }

        public TorrentInfo()
        {
            AnnounceList = new List<string>();
            Files = new List<(string, long)>();
        }
    }
}
