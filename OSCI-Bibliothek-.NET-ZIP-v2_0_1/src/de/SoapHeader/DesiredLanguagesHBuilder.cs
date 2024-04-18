using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Messagetypes;

namespace Osci.SoapHeader
{
    /// <exclude/>
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class DesiredLanguagesHBuilder
        : MessagePartParser
    {
        private readonly Log _log = LogFactory.GetLog(typeof(DesiredLanguagesHBuilder));

        internal DesiredLanguagesH Dlh;

        /// <summary> Constructor for the DesiredLanguagesHBuilder object
        /// </summary>
        /// <param name="xmlReader">
        /// </param>
        /// <param name="parentHandler">
        /// </param>
        /// <param name="atts">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public DesiredLanguagesHBuilder(OsciMessageBuilder parent, Attributes atts) : base(parent)
        {
            if (atts.GetValue("LanguagesList") == null)
            {
                throw new SaxException("Keine Sprachliste im DesiredLanguage-Header !");
            }
            Dlh = new DesiredLanguagesH(atts.GetValue("LanguagesList"));
            parent.OsciMessage.DesiredLanguagesH = Dlh;
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
            if (localName.Equals("DesiredLanguages") && uri.Equals(OsciXmlns))
            {
                Msg.DesiredLanguagesH = Dlh;
                XmlReader.ContentHandler = ParentHandler;
            }
            else
            {
                throw new SaxException("Unbekanntes Element gefunden: " + qName);
            }
        }
    }
}