using Osci.Common;
using Osci.Cryptographic;
using Osci.Exceptions;
using Osci.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Osci.Helper
{
    /// <summary> Diese Erweiterung der Klasse java.io.FilterInputStream führt eine 
	/// Ver-/Entschlüsselung der gelesenen Daten. Der Initialisierungsvektor
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
    /// <seealso cref="SymCipherOutputStream">
    /// </seealso>
    public class SymCipherInputStream
        : InputStream
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(SymCipherInputStream));
        private string _cipherName;
        private int _count;
        private readonly bool _encrypt;
        private readonly CryptoStream _baseStream;
        private readonly SecretKey _secretKey;

        public SymCipherInputStream(Stream stream, SecretKey secretKey, bool encrypt = false)
            : this(stream, secretKey, Constants.DefaultGcmIVLength, encrypt)
        {
        }

        public SymCipherInputStream(Stream stream, SecretKey secretKey, int ivLength, bool encrypt = false)
        {
            _encrypt = encrypt;
            _secretKey = secretKey;

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (secretKey == null)
            {
                throw new ArgumentNullException("secretKey", "Es wurde kein symmetrischer Schlüssel übergeben.");
            }

            if (_secretKey.AlgorithmType.GetXmlName().EndsWith("-cbc"))
            {
                _log.Warn("CBC will not be supported in the future and is not allowed for transport encryption any longer!");
            }

            _log.Trace("CipherAlgorithm: " + _secretKey.AlgorithmType);

            ICryptoTransform cryptoTransform;
            if (encrypt)
            {
                _secretKey.GenerateIV();
                cryptoTransform = _secretKey.CreateEncryptor();
            }
            else
            {
                byte[] iv = new byte[8];

                if (_secretKey.AlgorithmType.GetXmlName().EndsWith("tripledes-cbc"))
                {
                    iv = new byte[8];
                }
                else if (_secretKey.AlgorithmType.GetXmlName().EndsWith("-gcm"))
                {
                    if (ivLength == 16 || ivLength == 12)
                    {
                        _log.Trace("Use IV length for incoming AES-GCM-encrypted stream: " + ivLength);
                        iv = new byte[ivLength];

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
                    iv = new byte[16];
                }

                stream.Read(iv, 0, iv.Length);
                _secretKey.IV = iv;
                cryptoTransform = _secretKey.CreateDecryptor();
            }

            _baseStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Read);
        }

        public override int Read(byte[] buffer, int off, int len)
        {
            byte[] iv = _secretKey.IV;

            if (_encrypt && _count < iv.Length)
            {
                if (len < iv.Length - _count)
                {
                    Array.Copy(iv, _count, buffer, off, len);
                    _count += len;
                    return len;
                }
                else
                {
                    Array.Copy(iv, _count, buffer, off, iv.Length - _count);
                    int ret = _baseStream.CopyTo(buffer, off + iv.Length - _count, len - iv.Length + _count);
                    ret += iv.Length - _count;
                    _count = iv.Length;
                    return ret;
                }
            }
            else
            {
                try
                {
                    return _baseStream.Read(buffer, off, len);
                }
                catch (CryptographicException)
                {
                    // Prevent padding oracle attack: Do not expose cryptographic exceptions
                    return 0;
                }
            }
        }
    }
}