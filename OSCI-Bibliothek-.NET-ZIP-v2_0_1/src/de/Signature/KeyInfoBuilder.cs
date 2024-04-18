using System.Text;
using Osci.Common;
using Osci.Encryption;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;

namespace Osci.Signature
{
    /// <exclude/>
    /// <summary> Builder, der ein Element ds:KeyInfo bearbeitet.
    /// 
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: PPI Financial Systems GmbH</p> 
    /// </summary>
    public class KeyInfoBuilder
        : DefaultHandler
    {
        protected static readonly string DsXmlns = Namespace.XmlDSig;
        protected static readonly string XencXmlns = Namespace.XmlEnc;

        /// <summary> Ruft das aufgebaute KeyInfo ab.
        /// </summary>
        /// <value> KeyInfo.
        /// </value>
        public KeyInfo KeyInfo
        {
            get;
        }

        internal DefaultHandler ParentHandler;
        internal XmlReader XmlReader;
        protected static Log Log = LogFactory.GetLog(typeof(KeyInfoBuilder));

        /// <summary> Wird gesetzt, sobald bekannt ist, dass eine Instanz
        /// von KeyInfoX509 aufzubauen ist.
        /// </summary>
        private bool _isX509Er;

        private string _currentElement;
        private X509DataBuilder _x509Builder;

        /// <summary> Gibt an, ob die Methode
        /// bereits aufgerufen worden ist.
        /// </summary>
        private bool _internalEndElementCalled = false;

        /// <summary> Konstruktor.
        /// </summary>
        /// <param name="parentHandler">DefaultCursorHandler, der diesen Builder erzeugt hat.
        /// </param>
        /// <param name="xmlReader">Aktueller CursorXMLReader.
        /// </param>
        /// <param name="attributes">Attribute des Elements ds:KeyInfo.
        /// </param>
        public KeyInfoBuilder(XmlReader xmlReader, DefaultHandler parentHandler, Attributes attributes)
        {
            XmlReader = xmlReader;
            ParentHandler = parentHandler;
            KeyInfo = new KeyInfo();
            if (attributes != null && attributes.GetValue("Id") != null)
            {
                KeyInfo.Id = attributes.GetValue("Id");
            }
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            Log.Trace("Start-Element: " + localName);
            if (localName.Equals("KeyName") && uri.Equals(DsXmlns))
            {
                _currentElement = "";
            }
            else if (localName.Equals("KeyValue") && uri.Equals(DsXmlns))
            {
                throw new SaxException("KeyValue - Elemente werden nicht unterstützt");
            }
            else if (localName.Equals("RetrievalMethod") && uri.Equals(DsXmlns))
            {
                RetrievalMethodBuilder retrievalMethodBuilder = new RetrievalMethodBuilder(XmlReader, this, attributes);
                XmlReader.ContentHandler = retrievalMethodBuilder;
            }
            else if (localName.Equals("X509Data") && uri.Equals(DsXmlns))
            {
                _isX509Er = true;
                _x509Builder = new X509DataBuilder(XmlReader, this, attributes);
                XmlReader.ContentHandler = _x509Builder;
            }
            else if (localName.Equals("PGPData") && uri.Equals(DsXmlns))
            {
                throw new SaxException("PGPData - Elemente werden nicht unterstützt");
            }
            else if (localName.Equals("SPKIData") && uri.Equals(DsXmlns))
            {
                throw new SaxException("SPKI-Data - Elemente werden nicht unterstützt");
            }
            else if (localName.Equals("MgmtData") && uri.Equals(DsXmlns))
            {
                _currentElement = "";
            }
            else if (localName.Equals("EncryptedKey") && uri.Equals(XencXmlns))
            {
                XmlReader.ContentHandler = new EncryptedKeyBuilder(XmlReader, this, attributes);
            }
            //@todo wirklich nicht unterstützen?
            else if (localName.Equals("AgreementMethod") && uri.Equals(XencXmlns))
            {
                throw new SaxException("AgreementMethod- Elemente werden nicht unterstützt");
            }
            else
            {
                throw new SaxParseException("Nicht vorgesehenes Element: " + localName, null);
            }
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            Log.Trace("Start-Element :" + localName);
            if (localName.Equals("KeyInfo") && uri.Equals(DsXmlns))
            {
                if (ParentHandler is EncryptedDataBuilder)
                {
                    ((EncryptedDataBuilder)ParentHandler).EncryptedData.KeyInfo = KeyInfo;
                }
                else if (ParentHandler is EncryptedKeyBuilder)
                {
                    ((EncryptedKeyBuilder)ParentHandler).EncKey.KeyInfo = KeyInfo;
                }
                XmlReader.ContentHandler = ParentHandler;
            }
            else if (localName.Equals("KeyName") && uri.Equals(DsXmlns))
            {
                KeyInfo.KeyName = _currentElement;
            }
            else if (localName.Equals("KeyValue") && uri.Equals(DsXmlns))
            {
                throw new SaxException("KeyValue - Elemente werden nicht unterstützt");
            }
            else if (localName.Equals("RetrievalMethod"))
            {
            }
            else if (localName.Equals("X509Data") && uri.Equals(DsXmlns))
            {
            }
            else if (localName.Equals("PGPData") && uri.Equals(DsXmlns))
            {
                throw new SaxException("PGPData - Elemente werden nicht unterstützt");
            }
            else if (localName.Equals("SPKIData") && uri.Equals(DsXmlns))
            {
                throw new SaxException("SPKI-Data - Elemente werden nicht unterstützt");
            }
            else if (localName.Equals("MgmtData") && uri.Equals(DsXmlns))
            {
                KeyInfo.MgmtData = _currentElement;
            }
            else if (localName.Equals("EncryptedData") && uri.Equals(XencXmlns))
            {
            }
            //@todo wirklich nicht unterstützen?
            else if (localName.Equals("AgreementMethod") && uri.Equals(XencXmlns))
            {
                throw new SaxException("AgreementMethod- Elemente werden nicht unterstützt");
            }
            else
            {
                throw new SaxParseException("Nicht vorgesehenes Element: " + localName, null);
            }

            _currentElement = null;
        }


        public override void Characters(char[] ch, int start, int length)
        {
            Log.Trace("Character: " + new string(ch, start, length));
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