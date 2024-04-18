using System.IO;
using Osci.Common;
using Osci.Extensions;
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
    public class DesiredLanguagesH
        : HeaderEntry
    {

        public string LanguageList
        {
            get
            {
                return _languageList;
            }

        }

        /// <summary>Liste der Sprachen im Format de,fr 
        /// </summary>
        private readonly string _languageList;

        /// <summary> Creates a new DesiredLanguageH object.
        /// </summary>
        /// <param name="languageList">
        /// </param>
        public DesiredLanguagesH(string languageList)
        {
            _languageList = languageList;
        }

        public DesiredLanguagesH(OsciMessageBuilder parentHandler, Attributes attributes)
            : this(attributes.GetValue("LanguagesList"))
        {
            parentHandler.SignatureRelevantElements.AddElement("DesiredLanguages", Namespace.Osci, attributes);
            RefId = attributes.GetValue("Id");
        }

        public override void WriteXml(Stream stream)
        {
            stream.Write("<" + OsciNsPrefix + ":DesiredLanguages");
            stream.Write(Ns, 0, Ns.Length);
            stream.Write(" Id=\"desiredlanguages\" LanguagesList=\"" + _languageList);
            stream.Write("\" " + SoapNsPrefix + ":actor=\"http://schemas.xmlsoap.org/soap/actor/next\" " + SoapNsPrefix + ":mustUnderstand=\"1\"></" + OsciNsPrefix + ":DesiredLanguages>");
        }
    }
}