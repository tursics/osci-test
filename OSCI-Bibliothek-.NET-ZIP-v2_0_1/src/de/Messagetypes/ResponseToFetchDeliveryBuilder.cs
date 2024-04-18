using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><H4>ResponseToFetchDelivery-Parser</H4>
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
    public class ResponseToFetchDeliveryBuilder
        : OsciMessageBuilder
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(ResponseToForwardDeliveryBuilder));

        /// <summary> Objekt ProcessCardBundle für ProcessCard Information
        /// </summary>
        private ProcessCardBundleBuilder _processCardBuilder;

        /// <summary> Objekt Feedback für Feedback Information
        /// </summary>
        private FeedbackBuilder _feedbackBuilder;

        /// <summary> Constructor for the ResponseToForwardDeliveryBuilder object
        /// </summary>
        /// <param name="envelopeBuilder">
        /// </param>
        public ResponseToFetchDeliveryBuilder(OsciEnvelopeBuilder envelopeBuilder)
            : base(envelopeBuilder)
        {
            Msg = new ResponseToFetchDelivery(envelopeBuilder.DialogHandler);
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
            _log.Debug("Start Element RspFwdDel: " + qName);
            if (InsideBody)
            {
                if (localName.Equals("ContentPackage") && uri.Equals(OsciXmlns))
                {
                    SetContentPackageHandler(localName);
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
                else if (localName.Equals("responseToFetchDelivery") && uri.Equals(OsciXmlns))
                {
                    SignatureRelevantElements.AddElement(localName, uri, attributes);
                }
                else if (localName.Equals("ProcessCardBundle") && uri.Equals(OsciXmlns))
                {
                    int[] check = { 1, -1, -1, -1 };
                    _processCardBuilder = new ProcessCardBundleBuilder("ProcessCardBundle", EnvelopeBuilder.XmlReader, this, attributes, check);
                    EnvelopeBuilder.XmlReader.ContentHandler = _processCardBuilder;
                }
                else if (localName.Equals("Feedback") && uri.Equals(OsciXmlns))
                {
                    _feedbackBuilder = new FeedbackBuilder(this);
                    EnvelopeBuilder.XmlReader.ContentHandler = _feedbackBuilder;
                }
                else if (localName.Equals("MessageId") && uri.Equals(OsciXmlns))
                {
                    ((ResponseToFetchDelivery)Msg).SelectionMode = OsciMessage.SelectByMessageId;
                    CurrentElement = new System.Text.StringBuilder();
                }
                else if (localName.Equals("ReceptionOfDelivery") && uri.Equals(OsciXmlns))
                {
                    ((ResponseToFetchDelivery)Msg).SelectionMode = OsciMessage.SelectByDateOfReception;
                    CurrentElement = new System.Text.StringBuilder();
                }
                else if (localName.Equals("RecentModification") && uri.Equals(OsciXmlns))
                {
                    ((ResponseToFetchDelivery)Msg).SelectionMode = OsciMessage.SelectByRecentModification;
                    CurrentElement = new System.Text.StringBuilder();
                }
                else if (localName.Equals("IntermediaryCertificates") && uri.Equals(OsciXmlns))
                {
                    int[] check = { 0, -1 };
                    EnvelopeBuilder.XmlReader.ContentHandler = new IntermediaryCertificatesHBuilder(this, attributes, check);
                }
                else if (localName.ToUpper().Equals("NonIntermediaryCertificates".ToUpper()) && uri.Equals(OsciXmlns))
                {
                    int[] check = { -1, -1, -1, -1, -1, -1, 0 };
                    EnvelopeBuilder.XmlReader.ContentHandler = new NonIntermediaryCertificatesHBuilder(this, attributes, check);
                    _log.Debug("Accept Delivery Element: " + qName);
                }
                else if (localName.Equals("FeatureDescription") && uri.Equals(Osci2017Xmlns))
                {
                    FeatureDescriptionHBuilder featureBuilder = new FeatureDescriptionHBuilder(this, attributes);
                    EnvelopeBuilder.XmlReader.ContentHandler = featureBuilder;
                    featureBuilder.StartElement(uri, localName, qName, attributes);
                }
                else if (!(localName.Equals("fetchDelivery") && uri.Equals(OsciXmlns)) && !(qName.Equals("osci:SelectionRule") && uri.Equals(OsciXmlns)))
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
            _log.Debug("End-Element: " + qName);

            if (localName.Equals("MessageId") && uri.Equals(OsciXmlns))
            {
                ((ResponseToFetchDelivery)Msg).SelectionRule = Base64.Decode(CurrentElement.ToString()).AsString();
            }
            else if ((localName.Equals("ReceptionOfDelivery") && uri.Equals(OsciXmlns)) || (localName.Equals("RecentModification") && uri.Equals(OsciXmlns)))
            {
                ((ResponseToFetchDelivery)Msg).SelectionRule = CurrentElement.ToString();
            }
            else if (localName.Equals("Header") && uri.Equals(SoapXmlns))
            {
                ResponseToFetchDelivery rtmd = (ResponseToFetchDelivery)Msg;

                if (_processCardBuilder != null)
                {
                    rtmd.ProcessCardBundle = _processCardBuilder.ProcessCardBundleObject;
                    ((OsciMessage)rtmd).MessageId = _processCardBuilder.ProcessCardBundleObject.MessageId;
                }

                InsideHeader = false;
            }
            else
            {
                base.EndElement(uri, localName, qName);
            }

            CurrentElement = null;
        }
    }
}