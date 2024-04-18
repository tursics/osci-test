using System.Collections.Generic;
using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Messagetypes;

namespace Osci.MessageParts
{
    /// <summary><p><H4>Laufzettelparser</H4></p>Wird von Anwendungen nie direkt benötigt.
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
    internal class ProcessCardBundleBuilder
        : MessagePartParser
    {
        public ProcessCardBundle ProcessCardBundleObject
        {
            get
            {
                return ProcessCard;
            }
        }

        private static readonly Log _log = LogFactory.GetLog(typeof(ProcessCardBundleBuilder));
        internal ProcessCardBundle ProcessCard;
        private readonly string _name;
        private bool _isInProcessCard = false;
        internal TimestampBuilder TimeBuilder;
        private readonly XmlReader _xmlReader;
        private readonly DefaultHandler _parent;
        private InspectionBuilder _inspectionBuilder;
        private readonly int[] _check;
        private readonly List<Inspection> _inspections;

        /// <summary>
        /// </summary>
        /// <param name="xmlReader">
        /// </param>
        /// <param name="parentHandler">
        /// </param>
        /// <param name="atts">
        /// </param>
        /// <param name="check">Array von gültigen Elementen
        /// -1 Element dürfen beliebig oft auftreten
        /// 0 niemals
        /// 1 Einmal
        /// Es werden die Elemente der ProcessCard beschieben
        /// Der Erste Wert im Array bechreibt das Creation-Element
        /// 2. Das Forward Element
        /// 3. das Reception - Element
        /// 4. Das Subject Element
        /// </param>
        public ProcessCardBundleBuilder(string name, XmlReader xmlReader, DefaultHandler parentHandler, Attributes atts, int[] check)
        {
            _inspections = new List<Inspection>();
            _name = name;
            ProcessCard = new ProcessCardBundle(name);
            _check = check;
            OsciMessage msg = ((OsciMessageBuilder)parentHandler).OsciMessage;
            ProcessCard.SetNamespacePrefixes(msg);
            _xmlReader = xmlReader;
            _parent = parentHandler;
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start Element: " + localName);
            if (localName.Equals("MessageId") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new System.Text.StringBuilder();
            }
            else if (localName.Equals("ProcessCardBundle") && uri.Equals(OsciXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("ProcessCard") && uri.Equals(OsciXmlns))
            {
                ProcessCard.RecentModification = attributes.GetValue("RecentModification");
            }
            else if (localName.Equals("Creation") && uri.Equals(OsciXmlns)
                || localName.Equals("Forwarding") && uri.Equals(OsciXmlns)
                || localName.Equals("Reception"))
            {
                TimeBuilder = new TimestampBuilder(_xmlReader, this);
                _xmlReader.ContentHandler = TimeBuilder;
            }
            else if (localName.Equals("Subject") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new System.Text.StringBuilder();
            }
            else if (localName.Equals("InspectionReport") && uri.Equals(OsciXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("Inspection") && uri.Equals(OsciXmlns))
            {
                _inspectionBuilder = new InspectionBuilder(_xmlReader, this);
                _xmlReader.ContentHandler = _inspectionBuilder;
            }
            else
            {
                throw new SaxException("Das Element " + localName + " ist im Content von Process-Card nicht gültig");
            }
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End-Element: " + localName);
            if (localName.Equals(_name) && uri.Equals(OsciXmlns))
            {
                if (_inspections.Count > 0)
                {
                    ProcessCard.Inspections = _inspections.ToArray();
                }
                ProcessCard.Name = _name;
                _xmlReader.ContentHandler = _parent;
            }
            else if (localName.Equals("MessageId") && uri.Equals(OsciXmlns))
            {
                ProcessCard.MessageId = Base64.Decode(CurrentElement.ToString()).AsString();
            }
            else if (localName.Equals("Creation") && uri.Equals(OsciXmlns))
            {
                if (_check[0] != 0)
                {
                    ProcessCard.Creation = TimeBuilder.TimestampObject;
                    _check[0]--;
                }
                else
                {
                    throw new SaxException("Das Element  " + TimeBuilder.TimestampObject.Name + " ist im Content dieser Process-Card nicht gültig");
                }
            }
            else if (localName.Equals("Forwarding") && uri.Equals(OsciXmlns))
            {
                if (_check[1] != 0)
                {
                    ProcessCard.Forwarding = TimeBuilder.TimestampObject;
                    _check[1]--;
                }
                else
                {
                    throw new SaxException("Das Element  " + TimeBuilder.TimestampObject.Name + " ist im Content dieser Process-Card nicht gültig");
                }
            }
            else if (localName.Equals("Reception") && uri.Equals(OsciXmlns))
            {
                if (_check[2] != 0)
                {
                    ProcessCard.Reception = TimeBuilder.TimestampObject;
                    _check[2]--;
                }
                else
                {
                    throw new SaxException("Das Element " + TimeBuilder.TimestampObject.Name + " ist im Content dieser Process-Card nicht gültig");
                }
            }
            else if (localName.Equals("Subject") && uri.Equals(OsciXmlns))
            {
                if (_check[3] != 0)
                {
                    ProcessCard.Subject = CurrentElement.ToString();
                    _check[3]--;
                }
                else
                {
                    throw new SaxException("Das Element Subject ist im Content dieser Process-Card nicht gültig");
                }
            }
            else if (localName.Equals("InspectionReport") && uri.Equals(OsciXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("Inspection") && uri.Equals(OsciXmlns))
            {
                _inspections.Add(_inspectionBuilder.InspectionObject);
            }
            else if (localName.Equals("ProcessCard") && uri.Equals(OsciXmlns))
            {
                // nothing to do
            }
            else
            {
                throw new SaxException("Das End-Element " + localName + " ist im Content von Process-Card nicht gültig");
            }
            CurrentElement = null;
        }
    }
}