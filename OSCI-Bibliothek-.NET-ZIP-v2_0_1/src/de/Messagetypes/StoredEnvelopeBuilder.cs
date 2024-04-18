using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;

namespace Osci.Messagetypes
{
    /// <summary><H4>StoredEnvelope-Parser</H4>
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

    internal class StoredEnvelopeBuilder
        : OsciEnvelopeBuilder
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(StoredEnvelopeBuilder));

        public StoredEnvelopeBuilder(XmlReader xmlReader)
            : base(xmlReader, null)
        {
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start-Element: " + qName);

            // SOAP-Namespacedefinition suchen
            if ((SoapNsPrefix == null) || !localName.Equals("Envelope"))
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("msg_format_error") + ": " + localName);
            }
            string schemaLocation = attributes.GetValue("xsi:schemaLocation");

            _log.Trace("Nachrichtentyp: " + schemaLocation);

            if (schemaLocation == null)
            {
                MessageBuilder = new SoapFaultBuilder(this);
                XmlReader.ContentHandler = MessageBuilder;
            }
            else if (schemaLocation.Equals(XsdEncryptedData))
            {
                throw new SaxException(new System.ArgumentException(DialogHandler.ResourceBundle.GetString("encrypted_message")));
            }
            else
            {
                int msgType;

                if (schemaLocation.Equals(XsdStoreDelivery))
                    msgType = OsciMessage.StoreDelivery;
                else if (schemaLocation.Equals(XsdRspStoreDelivery))
                    msgType = OsciMessage.ResponseToStoreDelivery;
                else if (schemaLocation.Equals(XsdFetchDelivery))
                    msgType = OsciMessage.FetchDelivery;
                else if (schemaLocation.Equals(XsdRspFetchDelivery))
                    msgType = OsciMessage.ResponseToFetchDelivery;
                else if (schemaLocation.Equals(XsdForwardDelivery))
                    msgType = OsciMessage.ForwardDelivery;
                else if (schemaLocation.Equals(XsdAcceptDelivery))
                    msgType = OsciMessage.AcceptDelivery;
                else if (schemaLocation.Equals(XsdRspAcceptDelivery))
                    msgType = OsciMessage.ResponseToAcceptDelivery;
                else if (schemaLocation.Equals(XsdRspForwardDelivery))
                    msgType = OsciMessage.ResponseToForwardDelivery;
                else if (schemaLocation.Equals(XsdMediateDelivery))
                    msgType = OsciMessage.MediateDelivery;
                else if (schemaLocation.Equals(XsdProcessDelivery))
                    msgType = OsciMessage.ProcessDelivery;
                else if (schemaLocation.Equals(XsdRspProcessDelivery))
                    msgType = OsciMessage.ResponseToProcessDelivery;
                else if (schemaLocation.Equals(XsdRspMediateDelivery))
                    msgType = OsciMessage.ResponseToMediateDelivery;
                else if (schemaLocation.Equals(XsdFetchProcessCard))
                    msgType = OsciMessage.FetchProcessCard;
                else if (schemaLocation.Equals(XsdRspFetchProcessCard))
                    msgType = OsciMessage.ResponseToFetchProcessCard;
                else
                {
                    _log.Error("Nicht erlaubter Nachrichtentyp ! Nachrichtentypt ist:\n" + schemaLocation + "\n" + XsdRspFetchProcessCard);
                    throw new SaxException(DialogHandler.ResourceBundle.GetString("sax_exception_msgtype"));
                }

                MessageBuilder = new StoredMessageBuilder(this, msgType);
                XmlReader.ContentHandler = MessageBuilder;
            }
            MessageBuilder.OsciMessage.SoapNsPrefix = SoapNsPrefix;
            MessageBuilder.OsciMessage.OsciNsPrefix = OsciNsPrefix;
            MessageBuilder.OsciMessage.Osci2017NsPrefix = Osci2017NsPrefix;
            MessageBuilder.OsciMessage.Osci128NsPrefix = Osci128NsPrefix;
            MessageBuilder.OsciMessage.DsNsPrefix = DsNsPrefix;
            MessageBuilder.OsciMessage.XencNsPrefix = XencNsPrefix;
            MessageBuilder.OsciMessage.Ns = Namespaces.ToString();
            _log.Trace("Namespaces: " + MessageBuilder.OsciMessage.Ns);
        }
    }
}