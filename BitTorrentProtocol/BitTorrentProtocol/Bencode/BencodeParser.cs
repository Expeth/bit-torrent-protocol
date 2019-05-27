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
        private MemoryStream _reader;
        /// <summary>
        /// Dictionary of all torrent file's information
        /// </summary>
        private Dictionary<object, object> _parsedBencode;
        /// <summary>
        /// Torrent file's Local path
        /// </summary>
        private byte[] _bencodeText;

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="torrentPath">Local path to torrent file.</param>
        public BencodeParser(string filePath)
        {
            if (!File.Exists(filePath))
                throw new ArgumentException("File does not exist or has wrong encoding. " + Directory.GetCurrentDirectory());

            _bencodeText = File.ReadAllBytes(filePath);
            _reader = new MemoryStream(_bencodeText);
            _reader.ReadByte();
            _parsedBencode = ReadDictionary();
            _reader.Dispose();
        }

        public BencodeParser(byte[] bencodeText)
        {
            _bencodeText = bencodeText;
            _reader = new MemoryStream(_bencodeText);
            _reader.ReadByte();
            _parsedBencode = ReadDictionary();
            _reader.Dispose();
        }

        public Dictionary<object, object> GetParsedBencode()
        {
            return _parsedBencode;
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
            Dictionary<object, object> fileInfo = (Dictionary<object, object>)_parsedBencode["info"];

            // Adding main information about torrent file
            file.Comment = Encoding.UTF8.GetString(_parsedBencode["comment"] as byte[]);
            file.Announce = Encoding.UTF8.GetString(_parsedBencode["announce"] as byte[]);
            file.InfoHash = ComputeInfoHash(ReadInfoValue());
            file.PieceLength = (int)fileInfo["piece length"];

            // Adding announce-list (trackers)
            foreach (var lists in (List<object>)_parsedBencode["announce-list"])
                foreach (var announce in (List<object>)lists)
                    file.AnnounceList.Add(Encoding.UTF8.GetString(announce as byte[]));

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
                        path += Encoding.UTF8.GetString(j as byte[]) + "/";

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
                pair.Item1 = Encoding.UTF8.GetString(fileInfo["name"] as byte[]);

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
            string fileText = Encoding.UTF8.GetString(_bencodeText);

            // Index of property 'pieces'
            int piecesPropertyIndex = fileText.IndexOf("pieces") + 6;

            MemoryStream sr = new MemoryStream(_bencodeText);

            // Skipping unnessesary bytes
            sr.Seek(piecesPropertyIndex, SeekOrigin.Begin);

            // Reading length of property 'pieces'
            long piecesValueLength = 0;
            while ((char)Peek(sr) != ':')
            {
                char cymbol = (char)sr.ReadByte();
                piecesValueLength *= 10;
                piecesValueLength += Convert.ToInt32(cymbol.ToString());
            }

            // Index, where info dictionary begins
            long startIndex = fileText.IndexOf("info") + 4;

            // Index, where info dictionary ends
            long endIndex = piecesPropertyIndex + $"{piecesValueLength}".Length + piecesValueLength + 2;

            // Bytes of info dictionary
            byte[] infoValueBytes = new byte[endIndex - startIndex];
            for (int i = 0; i < infoValueBytes.Length; i++)
                infoValueBytes[i] = _bencodeText[i + startIndex];

            return infoValueBytes;
        }

        private Dictionary<object, object> ReadDictionary()
        {
            Dictionary<object, object> dictionary = new Dictionary<object, object>();
            do
            {
                byte[] property = ReadString();
                object value = ReadValue();

                dictionary.Add(Encoding.UTF8.GetString(property), value);

            } while ((char)Peek(_reader) != 'e' && _reader.CanRead);

            _reader.ReadByte();
            return dictionary;
        }

        private byte[] ReadString()
        {
            byte[] property = null;
            char symbol = (char)_reader.ReadByte();
            int propLength = 0;
            while (symbol != ':')
            {
                propLength *= 10;
                propLength += Convert.ToInt32(symbol.ToString());
                symbol = (char)_reader.ReadByte();
            }

            property = new byte[propLength];
            _reader.Read(property, 0, propLength);

            return property;
        }

        private int ReadInteger()
        {
            string integer = "";
            char symbol = (char)_reader.ReadByte();
            do
            {
                integer += symbol;
                symbol = (char)_reader.ReadByte();

            } while (symbol != 'e');
            return Convert.ToInt32(integer);
        }

        private List<object> ReadList()
        {
            List<object> list = new List<object>();
            do
            {
                list.Add(ReadValue());

            } while ((char)Peek(_reader) != 'e');

            _reader.ReadByte();
            return list;
        }

        private object ReadValue()
        {
            char symbol = (char)Peek(_reader);
            if (Char.IsDigit(symbol))
            {
                return ReadString();
            }
            else if (symbol == 'd')
            {
                _reader.ReadByte();
                return ReadDictionary();
            }
            else if (symbol == 'i')
            {
                _reader.ReadByte();
                return ReadInteger();
            }
            else if (symbol == 'l')
            {
                _reader.ReadByte();
                return ReadList();
            }
            return null;
        }

        private int Peek(MemoryStream stream)
        {
            int symbol = stream.ReadByte();
            stream.Position--;
            return symbol;
        }
    }
}
