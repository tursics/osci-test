using System.Collections;
using System.Text;
using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><H4>ResponseToFetchProcessCard-Parser</H4>
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
    public class ResponseToFetchProcessCardBuilder
        : OsciMessageBuilder
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(ResponseToForwardDeliveryBuilder));

        /// <summary>  Objekt ProcessCardBundle für ProcessCard Information
        /// </summary>
        private ProcessCardBundleBuilder _processCardBuilder;

        private readonly ArrayList _processCardBundles;

        /// <summary>  Objekt Feedback für Feedback Information
        /// </summary>
        private FeedbackBuilder _feedbackBuilder;
        private readonly StringBuilder _messageIds = new StringBuilder();
        /// <summary> Constructor for the ResponseToForwardDeliveryBuilder object
        /// </summary>
        /// <param name="envelopeBuilder">
        /// </param>
        public ResponseToFetchProcessCardBuilder(OsciEnvelopeBuilder envelopeBuilder) 
            : base(envelopeBuilder)
        {
            _processCardBundles = new ArrayList();
            Msg = new ResponseToFetchProcessCard(envelopeBuilder.DialogHandler);
        }

        private void SetSelectionAttributes(Attributes att)
        {
            if ("true".Equals(att.GetValue("NoReception")))
            {
                ((ResponseToFetchProcessCard)Msg).SelectNoReceptionOnly = true;
            }

            if ("Addressee".Equals(att.GetValue("Role")))
            {
                ((ResponseToFetchProcessCard)Msg).RoleForSelection = OsciMessage.SelectAddressee;
            }
            else if ("Originator".Equals(att.GetValue("Role")))
            {
                ((ResponseToFetchProcessCard)Msg).RoleForSelection = OsciMessage.SelectOriginator;
            }
        }

        /// <summary>
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
            _log.Debug("Start Element RspFtProc: " + localName);

            if (InsideBody)
            {
                if (localName.Equals("Feedback") && uri.Equals(OsciXmlns))
                {
                    _feedbackBuilder = new FeedbackBuilder(this);
                    EnvelopeBuilder.XmlReader.ContentHandler = _feedbackBuilder;
                }
                else if (localName.Equals("MessageId") && uri.Equals(OsciXmlns))
                {
                    ((ResponseToFetchProcessCard)Msg).SelectionMode = OsciMessage.SelectByMessageId;
                    SetSelectionAttributes(attributes);
                    CurrentElement = new StringBuilder();
                }
                else if (localName.Equals("ReceptionOfDelivery") && uri.Equals(OsciXmlns))
                {
                    ((ResponseToFetchProcessCard)Msg).SelectionMode = OsciMessage.SelectByDateOfReception;
                    SetSelectionAttributes(attributes);
                    CurrentElement = new StringBuilder();
                }
                else if (localName.Equals("RecentModification") && uri.Equals(OsciXmlns))
                {
                    ((ResponseToFetchProcessCard)Msg).SelectionMode = OsciMessage.SelectByRecentModification;
                    CurrentElement = new StringBuilder();
                }
                else if (localName.Equals("Quantity") && uri.Equals(OsciXmlns))
                {
                    ((ResponseToFetchProcessCard)Msg).QuantityLimit = long.Parse(attributes.GetValue("Limit"));
                }
                else if (localName.Equals("responseToFetchProcessCard") && uri.Equals(OsciXmlns))
                {
                    //nothing to do
                }
                else if (localName.Equals("ProcessCardBundle") && uri.Equals(OsciXmlns))
                {
                    if (_processCardBuilder != null)
                    {
                        _processCardBundles.Add(_processCardBuilder.ProcessCardBundleObject);
                    }
                    int[] check = { 1, -1, -1, -1 };
                    _processCardBuilder = new ProcessCardBundleBuilder("ProcessCardBundle", EnvelopeBuilder.XmlReader, this, attributes, check);
                    EnvelopeBuilder.XmlReader.ContentHandler = _processCardBuilder;
                }
            }
            else if (InsideHeader)
            {
                // ### Auswerten des ControlBlock Headers###
                if (localName.Equals("ControlBlock") && uri.Equals(OsciXmlns))
                {
                    EnvelopeBuilder.XmlReader.ContentHandler = new ControlBlockHBuilder(this, attributes, new[] { 1, 1, 1, 1 });
                    // ### Auswerten des ClientSignature Headers###
                }
                else if (localName.Equals("SupplierSignature") && uri.Equals(OsciXmlns))
                {
                    EnvelopeBuilder.XmlReader.ContentHandler = new OsciSignatureBuilder(EnvelopeBuilder.XmlReader, this, attributes, false);
                }
                else if (localName.Equals("IntermediaryCertificates") && uri.Equals(OsciXmlns))
                {
                    int[] check = { 0, -1 };
                    EnvelopeBuilder.XmlReader.ContentHandler = new IntermediaryCertificatesHBuilder(this, attributes, check);
                }
                else if (localName.Equals("FeatureDescription") && uri.Equals(Osci2017Xmlns))
                {
                    FeatureDescriptionHBuilder featureBuilder = new FeatureDescriptionHBuilder(this, attributes);
                    EnvelopeBuilder.XmlReader.ContentHandler = featureBuilder;
                    featureBuilder.StartElement(uri, localName, qName, attributes);
                }
                else
                {
                    StartCustomSoapHeader(uri, localName, qName, attributes);
                }
            }
            else
            {
                base.StartElement(uri, localName, qName, attributes);
            }
        }

        /// <summary> 
        /// </summary>
        /// <param name="uri">
        /// </param>
        /// <param name="localName">
        /// </param>
        /// <param name="qName">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Debug("End-Element: " + localName);

            if (localName.Equals("MessageId") && uri.Equals(OsciXmlns))
            {
                if (_messageIds.Length > 0)
                {
                    _messageIds.Append('&');
                }
                try
                {
                    _messageIds.Append(Base64.Decode(CurrentElement.ToString()).AsString());
                }
                catch (System.IO.IOException ex)
                {
                    throw new SaxException(ex);
                }
            }
            else if ((localName.Equals("ReceptionOfDelivery") && uri.Equals(OsciXmlns)) || localName.Equals("RecentModification") && uri.Equals(OsciXmlns))
            {
                ((ResponseToFetchProcessCard)Msg).SelectionRule = CurrentElement.ToString();
            }
            else if (localName.Equals("SelectionRule") && uri.Equals(OsciXmlns))
            {
                if (((ResponseToFetchProcessCard)Msg).SelectionMode == OsciMessage.SelectByMessageId)
                {
                    ((ResponseToFetchProcessCard)Msg).SelectionRule = _messageIds.ToString();
                }
            }
            else if (localName.Equals("Body") && uri.Equals(SoapXmlns))
            {
                ResponseToFetchProcessCard rtmd = (ResponseToFetchProcessCard)Msg;

                if (_processCardBuilder != null)
                {
                    _processCardBundles.Add(_processCardBuilder.ProcessCardBundleObject);
                }

                rtmd.processCardBundles = (ProcessCardBundle[])_processCardBundles.ToArray(typeof(ProcessCardBundle));
                InsideBody = false;
            }
            else
            {
                base.EndElement(uri, localName, qName);
            }

            CurrentElement = null;
        }
    }
}