using System;
using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;

namespace Osci.Signature
{
    /// <exclude/>
    /// <summary> Builder, der ein Element xdsig:X509Data bearbeitet.
    /// 
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: PPI Financial Systems GmbH</p> 
    /// </summary>
    internal class X509DataBuilder
        : DefaultHandler
    {
        protected static readonly string DsXmlns = Namespace.XmlDSig;
        protected static readonly string XencXmlns = Namespace.XmlEnc;

        /// <summary> Ruft den Inhalt des inneren Elements xdsig:X509Certificate ab.
        /// </summary>
        /// <value> StringBuffer.
        /// </value>
        public virtual System.Text.StringBuilder X509DataBuffer
        {
            get
            {
                return _buffer;
            }
        }

        internal DefaultHandler ParentHandler;
        private string _currentElement;
        private readonly X509Data _x509Data;
        internal XmlReader XmlReader;
        protected static Log Log = LogFactory.GetLog(typeof(X509DataBuilder));
        private readonly System.Text.StringBuilder _buffer;

        /// <summary> Konstruktor.
        /// </summary>
        /// <param name="parentHandler">DefaultCursorHandler, der diesen Builder erzeugt hat.
        /// </param>
        /// <param name="xmlReader">Aktueller CursorXMLReader.
        /// </param>
        /// <param name="attributes">Attribute des Elements xdsig:X509Data.
        /// </param>
        public X509DataBuilder(XmlReader xmlReader, DefaultHandler parentHandler, Attributes attributes)
        {
            _x509Data = new X509Data();
            _buffer = new System.Text.StringBuilder();
            XmlReader = xmlReader;
            ParentHandler = parentHandler;
            if (attributes == null)
            {
                throw new ArgumentException("Dem Konstruktor der Klasse X509DataBuilder wird null übergeben !");
            }
        }

        /// <summary> 
        /// <element name="X509IssuerSerial" type="ds:X509IssuerSerialType"/>
        /// <element name="X509SKI" type="base64Binary"/>
        /// <element name="X509SubjectName" type="string"/>
        /// <element name="X509Certificate" type="base64Binary"/>
        /// <element name="X509CRL" type="base64Binary"/>
        /// <any namespace="##other" processContents="lax"/>
        /// </summary>
        /// <param name="uri">
        /// </param>
        /// <param name="localName">
        /// </param>
        /// <param name="localName">
        /// </param>
        /// <param name="attributes">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            Log.Trace("Start-Element: " + localName + "localName: " + localName);
            if (localName.Equals("X509Certificate") && uri.Equals(DsXmlns))
            {
                _currentElement = "";
            }
            else if (localName.Equals("X509IssuerSerial") && uri.Equals(DsXmlns))
            {
                throw new SaxException("X509IssuerSerial Element wird nicht unterstüzt");
            }
            else if (localName.Equals("X509SKI") && uri.Equals(DsXmlns))
            {
                _currentElement = "";
            }
            else if (localName.Equals("X509SubjectName") && uri.Equals(DsXmlns))
            {
                _currentElement = "";
            }
            else if (localName.Equals("X509CRL") && uri.Equals(DsXmlns))
            {
                _currentElement = "";
            }
            else
            {
                throw new SaxParseException("Nicht vorgesehenes Element: " + localName, null);
            }
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            Log.Trace("End-Element: " + localName);
            if (localName.Equals("X509Data") && uri.Equals(DsXmlns))
            {
                if (ParentHandler is KeyInfoBuilder)
                {
                    ((KeyInfoBuilder)ParentHandler).KeyInfo.x509Data = _x509Data;
                    Log.Trace("Parent ist KeyInfo");
                }
                XmlReader.ContentHandler = ParentHandler;
            }
            else if (localName.Equals("X509Certificate") && uri.Equals(DsXmlns))
            {
                _x509Data.SetX509Certificate(Tools.CreateCertificateFromBase64String(_currentElement));
            }
            else if (localName.Equals("X509IssuerSerial") && uri.Equals(DsXmlns))
            {
                throw new SaxException("X509IssuerSerial Element wird nicht unterstüzt");
            }
            else if (localName.Equals("X509SKI") && uri.Equals(DsXmlns))
            {
                _x509Data.X509Ski = _currentElement;
            }
            else if (localName.Equals("X509SubjectName") && uri.Equals(DsXmlns))
            {
                _x509Data.X509SubjectName = _currentElement;
            }
            else if (localName.Equals("X509CRL") && uri.Equals(DsXmlns))
            {
                _x509Data.X509Crl = _currentElement;
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
                        //if ((ch[start + i] == '\r') || (ch[start + i] == '\n') || (ch[start + i] == ' '))
                        //				i++;
                        //			else {
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