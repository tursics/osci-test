using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Osci.Common;
using Osci.Extensions;
using Osci.Helper;

namespace Osci.Cryptographic
{
    public class SecretKey
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(SecretKey));

        public SymmetricCipherAlgorithm AlgorithmType
        {
            get;
        }

        public int KeySize => _innerAlgorithm.KeySize;

        public byte[] Key => _innerAlgorithm.Key;
        

        /// <exception cref="ArgumentNullException"></exception>
        // ReSharper disable once InconsistentNaming
        public byte[] IV
        {
            get
            {
                return _innerAlgorithm.IV;
            }
            set
            {
                _innerAlgorithm.IV = value;
            }
        }

        private readonly SymmetricAlgorithm _innerAlgorithm;


        #region c'tor

        /// <summary>
        /// Creates a SecretKey object with the default symmetric cipher algorithm.
        /// <para>see Constants.DefaultSymmetricCipherAlgorithm</para>
        /// </summary>
        public SecretKey()
            : this(Constants.DefaultSymmetricCipherAlgorithm)
        {
        }


        public SecretKey(string algorithm)
            : this(GetAlgorithm(algorithm))
        {
        }

        public SecretKey(SymmetricCipherAlgorithm algorithmType)
            : this(CreateRandomKey(algorithmType), algorithmType)
        {
        }

        public SecretKey(byte[] key, string algorithm)
            : this(key, GetAlgorithm(algorithm))
        {
        }

        public SecretKey(byte[] key, SymmetricCipherAlgorithm algorithmType)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (SymmetricCipherAlgorithm.Aes128Cbc.Equals(algorithmType) ||
                SymmetricCipherAlgorithm.Aes192Cbc.Equals(algorithmType) ||
                SymmetricCipherAlgorithm.Aes256Cbc.Equals(algorithmType))
            {
                _log.Warn("CBC will not be supported in the future!");
            }

            AlgorithmType = algorithmType;
            _innerAlgorithm = _algorithmMap[algorithmType]();
            _innerAlgorithm.Key = key;
        }

        #endregion


        public ICryptoTransform CreateDecryptor()
        {
            return _innerAlgorithm.CreateDecryptor();
        }

        public ICryptoTransform CreateEncryptor()
        {
            return _innerAlgorithm.CreateEncryptor();
        }

        // ReSharper disable once InconsistentNaming
        public void GenerateIV()
        {
            _innerAlgorithm.GenerateIV();
        }

        private static byte[] CreateRandomKey(SymmetricCipherAlgorithm algorithm)
        {
            SymmetricAlgorithm a = _algorithmMap[algorithm]();
            a.GenerateKey();
            return a.Key;
        }

        private static SymmetricCipherAlgorithm GetAlgorithm(string name)
        {
            if (_algorithmMap.Any(_ => _.Key.IsEqualTo(name)))
            {
                return _algorithmMap.Single(_ => _.Key.IsEqualTo(name)).Key;
            }
            throw new NotSupportedException("Algorithm is not supported: " + name);
        }

        private static readonly Dictionary<SymmetricCipherAlgorithm, Func<SymmetricAlgorithm>> _algorithmMap =
            new Dictionary<SymmetricCipherAlgorithm, Func<SymmetricAlgorithm>>
            {
                {SymmetricCipherAlgorithm.TripleDesCbc, () => new TripleDESCryptoServiceProvider()},

                {SymmetricCipherAlgorithm.Aes128Cbc, () => new RijndaelManaged {KeySize = 128}},
                {SymmetricCipherAlgorithm.Aes192Cbc, () => new RijndaelManaged {KeySize = 192}},
                {SymmetricCipherAlgorithm.Aes256Cbc, () => new RijndaelManaged {KeySize = 256}},

                {SymmetricCipherAlgorithm.Aes128Gcm, () => new AesGcmManaged(128, Constants.DefaultGcmIVLength)},
                {SymmetricCipherAlgorithm.Aes192Gcm, () => new AesGcmManaged(192, Constants.DefaultGcmIVLength)},
                {SymmetricCipherAlgorithm.Aes256Gcm, () => new AesGcmManaged(256, Constants.DefaultGcmIVLength)}
            };
    }
}