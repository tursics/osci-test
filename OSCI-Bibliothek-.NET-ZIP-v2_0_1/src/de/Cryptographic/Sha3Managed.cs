using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Digests;

namespace Osci.Cryptographic
{
    public sealed class Sha3Managed
        : HashAlgorithm
    {
        public override int HashSize
        {
            get
            {
                return _sha3Digest.GetDigestSize() * 8;
            }
        }

        private readonly List<int> _supportedBitLengths = new List<int> { 256, 384, 512 };
        private readonly Sha3Digest _sha3Digest;


        /// <summary>
        /// Creates the SHA3 digest algorithm with the given <paramref name="bitLength"/>.
        /// <para>Supported bit lengths are 256, 384, 512.</para>
        /// </summary>
        /// <param name="bitLength"></param>

        public Sha3Managed(int bitLength)
        {
            if (!_supportedBitLengths.Any(_ => _.Equals(bitLength)))
            {
                throw new NotSupportedException("Only the following bit-lengths are supported: " +
                                                string.Join(", ", _supportedBitLengths.Select(_ => _.ToString()).ToArray()));
            }
            _sha3Digest = new Sha3Digest(bitLength);
        }

        public override void Initialize()
        {
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _sha3Digest.BlockUpdate(array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            HashValue = new byte[_sha3Digest.GetDigestSize()];
            _sha3Digest.DoFinal(HashValue, 0);
            return HashValue;
        }

        protected override void Dispose(bool disposing)
        {
            _sha3Digest.Reset();
            base.Dispose(disposing);
        }
    }
}
