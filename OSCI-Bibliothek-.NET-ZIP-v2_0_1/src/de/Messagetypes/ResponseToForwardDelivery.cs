using Osci.Common;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Weiterleitungsantwort-Nachrichtenobjekt</H4></p>
    /// Dieses Klasse repräsentiert die Antwort des Intermediärs auf einen
    /// Weiterleitungsauftrag.
    /// Clients erhalten vom Intermediär eine Instanz dieser Klasse, die eine Rückmeldung
    /// über den Erfolg der Operation (getFeedback()) sowie ggf. den zugehörigen
    /// Laufzettel enthält.
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
    /// <seealso cref="ForwardDelivery">
    /// </seealso>
    public class ResponseToForwardDelivery
        : OsciResponseTo
    {
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
        /// <seealso cref="TimestampReception()">
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
            get; set;
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des Eingangs
        /// des Weiterleitungsauftrags beim Intermediär.
        /// </summary>
        /// <value> Zeitstempel der Einreichung beim Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public Timestamp TimestampCreation
        {
            get
            {
                return ProcessCardBundle.Creation;
            }
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des
        /// vollständigen Aufbaus des Annahmeauftrags vom Intermediär für den Empfänger.
        /// </summary>
        /// <value> Zeitstempel der Erstellung des Annahmeauftrags durch den Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public Timestamp TimestampForwarding
        {
            get
            {
                return ProcessCardBundle.Forwarding;
            }
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des
        /// Eingangs einer positiven Annahmeantwort vom Empfänger beim Intermediär.
        /// </summary>
        /// <value> Zeitstempel der Registrierung einer Empfangsbestätigung (Annahmeantwort)
        /// durch den Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public Timestamp TimestampReception
        {
            get
            {
                return ProcessCardBundle.Reception;
            }
        }

        /// <summary> Liefert die Ergebnisse der Zertifikatsprüfungen in Form von Inspection-Objekten,
        /// die im ProcessCardBundle-Objekt enthalten sind.
        /// </summary>
        /// <value> Prüfergebnisse
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public Inspection[] Inspections
        {
            get
            {
                return ProcessCardBundle.Inspections;
            }
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Betreff-Eintrag.
        /// </summary>
        /// <value> Betreff der Zustellung
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public string Subject
        {
            //  void setInspections(Inspection[] inspections) {this.inspections = inspections;}
            get
            {
                return ProcessCardBundle.Subject;
            }
        }

        /// <summary> Liefert das Datum der letzten Änderung des Laufzettels. Das Format
        /// entspricht dem XML-Schema nach http://www.w3.org/TR/xmlschema-2/#dateTime
        /// </summary>
        /// <value> Datum der letzten Änderung.
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public string RecentModification
        {
            get
            {
                return ProcessCardBundle.RecentModification;
            }
        }

        /// <summary> Liefert die Message-ID der Nachricht.
        /// </summary>
        /// <value> Message-ID
        /// </value>
        /// <seealso cref="ProcessCardBundle"> 
        /// </seealso>
        public string MessageId
        {
            get
            {
                return ProcessCardBundle.MessageId;
            }
        }

        /// <summary> Diese Methode registriert (statisch) eine OSCIDataSource-Implementierung,
        /// die für die Speicherung aller eingehenden Nachrichten dieses Typs
        /// verwendet wird. Beim Empfang einer Nachricht wird vom registrierten
        /// OSCIDataSource-Objekt eine neue Instanz geholt (OSCIDataSource.newInstance())
        /// und die Nachricht (der eingehende Bytestrom) in deren OutputStream geschrieben.
        /// </summary>
        /// <value></value>
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

        private static readonly Log _log = LogFactory.GetLog(typeof(ResponseToForwardDelivery));
        private static OsciDataSource _inputDataSourceImpl;
        private OsciDataSource _inputDataSource;

        /// <summary> Dieser Konstruktor ist für den Intermediär
        /// </summary>
        /// <param name="accDel">
        /// </param>
        /// <param name="rspAccDel">
        /// </param>
        internal ResponseToForwardDelivery(AcceptDelivery accDel, ResponseToAcceptDelivery rspAccDel) : base(accDel.FwdDh)
        {
            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            if (DialogHandler.Controlblock.SequenceNumber == -1)
            {
                DialogHandler.Controlblock.SequenceNumber = 0;
            }
            FeedBack = rspAccDel.FeedBack;
            Body = new Body("");
            Body.SetNamespacePrefixes(this);
        }

        internal ResponseToForwardDelivery(DialogHandler dialogHandler) 
            : base(dialogHandler)
        {
            MessageType = ResponseToForwardDelivery;
            /// <summary> @todo Supplier oder Client??
            /// </summary>
            Body = new Body("");
            Body.SetNamespacePrefixes(this);
        }

        /// <summary> Liefert die erstellte OSCI-Nachricht als Stream zur Übertragung an den Empfänger.
        /// </summary>
        /// <param name="out_Renamed">der Outputstream in den serialisiert werden soll
        /// </param>
        public virtual void GetMessageAsStream(System.IO.Stream outRenamed)
        {
            WriteXml(outRenamed);
        }

        public override void Compose()
        {
            base.Compose();
            if (ProcessCardBundle == null)
            {
                OsciH = new OsciH("responseToForwardDelivery", WriteFeedBack());
            }
            else
            {
                System.IO.MemoryStream outRenamed = new System.IO.MemoryStream();
                ProcessCardBundle.WriteXml(outRenamed);
                char[] tmpChar;
                byte[] tmpByte;
                tmpByte = outRenamed.GetBuffer();
                tmpChar = new char[outRenamed.Length];
                System.Array.Copy(tmpByte, 0, tmpChar, 0, tmpChar.Length);
                OsciH = new OsciH("responseToForwardDelivery", WriteFeedBack() + new string(tmpChar));
            }
            StateOfMessage |= StateComposed;
        }

        public override void WriteXml(System.IO.Stream outRenamed)
        {
            _log.Debug("CreateMsg child");
            base.WriteXml(outRenamed);
            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(outRenamed);
            }
            OsciH.WriteXml(outRenamed);
            if (IntermediaryCertificatesH != null)
            {
                IntermediaryCertificatesH.WriteXml(outRenamed);
            }
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(outRenamed);
            }
            CompleteMessage(outRenamed);
        }
    }
}