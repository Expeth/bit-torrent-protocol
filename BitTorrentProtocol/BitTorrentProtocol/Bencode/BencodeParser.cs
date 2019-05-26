using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Bencode
{
    /// <summary>
    /// Class to read torrent files
    /// </summary>
    public class BencodeParser
    {
        /// <summary>
        /// StreamReader for reading a file
        /// </summary>
        private StreamReader _reader;
        /// <summary>
        /// Dictionary of all torrent file's information
        /// </summary>
        private Dictionary<object, object> _torrentFile;
        /// <summary>
        /// Torrent file's Local path
        /// </summary>
        private string _torrentPath;

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="torrentPath">Local path to torrent file.</param>
        public BencodeParser(string torrentPath)
        {
            if (!File.Exists(torrentPath) || Path.GetExtension(torrentPath) != ".torrent")
                throw new ArgumentException("File does not exist or has wrong extension. " + Directory.GetCurrentDirectory());

            _torrentPath = torrentPath;
            _reader = new StreamReader(torrentPath);
            _reader.Read();
            _torrentFile = ReadDictionary();
            _reader.Dispose();
        }

        /// <summary>
        /// Parse Bencode
        /// </summary>
        /// <returns>Initialized instance of TorrentInfo</returns>
        public TorrentInfo GetTorrentInfo()
        {
            // Object that will be returned
            TorrentInfo file = new TorrentInfo();

            // Info dictionary of torrent file
            Dictionary<object, object> fileInfo = (Dictionary<object, object>)_torrentFile["info"];

            // Adding main information about torrent file
            file.Filename = Path.GetFileName(_torrentPath);
            file.Comment = _torrentFile["comment"] as string;
            file.Announce = _torrentFile["announce"] as string;
            file.InfoHash = ComputeInfoHash(ReadInfoValue());
            file.PieceLength = (int)fileInfo["piece length"];

            // Adding announce-list (trackers)
            foreach (var lists in (List<object>)_torrentFile["announce-list"])
                foreach (var announce in (List<object>)lists)
                    file.AnnounceList.Add(announce as string);

            // If info dictionary in multi file mode
            try
            {
                foreach (var i in (List<object>)fileInfo["files"])
                {
                    // Item1 - file path
                    // Item2 - file length
                    (string, long) pair;

                    pair.Item2 = (int)((Dictionary<object, object>)i)["length"];

                    string path = "";
                    foreach (var j in (List<object>)((Dictionary<object, object>)i)["path"])
                        path += (j as string) + "/";

                    // Removing last '/' from file path
                    path = path.Remove(path.LastIndexOf('/'), 1);
                    pair.Item1 = path;

                    // Adding to list of files
                    file.Files.Add(pair);
                }
            }
            // If info dictionary in single file mode
            catch (Exception)
            {
                (string, long) pair;

                pair.Item2 = (int)fileInfo["length"];
                pair.Item1 = (string)fileInfo["name"];

                file.Files.Add(pair);
            }

            return file;
        }

        /// <summary>
        /// Compute SHA1 Hash of Info Dictionary
        /// </summary>
        /// <param name="infoValue"></param>
        /// <returns></returns>
        private string ComputeInfoHash(byte[] infoValue)
        {
            return BitConverter.ToString(SHA1.Create().ComputeHash(infoValue)).Replace("-", "");
        }

        /// <summary>
        /// Read Info Dictionary
        /// </summary>
        /// <returns></returns>
        private byte[] ReadInfoValue()
        {
            // Text of file
            string fileText = File.ReadAllText(_torrentPath);

            // Index of property 'pieces'
            int piecesPropertyIndex = fileText.IndexOf("pieces") + 6;

            StreamReader sr = new StreamReader(_torrentPath);

            // Skipping unnessesary bytes
            sr.BaseStream.Seek(piecesPropertyIndex, SeekOrigin.Begin);

            // Reading length of property 'pieces'
            long piecesValueLength = 0;
            while ((char)sr.Peek() != ':')
            {
                char cymbol = (char)sr.Read();
                piecesValueLength *= 10;
                piecesValueLength += Convert.ToInt32(cymbol.ToString());
            }

            // Index, where info dictionary begins
            long startIndex = fileText.IndexOf("info") + 4;

            // Index, where info dictionary ends
            long endIndex = piecesPropertyIndex + $"{piecesValueLength}".Length + piecesValueLength + 2;

            // Bytes of file
            byte[] bytes = File.ReadAllBytes(_torrentPath);

            // Bytes of info dictionary
            byte[] infoValueBytes = new byte[endIndex - startIndex];
            for (int i = 0; i < infoValueBytes.Length; i++)
                infoValueBytes[i] = bytes[i + startIndex];

            return infoValueBytes;
        }

        private Dictionary<object, object> ReadDictionary()
        {
            Dictionary<object, object> dictionary = new Dictionary<object, object>();
            do
            {
                string property = ReadString();
                object value = ReadValue();

                dictionary.Add(property, value);

            } while ((char)_reader.Peek() != 'e' && !_reader.EndOfStream);

            _reader.Read();
            return dictionary;
        }

        private string ReadString()
        {
            char[] property = null;
            char symbol = (char)_reader.Read();
            int propLength = 0;
            while (symbol != ':')
            {
                propLength *= 10;
                propLength += Convert.ToInt32(symbol.ToString());
                symbol = (char)_reader.Read();
            }

            property = new char[propLength];
            _reader.Read(property, 0, propLength);

            return new string(property);
        }

        private int ReadInteger()
        {
            string integer = "";
            char symbol = (char)_reader.Read();
            do
            {
                integer += symbol;
                symbol = (char)_reader.Read();

            } while (symbol != 'e');
            return Convert.ToInt32(integer);
        }

        private List<object> ReadList()
        {
            List<object> list = new List<object>();
            do
            {
                list.Add(ReadValue());

            } while ((char)_reader.Peek() != 'e');

            _reader.Read();
            return list;
        }

        private object ReadValue()
        {
            char symbol = (char)_reader.Peek();
            if (Char.IsDigit(symbol))
            {
                return ReadString();
            }
            else if (symbol == 'd')
            {
                _reader.Read();
                return ReadDictionary();
            }
            else if (symbol == 'i')
            {
                _reader.Read();
                return ReadInteger();
            }
            else if (symbol == 'l')
            {
                _reader.Read();
                return ReadList();
            }
            return null;
        }
    }
}
