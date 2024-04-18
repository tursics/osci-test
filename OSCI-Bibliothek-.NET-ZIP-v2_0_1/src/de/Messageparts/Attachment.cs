using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Encryption;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;

namespace Osci.MessageParts
{
    /// <summary><p>Die Attachment-Klasse repräsentiert einen Anhang einer OSCI-Nachricht.
    /// Attachments werden in Content-Elementen mittels eines href-Attributs referenziert.
    /// Sie besitzen hierfür einen Identifier (refId), der innerhalb der Nachricht, an die das
    /// Attachment gehängt wird, eindeutig sein muss. Da dieser String als Referenz verwendet
    /// wird, empfielt sich die Verwendung von URL-encoding
    /// (s. System.Web.HttpUtility.UrlEncode(String, Encoding)) bzw. URL-decoding.</p>
    /// <p>Ein Attachment kann mit einem eigenem symmetrischen Schlüssel versehen werden.
    /// Hierdurch kann in verschiedenen verschlüsselten Inhaltsdatencontainern (EncryptedDataOSCI)
    /// dasselbe Attachment referenziert werden.</p>
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
    /// <seealso cref="Content">
    /// </seealso>
    /// <seealso cref="ContentContainer"> 
    /// </seealso>
    public class Attachment
        : MessagePart
    {
        public const int StateOfAttachmentClient = 0;
        public const int StateOfAttachmentParsing = 1;
        public const int StateOfAttachmentEncrypted = 2;

        public static string MyDigest = "";
        private Hashtable _mimeHeaders;
        public string BoundaryString = DialogHandler.Boundary;

        internal Hashtable EncryptedDigestValues = new Hashtable();
        internal int StateOfAttachment;
        internal SecretKey SecretKey;

        /// <summary>InputStream des Attachments.
        /// </summary>
        private OsciDataSource _swapBuffer;
        private static readonly Log _log = LogFactory.GetLog(typeof(Attachment));
        private string _contentType = "text";
        internal int _ivLength = Constants.DefaultGcmIVLength;

        /// <summary>Die Länge des Attachments in Bytes.
        /// </summary>
        private long _length = -1;


        /// <summary> Liefert die Länge des Attachments in Byte.
        /// </summary>
        /// <value> Länge des Attachments
        /// </value>
        public long Length
        {
            get
            {
                if (IsBase64Encoded)
                {
                    return Helper.Base64.CalculateBase64Length(_length);
                }
                else
                {
                    return _length;
                }
            }
        }

        public int IvLength
        {
            get
            {
                return _ivLength;
            }
            set
            {
                _ivLength = value;
            }
        }

        public bool IsBase64Encoded
        {
            get; set;
        }

        public string Boundary
        {
            set
            {
                BoundaryString = value;
            }
        }

        /// <summary> Ruft den InputStream der Daten ab, wenn das Attachment einer
        /// empfangenen Nachricht entnommen wurde, oder legt diesen fest.
        /// </summary>
        /// <value> InputStream der Anhangsdaten, oder null, wenn die Nachricht
        /// nicht empfangen (d.h. mit Hilfe eine Konstruktors erzeugt wurde).
        /// </value>
        public Stream Stream
        {
            get
            {
                _log.Trace("Attachment Object: ");

                if (_swapBuffer == null)
                {
                    throw new SystemException(DialogHandler.ResourceBundle.GetString("invalid_stateofobject") + "no OSCI-DataSource Object.");
                }
                Stream inputStream = _swapBuffer.InputStream;
                inputStream.Seek(0, SeekOrigin.Begin);

                if (Encrypted || (StateOfAttachment == StateOfAttachmentEncrypted))
                {
                    if (SecretKey != null)
                    {

                        _log.Trace("Attachment ist verschlüsselt codiert.");
                        return new SymCipherInputStream(inputStream, SecretKey, _ivLength);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    _log.Trace("Daten sind nur BASE64igt");
                    return inputStream;
                }
            }
        }

        /// <summary> Ruft den Content-Type (Http-Content-Type) der Binärdaten ab, oder legt diesen fest.
        /// </summary>
        /// <value> Content der Binärdaten, sollte ein gültiges Mime-Format sein (z.B. 'text/html' oder 'image/gif').
        /// </value>
        public string ContentType
        {
            get
            {
                return _contentType;
            }
            set
            {
                _contentType = value;
            }
        }

        /// <summary> Setzt zusätzliche MIME-Headereinträge für den MIME-boundary
        /// Abschnitt des Attachments. Die key- und value-Strings der Hashtable müssen
        /// den MIME-Spezifikationen genügen. Die Header
        /// Content-Transfer-Encoding, Content-ID, Content-Type  und Content-Length werden
        /// von der Implementierung gesetzt und werden hier ignoriert.
        /// </summary>
        /// <value> headers MIME-Header als key-value-Paare
        /// </value>
        public Hashtable MimeHeaders
        {
            get
            {
                return _mimeHeaders;
            }
            set
            {
                _mimeHeaders = value;
                RemoveHeaders(_mimeHeaders);
            }
        }

        /// <summary> Liefert true, wenn es sich um ein verschlüsseltes Attachment handelt.
        /// </summary>
        /// <value>verschlüsselt -> true, unverschlüsselt -> false
        /// </value>
        public bool Encrypted
        {
            get; internal set;
        }

        #region c'tor

        private Attachment()
        {
            StateOfAttachment = StateOfAttachmentClient;
            IsBase64Encoded = true;
        }

        /// <summary> Dieser Konstruktor wird vom Parser aufgerufen.
        /// </summary>
        internal Attachment(Stream ins, string refId, long length, string transportDigestAlgorithm)
            : this()
        {
            RefId = refId;
            _length = length;

            if (ins != null)
            {
                setInputStream(ins, false, length, transportDigestAlgorithm);
            }
        }

        /// <summary> Erzeugt ein neues Attachment-Objekt aus dem InputStream. Der geheime
        /// Schlüssel wird für die Verschlüsselung des Attachments benutzt.
        /// </summary>
        /// <param name="inputStream">der InputStream, aus dem die Daten gelesen und an die Nachricht
        /// angehängt wird.
        /// </param>
        /// <param name="refId">Identifier des Anhangs, z.B. Dateiname. Dieser Identifier muss innerhalb
        /// der Nachricht, an die das Attachment gehängt wird, eindeutig sein
        /// </param>
        /// <param name="secretKey">Secret-Key mit dem verschlüsselt werden soll.
        /// Der übergebene Schlüssel muss daher für diesen Algorithmus anwendbar sein.
        /// Wird dieser Parameter mit 'null' übergeben, wird ein neuer AES-256-Schlüssel erzeugt.
        /// </param>
        public Attachment(Stream inputStream, string refId, SecretKey secretKey)
            : this(inputStream, refId, secretKey, Constants.DefaultGcmIVLength)
        {
        }

        /// <summary> Erzeugt ein neues Attachment-Objekt aus dem InputStream. Der geheime
        /// Schlüssel wird für die Verschlüsselung des Attachments benutzt.
        /// </summary>
        /// <param name="inputStream">der InputStream, aus dem die Daten gelesen und an die Nachricht
        /// angehängt wird.
        /// </param>
        /// <param name="refId">Identifier des Anhangs, z.B. Dateiname. Dieser Identifier muss innerhalb
        /// der Nachricht, an die das Attachment gehängt wird, eindeutig sein
        /// </param>
        /// <param name="secretKey">Secret-Key mit dem verschlüsselt werden soll.
        /// Der übergebene Schlüssel muss daher für diesen Algorithmus anwendbar sein.
        /// Wird dieser Parameter mit 'null' übergeben, wird ein neuer AES-256-Schlüssel erzeugt.
        /// </param>
        /// <param name="ivLength">Länge des IV in Bytes
        /// </param>
        public Attachment(Stream inputStream, string refId, SecretKey secretKey, int ivLength)
            : this()
        {
            if (inputStream == null)
            {
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString("invalid_firstargument") + " ins");
            }
            if (refId == null)
            {
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString("invalid_secondargument") + " refId");
            }
            if (secretKey == null)
            {
                SecretKey = new SecretKey();
            }
            else
            {
                SecretKey = secretKey;
            }

            _ivLength = ivLength;

            Encrypted = true;

            RefId = refId;
            _log.Debug("RefId Name: " + refId);
            MakeTempFile(inputStream);
        }

        /// <summary> Erzeugt ein neues Attachment-Objekt aus dem InputStream. Das Attachment wird unverschlüsselt übertragen.
        /// </summary>
        /// <param name="ins">der InputStream, aus dem die Daten gelesen und an die Nachricht
        /// angehängt wird.
        /// </param>
        /// <param name="refId">Identifier des Anhangs, z.B. Dateiname. Dieser Identifier muss innerhalb
        /// der Nachricht, an die das Attachment gehängt wird, eindeutig sein.
        /// </param>
        public Attachment(Stream inputStream, string refId)
            : this()
        {
            if (inputStream == null)
            {
                throw new ArgumentException("Der InputStream darf nicht null sein.");
            }

            if (refId == null)
            {
                throw new ArgumentException("Die refId muss eingestellt werden.");
            }

            Encrypted = false;

            RefId = refId;
            _log.Debug("RefId Name: " + refId);
            MakeTempFile(inputStream);
        }

        #endregion


        private static void RemoveHeaders(Hashtable ht)
        {
            string[] hd = { "content-transfer-encoding", "content-id", "content-length", "content-type" };

            string[] k = new string[ht.Keys.Count];
            ht.Keys.CopyTo(k, 0);
            for (int i = 0; i < k.Length; i++)
            {
                for (int j = 0; j < hd.Length; j++)
                {
                    if (k[i].ToLower().Equals(hd[j]))
                    {
                        ht.Remove(k[i]);
                    }
                }
            }
        }

        /// <summary> Liefert den Hashwert des Attachments.
        /// </summary>
        /// <value> Hashwert des Attachments
        /// </value>
        public override byte[] GetDigestValue(string algorithm)
        {
            if (!DigestValues.ContainsKey(algorithm))
            {
                DigestValues.Add(algorithm, CreateDigest(algorithm, Encrypted));
            }

            return (byte[])DigestValues[algorithm];
        }



        /// <summary> Liefert den Hashwert nach der Verschlüsselung für die Nachrichtensignatur.
        /// </summary>
        /// <value>Hashwert der Nachrichtensignatur
        /// </value>
        public byte[] GetEncryptedDigestValue(string digestAlgorithm)
        {
            if (!EncryptedDigestValues.ContainsKey(digestAlgorithm))
            {
                EncryptedDigestValues.Add(digestAlgorithm, CreateDigest(digestAlgorithm, false));
            }
            return (byte[])EncryptedDigestValues[digestAlgorithm];
        }

        /// <summary> Diese Methode wird nur vom Parser aufgerufen.
        /// </summary>
        /// <param name="inputStream">Inputstream des Attachments.
        /// </param>
        /// <param name="encrypted">Status des Attachments (sind die Daten verschlüsselt).
        /// </param>
        /// <param name="length">Länge des Streams.
        /// </param>
        internal void setInputStream(Stream inputStream, bool encrypted, long length, string transportDigestAlgorithm)
        {
            _log.Debug("setInputStream: " + inputStream);
            _length = length;
            Encrypted = encrypted;

            if (inputStream == null)
            {
                throw new ArgumentException("Der Inputstream darf nicht 'null' sein");
            }

            _swapBuffer = DialogHandler.NewDataBuffer;

            Stream outputStream = _swapBuffer.OutputStream;
            HashAlgorithm messageDigest = null;
            if (transportDigestAlgorithm != null)
            {
                messageDigest = Crypto.CreateMessageDigest(transportDigestAlgorithm);
                outputStream = new CryptoStream(outputStream, messageDigest, CryptoStreamMode.Write);
            }

            inputStream.CopyToStream(outputStream);
            inputStream.Close();
            outputStream.Close();

            if (transportDigestAlgorithm != null)
            {
                EncryptedDigestValues.Add(transportDigestAlgorithm, messageDigest.Hash);
            }
        }

        public bool HasDigestValue(string digestAlgorithm)
        {
            return DigestValues.ContainsKey(digestAlgorithm);
        }

        /// <summary> Diese Methode ließt die Daten aus dem InputStream und verschlüsselt sie und
        /// ermittelt den Digest des Attachments. Das verschlüsselte Ergebnis wird in einen
        /// OSCIDataSource geschrieben.
        /// </summary>
        private void MakeTempFile(Stream inputStream)
        {
            if (_swapBuffer != null)
            {
                return;
            }

            _log.Trace("MakeTempFile wurde aufgerufen.");
            _swapBuffer = DialogHandler.NewDataBuffer;
            Stream outputStream = _swapBuffer.OutputStream;
            HashAlgorithm encMsgDigest = new SHA256Managed();
            HashAlgorithm msgDigest = new SHA256Managed();

            if (Encrypted)
            {
                CryptoStream encMsgDigestStream = new CryptoStream(outputStream, encMsgDigest, CryptoStreamMode.Write);
                SymCipherOutputStream cipherOut = new SymCipherOutputStream(encMsgDigestStream, SecretKey, _ivLength, true);
                outputStream = cipherOut;
            }
            else
            {
                _log.Trace("Unverschlüsseltes Attachment wird erstellt");
            }

            CryptoStream msgDigestStream = new CryptoStream(outputStream, msgDigest, CryptoStreamMode.Write);
            inputStream.CopyToStream(msgDigestStream);
            msgDigestStream.Close();

            DigestValues.Add(Constants.DigestAlgorithmSha256, msgDigest.Hash);

            if (Encrypted)
            {
                EncryptedDigestValues.Add(Constants.DigestAlgorithmSha256, encMsgDigest.Hash);
            }
            else
            {
                EncryptedDigestValues.Add(Constants.DigestAlgorithmSha256, DigestValues[Constants.DigestAlgorithmSha256]);
            }

            _length = _swapBuffer.Length;
        }

        private byte[] CreateDigest(string algorithm, bool encrypt)
        {
            Stream inputStream = _swapBuffer.InputStream;

            if (inputStream == null)
            {
                throw new IOException(DialogHandler.ResourceBundle.GetString("invalid_inputstream"));
            }

            inputStream.Seek(0, SeekOrigin.Begin);

            _log.Debug("State of Attachment: " + StateOfAttachment + " Verschlüsselt: " + Encrypted);

            if (StateOfAttachment != StateOfAttachmentParsing && encrypt)
            {
                inputStream = new SymCipherInputStream(inputStream, SecretKey, _ivLength);
            }

            HashAlgorithm msgDigest = Crypto.CreateMessageDigest(algorithm);

            using (CryptoStream cryptoStream = new CryptoStream(inputStream, msgDigest, CryptoStreamMode.Read))
            {
                byte[] bytes = new byte[1024];
                while (cryptoStream.Read(bytes, 0, bytes.Length) > 0)
                {
                }
            }
            inputStream.Close();

            return msgDigest.Hash;
        }

        /// <summary>
        /// Serialisiert das MIME-Boundary des Attachments.
        /// </summary>
        /// <param name="outRenamed">Outputstream, in den das Attachment serialisiert wird.
        /// </param>
        /// <exception cref="System.Exception">IOException
        /// </exception>
        public override void WriteXml(Stream stream)
        {
            stream.Write("\r\n--" + BoundaryString + "\r\nContent-Type: " + _contentType + "\r\n");
            stream.Write("Content-Transfer-Encoding: ");
            stream.Write(IsBase64Encoded ? "base64" : "8bit");
            stream.Write("\r\nContent-ID: <" + RefId + ">\r\n");
            stream.Write("Content-Length: " + Length + "\r\n");

            if (_mimeHeaders != null)
            {
                Hashtable mh = (Hashtable)_mimeHeaders.Clone();
                RemoveHeaders(mh);
                for (IDictionaryEnumerator e = mh.GetEnumerator(); e.MoveNext();)
                {
                    string key = (string)e.Key;
                    stream.Write(key.Trim() + ": " + ((string)mh[key]).Trim() + "\r\n");
                }
            }
            stream.Write("\r\n");

            _log.Trace("######### SwapBuffer#### " + _swapBuffer);

            Stream inputStream = _swapBuffer.InputStream;
            inputStream.Seek(0, SeekOrigin.Begin);

            if (IsBase64Encoded)
            {
                stream = new Base64OutputStream(stream, true);
            }

            inputStream.CopyToStream(stream);
            inputStream.Close();

            if (IsBase64Encoded)
            {
                stream.Flush();
            }
        }
    }
}