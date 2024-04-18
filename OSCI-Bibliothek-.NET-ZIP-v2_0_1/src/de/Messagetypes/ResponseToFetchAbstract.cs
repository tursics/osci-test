using System;
using System.IO;
using Osci.Common;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Zustellungsabholantwort-Nachrichtenobjekt</H4></p>
    /// Diese Klasse repräsentiert die Antwort des Intermediärs auf einen
    /// Zustellungsabholauftrag.
    /// Clients erhalten vom Intermediär eine Instanz dieser Klasse, die eine Rückmeldung
    /// über den Erfolg der Operation (getFeedback()) sowie ggf. die angeforderten verschlüsselten
    /// und/oder unverschlüsselten Inhaltsdaten einschl. des zugehörigen Laufzettels enthält.
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
    /// <seealso cref="FetchDelivery">
    /// </seealso>

    public class ResponseToFetchAbstract
        : OsciResponseTo
        , IContentPackage
    {

        
        private int _selectionMode = -1;


        protected ResponseToFetchAbstract(DialogHandler dh) : base(dh)
        {
        }

        /// <summary> Liefert die gesetzte Auswahlregel. Der Inhalt des zruückgegebenen
        /// Strings hängt vom gesetzten Auswahlmodus ab und kann
        /// entweder in einer Base64-codierten Message-Id oder einem Datum bestehen.
        /// Das Format eines Datums entspricht ß dem XML-Schema nach
        /// <a href="http://www.w3.org/TR/xmlschema-2/#dateTime">
        /// http://www.w3.org/TR/xmlschema-2/#dateTime</a>.
        /// Wurde keine Regel gesetzt, wird als default null zurückgegeben.
        /// </summary>
        /// <value> Auswahlregel (Message-Id oder Datum)
        /// </value>
        /// <seealso cref="SelectionRule()">
        /// </seealso>
        // <getset></getset>
        public string SelectionRule
        {
            get; internal set;
        }

        /// <summary> Liefert den gesetzten Auswahlmodus.
        /// </summary>
        /// <value> Auswahlmodus SELECT_BY_MESSAGE_ID, SELECT_BY_DATE_OF_RECEPTION
        /// oder NO_SELECTION_RULE
        /// </value>
        public int SelectionMode
        {
            get
            {
                return _selectionMode;
            }
            internal set
            {
                _selectionMode = value;
            }
        }

        private ProcessCardBundle _processCardBundle;

        /// <summary> Diese Methode liefert den Laufzettel der Zustellung zurück oder null,
        /// wenn bei der Verarbeitung der Nachricht ein Fehler aufgetereten ist.
        /// Die Informationen im Laufzettel können auch direkt über die einzelnen
        /// getX()-Methoden ausgewertet werden.
        /// </summary>
        /// <value> Laufzettel als ProcessCardBundle-Objekt, im Fehlerfall null
        /// </value>
        /// <seealso cref="TimestampCreation()">
        /// </seealso>
        /// <seealso cref="TimestampForwarding()">
        /// </seealso>
        /// <seealso cref="Inspections()">
        /// </seealso>
        /// <seealso cref="Subject()">
        /// </seealso>
        /// <seealso cref="RecentModification()">
        /// </seealso>
        /// <seealso cref="MessageId()">
        /// </seealso>
        public ProcessCardBundle ProcessCardBundle
        {
            get
            {
                return _processCardBundle;
            }
            internal set
            {
                _processCardBundle = value;
            }
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des Eingangs
        /// des Zustellungsauftrags beim Intermediär.
        /// </summary>
        /// <value> Zeitstempel der Einreichung beim Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public Timestamp TimestampCreation
        {
            get
            {
                if (_processCardBundle == null)
                {
                    return null;
                }
                return _processCardBundle.Creation;
            }
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des
        /// vollständigen Aufbaus der Abholantwort vom Intermediär für den Empfänger.
        /// </summary>
        /// <value> Zeitstempel der Erstellung des Abholantwort durch den Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public Timestamp TimestampForwarding
        {
            get
            {
                if (_processCardBundle == null)
                {
                    return null;
                }
                return _processCardBundle.Forwarding;
            }
        }

        /// <summary> Liefert die Ergebnisse der Zertifikatsprüfungen in Form von Inspection-Objekten,
        /// die im ProcessCardBundle-Objekt enthalten sind.
        /// </summary>
        /// <value> Prüfergebnisse
        /// </value>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>		
        public Inspection[] Inspections
        {
            get
            {
                if (_processCardBundle == null)
                {
                    return null;
                }
                return _processCardBundle.Inspections;
            }
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Betreff-Eintrag.
        /// </summary>
        /// <value> Betreff der Zustellung
        /// </value>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public string Subject
        {
            get
            {
                if (_processCardBundle == null)
                {
                    return null;
                }
                return _processCardBundle.Subject;
            }
        }

        /// <summary> Liefert das Datum der letzten Änderung des Laufzettels. Das Format
        /// entspricht dem XML-Schema nach http://www.w3.org/TR/xmlschema-2/#dateTime
        /// </summary>
        /// <value> Datum der letzten Änderung.
        /// </value>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>		
        public string RecentModification
        {
            get
            {
                if (_processCardBundle == null)
                {
                    return null;
                }
                return _processCardBundle.RecentModification;
            }
        }

        /// <summary> Liefert die Message-ID der Nachricht.
        /// </summary>
        /// <value> Message-ID
        /// </value>		
        public string MessageId
        {
            get
            {
                if (_processCardBundle == null)
                {
                    return null;
                }
                return _processCardBundle.MessageId;
            }
            set
            {
                base.MessageId = value;
            }
        }



        /// <summary> Diese Methode registriert (statisch) eine OSCIDataSource-Implementierung,
        /// die für die Speicherung aller eingehenden Nachrichten dieses Typs
        /// verwendet wird. Beim Empfang einer Nachricht wird vom registrierten
        /// OSCIDataSource-Objekt eine neue Instanz geholt (OSCIDataSource.newInstance())
        /// und die Nachricht (der eingehende Bytestrom) in deren OutputStream geschrieben.
        /// </summary>
        /// <seealso cref="OsciDataSource">
        /// </seealso>		
        public static OsciDataSource InputDataSourceImpl
        {
            set
            {
                _inputDataSourceImpl = value;
            }
        }

        /// <summary> Liefert die Instanz des registrierten OSCIDataSource-Objektes, welches
        /// für die Speicherung der Nachricht beim Empfang verwendet wurde.
        /// Die Methode liefert null, wenn keine OSCIDataSource-Implementierung
        /// registriert wurde.
        /// </summary>
        /// <value> Instanz von OSCIDataSource
        /// </value>
        public OsciDataSource InputDataSource
        {
            get
            {
                return _inputDataSource;
            }
        }

        private static OsciDataSource _inputDataSourceImpl;
        private OsciDataSource _inputDataSource;

    }
}