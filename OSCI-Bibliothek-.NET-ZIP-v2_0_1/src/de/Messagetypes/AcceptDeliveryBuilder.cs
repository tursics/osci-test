using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><H4>AcceptDelivery-Parser</H4>
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
    public class AcceptDeliveryBuilder
        : OsciMessageBuilder
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(AcceptDeliveryBuilder));

        // ProcessCards liegen im soap:Header und müssen beim parsen ausgewertet werden.
        private ProcessCardBundleBuilder _processCardBuilder;

        /// <summary> Konstruktor für den aufrufenden Mime-Parser.
        /// </summary>
        /// <param name="parentBuilder"> Parent Object in den meisten Fällen der OSCIEnvelopeBuilder.
        /// </param>
        public AcceptDeliveryBuilder(OsciEnvelopeBuilder parentBuilder)
            : base(parentBuilder)
        {
            Msg = new AcceptDelivery();
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
            _log.Debug("Start-Element: " + localName);

            if (InsideBody)
            {
                if (localName.ToUpper().Equals("ContentPackage".ToUpper()) && uri.Equals(OsciXmlns))
                {
                    SetContentPackageHandler(localName);
                }
                else
                {
                    throw new SaxException(DialogHandler.ResourceBundle.GetString("unsupported_entry") + ": " + localName);
                }
            }
            else if (InsideHeader)
            {
                _log.Debug("inside Header");

                // ### Auswerten des ControlBlock Headers ###
                if (localName.ToUpper().Equals("ControlBlock".ToUpper()) && uri.Equals(OsciXmlns))
                {
                    EnvelopeBuilder.XmlReader.ContentHandler = new ControlBlockHBuilder(this, attributes, new[] { 0, 1, 0, 0 });
                    // ### Auswerten des ClientSignature Headers ###
                }
                else if (localName.ToUpper().Equals("ClientSignature".ToUpper()) && uri.Equals(OsciXmlns))
                {
                    EnvelopeBuilder.XmlReader.ContentHandler = new OsciSignatureBuilder(EnvelopeBuilder.XmlReader, this, attributes, false);
                }
                else if (localName.Equals("DesiredLanguages") && uri.Equals(OsciXmlns))
                {
                    Msg.DesiredLanguagesH = new DesiredLanguagesH(this, attributes );
                    Msg.DialogHandler.LanguageList = attributes.GetValue("LanguagesList");
                }
                else if (localName.Equals("acceptDelivery") && uri.Equals(OsciXmlns))
                {
                    SignatureRelevantElements.AddElement(localName, uri, attributes);
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

            if (localName.Equals("Header") && uri.Equals(SoapXmlns))
            {
                ((AcceptDelivery)Msg).ProcessCardBundle = _processCardBuilder.ProcessCardBundleObject;
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