using System;
using System.IO;
using System.Security.Cryptography;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Exceptions;

namespace Osci.Helper
{
    /// <summary> Diese Erweiterung der Klasse java.io.FilterOutputStream führt eine 
    /// Ver-/Entschlüsselung der geschriebenen Daten durch. Der Initialisierungsvektor
    /// wird gemäß der XML-Encryption-Spezifikation den Inhaltsdaten hinzugefügt bzw.
    /// entnommen.
    /// 
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    /// <seealso cref="SymCipherInputStream">
    /// </seealso>
    public class SymCipherOutputStream
        : OutputStream
    {
		private static readonly Log _log = LogFactory.GetLog(typeof(SymCipherOutputStream));
		private int _index;
        private readonly SecretKey _secretKey;
        private readonly bool _encrypt;
        private sbyte[] _tmp;

        private CryptoStream _baseStream;
        private Stream _outStream;

        public SymCipherOutputStream(Stream outStream, SecretKey secretKey, int ivLength, byte[] initializationVector)
            : this(outStream, secretKey, true, ivLength, initializationVector)
        {
        }

        public SymCipherOutputStream(Stream outStream, SecretKey secretKey, byte[] initializationVector)
            : this(outStream, secretKey, true, Constants.DefaultGcmIVLength, initializationVector)
        {
        }
        public SymCipherOutputStream(Stream outStream, SecretKey secretKey, int ivLength, bool encrypt)
            : this(outStream, secretKey, encrypt, ivLength, null)
        {
        }

        public SymCipherOutputStream(Stream outStream, SecretKey secretKey, bool encrypt)
            : this(outStream, secretKey, encrypt, Constants.DefaultGcmIVLength, null)
        {
        }

        internal SymCipherOutputStream(Stream outStream, SecretKey secretKey, bool encrypt, int ivLength, byte[] initializationVector)
        {
            _secretKey = secretKey;
            _encrypt = encrypt;

			if (encrypt)
            {
                if (secretKey == null)
                {
                    secretKey = new SecretKey();
                }

                if (initializationVector != null)
                {
                    secretKey.IV = initializationVector;
                }
            }

            if (secretKey.AlgorithmType.GetXmlName().EndsWith("tripledes-cbc"))
            {
                _secretKey.IV = new byte[8];
            }
            else if (_secretKey.AlgorithmType.GetXmlName().EndsWith("-gcm"))
            {
                if (ivLength == 16 || ivLength == 12)
                {
					_log.Trace("Use IV length for outgoing AES-GCM-encrypted stream: " + ivLength);
					_secretKey.IV = new byte[ivLength];

					if (ivLength == 16)
					{
						_log.Warn(DialogHandler.ResourceBundle.GetString("warning_iv_length"));
					}
				}
                else
                {
                    throw new IllegalArgumentException(string.Format("Wrong IV Length {0} given for AES-GCM-based algorithm! Cannot decrypt!", ivLength));
                }
            }
            else
            {
                _secretKey.IV = new byte[16];
            }

            if (encrypt)
            {
                outStream.Write(_secretKey.IV, 0, _secretKey.IV.Length);
                _baseStream = new CryptoStream(outStream, secretKey.CreateEncryptor(), CryptoStreamMode.Write);
            }

        }

        public override void Write(int b)
        {
            Write(new[] { (byte)b }, 0, 1);
        }

        public override void Write(byte[] data, int offset, int length)
        {
            if ((offset | length | (data.Length - (length + offset)) | (offset + length)) < 0)
            {
                throw new IndexOutOfRangeException();
            }

            if (!_encrypt && _index < _secretKey.IV.Length)
            {
                if (length < (_secretKey.IV.Length - _index))
                {
                    for (int i = 0; i < length; i++)
                    {
                        _secretKey.IV[_index++] = data[offset + i];
                    }
                }
                else
                {
                    int i;

                    for (i = 0; i < _secretKey.IV.Length - _index; i++)
                    {
                        _secretKey.IV[i + _index] = data[offset + i];
                    }

                    _index += i;

                    _baseStream = new CryptoStream(_outStream, _secretKey.CreateDecryptor(), CryptoStreamMode.Write);
                    _baseStream.Write(data, offset + i, length - i);
                }
            }
            else
            {
                _baseStream.Write(data, offset, length);
            }
        }

        public override void Close()
        {
            _baseStream.FlushFinalBlock();
        }
    }
}