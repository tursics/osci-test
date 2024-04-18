using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Messagetypes;

namespace Osci.MessageParts
{
    /// <summary> Interne Klasse, wird von Anwendungen nicht direkt benötigt.
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
    public class MessagePartParser
        : DefaultHandler
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(MessagePartParser));
        protected DefaultHandler ParentHandler;
        protected XmlReader XmlReader;
        protected System.Text.StringBuilder CurrentElement;
        protected internal OsciMessage Msg;

        protected internal static string SoapXmlns = Namespace.SoapEnvelope;
        protected internal static string OsciXmlns = Namespace.Osci;
        protected internal static string Osci2017Xmlns = Namespace.Osci2017;
        protected internal static string Osci128Xmlns = Namespace.Osci128;
        protected internal static string DsXmlns = Namespace.XmlDSig;
        protected internal static string XencXmlns = Namespace.XmlEnc;
        protected internal static string XsiXmlns = Namespace.XsiSchema;
        protected internal static string XadesXmlns = "http://uri.etsi.org/01903/v1.3.2#";

        /// <summary>
        /// Dieser Konstruktur ist nur für den Fall das ein Encrypted ContentContainer
        /// decrypted wird, er wird nur von ContentPackageBuilder aufgerufen
        /// </summary>
        protected MessagePartParser()
        {
            // nothing to do
        }

        protected MessagePartParser(OsciMessageBuilder parentHandler)
        {
            ParentHandler = parentHandler;
            Msg = parentHandler.OsciMessage;
            XmlReader = parentHandler.EnvelopeBuilder.XmlReader;
        }

        protected MessagePartParser(XmlReader xmlReader, DefaultHandler parentHandler)
        {
            ParentHandler = parentHandler;
            XmlReader = xmlReader;
        }

        /// <summary>
        /// </summary>
        /// <param name="ch">
        /// </param>
        /// <param name="start">
        /// </param>
        /// <param name="length">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void Characters(char[] ch, int start, int length)
        {
            _log.Trace("Character: " + new string(ch, start, length));
            if (CurrentElement == null)
            {
                for (int i = 0; i < length; i++)
                {
                    if (ch[start + i] > ' ')
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_char"));
                    }
                }
            }
            else
            {
                CurrentElement.Append(ch, start, length);
            }
        }
    }
}