using System;
using System.Collections.Generic;
using System.Text;
using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;

namespace Osci.Messagetypes
{
    /// <summary> Diese Klasse ist die Superklasse der Nachrichtenparser. Wird vom Anwender nicht benötigt.
    /// 
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N.Büngener</p>
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class OsciMessageBuilder
        : DefaultHandler
    {
        internal UniqueElementTracker SignatureRelevantElements
        {
            get;
        }

        /// <summary> Gets the OSCIMessage attribute of the OSCIMessageBuilder object
        /// </summary>
        /// <value> The OSCIMessage value
        /// </value>
        public OsciMessage OsciMessage
        {
            get
            {
                return Msg;
            }
        }
        public Canonizer CanStream
        {
            get
            {
                return EnvelopeBuilder.HashNCanStream;
            }
        }

        public OsciEnvelopeBuilder EnvelopeBuilder
        {
            get;
        }

        // TODO: remove, use Namespace.x instead
        public static readonly string SoapXmlns = Namespace.SoapEnvelope;
        public static readonly string OsciXmlns = Namespace.Osci;
        public static readonly string Osci2017Xmlns = Namespace.Osci2017;
        public static readonly string DsXmlns = Namespace.XmlDSig;
        public static readonly string XencXmlns = Namespace.XmlEnc;
        public static readonly string XsiXmlns = Namespace.XsiSchema;

        private static readonly Log _log = LogFactory.GetLog(typeof(OsciMessageBuilder));

        protected StringBuilder CurrentElement;
        protected internal OsciMessage Msg = null;
        protected bool ContentPackageAlreadySet;
        protected bool InsideHeader;
        protected bool InsideBody;

        protected List<string> CustomSoapHeader = new List<string>();

        /// <summary> Constructor for the OSCIMessageBuilder object
        /// </summary>
        public OsciMessageBuilder(OsciEnvelopeBuilder envelopeBuilder)
        {
            EnvelopeBuilder = envelopeBuilder;
            SignatureRelevantElements = new UniqueElementTracker();
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            if (localName.Equals("Header") && uri.Equals(SoapXmlns))
            {
                InsideBody = false;
                InsideHeader = true;
            }
            else if (localName.Equals("Body") && uri.Equals(SoapXmlns))
            {
                InsideBody = true;
                InsideHeader = false;
                SignatureRelevantElements.AddElement(localName, uri, attributes);
            }
            else
            {
                throw new SaxException("Nicht vorgesehens Element! Element Name:" + localName);
            }
        }

        protected void StartCustomSoapHeader(string uri, string localName, string qName, Attributes attributes)
        {
            try
            {
                CanParser cp = new CanParser(CustomSoapHeader, EnvelopeBuilder.XmlReader, EnvelopeBuilder.XmlReader.ContentHandler, qName);
                EnvelopeBuilder.XmlReader.ContentHandler = cp;
                cp.StartDocument();
                cp.StartElement(uri, localName, qName, attributes);
                SignatureRelevantElements.AddElement(localName, uri, attributes);
            }
            catch (SaxException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("sax_exception_customheader"), ex);
            }
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            if (localName.Equals("Header") && uri.Equals(SoapXmlns))
            {
                InsideHeader = false;
            }
            else if (localName.Equals("Body") && uri.Equals(SoapXmlns))
            {
                InsideBody = false;
            }
            else if (localName.Equals("Envelope") && uri.Equals(SoapXmlns))
            {
                EnvelopeBuilder.XmlReader.ContentHandler = EnvelopeBuilder;
                for (int i = 0; i < CustomSoapHeader.Count; i++)
                {
                    Msg.AddCustomHeader(CustomSoapHeader[i]);
                }
            }
            CurrentElement = null;
        }

        public override void Characters(char[] ch, int start, int length)
        {
            _log.Debug("Character: " + new string(ch, start, length));
            if (CurrentElement == null)
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
                CurrentElement.Append(ch, start, length);
            }
        }

        public override void StartPrefixMapping(string prefix, string uri)
        {
            if (uri.Equals(OsciMessageBuilder.Osci2017Xmlns))
            {
                Msg.Osci2017NsPrefix = prefix;
            }
        }

        public void SetContentPackageHandler(string localName)
        {
            if (ContentPackageAlreadySet)
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("unsupported_entry") + ": " + localName);
            }
            ContentPackageAlreadySet = true;
            EnvelopeBuilder.XmlReader.ContentHandler = new ContentPackageBuilder(this);
        }
    }
}