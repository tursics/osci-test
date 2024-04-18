using System.IO;
using System.Security.Cryptography;
using Osci.Common;

namespace Osci.Helper
{
    /// <summary>
    /// Zusammenfassung für DigestStream.
    /// </summary>
    /// 
    public class DigestStream 
        : OutputStream
    {
        private bool _active = false;
        private readonly Stream _outStream;
        private CryptoStream _hashOut;
        private HashAlgorithm _msgDigest;

        public DigestStream(Stream outRenamed)
        {
            _outStream = outRenamed;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_active)
            {
                _hashOut.Write(buffer, offset, count);
            }
            else
                _outStream.Write(buffer, offset, count);
        }

        public void SetMessageDigest(HashAlgorithm md)
        {
            _msgDigest = md;
        }

        public void On(bool on)
        {
            if (on)
            {
                _msgDigest.Initialize();
                _active = true;
                //                msgDigest = new SHA1Managed();
                _hashOut = new CryptoStream(_outStream, _msgDigest, CryptoStreamMode.Write);
            }
            else
            {
                _active = false;
                //                hashOut.FlushFinalBlock();
            }
        }


        public byte[] GetDigest()
        {
            _hashOut.FlushFinalBlock();
            return _msgDigest.Hash;
        }
    }
}
