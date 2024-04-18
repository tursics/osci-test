using Osci.Common;
using Osci.Encryption;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.Messagetypes;

namespace Osci.MessageParts
{
    /// <summary><H4>ContentPackage-Parser</H4>
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
    public class ContentPackageBuilder
        : MessagePartParser
    {

        public object LastCreatedObject
        {
            get
            {
                return _lastObject;
            }
        }

        private static readonly Log _log = LogFactory.GetLog(typeof(ContentPackageBuilder));
        private object _lastObject;
        private EncryptedDataBuilder _encDataBuilder;
        private ContentContainerBuilder _cocoBuilder;
        private string _soapNsPrefix, _osciNsPrefix, _dsNsPrefix, _xencNsPrefix, _xsiNsPrefix;
        // Canonizer, aus dem der Parser liest 
        private readonly Canonizer _can;

        public ContentPackageBuilder(XmlReader reader, OsciMessage msg, Canonizer can) 
            : base(reader, null)
        {
            _log.Trace("############## Konstruktor: " + msg.GetType());
            XmlReader = reader;
            Msg = msg;
            _can = can;
        }

        public ContentPackageBuilder(OsciMessageBuilder parentBuilder) 
            : base(parentBuilder)
        {
            _can = parentBuilder.CanStream;
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start-Element: " + qName);

            if (localName.Equals("ContentPackage") && uri.Equals(OsciXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("EncryptedData") && uri.Equals(XencXmlns))
            {
                _encDataBuilder = new EncryptedDataBuilder(XmlReader, this, attributes);
                XmlReader.ContentHandler = _encDataBuilder;
            }
            else if (localName.Equals("ContentContainer") && uri.Equals(OsciXmlns))
            {
                _cocoBuilder = new ContentContainerBuilder(XmlReader, this, Msg, attributes, _can);
                XmlReader.ContentHandler = _cocoBuilder;
            }
            else
            {
                throw new SaxException("Nicht vorgesehens Element! Element Name:" + localName);
            }
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End-Element: " + qName);
            if (localName.Equals("ContentPackage") && uri.Equals(OsciXmlns))
            {
                XmlReader.ContentHandler = ParentHandler;
            }
            else if (localName.Equals("ContentContainer") && uri.Equals(OsciXmlns))
            {
                _lastObject = _cocoBuilder.ContentContainer;
                if (ParentHandler != null)
                {
                    Msg.AddContentContainer((ContentContainer)_lastObject);
                }
            }
            else if (localName.Equals("EncryptedData") && uri.Equals(XencXmlns))
            {
                _log.Trace("Encrypted-Data wird hinzugefügt.");
                _lastObject = new EncryptedDataOsci(_encDataBuilder.EncryptedData, Msg);
                if (ParentHandler != null)
                {
                    Msg.EncryptedDataList.Put(((EncryptedDataOsci)_lastObject).RefId, _lastObject);
                }
            }
            else
            {
                throw new SaxException("Nicht vorgesehens Element! Element Name:" + localName);
            }
        }
    }
}