using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;

namespace Osci.Signature
{
    /// <exclude/>
    /// <summary> Builder, der ein Element ds:RetrievalMethod bearbeitet.
    /// 
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: PPI Financial Systems GmbH</p> 
    /// </summary>
    class RetrievalMethodBuilder
        : DefaultHandler
    {
        protected static readonly string DsXmlns = Namespace.XmlDSig;

        /// <summary> Liefert die aufgebaute RetrievalMethod.
        /// </summary>
        /// <value> RetrievalMethod.
        /// </value>
        public RetrievalMethod RetrievalMethod
        {
            get
            {
                return _retrievalObject;
            }
        }

        internal DefaultHandler ParentHandler;
        internal XmlReader XmlReader;
        protected static Log Log = LogFactory.GetLog(typeof(RetrievalMethodBuilder));

        /// <summary>Aufgebaute RetrievalMethod. 
        /// </summary>
        private readonly RetrievalMethod _retrievalObject;

        /// <summary> Konstruktor
        /// </summary>
        /// <param name="parentHandler">DefaultCursorHandler, der diesen Builder erzeugt hat.
        /// </param>
        /// <param name="xmlReader">Aktueller CursorXMLReader.
        /// </param>
        /// <param name="attributes">Attribute des Elements ds:RetrievalMethod.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        public RetrievalMethodBuilder(XmlReader xmlReader, DefaultHandler parentHandler, Attributes attributes)
        {
            _retrievalObject = new RetrievalMethod();
            XmlReader = xmlReader;
            ParentHandler = parentHandler;
            if (attributes == null)
            {
                throw new System.ArgumentException("Dem Konstruktor der Klasse RetrievalMethodBuilder wird null übergeben !");
            }
            string uri = attributes.GetValue("URI");
            _retrievalObject.Uri = uri;
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            Log.Debug("End-Element: " + localName);
            if (localName.Equals("RetrievalMethod") && uri.Equals(DsXmlns))
            {
                if (ParentHandler is KeyInfoBuilder)
                {
                    ((KeyInfoBuilder)ParentHandler).KeyInfo.RetrievalMethod = _retrievalObject;
                }

                XmlReader.ContentHandler = ParentHandler;
            }
            else if (localName.Equals("Transforms") && uri.Equals(DsXmlns))
            {
                //nothing to do
            }
            else if (localName.Equals("Transform") && uri.Equals(DsXmlns))
            {
                //nothing to do
            }
            else
            {
                throw new SaxParseException("Nicht vorgesehenes Element: " + localName, null);
            }
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            Log.Debug("Start Element: " + localName);
            if (localName.Equals("Transforms") && uri.Equals(DsXmlns))
            {
                //nothing to do
            }
            else if (localName.Equals("Transform") && uri.Equals(DsXmlns))
            {
                _retrievalObject.AddTransformer(attributes.GetValue("Algorithm"));
            }
            else
            {
                throw new SaxParseException("Nicht vorgesehenes Element: " + localName, null);
            }
        }
    }
}