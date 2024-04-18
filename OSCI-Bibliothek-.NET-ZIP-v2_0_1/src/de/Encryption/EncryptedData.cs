using System;
using System.IO;
using System.Security.Cryptography;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Extensions;
using Osci.Helper;

namespace Osci.Encryption
{
    /// <exclude/>
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class EncryptedData
        : EncryptedType
    {
        public string Namespace
        {
            get; internal set;
        }

        internal string SoapNsPrefix
        {
            get; private set;
        }
        internal string OsciNsPrefix
        {
            get; private set;
        }
        internal string DsNsPrefix
        {
            get; private set;
        }
        internal string XencNsPrefix
        {
            get; private set;
        }
        internal string XsiNsPrefix
        {
            get; private set;
        }

        private static readonly byte[] _namespaces = (" " + Constants.DefaultNamespaces).ToByteArray();
        private static readonly Log _log = LogFactory.GetLog(typeof(EncryptedData));

        #region c'tor

        private EncryptedData(string id)
        {
            SoapNsPrefix = "soap";
            OsciNsPrefix = "osci";
            DsNsPrefix = "ds";
            XencNsPrefix = "xenc";
            XsiNsPrefix = "xsi";

            Id = id;
            Type = Constants.TypeContent;
            MimeType = "text/xml";
        }

		/// <summary> Erstellt ein EncryptedData Objekt mit einem CipherValue und der Default-IV-Länge.
		/// </summary>
		/// <param name="cipherValue">Cipher Value Objekt Nutzdatenverschlüsselung
		/// </param>
		/// <param name="encryptionMethodAlgorithm">Algorithmus
		/// </param>
		/// <param name="iD">ID des EncryptedData Objektes
		/// </param>
		/// <param name="key">symmetrischer Schlüssel für die Nutzdatenverschlüsselung
		/// </param>        
		public EncryptedData(CipherValue cipherValue, string id, SecretKey key)
            : this(cipherValue, id, key, Constants.DefaultGcmIVLength)
        {
		}

        /// <summary> Erstellt ein EncryptedData Objekt mit einem CipherValue.
        /// </summary>
		/// <param name="cipherValue">Cipher Value Objekt Nutzdatenverschlüsselung
		/// </param>
		/// <param name="encryptionMethodAlgorithm">Algorithmus
		/// </param>
		/// <param name="iD">ID des EncryptedData Objektes
		/// </param>
		/// <param name="key">symmetrischer Schlüssel für die Nutzdatenverschlüsselung
		/// </param>
		/// <param name="ivLength">Länge des IV in Bytes
		/// </param>
		public EncryptedData(CipherValue cipherValue, string id, SecretKey key, int ivLength)
            : this(id)
        {
            if (cipherValue == null)
            {
                throw new ArgumentNullException("cipherValue");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            cipherValue.SetSecretKey(key, ivLength);
            CipherData cipherData = new CipherData(cipherValue);
            EncryptionMethodAlgorithm = key.AlgorithmType.GetXmlName();
            IVLength = ivLength;
            CipherData = cipherData;
        }

		/// <summary> Erstellt ein EncryptedData Objekt mit einem CipherReference und der Default-IV-Länge.
		/// </summary>
		/// <param name="cipherReference">Cipher Reference Objekt Nutzdatenverschlüsselung
		/// </param>
		/// <param name="encryptionMethodAlgorithm">Algorithmus
		/// </param>
		/// <param name="iD">ID des EncryptedData Objektes
		/// </param>
		/// <param name="key">symmetrischer Schlüssel für die Nutzdatenverschlüsselung
		/// </param>
		public EncryptedData(CipherReference cipherReference, string id, SecretKey key)
            : this(cipherReference, id, key, Constants.DefaultGcmIVLength)
        {
        }


        /// <summary> Erstellt ein EncryptedData Objekt mit einem CipherReference
        /// </summary>
        /// <param name="cipherReference">Cipher Reference Objekt Nutzdatenverschlüsselung
        /// </param>
        /// <param name="encryptionMethodAlgorithm">Algorithmus
        /// </param>
        /// <param name="iD">ID des EncryptedData Objektes
        /// </param>
        /// <param name="key">symmetrischer Schlüssel für die Nutzdatenverschlüsselung
        /// </param>
        /// <param name="ivLength">Länge des IV in Bytes
        /// </param>
        public EncryptedData(CipherReference cipherReference, string id, SecretKey key, int ivLength)
            : this(id)
        {
            if (cipherReference == null)
            {
                throw new ArgumentNullException("cipherReference");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            CipherData cipherData = new CipherData(cipherReference);
            EncryptionMethodAlgorithm = key.AlgorithmType.GetXmlName();
            IVLength = ivLength;
            CipherData = cipherData;
        }

        /// <summary> Ein Konstruktor für den Parser
        /// </summary>
        /// <param name="encryptionMethodAlgorithm">Algorithmus
        /// </param>
        /// <param name="iD">ID des EncryptedData Objektes
        /// </param>
        internal EncryptedData(SymmetricCipherAlgorithm encryptionMethodAlgorithm, string id)
            : this(id)
        {
            EncryptionMethodAlgorithm = encryptionMethodAlgorithm.GetXmlName();
        }

        #endregion


        public EncryptedKey FindEncrypedKey(string refId)
        {
            EncryptedKey[] keys = KeyInfo.EncryptedKeys;

            foreach (EncryptedKey key in keys)
            {
                _log.Debug("KeyInfo Eintrag:" + key.KeyInfo.RetrievalMethod.Uri);
                _log.Debug("Vergleiche refID: refId: " + refId + " ## mit: " + key.KeyInfo.RetrievalMethod.Uri.EndsWith(refId));
                if (key.KeyInfo.RetrievalMethod.Uri.EndsWith(refId))
                {
                    return key;
                }
            }
            return null;
        }

        public void WriteXxml(Stream stream)
        {
            WriteXml(stream, true);
        }

        public void WriteXml(Stream stream, bool inner)
        {
            if (CipherData == null)
            {
                throw new OsciCipherException("Es wurde noch kein CipherData Object eingestellt.");
            }
            if (KeyInfo == null)
            {
                throw new OsciCipherException("KeyInfo ist nicht gesetzt.");
            }

            stream.Write("<" + XencNsPrefix + ":EncryptedData");
            if (!(stream is CryptoStream) && !inner)
            {
                if (Namespace == null)
                {
                    stream.Write(_namespaces, 0, _namespaces.Length);
                }
                else
                {
                    stream.Write(Namespace);
                }
            }
            if (!string.IsNullOrEmpty(Id))
            {
                stream.Write(" Id=\"" + Id + "\"");
            }

            stream.Write(" MimeType=\"" + MimeType + "\"><" + XencNsPrefix + ":EncryptionMethod Algorithm=\""+ EncryptionMethodAlgorithm + "\">");

            // Element nur schreiben, wenn ungleich altem Default-Wert (16), um Abwärtskompatibilität zu wahren
            if (IVLength != 16)
            {
                stream.Write("<osci128:IvLength xmlns:osci128=\"" + Osci.Common.Namespace.Osci128 + "\" Value=\"" + IVLength + "\"></osci128:IvLength>");
            }
            stream.Write("</" + XencNsPrefix + ":EncryptionMethod>");
            KeyInfo.WriteXml(stream, DsNsPrefix, XencNsPrefix);
            CipherData.WriteXml(stream, DsNsPrefix, XencNsPrefix);
            stream.Write("</" + XencNsPrefix + ":EncryptedData>");
        }
    }
}