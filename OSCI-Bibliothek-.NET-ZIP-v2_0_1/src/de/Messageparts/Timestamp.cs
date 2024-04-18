using System.Collections.Generic;
using System.IO;
using Osci.Common;
using Osci.Extensions;

namespace Osci.MessageParts
{
    /// <summary>Diese Klasse repräsentiert das OSCI-Timestamp-Element, es enthält
    /// Zeitstempelinformationen
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
    public class Timestamp
        : MessagePart
    {
        /// <summary> Liefert den Algorithmus (für kryptographische Zeitstempel)
        /// </summary>
        /// <value> der Algorithmus
        /// </value>
        public string Algorithm
        {
            get; internal set;
        }

        /// <summary> Liefert den Typ des Zeitstempels als String. Mögliche Werte sind
        /// "Creation", "Forwarding", "Reception" und "Timestamp".
        /// </summary>
        /// <value> id
        /// </value>
        public string Name
        {
            get
            {
                return _timestampNames[NameId];
            }
        }


        /// <summary> Liefert den Zeitstempelstring selbst.
        /// </summary>
        /// <value> Zeitstempel
        /// </value>
        public string TimeStamp
        {
            get; internal set;
        }

        /// <summary> Liefert den Identifier des Zeitstempeltyps. Mögliche Werte sind
        /// PROCESS_CARD_CREATION, PROCESS_CARD_FORWARDING, PROCESS_CARD_RECEPTION
        /// und PROCESS_CARD_TIMESTAMP.
        /// </summary>
        /// <value> id Typ
        /// </value>
        public int NameId
        {
            get; internal set;
        }

        private static readonly string[] _timestampNames = { "Creation", "Forwarding", "Reception", "Timestamp" };
        public static IEnumerable<string> TimestampNames
        {
            get
            {
                return _timestampNames;
            }
        }


        public const int ProcessCardCreation = 0;
        public const int ProcessCardForwarding = 1;
        public const int ProcessCardReception = 2;
        public const int ProcessCardTimestamp = 3;

        internal Timestamp()
        {
        }

        // Bei algorithm == null oder Leerstring ist der Typ "plain"
        public Timestamp(int nameId, string algorithm, string value)
        {
            NameId = nameId;
            Algorithm = algorithm;
            TimeStamp = value;
        }

        public override void WriteXml(Stream stream)
        {
           stream.Write("<" + OsciNsPrefix + ":" + _timestampNames[NameId] + ">");
            if ((Algorithm == null) || (Algorithm.Length == 0))
            {
                stream.Write("<" + OsciNsPrefix + ":Plain>" + TimeStamp + "</" + OsciNsPrefix + ":Plain>");
            }
            else
            {
                stream.Write("<" + OsciNsPrefix + ":Cryptographic Algorithm=\"" + Algorithm + "\">"
                    + Helper.Base64.Encode(TimeStamp, Constants.CharEncoding)
                    + "</" + OsciNsPrefix + ":Cryptographic>");
            }
            stream.Write("</" + OsciNsPrefix + ":" + _timestampNames[NameId] + ">");
        }
    }
}