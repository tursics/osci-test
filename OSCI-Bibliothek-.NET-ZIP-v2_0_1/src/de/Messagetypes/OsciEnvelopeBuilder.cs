using System;
using System.Collections.Generic;
using System.Text;
using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;

namespace Osci.Messagetypes
{
    /// <summary><H4>Nachrichtenparser</H4> 
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
    public class OsciEnvelopeBuilder
        : DefaultHandler
    {
        public XmlReader XmlReader
        {
            get; private set;
        }

        #region XSD Namespaces

        internal static string XsdInitDialog
        {
            get
            {
                return _messageTypeBuilder("soapInitDialog");
            }
        }

        internal static string XsdRspInitDialog
        {
            get
            {
                return _messageTypeBuilder("soapResponseToInitDialog");
            }
        }

        internal static string XsdEndDialog
        {
            get
            {
                return _messageTypeBuilder("soapExitDialog");
            }
        }

        internal static string XsdRspEndDialog
        {
            get
            {
                return _messageTypeBuilder("soapResponseToExitDialog");
            }
        }

        internal static string XsdGetMsgId
        {
            get
            {
                return _messageTypeBuilder("soapGetMessageId");
            }
        }

        internal static string XsdRspGetMsgId
        {
            get
            {
                return _messageTypeBuilder("soapResponseToGetMessageId");
            }
        }

        internal static string XsdStoreDelivery
        {
            get
            {
                return _messageTypeBuilder("soapStoreDelivery");
            }
        }

        internal static string XsdRspStoreDelivery
        {
            get
            {
                return _messageTypeBuilder("soapResponseToStoreDelivery");
            }
        }

        internal static string XsdRspPartialStoreDelivery
        {
            get
            {
                return _messageTypeBuilder("soapResponseToPartialStoreDelivery");
            }
        }

        internal static string XsdRspPartialFetchDelivery
        {
            get
            {
                return _messageTypeBuilder("soapResponseToPartialFetchDelivery");
            }
        }


        internal static string XsdFetchDelivery
        {
            get
            {
                return _messageTypeBuilder("soapFetchDelivery");
            }
        }

        internal static string XsdRspFetchDelivery
        {
            get
            {
                return _messageTypeBuilder("soapResponseToFetchDelivery");
            }
        }

        internal static string XsdFetchProcessCard
        {
            get
            {
                return _messageTypeBuilder("soapFetchProcessCard");
            }
        }

        internal static string XsdRspFetchProcessCard
        {
            get
            {
                return _messageTypeBuilder("soapResponseToFetchProcessCard");
            }
        }

        internal static string XsdForwardDelivery
        {
            get
            {
                return _messageTypeBuilder("soapForwardDelivery");
            }
        }

        internal static string XsdRspForwardDelivery
        {
            get
            {
                return _messageTypeBuilder("soapResponseToForwardDelivery");
            }
        }

        internal static string XsdAcceptDelivery
        {
            get
            {
                return _messageTypeBuilder("soapAcceptDelivery");
            }
        }

        internal static string XsdRspAcceptDelivery
        {
            get
            {
                return _messageTypeBuilder("soapResponseToAcceptDelivery");
            }
        }

        internal static string XsdMediateDelivery
        {
            get
            {
                return _messageTypeBuilder("soapMediateDelivery");
            }
        }

        internal static string XsdRspMediateDelivery
        {
            get
            {
                return _messageTypeBuilder("soapResponseToMediateDelivery");
            }
        }

        internal static string XsdProcessDelivery
        {
            get
            {
                return _messageTypeBuilder("soapProcessDelivery");
            }
        }

        internal static string XsdRspProcessDelivery
        {
            get
            {
                return _messageTypeBuilder("soapResponseToProcessDelivery");
            }
        }

        internal static string XsdMessageFault
        {
            get
            {
                return _messageTypeBuilder("soapMessageFault");
            }
        }

        internal static string XsdEncryptedData
        {
            get
            {
                return _messageTypeBuilder("soapMessageEncrypted");
            }
        }

        private const string _xsdEncSig = "http://www.w3.org/2000/09/xmldsig# oscisig.xsd http://www.w3.org/2001/04/xmlenc# oscienc.xsd";
        private static readonly Func<string, string> _messageTypeBuilder = _ => string.Format(Namespace.SoapEnvelope + " {0}.xsd " + _xsdEncSig, _);

        #endregion

        internal OsciMessageBuilder MessageBuilder
        {
            get; set;
        }

        internal string SoapNsPrefix
        {
            get; private set;
        }

        internal string OsciNsPrefix
        {
            get; private set;
        }
        internal string Osci2017NsPrefix
        {
            get; private set;
        }

        internal string Osci128NsPrefix
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

        internal StringBuilder Namespaces
        {
            get; private set;
        }

        protected OsciMessage Msg = null;
        protected internal Canonizer HashNCanStream = null;
        protected internal DialogHandler DialogHandler;

        private static readonly Log _log = LogFactory.GetLog(typeof(OsciEnvelopeBuilder));


        /// <summary> Constructor for the OSCIMessageBuilder object
        /// </summary>
        /// <param name="xmlReader">
        /// </param>
        /// <param name="dialogHandler">
        /// </param>
        public OsciEnvelopeBuilder(XmlReader xmlReader, DialogHandler dialogHandler)
        {
            XmlReader = xmlReader;
            DialogHandler = dialogHandler;
            Namespaces = new StringBuilder();
        }

        public override void StartPrefixMapping(string prefix, string uri)
        {
            if (uri.Equals(Namespace.SoapEnvelope))
            {
                SoapNsPrefix = prefix;
            }
            else if (uri.Equals(Namespace.Osci))
            {
                OsciNsPrefix = prefix;
            }
            else if (uri.Equals(Namespace.Osci2017))
            {
                Osci2017NsPrefix = prefix;
            }
            else if (uri.Equals(Namespace.Osci128))
            {
                Osci128NsPrefix = prefix;
            }
            else if (uri.Equals(Namespace.XmlDSig))
            {
                DsNsPrefix = prefix;
            }
            else if (uri.Equals(Namespace.XmlEnc))
            {
                XencNsPrefix = prefix;
            }
            Namespaces.Append(" xmlns:" + prefix + "=\"" + uri + "\"");
        }

        /// <summary> Start element.
        /// </summary>
        /// <param name="uri">
        /// </param>
        /// <param name="localName">
        /// </param>
        /// <param name="qName">
        /// </param>
        /// <param name="attributes">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start-Element: " + qName);

            if ((SoapNsPrefix == null) || !localName.Equals("Envelope"))
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("msg_format_error") + ": " + localName);
            }

            string schemaLocation = attributes.GetValue("xsi:schemaLocation");
            _log.Trace("Nachrichtentyp: " + schemaLocation);

            if (schemaLocation == null)
            {
                MessageBuilder = new SoapFaultBuilder(this);
            }
            else if (_messageBuilderMap.ContainsKey(schemaLocation))
            {
                MessageBuilder = _messageBuilderMap[schemaLocation](this);
            }
            else
            {
                _log.Error("Ergebnis Schema Test: " + schemaLocation.Equals(XsdForwardDelivery));
                _log.Error("Falsche OSCI-Nachricht. Nachrichtentyp nicht bekannt! Nachrichtentypt ist:\n" + schemaLocation + "\n" + (XsdRspFetchProcessCard));
                throw new SaxException("Falsche OSCI-Nachricht. Nachrichtentyp nicht bekannt! Nachrichtentyp ist: " + schemaLocation);
            }

            XmlReader.ContentHandler = MessageBuilder;
            MessageBuilder.OsciMessage.SoapNsPrefix = SoapNsPrefix;
            MessageBuilder.OsciMessage.OsciNsPrefix = OsciNsPrefix;
            MessageBuilder.OsciMessage.Osci2017NsPrefix = Osci2017NsPrefix;
            MessageBuilder.OsciMessage.Osci128NsPrefix = Osci128NsPrefix;
            MessageBuilder.OsciMessage.DsNsPrefix = DsNsPrefix;
            MessageBuilder.OsciMessage.XencNsPrefix = XencNsPrefix;
            MessageBuilder.OsciMessage.Ns = Namespaces.ToString();
        }

        private readonly Dictionary<string, Func<OsciEnvelopeBuilder, OsciMessageBuilder>> _messageBuilderMap = new Dictionary<string, Func<OsciEnvelopeBuilder, OsciMessageBuilder>>
        {
            { XsdRspFetchProcessCard, _ => new ResponseToFetchProcessCardBuilder(_) },
            { XsdRspFetchDelivery, _ => new ResponseToFetchDeliveryBuilder(_) },
            { XsdRspStoreDelivery, _ => new ResponseToStoreDeliveryBuilder(_) },
            { XsdRspEndDialog, _ => new ResponseToExitDialogBuilder(_) },
            { XsdRspGetMsgId, _ => new ResponseToGetMessageIdBuilder(_) },
            { XsdRspMediateDelivery, _ => new ResponseToMediateDeliveryBuilder(_) },
            { XsdProcessDelivery, _ => new ProcessDeliveryBuilder(_) },
            { XsdRspInitDialog, _ => new ResponseToInitDialogBuilder(_) },
            { XsdRspForwardDelivery, _ => new ResponseToForwardDeliveryBuilder(_) },
            { XsdAcceptDelivery, _ => new AcceptDeliveryBuilder(_) },
            { XsdEncryptedData, _ => new SoapMessageEncryptedBuilder(_) },
            { XsdRspPartialStoreDelivery, _ => new ResponseToPartialStoreDeliveryBuilder(_) },
            { XsdRspPartialFetchDelivery, _ => new ResponseToPartialFetchDeliveryBuilder(_) }
        };

        public override void EndDocument()
        {
            MessageBuilder.OsciMessage.HashableMsgPart = HashNCanStream.DigestValues;
            MessageBuilder.OsciMessage.SignatureRelevantElements = MessageBuilder.SignatureRelevantElements;

            if (MessageBuilder.OsciMessage.SignatureHeader != null)
            {
                MessageBuilder.OsciMessage.SignatureHeader.SignedInfo = HashNCanStream.SignedInfos[0];
                HashNCanStream.SignedInfos.RemoveAt(0);
            }

            MessageBuilder.OsciMessage.StateOfMessage |= OsciMessage.StateParsed;
        }
    }
}