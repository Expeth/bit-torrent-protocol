using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentProtocol.Encrypting
{
    public class Sha1Encryptor : IEncryptor
    {
        public string GetHash(byte[] value)
        {
            return BitConverter.ToString(SHA1.Create().ComputeHash(value)).Replace("-", "");
        }

        public string GetUrlEncodedHash(byte[] value)
        {
            return "%" + BitConverter.ToString(SHA1.Create().ComputeHash(value)).Replace("-", "%").ToLower();
        }

        public byte[] GetHashBytes(byte[] value)
        {
            return SHA1.Create().ComputeHash(value);
        }
    }
}
