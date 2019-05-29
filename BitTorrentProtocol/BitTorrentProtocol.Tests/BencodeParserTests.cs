using System;
using BitTorrentProtocol.Bencode;
using BitTorrentProtocol.Encrypting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitTorrentProtocol.Tests
{
    [TestClass]
    public class BencodeParserTests
    {
        private const string _viyFilePath = @"Files/viy.torrent";
        private const string _ubuntuFIlePath = @"Files/ubuntu-19.04-desktop.torrent";

        [TestMethod]
        public void ComputeInfoHash_UbuntuFile()
        {
            BencodeParser parser = new BencodeParser(_ubuntuFIlePath);
            TorrentInfo torrentFile = new TorrentInfo();
            torrentFile.InfoHash = new Sha1Encryptor().GetHash(parser.ReadInfoValue());

            Assert.AreEqual("D540FC48EB12F2833163EED6421D449DD8F1CE1F", torrentFile.InfoHash);
        }

        [TestMethod]
        public void TorrentInfo_UbuntuFile()
        {
            TorrentInfo torrentFile = new TorrentInfo();
            torrentFile.Announce = "http://torrent.ubuntu.com:6969/announce";
            torrentFile.Comment = "Ubuntu CD releases.ubuntu.com";
            torrentFile.PieceLength = 524288;
            torrentFile.AnnounceList = new System.Collections.Generic.List<string>
            {
                "http://torrent.ubuntu.com:6969/announce", "http://ipv6.torrent.ubuntu.com:6969/announce"
            };
            torrentFile.Files = new System.Collections.Generic.List<(string, long)>
            {
                ("ubuntu-19.04-desktop-amd64.iso", 2097152000)
            };

            BencodeParser parser = new BencodeParser(_ubuntuFIlePath);
            TorrentInfo expectedTorrentFile = parser.GetTorrentInfo();
            torrentFile.InfoHash = new Sha1Encryptor().GetHash(parser.ReadInfoValue());

            Assert.AreEqual(expectedTorrentFile.Announce, torrentFile.Announce);
            Assert.AreEqual(expectedTorrentFile.Comment, torrentFile.Comment);
            Assert.AreEqual(expectedTorrentFile.PieceLength, torrentFile.PieceLength);
            CollectionAssert.AreEqual(expectedTorrentFile.AnnounceList, torrentFile.AnnounceList);
            CollectionAssert.AreEqual(expectedTorrentFile.Files, expectedTorrentFile.Files);
        }

        [TestMethod]
        public void ComputeInfoHash_ViyFile()
        {
            BencodeParser parser = new BencodeParser(_viyFilePath);
            TorrentInfo torrentFile = new TorrentInfo();
            torrentFile.InfoHash = new Sha1Encryptor().GetHash(parser.ReadInfoValue());

            Assert.AreEqual("FA96C95C4DD0174BEC025A78ABEB6AC286A757BA", torrentFile.InfoHash);
        }

        [TestMethod]
        public void TorrentInfo_ViyFile()
        {
            TorrentInfo torrentFile = new TorrentInfo();
            torrentFile.Announce = "http://bt4.rutracker.org/ann?uk=wa36F12BA3";
            torrentFile.Comment = "http://rutracker.org/forum/viewtopic.php?t=4390356";
            torrentFile.PieceLength = 262144;
            torrentFile.AnnounceList = new System.Collections.Generic.List<string>
            {
                "http://bt4.rutracker.org/ann?uk=wa36F12BA3", "http://retracker.local/announce"
            };
            torrentFile.Files = new System.Collections.Generic.List<(string, long)>
            {
                ("Soft7/DirectX14/dxwebsetup.exe", 299864), ("Soft6/Redist16/vcredist_x86.exe", 4995416),
                ("Soft6/Redist16/vcredist_x64.exe", 4877648), ("Data.bin", 180935132),
                ("Setup.exe", 7389257), ("Survivors Viy.ico", 118921), ("autorun.inf", 70),
            };

            BencodeParser parser = new BencodeParser(_viyFilePath);
            TorrentInfo expectedTorrentFile = parser.GetTorrentInfo();
            torrentFile.InfoHash = new Sha1Encryptor().GetHash(parser.ReadInfoValue());

            Assert.AreEqual(expectedTorrentFile.Announce, torrentFile.Announce);
            Assert.AreEqual(expectedTorrentFile.Comment, torrentFile.Comment);
            Assert.AreEqual(expectedTorrentFile.PieceLength, torrentFile.PieceLength);
            CollectionAssert.AreEqual(expectedTorrentFile.AnnounceList, torrentFile.AnnounceList);
            CollectionAssert.AreEqual(expectedTorrentFile.Files, expectedTorrentFile.Files);
        }
    }
}
