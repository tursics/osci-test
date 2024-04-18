using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.SoapHeader;
using System;

namespace Osci.Messagetypes
{
    /// <summary><H4>ResponseToPartialFetchDelivery-Parser</H4>
    ///
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    ///
    /// <p>Author: R. Lindemann, A. Mergenthal</p>
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class ResponseToPartialFetchDeliveryBuilder
        : OsciMessageBuilder
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(ResponseToPartialFetchDeliveryBuilder));

        private bool? _insideResponseToPartialFetchDelivery = null;

        private bool? _insideFetchDelivery = null;

        private ChunkInformationBuilder chunkInformationBuilder = null;

        private ResponseToPartialFetchDelivery rspToPartialFetchDel = null;
        
        /// <summary> Objekt Feedback für Feedback Information
        /// </summary>
        private FeedbackBuilder _feedbackBuilder;

        /// <summary> Constructor for the ResponseToForwardDeliveryBuilder object
        /// </summary>
        /// <param name="envelopeBuilder">
        /// </param>
        public ResponseToPartialFetchDeliveryBuilder(OsciEnvelopeBuilder envelopeBuilder)
            : base(envelopeBuilder)
        {
            rspToPartialFetchDel = new ResponseToPartialFetchDelivery(envelopeBuilder.DialogHandler);
            Msg = rspToPartialFetchDel;
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
            _log.Debug("Start Element RspParFetDel: " + qName);
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
                else if (localName.Equals("responseToPartialFetchDelivery") && uri.Equals(Osci2017Xmlns))
                {
                    SignatureRelevantElements.AddElement(localName, uri, attributes);

                    if (_insideResponseToPartialFetchDelivery.HasValue)
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                    }
                    _insideResponseToPartialFetchDelivery = true;
                }
                else if (_insideResponseToPartialFetchDelivery.GetValueOrDefault())
                {
                    if (localName.Equals("ChunkInformation") && uri.Equals(Osci2017Xmlns))
                    {
                        chunkInformationBuilder = new ChunkInformationBuilder(EnvelopeBuilder.XmlReader, this, CheckInstance.ResponsePartialFetchDelivery);
                        EnvelopeBuilder.XmlReader.ContentHandler = chunkInformationBuilder;
                        chunkInformationBuilder.StartElement(uri, localName, qName, attributes);
                    }
                    else if (localName.Equals("Feedback") && uri.Equals(OsciXmlns))
                    {
                        _feedbackBuilder = new FeedbackBuilder(this, false);
                        EnvelopeBuilder.XmlReader.ContentHandler = _feedbackBuilder;
                    }
                    else if (localName.Equals("fetchDelivery") && uri.Equals(OsciXmlns))
                    {
                        if (_insideFetchDelivery != null)
                        {
                            throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                        }
                        _insideFetchDelivery = true;
                    }
                    else if (localName.Equals("MessageId") && uri.Equals(OsciXmlns))
                    {
                        CurrentElement = new System.Text.StringBuilder();
                    }
                    else if (_insideResponseToPartialFetchDelivery != null && _insideResponseToPartialFetchDelivery.Value)
                    {
                        if (localName.Equals("MessageId") && uri.Equals(OsciXmlns))
                        {
                            ((ResponseToPartialFetchDelivery)Msg).SelectionMode = OsciMessage.SelectByMessageId;
                            CurrentElement = new System.Text.StringBuilder();
                        }
                        else if (localName.Equals("ReceptionOfDelivery") && uri.Equals(OsciXmlns))
                        {
                            ((ResponseToPartialFetchDelivery)Msg).SelectionMode = OsciMessage.SelectByDateOfReception;
                            CurrentElement = new System.Text.StringBuilder();
                        }
                        else if (localName.Equals("RecentModification") && uri.Equals(OsciXmlns))
                        {
                            ((ResponseToPartialFetchDelivery)Msg).SelectionMode = OsciMessage.SelectByRecentModification;
                            CurrentElement = new System.Text.StringBuilder();
                        }
                    }
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
                else if (!(localName.Equals("osci:SelectionRule") && uri.Equals(OsciXmlns)))
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

            if (localName.Equals("MessageId") && uri.Equals(OsciXmlns) && _insideResponseToPartialFetchDelivery.Value && !_insideFetchDelivery.Value)
            {
                rspToPartialFetchDel.MessageId = Base64.Decode(CurrentElement.ToString()).AsString();
            }
            else if (localName.Equals("MessageId") && uri.Equals(OsciXmlns) && _insideFetchDelivery.Value)
            {
                ((ResponseToPartialFetchDelivery)Msg).SelectionRule = Base64.Decode(CurrentElement.ToString()).AsString();
            }
            else if (((localName.Equals("ReceptionOfDelivery") && uri.Equals(OsciXmlns)) || (localName.Equals("RecentModification") && uri.Equals(OsciXmlns))) && _insideFetchDelivery.Value)
            {
                ((ResponseToPartialFetchDelivery)Msg).SelectionRule = CurrentElement.ToString();
            }
            else if (localName.Equals("ChunkInformation") && uri.Equals(Osci2017Xmlns))
            {
                ((ResponseToPartialFetchDelivery)Msg).ChunkInformation = chunkInformationBuilder.GetChunkInformationObject();
            }
            else if (localName.Equals("Header") && uri.Equals(SoapXmlns))
            {
                InsideHeader = false;
            }
            else if (localName.Equals("responseToPartialFetchDelivery") && uri.Equals(Osci2017Xmlns))
            {
                _insideResponseToPartialFetchDelivery = false;
            }
            else if (localName.Equals("fetchDelivery") && uri.Equals(OsciXmlns))
            {
                _insideFetchDelivery = false;
            }
            else
            {
                base.EndElement(uri, localName, qName);
            }

            CurrentElement = null;
        }
    }
}