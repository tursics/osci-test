using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Osci.Encryption;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;

namespace Osci.Signature
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
    public class KeyInfo
    {
        public string KeyName
        {
            get; set;
        }

        public RetrievalMethod RetrievalMethod
        {
            get
            {
                return _retrievalMethod;
            }
            set
            {
                if (_keyType > KeyType.NotSet)
                {
                    {
                        throw new Exception("KeyInfo wurde schon anders Instanziert (X509Data, oder EncryptedKey)");
                    }
                }
                _keyType = KeyType.Retrieval;
                _retrievalMethod = value;
            }
        }

        public string KeyValue
        {
            get; set;
        }

        public EncryptedKey[] EncryptedKeys
        {
            get
            {
                return _encryptedKey.ToArray();
            }
        }

        public string AgreementMethod
        {
            get; set;
        }

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public int TypeOfKey
        {
            get
            {
                return (int)_keyType;
            }
        }

        public X509Data X509Data
        {
            get
            {
                return x509Data;
            }
        }

        public string MgmtData
        {
            get
            {
                return _mgmtData;
            }
            set
            {
                _mgmtData = value;
            }
        }

        internal X509Data x509Data;

        private static readonly Log _log = LogFactory.GetLog(typeof(KeyInfo));

        private enum KeyType
        {
            NotSet = -1,
            EncryptedKey = 0,
            Retrieval = 1,
            X509Data = 2
        }

        private KeyType _keyType;

        private RetrievalMethod _retrievalMethod;
        private string _mgmtData;

        /// <summary>Liste enthaltener Encryptedkey Elemente. 
        /// </summary>
        private readonly List<EncryptedKey> _encryptedKey;

        private string _id;

        /// <summary>DefaultHandler auf den zurückgesprungen wird. (SAX) 
        /// </summary>
        private DefaultHandler _defaultHandler = null;

        /// <summary>SAX Parser. 
        /// </summary>
        private XmlReader _xmlReader = null;

        /// <summary>Element, welches gerade geparsed wird. 
        /// </summary>
        private string _currentElement = null;

        /// <summary>Stringbuffer für Characters. 
        /// </summary>
        private StringBuilder _tempStrBuffer;

        /// <summary> Creates a new KeyInfo object.
        /// </summary>
        /// <param name="id">
        /// </param>
        /// <param name="uRI">
        /// </param>
        public KeyInfo(string uri, string id)
            : this()
        {
            _id = id;
            _keyType = KeyType.Retrieval;
            _log.Trace("Konstruktortype: RetrievelMethod");
            RetrievalMethod retrievalMethod = new RetrievalMethod();
            retrievalMethod.Uri = uri;
            _retrievalMethod = retrievalMethod;
        }

        public KeyInfo(string uri)
            : this()
        {
            _keyType = KeyType.Retrieval;
            _log.Trace("Konstruktortype: RetrievelMethod");
            RetrievalMethod retrievalMethod = new RetrievalMethod();
            retrievalMethod.Uri = uri;
            _retrievalMethod = retrievalMethod;
        }

        /// <summary> Creates a new KeyInfo object.
        /// </summary>
        public KeyInfo()
        {
            _encryptedKey = new List<EncryptedKey>();
            _keyType = KeyType.NotSet;
        }

        /// <summary>Konstruktor. Transportverschluesselung.
        /// </summary>
        public KeyInfo(X509Certificate cert)
            : this()
        {
            _keyType = KeyType.X509Data;
            _log.Trace("Konstruktortype: X509Certificate Data");
            x509Data = new X509Data();
            x509Data.SetX509Certificate(cert);
        }

        public void AddEncryptedKey(EncryptedKey encryptedKey)
        {
            _encryptedKey.Add(encryptedKey);
        }

        public void WriteXml(Stream stream, string ds, string xenc)
        {
            stream.Write("<" + ds + ":KeyInfo");
            if (_id != null)
            {
                stream.Write(" Id=\"" + _id + "\">");
            }
            else
            {
                byte[] b = new byte[1];
                b[0] = 0x3e;
                stream.Write(b, 0, 1);
            }
            if (_encryptedKey.Count > 0)
            {
                for (int i = 0; i < _encryptedKey.Count; i++)
                {
                    _encryptedKey[i].WriteXml(stream, ds, xenc);
                }
            }
            if (_retrievalMethod != null)
            {
                stream.Write("<" + ds + ":RetrievalMethod Type=\"" + RetrievalMethod.Type + "\" URI=\"" + RetrievalMethod.Uri + "\"></" + ds + ":RetrievalMethod>");
            }
            if (x509Data != null)
            {
                x509Data.WriteXml(stream, ds);
            }
            else if (_mgmtData != null)
            {
                stream.Write("<" + ds + ":MgmtData>" + _mgmtData + "</ds:MgmtData>");
            }
            stream.Write("</ds:KeyInfo>");
        }
    }
}