using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;

namespace Osci.MessageParts
{
    /// <summary><p><H4>Zeitstempelparser</H4></p>Wird von Anwendungen nie direkt benötigt.
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
    internal class TimestampBuilder
        : MessagePartParser
    {

        internal Timestamp TimestampObject
        {
            get
            {
                return _timestamp;
            }
        }

        private static readonly Log _log = LogFactory.GetLog(typeof(TimestampBuilder));
        private readonly Timestamp _timestamp;
        private readonly XmlReader _xmlReader;
        private readonly DefaultHandler _parent;
        public TimestampBuilder(XmlReader xmlReader, DefaultHandler parentHandler)
        {
            MessagePart mp;
            _timestamp = new Timestamp();
            if (parentHandler is InspectionBuilder)
            {
                mp = ((InspectionBuilder)parentHandler).Inspection;
            }
            else
            {
                mp = ((ProcessCardBundleBuilder)parentHandler).ProcessCard;
            }

            _timestamp.SetNamespacePrefixes(mp.SoapNsPrefix, mp.OsciNsPrefix, mp.DsNsPrefix, mp.XencNsPrefix, mp.XsiNsPrefix);
            _xmlReader = xmlReader;
            _parent = parentHandler;
            _timestamp = new Timestamp();
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start-Element: " + localName);
            if (localName.Equals("Plain") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new System.Text.StringBuilder();
            }
            else if (localName.Equals("Cryptographic") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new System.Text.StringBuilder();
                string algo = attributes.GetValue("Algorithm");
                if (algo == null || algo.ToUpper().Equals("".ToUpper()))
                {
                    throw new SaxException("Das Attribut Algorthm fehlt im Element Timestamp!");
                }
                _timestamp.Algorithm = algo;
            }
            else
            {
                throw new SaxException("Das Element " + localName + " ist im Content von Timestamp nicht gültig");
            }
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End-Element: " + localName);
            if (localName.Equals("Plain") && uri.Equals(OsciXmlns))
            {
                _timestamp.TimeStamp = CurrentElement.ToString();
            }
            else if (localName.Equals("Cryptographic") && uri.Equals(OsciXmlns))
            {
                _timestamp.TimeStamp = Base64.Decode(CurrentElement.ToString()).AsString();
            }
            else if (localName.Equals("Timestamp") && uri.Equals(OsciXmlns))
            {
                _timestamp.NameId = Timestamp.ProcessCardTimestamp;
                _parent.EndElement(uri, localName, qName);
                _xmlReader.ContentHandler = _parent;
            }
            else if (localName.Equals("Creation") && uri.Equals(OsciXmlns))
            {
                _timestamp.NameId = Timestamp.ProcessCardCreation;
                _parent.EndElement(uri, localName, qName);
                _xmlReader.ContentHandler = _parent;
            }
            else if (localName.Equals("Forwarding") && uri.Equals(OsciXmlns))
            {
                _timestamp.NameId = Timestamp.ProcessCardForwarding;
                _parent.EndElement(uri, localName, qName);
                _xmlReader.ContentHandler = _parent;
            }
            else if (localName.Equals("Reception") && uri.Equals(OsciXmlns))
            {
                _timestamp.NameId = Timestamp.ProcessCardReception;
                _parent.EndElement(uri, localName, qName);
                _xmlReader.ContentHandler = _parent;
            }
            else
            {
                throw new SaxException("Das Element " + localName + " ist im Content von TimeStamp nicht gültig");
            }
            CurrentElement = null;
        }
    }
}