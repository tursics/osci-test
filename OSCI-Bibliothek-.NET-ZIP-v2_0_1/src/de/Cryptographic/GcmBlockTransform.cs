using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;

namespace Osci.Cryptographic
{
    internal sealed class GcmBlockTransform
        : ICryptoTransform
    {
        private readonly GcmBlockCipher _gcmBlockCipher;

        public int InputBlockSize => _gcmBlockCipher.GetBlockSize();

        public int OutputBlockSize => _gcmBlockCipher.GetBlockSize();

        public bool CanTransformMultipleBlocks => false;

        public bool CanReuseTransform => false;

        public GcmBlockTransform(ICipherParameters cipherParameters, bool forEncryption)
        {
            _gcmBlockCipher = new GcmBlockCipher(new AesFastEngine());
            _gcmBlockCipher.Init(forEncryption, cipherParameters);
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return _gcmBlockCipher.ProcessBytes(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] output = new byte[_gcmBlockCipher.GetOutputSize(inputCount)];
            int length = _gcmBlockCipher.ProcessBytes(inputBuffer, inputOffset, inputCount, output, 0);
            _gcmBlockCipher.DoFinal(output, length);
            return output;
        }

        public void Dispose()
        {
            _gcmBlockCipher.Reset();
        }
    }
}