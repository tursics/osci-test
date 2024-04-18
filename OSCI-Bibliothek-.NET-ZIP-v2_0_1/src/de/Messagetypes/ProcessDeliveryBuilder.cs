using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{

    /// <summary><H4>ProcessDelivery-Parser</H4>
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
    public class ProcessDeliveryBuilder
        : OsciMessageBuilder
    {

        private static readonly Log _log = LogFactory.GetLog(typeof(ProcessDeliveryBuilder));
        private ProcessCardBundleBuilder _processCardBuilder;

        /// <summary> Konstruktutor für den aufrufenden Mime-Parser
        /// </summary>
        /// <param name="xmlReader"> XML-Reader für den ContentHandler
        /// </param>
        /// <param name="parentBuilder"> Parent Object in den meisten Fällen der OSCIEnvelopeBuilder
        /// </param>
        /// <param name="dh"> DialogHandler zum Überprüfen der Dialoginformationen
        /// </param>
        public ProcessDeliveryBuilder(OsciEnvelopeBuilder parentBuilder)
            : base(parentBuilder)
        {
            _log.Debug("Konstruktor");
            Msg = new ProcessDelivery();
        }

        /// <summary> Überschreibt die character Methode des DefaultHandler
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
            try
            {
                _log.Debug("Start-Element: " + localName);
                if (InsideBody)
                {
                    if (localName.ToUpper().Equals("ContentPackage".ToUpper()) && uri.Equals(OsciXmlns))
                    {
                        SetContentPackageHandler(localName);
                    }
                }
                else if (InsideHeader)
                {
                    _log.Debug("inside Header");
                    // ### Auswerten des ControlBlock Headers###
                    if (localName.ToUpper().Equals("ControlBlock".ToUpper()) && uri.Equals(OsciXmlns))
                    {
                        EnvelopeBuilder.XmlReader.ContentHandler = new ControlBlockHBuilder(this, attributes, new[] { 0, 1, 0, 0 });
                        // ### Auswerten des ClientSignature Headers###
                    }
                    else if (localName.ToUpper().Equals("ClientSignature".ToUpper()) && uri.Equals(OsciXmlns))
                    {
                        EnvelopeBuilder.XmlReader.ContentHandler = new OsciSignatureBuilder(EnvelopeBuilder.XmlReader, this, attributes, false);
                    }
                    else if (localName.ToUpper().Equals("DesiredLanguages".ToUpper()) && uri.Equals(OsciXmlns))
                    {
                        EnvelopeBuilder.XmlReader.ContentHandler = new DesiredLanguagesHBuilder(this, attributes);
                    }
                    else if (localName.Equals("processDelivery") && uri.Equals(OsciXmlns))
                    {
                        SignatureRelevantElements.AddElement(localName, uri, attributes);
                    }
                    else if (localName.Equals("MessageIdResponse") && uri.Equals(OsciXmlns))
                    {
                        CurrentElement = new System.Text.StringBuilder();
                    }
                    else if (localName.ToUpper().Equals("ProcessCardBundle".ToUpper()) && uri.Equals(OsciXmlns))
                    {
                        int[] check = { 1, 1, 0, -1 };
                        _processCardBuilder = new ProcessCardBundleBuilder("ProcessCardBundle", EnvelopeBuilder.XmlReader, this, attributes, check);
                        EnvelopeBuilder.XmlReader.ContentHandler = _processCardBuilder;
                    }
                    else if (localName.ToUpper().Equals("IntermediaryCertificates".ToUpper()) && uri.Equals(OsciXmlns))
                    {
                        int[] check = { -1, -1 };
                        EnvelopeBuilder.XmlReader.ContentHandler = new IntermediaryCertificatesHBuilder(this, attributes, check);
                    }
                    else if (localName.ToUpper().Equals("NonIntermediaryCertificates".ToUpper()) && uri.Equals(OsciXmlns))
                    {
                        int[] check = { -1, -1, -1, -1, -1, -1, 0 };
                        EnvelopeBuilder.XmlReader.ContentHandler = new NonIntermediaryCertificatesHBuilder(this, attributes, check);
                        _log.Debug("Accept Delivery Element: " + localName);
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
            catch (SaxException ex)
            {
                _log.Error("SAX Fehler:", ex);
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
                if (_processCardBuilder != null)
                {
                    ((ProcessDelivery)Msg).processCardBundle = _processCardBuilder.ProcessCardBundleObject;
                }
                InsideHeader = false;
            }
            else if (InsideHeader && localName.Equals("MessageIdResponse") && uri.Equals(OsciXmlns))
            {
                Msg.MessageId = Base64.Decode(CurrentElement.ToString()).AsString();
            }
            else
            {
                base.EndElement(uri, localName, localName);
            }
            CurrentElement = null;
        }
    }
}