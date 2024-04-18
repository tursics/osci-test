using Osci.Common;
using Osci.Encryption;
using Osci.Exceptions;
using Osci.Helper;

namespace Osci.Messagetypes
{
    /// <summary><H4>SOAPMessageEncrypted-Parser</H4>
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
    public class SoapMessageEncryptedBuilder
        : OsciMessageBuilder
    {

        private static readonly Log _log = LogFactory.GetLog(typeof(SoapMessageEncryptedBuilder));
        private EncryptedDataBuilder _edb;

        /// <summary> Constructor for the SOAPMessageEncryptedBuilder object
        /// </summary>
        /// <param name="eb">
        /// </param>
        public SoapMessageEncryptedBuilder(OsciEnvelopeBuilder eb)
            : base(eb)
        {
            _log.Trace("Konstruktor");
            Msg = new SoapMessageEncrypted(null, null);
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
            _log.Trace("Start-Element: " + localName);
            if (localName.ToUpper().Equals("EncryptedData".ToUpper()) && uri.Equals(XencXmlns))
            {
                _edb = new EncryptedDataBuilder(EnvelopeBuilder.XmlReader, this, attributes);
                EnvelopeBuilder.XmlReader.ContentHandler = _edb;
            }
            else if (localName.ToLower().Equals("body") && uri.Equals(SoapXmlns))
            {
                // nothing to do
            }
            else
            {
                throw new SaxException("Unerwartetes Element in SOAPMessageEncrypted: " + localName);
            }
        }

        public override void EndDocument()
        {
            _log.Trace("End-Document SOAP");
            ((SoapMessageEncrypted)Msg).EncryptedData = _edb.EncryptedData;
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End-Element: " + localName);
        }
    }
}