using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><H4>ResponseToPartialStoreDelivery-Parser</H4>
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
    public class ResponseToPartialStoreDeliveryBuilder
        : OsciMessageBuilder
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(ResponseToPartialStoreDeliveryBuilder));

        private bool insideRspToPartialStoreDelivery = false;

        /// <summary>  Objekt ProcessCardBundle für ProcessCard Information
        /// </summary>
        private ProcessCardBundleBuilder _processCardBuilder;

        /// <summary>  Objekt Feedback für Feedback Information
        /// </summary>
        private FeedbackBuilder _feedbackBuilder;

        ChunkInformationBuilder chunkInformationBuilder = null;

        /// <summary>  Constructor for the ResponseToForwardDeliveryBuilder object
        /// </summary>
        /// <param name="xmlReader">Description of Parameter
        /// </param>
        /// <param name="envelopeBuilder"> Description of Parameter
        /// </param>
        /// <param name="dh">Description of Parameter
        /// </param>
        public ResponseToPartialStoreDeliveryBuilder(OsciEnvelopeBuilder envelopeBuilder)
            : base(envelopeBuilder)
        {
            Msg = new ResponseToPartialStoreDelivery(envelopeBuilder.DialogHandler, true);
        }

        /// <summary>  Description of the Method
        /// </summary>
        /// <param name="uri">Description of Parameter
        /// </param>
        /// <param name="localName">Description of Parameter
        /// </param>
        /// <param name="qName">Description of Parameter
        /// </param>
        /// <param name="attributes">Description of Parameter
        /// </param>
        /// <exception cref="SaxException">Description of Exception
        /// </exception>
        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Debug("Start Element RspStrDel: " + qName + InsideHeader + localName + uri);

            if (InsideHeader)
            {
                // ### Auswerten des ControlBlock Headers###
                if (localName.Equals("ControlBlock") && uri.Equals(OsciXmlns))
                {
                    EnvelopeBuilder.XmlReader.ContentHandler = new ControlBlockHBuilder(this, attributes, new[] { 1, -1, 1, 1 });
                }
                // ### Auswerten des ClientSignature Headers###
                else if (localName.Equals("SupplierSignature") && uri.Equals(OsciXmlns))
                {
                    EnvelopeBuilder.XmlReader.ContentHandler = new OsciSignatureBuilder(EnvelopeBuilder.XmlReader, this, attributes, false);
                }
                else if (localName.Equals("responseToPartialStoreDelivery") && uri.Equals(Osci2017Xmlns))
                {
                    insideRspToPartialStoreDelivery = true;
                }
                else if (localName.Equals("ProcessCardBundle") && uri.Equals(OsciXmlns))
                {
                    int[] check = { 1, -1, -1, -1 };
                    _processCardBuilder = new ProcessCardBundleBuilder("ProcessCardBundle", EnvelopeBuilder.XmlReader, this, attributes, check);
                    EnvelopeBuilder.XmlReader.ContentHandler = _processCardBuilder;
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
                else if (localName.Equals("FeatureDescription") && uri.Equals(Osci2017Xmlns))
                {
                    FeatureDescriptionHBuilder featureBuilder = new FeatureDescriptionHBuilder(this, attributes);
                    EnvelopeBuilder.XmlReader.ContentHandler = featureBuilder;
                    featureBuilder.StartElement(uri, localName, qName, attributes);
                }
                else if (insideRspToPartialStoreDelivery)
                {
                    if (localName.Equals("ChunkInformation") && uri.Equals(Osci2017Xmlns))
                    {
                        chunkInformationBuilder = new ChunkInformationBuilder(EnvelopeBuilder.XmlReader, this, CheckInstance.ResponsePartialStoreDelivery);
                        EnvelopeBuilder.XmlReader.ContentHandler = chunkInformationBuilder;
                        chunkInformationBuilder.StartElement(uri, localName, qName, attributes);
                    }
                    else if (localName.Equals("InsideFeedback") && uri.Equals(Osci2017Xmlns))
                    {
                        _feedbackBuilder = new FeedbackBuilder(this, true);
                        EnvelopeBuilder.XmlReader.ContentHandler = _feedbackBuilder;
                    }
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

        /// <summary>  Description of the Method
        /// </summary>
        /// <param name="uri">Description of Parameter
        /// </param>
        /// <param name="localName">Description of Parameter
        /// </param>
        /// <param name="qName">Description of Parameter
        /// </param>
        /// <exception cref="SaxException">Description of Exception
        /// </exception>
        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Debug("End-Element: " + localName);
            if (localName.Equals("Header") && uri.Equals(SoapXmlns))
            {
                if (_processCardBuilder != null)
                {
                    ((ResponseToPartialStoreDelivery)Msg).ProcessCardBundle = _processCardBuilder.ProcessCardBundleObject;
                }
                InsideHeader = false;
            }
            else if (localName.Equals("responseToPartialStoreDelivery") && uri.Equals(Osci2017Xmlns))
            {
                insideRspToPartialStoreDelivery = true;
            }
            else if (localName.Equals("ChunkInformation") && uri.Equals(Osci2017Xmlns))
            {
                ((ResponseToPartialStoreDelivery)Msg).chunkInformation = chunkInformationBuilder.GetChunkInformationObject();
            }
            else if (localName.Equals("InsideFeedback") && uri.Equals(Osci2017Xmlns))
            {
                ((ResponseToPartialStoreDelivery)Msg).InsideFeedBack = _feedbackBuilder.GetFeedback();
            }
            else
            {
                base.EndElement(uri, localName, qName);
            }
            CurrentElement = null;
        }
    }
}