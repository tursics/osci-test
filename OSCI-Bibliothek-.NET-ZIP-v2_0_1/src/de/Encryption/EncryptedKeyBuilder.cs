using System.Collections;
using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Signature;
using System;

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
    public class EncryptedKeyBuilder
        : DefaultHandler
    {
        protected static string DsXmlns = Namespace.XmlDSig;
        protected static string XencXmlns = Namespace.XmlEnc;
        protected static string Xenc11Xmlns = "http://www.w3.org/2009/xmlenc11#";

        public EncryptedKey EncKey
        {
            get
            {
                return _encKey;
            }
        }

        private static readonly Log _log = LogFactory.GetLog(typeof(EncryptedKeyBuilder));
        private readonly EncryptedKey _encKey;
        private readonly ArrayList _transformer;
        private CipherReference _cipherRef;
        private CipherValue _cipherValue;
        private CipherData _cipherData;
        private string _currentElement;
        private readonly KeyInfoBuilder _parentHandler;
        private readonly XmlReader _xmlReader;

        public EncryptedKeyBuilder(XmlReader xmlReader, DefaultHandler parentHandler, Attributes attributes)
        {
            _transformer = new ArrayList();

            if (parentHandler is KeyInfoBuilder)
            {
                _parentHandler = (KeyInfoBuilder)parentHandler;
            }
            else
            {
                throw new SaxException("Encrypted Data darf nur als Unterelement von KeyInfo auftreten.");
            }
            _xmlReader = xmlReader;
            _encKey = new EncryptedKey();
            if (attributes != null)
            {
                if (attributes.GetValue("Recipient") != null)
                {
                    _encKey.Recipient = attributes.GetValue("Recipient");
                }
                if (attributes.GetValue("Id") != null)
                {
                    _encKey.Id = attributes.GetValue("Id");
                }
                if (attributes.GetValue("Type") != null)
                {
                    _encKey.Type = attributes.GetValue("Type");
                }
                if (attributes.GetValue("MimeType") != null)
                {
                    _encKey.MimeType = attributes.GetValue("MimeType");
                }
                if (attributes.GetValue("Encoding") != null)
                {
                    _encKey.Encoding = attributes.GetValue("Encoding");
                }
            }
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start-Element: " + localName + " " + uri);
            if (localName.Equals("EncryptedKey") && uri.Equals(XencXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("KeyInfo") && uri.Equals(DsXmlns))
            {
                _xmlReader.ContentHandler = new KeyInfoBuilder(_xmlReader, this, attributes);
            }
            else if (localName.Equals("EncryptionMethod") && uri.Equals(XencXmlns))
            {
                _encKey.EncryptionMethodAlgorithm = attributes.GetValue("Algorithm");
                
                // asymmetric algorithms are also stored separately
                foreach (AsymmetricCipherAlgorithm algo in Enum.GetValues(typeof(AsymmetricCipherAlgorithm)))
                {
                    if (algo.GetXmlName().Equals(_encKey.EncryptionMethodAlgorithm))
                    {
                        _encKey.AsymmetricCipherAlgorithm = algo;
                    }
                }
            }
            else if (localName.Equals("MGF") && uri.Equals(Xenc11Xmlns))
            {
                _encKey.MgfAlgorithm = attributes.GetValue("Algorithm");
            }
            else if (localName.Equals("DigestMethod") && uri.Equals(DsXmlns))
            {
                _encKey.DigestAlgorithm = attributes.GetValue("Algorithm");
            }
            else if (localName.Equals("CipherData") && uri.Equals(XencXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("CipherValue") && uri.Equals(XencXmlns))
            {
                _currentElement = "";
            }
            else if (localName.Equals("CipherReference") && uri.Equals(XencXmlns))
            {
                string cp = attributes.GetValue("URI");
                _cipherRef = new CipherReference(cp);
            }
            else if (localName.Equals("Transforms") && uri.Equals(DsXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("Transform") && uri.Equals(DsXmlns))
            {
                if (attributes.GetValue("Algorithm") == null)
                {
                    throw new SaxException("Das Attribute Algorithm ist beim Element 'Transform' erforderlich.");
                }
                _transformer.Add(attributes.GetValue("Algorithm"));
            }
            else if (localName.Equals("EncryptionProperties") && uri.Equals(XencXmlns))
            {
                throw new SaxException("Element EncryptionProperties wird nicht unterstützt");
            }
            else if (localName.Equals("RefernceList") && uri.Equals(XencXmlns))
            {
                throw new SaxException("Element RefernceList wird nicht unterstützt");
            }
            else if (localName.Equals("CarriedKeyName") && uri.Equals(XencXmlns))
            {
                _currentElement = "";
            }
            else
            {
                throw new SaxException("Unerwartetes Element im EncryptedKey-Builde: " + localName);
            }
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End-Element: " + localName);
            if (localName.Equals("EncryptedKey") && uri.Equals(XencXmlns))
            {
                _parentHandler.KeyInfo.AddEncryptedKey(_encKey);
                _xmlReader.ContentHandler = _parentHandler;
            }
            else if (localName.Equals("KeyInfo") && uri.Equals(DsXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("MGF") && uri.Equals(Xenc11Xmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("DigestMethod") && uri.Equals(DsXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("CipherData") && uri.Equals(XencXmlns))
            {
                if (_cipherRef != null)
                {
                    _cipherData = new CipherData(_cipherRef);
                }
                else
                {
                    _cipherData = new CipherData(_cipherValue);
                    _cipherValue.StateOfObject = CipherValue.StateEncrypted;
                }
                _encKey.CipherData = _cipherData;
            }
            else if (localName.Equals("CipherReference") && uri.Equals(XencXmlns))
            {
                IEnumerator transformers = _transformer.GetEnumerator();
                while (transformers.MoveNext())
                {
                    _cipherRef.AddTransform((string)transformers.Current);
                }
                _transformer.Clear();
            }
            else if (localName.Equals("Transforms") && uri.Equals(XencXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("Transform") && uri.Equals(DsXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("CipherValue") && uri.Equals(XencXmlns))
            {
                _cipherValue = new CipherValue(_currentElement);
            }
            else if (localName.Equals("EncryptionMethod") && uri.Equals(XencXmlns))
            {
                // not supported
            }
            else if (localName.Equals("EncryptionProperties") && uri.Equals(XencXmlns))
            {
                // not supported
            }
            else if (localName.Equals("RefernceList") && uri.Equals(XencXmlns))
            {
                // not supported
            }
            else if (localName.Equals("CarriedKeyName") && uri.Equals(XencXmlns))
            {
                _encKey.CarriedKeyName = _currentElement;
            }
            else
            {
                throw new SaxException("Unerwartetes Element im EncryptedKey-Builder: " + localName);
            }
            _currentElement = null;
        }

        public override void Characters(char[] ch, int start, int length)
        {
            _log.Trace("Character: " + new string(ch, start, length));
            if (_currentElement == null)
            {
                for (int i = 0; i < length; i++)
                {
                    if (ch[start + i] > ' ')
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_char"));
                    }
                }
            }
            else
            {
                _currentElement += new string(ch, start, length);
            }
        }
    }
}