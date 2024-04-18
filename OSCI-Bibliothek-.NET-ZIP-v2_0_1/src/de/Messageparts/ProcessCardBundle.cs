using System.IO;
using Osci.Common;
using Osci.Extensions;
using Osci.Helper;

namespace Osci.MessageParts
{
    /// <summary> Diese Klasse bildet den OSCI-Laufzettel ab.
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
    public class ProcessCardBundle
        : MessagePart
    {
        private string _name;

        /// <summary> Liefert den Zeitstempel der Erstellung.
        /// </summary>
        /// <value> Zeitstempel
        /// </value>
        public Timestamp Creation
        {
            get; internal set;
        }

        /// <summary> Liefert den Zeitstempel der Weiterleitung.
        /// </summary>
        /// <value> Zeitstempel
        /// </value>
        public Timestamp Forwarding
        {
            get; internal set;
        }

        /// <summary> Liefert den Zeitstempel des Empfangs.
        /// </summary>
        /// <value> Zeitstempel
        /// </value>
        public Timestamp Reception
        {
            get; internal set;
        }

        /// <summary> Liefert die Ergebnisse der Zertifikatsprüfungen in Form von Inspection-Objekten,
        /// die im ProcessCardBundle-Objekt enthalten sind.
        /// </summary>
        /// <value> die Prüfergebnisse
        /// </value>
        public Inspection[] Inspections
        {
            get; internal set;

        }

        /// <summary> Liefert die Message-ID der Nachricht.
        /// </summary>
        /// <value> die Message-ID
        /// </value>
        public string MessageId
        {
            get; internal set;
        }

        /// <summary> Liefert das Datum der letzten Änderung des Laufzettels. Das Format
        /// entspricht dem XML-Schema nach <a href="http://www.w3.org/TR/xmlschema-2/#dateTime">
        /// http://www.w3.org/TR/xmlschema-2/#dateTime</a>.
        /// </summary>
        /// <value> Datum der letzten Änderung.
        /// </value>
        public string RecentModification
        {
            get; internal set;
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Betreff-Eintrag.
        /// </summary>
        /// <value> den Betreff der Zustellung
        /// </value>
        public string Subject
        {
            get;
            internal set;
        }

        internal string Name
        {
            set
            {
                _name = value;
            }
        }

        private static int _idNr = -1;
        private static long _serialVersionUid = -5723258187933747170L;

        private static readonly Log _log = LogFactory.GetLog(typeof(ProcessCardBundle));

        internal ProcessCardBundle(string name)
        {
            _name = name;
        }

        internal ProcessCardBundle(string name, string messageId, string recentModification, Timestamp creation, Timestamp forwarding, Timestamp reception, string subject, Inspection[] inspections)
        {
            _idNr++;
            RefId = "processcardbundle" + _idNr;
            _name = name;
            MessageId = messageId;
            RecentModification = recentModification;
            Creation = creation;
            Forwarding = forwarding;
            Reception = reception;
            Subject = subject;
            Inspections = inspections;
        }

        public string WriteToString()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                WriteXml(memoryStream);
                byte[] b = memoryStream.GetBuffer();
                string s = b.AsString();
                int i = s.LastIndexOf(">");
                if (i < b.Length - 1)
                {
                    return s.Substring(0, i + 1);
                }
                return s;
            }
        }

        public override void WriteXml(Stream outStream)
        {
            WriteXml(outStream, false);
        }

        private void WriteXml(Stream stream, bool writeObj)
        {
            _log.Trace("writeXML");
            _log.Trace("Name: " + _name);
            _log.Trace("MessageID: " + MessageId);
            _log.Trace("recentModi: " + RecentModification);
            stream.Write("<" + OsciNsPrefix + ":" + _name);
            if (writeObj)
            {
                stream.Write(" xmlns:" + OsciNsPrefix + "=\"" + Namespace.Osci + "\"");
            }

            stream.Write("><" + OsciNsPrefix + ":MessageId>" + Helper.Base64.Encode(MessageId) + "</" + OsciNsPrefix + ":MessageId><" + OsciNsPrefix + ":ProcessCard RecentModification=\"" + RecentModification + "\">");
            if (Creation != null)
            {
                Creation.WriteXml(stream);
            }
            if (Forwarding != null)
            {
                Forwarding.WriteXml(stream);
            }
            if (Reception != null)
            {
                Reception.WriteXml(stream);
            }
            if (Subject != null)
            {
                stream.Write("<" + OsciNsPrefix + ":Subject>" + Subject + "</" + OsciNsPrefix + ":Subject>");
            }
            stream.Write("</" + OsciNsPrefix + ":ProcessCard><" + OsciNsPrefix + ":InspectionReport>");
            if (Inspections != null)
            {
                for (int i = 0; i < Inspections.Length; i++)
                {
                    Inspections[i].WriteXml(stream);
                }
            }
            stream.Write("</" + OsciNsPrefix + ":InspectionReport></" + OsciNsPrefix + ":" + _name + ">");
        }

        internal static string Encode(string text)
        {
            text = text.Replace("&", "&amp;");
            text = text.Replace("<", "&lt;");
            text = text.Replace(">", "&gt;");

            return text.Replace("\r", "&#xD;");
        }
    }
}