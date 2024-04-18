using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Osci.Common;

namespace Osci.Cryptographic
{
    public sealed class AesGcmManaged
        : Aes
    {
        public override byte[] IV
        {
            get
            {
                return IVValue;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (value.Length != 16 && value.Length != 12)
                {
                    throw new NotSupportedException("IV must be 16 or 12 bytes.");
                }
                IVValue = value;
            }
        }


        private readonly List<int> _supportedKeySizes = new List<int> { 128, 192, 256 };

		private int GcmIvLength = Constants.DefaultGcmIVLength;

		/// <summary>
		/// Creates the AES/GCM encryption algorithm with the given <paramref name="keySize"/>.
		/// <para>Supported bit lengths are 128, 192, 256. Use default IV length size</para>
		/// </summary>
		/// <param name="keySize"></param>
		public AesGcmManaged(int keySize)
			: this(keySize, Constants.DefaultGcmIVLength)
		{}


		/// <summary>
		/// Creates the AES/GCM encryption algorithm with the given <paramref name="keySize"/>.
		/// <para>Supported bit lengths are 128, 192, 256. Supported IV length sizes are 12 and 16 bytes</para>
		/// </summary>
		/// <param name="keySize"></param>
		public AesGcmManaged(int keySize, int ivLength)
        {
            if (!_supportedKeySizes.Any(_ => _.Equals(keySize)))
            {
                throw new NotSupportedException("Only the following key sizes are supported: " +
                                                string.Join(", ", _supportedKeySizes.Select(_ => _.ToString()).ToArray()));
            }

            BlockSizeValue = 16;
            FeedbackSizeValue = 128;
            KeySizeValue = keySize;
            LegalBlockSizesValue = new[] { new KeySizes(16, 16, 0) };
            LegalKeySizesValue = new[] { new KeySizes(keySize, keySize, 0) };
            ModeValue = CipherMode.CTS;
            PaddingValue = PaddingMode.None;
			GcmIvLength = ivLength;

			GenerateKey();
            GenerateIV();
        }

        internal ICipherParameters CreateCipherParameters()
        {
            KeyParameter keyParameter = ParameterUtilities.CreateKeyParameter("AES", Key);
            return new ParametersWithIV(keyParameter, IV);
        }

        public override ICryptoTransform CreateDecryptor()
        {
            return new GcmBlockTransform(CreateCipherParameters(), false);
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIv)
        {
            KeyValue = rgbKey;
            IVValue = rgbIv;
            return CreateDecryptor();
        }

        public override ICryptoTransform CreateEncryptor()
        {
            return new GcmBlockTransform(CreateCipherParameters(), true);
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIv)
        {
            KeyValue = rgbKey;
            IVValue = rgbIv;
            return CreateEncryptor();
        }

        public override void GenerateKey()
        {
            CipherKeyGenerator keyGenerator = new CipherKeyGenerator();
            keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), KeySize));
            KeyValue = keyGenerator.GenerateKey();
        }

        public override void GenerateIV()
        {
            CipherKeyGenerator keyGenerator = new CipherKeyGenerator();
            keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), GcmIvLength * 8));
            IVValue = keyGenerator.GenerateKey();
        }
    }
}
