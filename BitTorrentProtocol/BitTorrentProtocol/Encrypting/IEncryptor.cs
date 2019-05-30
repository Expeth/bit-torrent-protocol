using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Encrypting
{
    public interface IEncryptor
    {
        string GetHash(byte[] value);
        string GetUrlEncodedHash(byte[] value);
        byte[] GetHashBytes(byte[] value);
    }
}
