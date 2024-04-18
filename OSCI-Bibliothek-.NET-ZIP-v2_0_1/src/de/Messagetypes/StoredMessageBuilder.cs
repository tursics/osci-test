using System;
using System.Collections;
using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><H4>StoredMessage-Parser</H4>
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
    internal class StoredMessageBuilder
        : OsciMessageBuilder
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(StoredMessageBuilder));

        /// <summary> Objekt ProcessCardBundle für ProcessCard Information
        /// </summary>
        private ProcessCardBundleBuilder _processCardBuilder;
        private readonly ArrayList _processCardBundles;
        private ProcessCardBundleBuilder _replyProcessCardBuilder;

        /// <summary> Constructor for the ResponseToForwardDeliveryBuilder object
        /// </summary>
        public StoredMessageBuilder(OsciEnvelopeBuilder parentHandler, int msgType) : base(parentHandler)
        {
            _processCardBundles = new ArrayList();
            Msg = new StoredMessage(msgType);
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start-Element: " + localName);


            // TODO
            if (localName.Equals("MessageId") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new System.Text.StringBuilder();
            }
            else if (InsideHeader && localName.Equals("MessageIdResponse") && uri.Equals(OsciXmlns) && Msg.MessageType == OsciMessage.ProcessDelivery)
            {
                CurrentElement = new System.Text.StringBuilder();
            }
            else if (localName.Equals("SelectionRule") && uri.Equals(OsciXmlns))
            {
            }
            else if (localName.Equals("Quantity") && uri.Equals(OsciXmlns))
                ((StoredMessage)Msg).quantityLimit = long.Parse(attributes.GetValue("Limit"));

            else if (localName.Equals("Feedback") && uri.Equals(OsciXmlns))
            {
                FeedbackBuilder feedbackBuilder = new FeedbackBuilder(this, false);
                EnvelopeBuilder.XmlReader.ContentHandler = feedbackBuilder;
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
                    EnvelopeBuilder.XmlReader.ContentHandler = new ControlBlockHBuilder(this, attributes, new[] { -1, -1, -1, -1 });
                }

                // ### Auswerten des ClientSignature Headers###
                else if (localName.Equals("SupplierSignature") && uri.Equals(OsciXmlns) || localName.Equals("ClientSignature") && uri.Equals(OsciXmlns))
                {
                    EnvelopeBuilder.XmlReader.ContentHandler = MessagePartsFactory.CreateOsciSignatureBuilder(EnvelopeBuilder.XmlReader, this, attributes);
                }
                else if (localName.Equals("DesiredLanguages") && uri.Equals(OsciXmlns))
                {
                    Msg.DesiredLanguagesH = new DesiredLanguagesH(this, attributes);
                    Msg.DialogHandler.LanguageList = attributes.GetValue("LanguagesList");
                    Msg.DesiredLanguagesH.SetNamespacePrefixes(Msg);
                }
                else if (localName.Equals("QualityOfTimestamp") && uri.Equals(OsciXmlns))
                {
                    EnvelopeBuilder.XmlReader.ContentHandler = new QualityOfTimestampHBuilder(this, attributes);
                }
                else if (localName.Equals("RequestProcessCardBundle") && uri.Equals(OsciXmlns))
                {
                    int[] check = { -1, -1, -1, -1 };
                    _processCardBuilder = new ProcessCardBundleBuilder("RequestProcessCardBundle", EnvelopeBuilder.XmlReader, this, attributes, check);
                    EnvelopeBuilder.XmlReader.ContentHandler = _processCardBuilder;
                }
                else if (localName.Equals("ReplyProcessCardBundle") && uri.Equals(OsciXmlns))
                {
                    int[] check = { -1, -1, -1, -1 };
                    _replyProcessCardBuilder = new ProcessCardBundleBuilder("ReplyProcessCardBundle", EnvelopeBuilder.XmlReader, this, attributes, check);
                    EnvelopeBuilder.XmlReader.ContentHandler = _replyProcessCardBuilder;
                }
                else if (localName.Equals("IntermediaryCertificates") && uri.Equals(OsciXmlns))
                {
                    int[] check = { -1, -1 };

                    try
                    {
                        EnvelopeBuilder.XmlReader.ContentHandler = new IntermediaryCertificatesHBuilder(this, attributes, check);
                    }
                    catch (Exception ex)
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("cert_gen_error"), ex);
                    }
                }
                else if (localName.Equals("NonIntermediaryCertificates") && uri.Equals(OsciXmlns))
                {
                    int[] check = { -1, -1, -1, -1, -1, -1, -1 };

                    try
                    {
                        EnvelopeBuilder.XmlReader.ContentHandler = new NonIntermediaryCertificatesHBuilder(this, attributes, check);
                    }
                    catch (Exception ex)
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("cert_gen_error"), ex);
                    }
                }
                else if (localName.Equals("FeatureDescription") && uri.Equals(Osci2017Xmlns))
                {
                    FeatureDescriptionHBuilder featureBuilder = new FeatureDescriptionHBuilder(this, attributes);
                    EnvelopeBuilder.XmlReader.ContentHandler = featureBuilder;
                    featureBuilder.StartElement(uri, localName, qName, attributes);
                }
                else if (localName.Equals("Subject") && uri.Equals(OsciXmlns))
                {
                    CurrentElement = new System.Text.StringBuilder();
                }
                else if (localName.Equals("ContentReceiver") && uri.Equals(OsciXmlns))
                {
                    try
                    {
                        ((StoredMessage)Msg).uriReceiver = new Uri(attributes.GetValue("URI"));
                    }
                    catch (Exception ex)
                    {
                        throw new SaxException(ex);
                    }
                }
                else if (localName.Equals("QualityOfTimestamp") && uri.Equals(OsciXmlns))
                {
                    EnvelopeBuilder.XmlReader.ContentHandler = new QualityOfTimestampHBuilder(this, attributes);
                }
                else if (localName.Equals("mediateDelivery") && uri.Equals(OsciXmlns)
                         || localName.Equals("responseToMediateDelivery") && uri.Equals(OsciXmlns)
                         || localName.Equals("storeDelivery") && uri.Equals(OsciXmlns)
                         || localName.Equals("responseToStoreDelivery") && uri.Equals(OsciXmlns)
                         || localName.Equals("fetchDelivery") && uri.Equals(OsciXmlns)
                         || localName.Equals("responseToFetchDelivery") && uri.Equals(OsciXmlns)
                         || localName.Equals("fetchProcessCard") && uri.Equals(OsciXmlns)
                         || localName.Equals("responseToFetchProcessCard") && uri.Equals(OsciXmlns)
                         || localName.Equals("acceptDelivery") && uri.Equals(OsciXmlns)
                         || localName.Equals("responseToAcceptDelivery") && uri.Equals(OsciXmlns)
                         || localName.Equals("processDelivery") && uri.Equals(OsciXmlns)
                         || localName.Equals("responseToProcessDelivery") && uri.Equals(OsciXmlns)
                    )
                {
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

        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End-Element: " + localName);

            if (localName.Equals("Envelope") && uri.Equals(SoapXmlns))
            {
                StoredMessage sm = (StoredMessage)Msg;

                if (_processCardBuilder != null)
                {
                    _processCardBundles.Add(_processCardBuilder.ProcessCardBundleObject);
                    sm.processCardBundles = (ProcessCardBundle[])_processCardBundles.ToArray(typeof(ProcessCardBundle));

                    if (_replyProcessCardBuilder != null)
                    {
                        sm.processCardBundleReply = _replyProcessCardBuilder.ProcessCardBundleObject;
                    }
                }

                for (int i = 0; i < CustomSoapHeader.Count; i++)
                {
                    Msg.AddCustomHeader(CustomSoapHeader[i]);
                }

                Msg.DialogHandler.CheckSignatures = false;
                EnvelopeBuilder.XmlReader.ContentHandler = EnvelopeBuilder;
            }
            else if (localName.Equals("MessageId") && uri.Equals(OsciXmlns))
            {
                try
                {
                    if ((Msg.MessageType == OsciMessage.FetchDelivery) || (Msg.MessageType == OsciMessage.ResponseToFetchDelivery) || (Msg.MessageType == OsciMessage.FetchProcessCard) || (Msg.MessageType == OsciMessage.ResponseToFetchProcessCard))
                    {
                        ((StoredMessage)Msg).selectionMode = OsciMessage.SelectByMessageId;
                        ((StoredMessage)Msg).selectionRule = Base64.Decode(CurrentElement.ToString()).AsString();
                    }
                    else
                    {
                        Msg.MessageId = Base64.Decode(CurrentElement.ToString()).AsString();
                    }
                }
                catch (Exception ex)
                {
                    throw new SaxException(ex);
                }
            }
            else if (localName.Equals("ReceptionOfDelivery") && uri.Equals(OsciXmlns))
            {
                ((StoredMessage)Msg).selectionMode = OsciMessage.SelectByDateOfReception;
                ((StoredMessage)Msg).selectionRule = CurrentElement.ToString();
            }
            else if (localName.Equals("Subject") && uri.Equals(OsciXmlns))
            {
                ((StoredMessage)Msg).subject = CurrentElement.ToString();
            }
            else
            {
                base.EndElement(uri, localName, qName);
            }

            CurrentElement = null;
        }
    }
}