using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><H4>ResponseToMediateDelivery-Parser</H4>
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
    public class ResponseToMediateDeliveryBuilder
        : OsciMessageBuilder
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(ResponseToForwardDeliveryBuilder));

        /// <summary> Objekt ProcessCardBundle für ProcessCard Information
        /// </summary>
        private ProcessCardBundleBuilder _requestProcessCardBuilder;
        private ProcessCardBundleBuilder _replyProcessCardBuilder;

        /// <summary>  Objekt Feedback für Feedback Information
        /// </summary>
        private FeedbackBuilder _feedbackBuilder;

        /// <summary> Constructor for the ResponseToForwardDeliveryBuilder object
        /// </summary>
        /// <param name="envelopeBuilder"> Description of Parameter
        /// </param>
        public ResponseToMediateDeliveryBuilder(OsciEnvelopeBuilder envelopeBuilder)
            : base(envelopeBuilder)
        {
            Msg = new ResponseToMediateDelivery(envelopeBuilder.DialogHandler);
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
            _log.Debug("Start Element RspFwdDel: " + localName);

            if (localName.Equals("Header") && uri.Equals(SoapXmlns))
            {
                InsideBody = false;
                InsideHeader = true;
            }
            else if (localName.Equals("Body") && uri.Equals(SoapXmlns))
            {
                InsideBody = true;
                InsideHeader = false;
            }
            else if (InsideBody)
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
                else if (localName.Equals("responseToMediateDelivery") && uri.Equals(OsciXmlns))
                {
                    SignatureRelevantElements.AddElement(localName, uri, attributes);
                }
                else if (localName.Equals("RequestProcessCardBundle") && uri.Equals(OsciXmlns))
                {
                    int[] check = { 1, -1, -1, -1 };
                    _requestProcessCardBuilder = new ProcessCardBundleBuilder("RequestProcessCardBundle", EnvelopeBuilder.XmlReader, this, attributes, check);
                    EnvelopeBuilder.XmlReader.ContentHandler = _requestProcessCardBuilder;
                }
                else if (localName.Equals("ReplyProcessCardBundle") && uri.Equals(OsciXmlns))
                {
                    int[] check = { 1, -1, -1, -1 };
                    _replyProcessCardBuilder = new ProcessCardBundleBuilder("ReplyProcessCardBundle", EnvelopeBuilder.XmlReader, this, attributes, check);
                    EnvelopeBuilder.XmlReader.ContentHandler = _replyProcessCardBuilder;
                }
                else if (localName.Equals("Feedback") && uri.Equals(OsciXmlns))
                {
                    _feedbackBuilder = new FeedbackBuilder(this, false);
                    EnvelopeBuilder.XmlReader.ContentHandler = _feedbackBuilder;
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
                    _log.Debug("Accept Delivery Element: " + localName);
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
                throw new SaxException("Nicht vorgesehens Element! Element Name:" + localName);
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

            if (localName.Equals("Header") && uri.Equals(SoapXmlns))
            {
                ResponseToMediateDelivery rtmd = (ResponseToMediateDelivery)Msg;
                if (_requestProcessCardBuilder != null)
                {
                    rtmd.ProcessCardBundleRequest = _requestProcessCardBuilder.ProcessCardBundleObject;
                    ((OsciMessage)rtmd).MessageId = _requestProcessCardBuilder.ProcessCardBundleObject.MessageId;
                    if (_replyProcessCardBuilder != null)
                    {
                        rtmd.ProcessCardBundleReply = _replyProcessCardBuilder.ProcessCardBundleObject;
                    }
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