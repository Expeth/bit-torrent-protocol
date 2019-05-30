using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Torrent
{
    /// <summary>
    /// Class for creating request url to tracker
    /// </summary>
    public static class TrackerRequestUrl
    {
        public static string GetRequestUrl(string announce, string info_hash,
                                           string peer_id, short port, long downloaded,
                                           long left, long uploaded, int compact = 1,
                                           string eventName = "started")
        {
            string url = announce;
            char symbol = url.IndexOf('?') == -1 ? '?' : '&';
            if (symbol == '&')
                url += "/";
            url += $"{symbol}info_hash={info_hash}&peer_id={peer_id}&port={port}&downloaded={downloaded}&left={left}&uploaded={uploaded}&compact={compact}&event={eventName}";
            return url;
        }
    }
}
