using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;

namespace Osci.Messagetypes
{
    /// <summary><H4>SOAPFault-Parser</H4>
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
    public class SoapFaultBuilder 
        : OsciMessageBuilder
    {

        private static readonly Log _log = LogFactory.GetLog(typeof(SoapFaultBuilder));
        private string _faultcode, _faultstring, _oscicode;

        public SoapFaultBuilder(OsciEnvelopeBuilder parentBuilder) : base(parentBuilder)
        {
            Msg = new SoapFault("");
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
            _log.Debug("Start Element SOAPFaultBuilder: " + localName + "-" + localName);

            if (localName.Equals("Body"))
            {
                InsideBody = true;
            }
            else if (InsideBody)
            {
                if (localName.Equals("faultcode") || localName.Equals("faultstring") || localName.Equals("Code") && uri.Equals(OsciXmlns))
                {
                    CurrentElement = new System.Text.StringBuilder();
                }
            }
            else if (!localName.Equals("detail"))
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
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
            if (localName.Equals("faultcode"))
            {
                _faultcode = CurrentElement.ToString();
            }
            else if (localName.Equals("faultstring"))
            {
                _faultstring = CurrentElement.ToString();
            }
            else if (localName.Equals("Code") && uri.Equals(OsciXmlns))
            {
                _oscicode = CurrentElement.ToString();
            }
            else if (localName.Equals("Envelope") && uri.Equals(SoapXmlns))
            {
                if (_faultcode.Equals(OsciMessage.SoapNsPrefix + ":Server") && uri.Equals(SoapXmlns))
                {
                    throw new SaxException(new SoapServerException(_oscicode, _faultstring));
                }
                else
                {
                    throw new SaxException(new SoapClientException(_oscicode, _faultstring));
                }

            }
            CurrentElement = null;
        }
    }
}
