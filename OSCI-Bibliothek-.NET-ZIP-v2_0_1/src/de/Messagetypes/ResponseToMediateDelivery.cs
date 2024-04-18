using System.IO;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Abwicklungsantwort-Nachrichtenobjekt</H4></p>
    /// Diese Klasse repräsentiert die Antwort des Intermediärs auf einen
    /// Abwicklungsauftrag.
    /// Clients erhalten vom Intermediär eine Instanz dieser Klasse, die eine Rückmeldung
    /// über den Erfolg der Operation (getFeedback()) sowie ggf. den zum Auftrag
    /// (Abwicklungs-/Bearbeitungsauftrag) gehörenden Laufzettel, den zur Antwort
    /// (Bearbeitungs-/Abwicklungsantwort) gehörenden Laufzettel und
    /// verschlüsselte bzw. unverschlüsselte Inhaltsdaten enthält.
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
    /// <seealso cref="MediateDelivery">
    /// </seealso>
    public class ResponseToMediateDelivery
        : OsciResponseTo
        , IContentPackage
    {

        /// <summary> Diese Methode liefert den Laufzettel des Auftrags zurück oder null,
        /// wenn bei der Verarbeitung der Nachricht ein Fehler aufgetereten ist.
        /// Die Informationen im Laufzettel können auch direkt über die einzelnen
        /// getX()-Methoden ausgewertet werden.
        /// </summary>
        /// <value> Laufzettel des Auftrags als ProcessCardBundle-Objekt, im Fehlerfall null
        /// </value>
        /// <seealso cref="ProcessCardBundleReply()">
        /// </seealso>
        /// <seealso cref="TimestampCreationRequest()">
        /// </seealso>
        /// <seealso cref="TimestampForwardingRequest()">
        /// </seealso>
        /// <seealso cref="InspectionsRequest()">
        /// </seealso>
        /// <seealso cref="SubjectRequest()">
        /// </seealso>
        /// <seealso cref="RecentModificationRequest()">
        /// </seealso>
        public virtual ProcessCardBundle ProcessCardBundleRequest
        {
            get;
            internal set;
        }

        /// <summary> Diese Methode liefert den Laufzettel der Antwort zurück oder null,
        /// wenn bei der Verarbeitung der Nachricht ein Fehler aufgetereten ist.
        /// Die Informationen im Laufzettel können auch direkt über die einzelnen
        /// getX()-Methoden ausgewertet werden.
        /// </summary>
        /// <value> Laufzettel der Antwort als ProcessCardBundle-Objekt, im Fehlerfall null
        /// </value>
        /// <seealso cref="ProcessCardBundleRequest()">
        /// </seealso>
        /// <seealso cref="TimestampCreationReply()">
        /// </seealso>
        /// <seealso cref="TimestampForwardingReply()">
        /// </seealso>
        /// <seealso cref="InspectionsReply()">
        /// </seealso>
        /// <seealso cref="SubjectReply()">
        /// </seealso>
        /// <seealso cref="RecentModificationReply()">
        /// </seealso>
        /// <seealso cref="MessageId()">
        /// </seealso>
        /// <seealso cref="RecentModificationReply()">
        /// </seealso>
        public virtual ProcessCardBundle ProcessCardBundleReply
        {
            get;
            internal set;
        }

        /// <summary> Liefert den im Auftragslaufzettel enthaltenen Zeitstempel vom Zeitpunkt des Eingangs
        /// des Abwicklungsauftrags beim Intermediär.
        /// </summary>
        /// <value> Zeitstempel der Einreichung beim Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundleReply()">
        /// </seealso>
        /// <seealso cref="TimestampCreationReply()">
        /// </seealso>
        public virtual Timestamp TimestampCreationRequest
        {
            get
            {
                return ProcessCardBundleRequest == null ? null : ProcessCardBundleRequest.Creation;
            }
        }

        /// <summary> Liefert den im Antwortlaufzettel enthaltenen Zeitstempel vom Zeitpunkt des Eingangs
        /// der Bearbeitungsantwort beim Intermediär.
        /// </summary>
        /// <value> Zeitstempel der Einreichung beim Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundleReply()">
        /// </seealso>
        /// <seealso cref="TimestampCreationRequest()">
        /// </seealso>
        public virtual Timestamp TimestampCreationReply
        {
            get
            {
                if (ProcessCardBundleRequest == null)
                {
                    return null;
                }
                return ProcessCardBundleReply.Creation;
            }

        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des
        /// vollständigen Aufbaus des Bearbeitungsauftrags vom Intermediär für den Empfänger.
        /// </summary>
        /// <value> Zeitstempel der Erstellung des Bearbeitungsauftrags durch den Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundleRequest()">
        /// </seealso>
        /// <seealso cref="TimestampForwardingReply()">
        /// </seealso>
        public virtual Timestamp TimestampForwardingRequest
        {
            get
            {
                return ProcessCardBundleRequest == null ? null : ProcessCardBundleRequest.Forwarding;
            }

        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des
        /// vollständigen Aufbaus der Abwicklungsantwort vom Intermediär für den Sender.
        /// </summary>
        /// <value> Zeitstempel der Erstellung der Abwicklungsantwort durch den Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundleReply()">
        /// </seealso>
        /// <seealso cref="TimestampForwardingRequest()">
        /// </seealso>
        public virtual Timestamp TimestampForwardingReply
        {
            get
            {
                if (ProcessCardBundleRequest == null)
                {
                    return null;
                }
                return ProcessCardBundleReply.Forwarding;
            }

        }

        /// <summary> Liefert den im Auftragslaufzettel enthaltenen Zeitstempel vom Zeitpunkt des
        /// Eingangs einer positiven Bearbeitungsantwort vom Empfänger
        /// beim Intermediär.
        /// </summary>
        /// <value> Zeitstempel der Registrierung einer Empfangsbestätigung (Bearbeitungsantwort)
        /// durch den Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundleRequest()">
        /// </seealso>
        /// <seealso cref="TimestampForwardingRequest()">
        /// </seealso>
        public virtual Timestamp TimestampReceptionRequest
        {
            get
            {
                return ProcessCardBundleRequest == null ? null : ProcessCardBundleRequest.Reception;
            }

        }

        /// <summary> Liefert die Ergebnisse der Zertifikatsprüfungen des Abwicklungsauftrags in
        /// Form von Inspection-Objekten, die im Laufzettel des Auftrags enthalten sind.
        /// </summary>
        /// <value> Prüfergebnisse
        /// </value>
        /// <seealso cref="ProcessCardBundleRequest()">
        /// </seealso>
        /// <seealso cref="InspectionsReply()">
        /// </seealso>
        public virtual Inspection[] InspectionsRequest
        {
            get
            {
                return ProcessCardBundleRequest == null ? null : ProcessCardBundleRequest.Inspections;
            }

        }

        /// <summary> Liefert die Ergebnisse der Zertifikatsprüfungen des Abwicklungsauftrags in
        /// Form von Inspection-Objekten, die im Laufzettel der Antwort enthalten sind.
        /// </summary>
        /// <value> Prüfergebnisse
        /// </value>
        /// <seealso cref="ProcessCardBundleReply()">
        /// </seealso>
        /// <seealso cref="InspectionsRequest()">
        /// </seealso>
        public virtual Inspection[] InspectionsReply
        {
            get
            {
                return ProcessCardBundleRequest == null ? null : ProcessCardBundleReply.Inspections;
            }

        }

        /// <summary> Liefert den im Auftragslaufzettel enthaltenen Betreff-Eintrag.
        /// </summary>
        /// <value> Betreff der Auftragsnachricht
        /// </value>
        /// <seealso cref="ProcessCardBundleRequest()">
        /// </seealso>
        /// <seealso cref="SubjectReply()">
        /// </seealso>
        public virtual string SubjectRequest
        {
            get
            {
                return ProcessCardBundleRequest == null ? null : ProcessCardBundleRequest.Subject;
            }

        }

        /// <summary> Liefert den im Antwortlaufzettel enthaltenen Betreff-Eintrag.
        /// </summary>
        /// <value> Betreff der Antwortnachricht
        /// </value>
        /// <seealso cref="ProcessCardBundleRequest()">
        /// </seealso>
        /// <seealso cref="SubjectRequest()">
        /// </seealso>
        /// <deprecated> Im Zuge der Erweiterung des Interfaces (Version 1.0.3)</deprecated>
        public virtual string SubjectReply
        {
            get
            {
                return ProcessCardBundleRequest == null ? null : ProcessCardBundleReply.Subject;
            }
        }

        /// <summary> Liefert den im Antwortlaufzettel enthaltenen Betreff-Eintrag.
        /// </summary>
        /// <value> Betreff der Antwortnachricht
        /// </value>
        /// <seealso cref="ProcessCardBundleRequest()">
        /// </seealso>
        /// <seealso cref="SubjectRequest">
        /// </seealso>
        public string Subject
        {
            get
            {
                if (ProcessCardBundleRequest == null)
                {
                    return null;
                }
                return ProcessCardBundleReply.Subject;
            }
        }

        /// <summary> Liefert das Datum der letzten Änderung des Auftragslaufzettels. Das Format
        /// entspricht dem XML-Schema nach <a href="http://www.w3.org/TR/xmlschema-2/#dateTime">
        /// http://www.w3.org/TR/xmlschema-2/#dateTime</a>.
        /// </summary>
        /// <value> Datum der letzten Änderung.
        /// </value>
        /// <seealso cref="ProcessCardBundleRequest()">
        /// </seealso>
        /// <seealso cref="RecentModificationReply()">
        /// </seealso>
        public virtual string RecentModificationRequest
        {
            get
            {
                return ProcessCardBundleRequest == null ? null : ProcessCardBundleRequest.RecentModification;
            }
        }

        /// <summary> Liefert das Datum der letzten Änderung des Antwortlaufzettels. Das Format
        /// entspricht dem XML-Schema nach <a href="http://www.w3.org/TR/xmlschema-2/#dateTime">
        /// http://www.w3.org/TR/xmlschema-2/#dateTime</a>.
        /// </summary>
        /// <value> Datum der letzten Änderung.
        /// </value>
        /// <seealso cref="ProcessCardBundleRequest()">
        /// </seealso>
        /// <seealso cref="RecentModificationRequest()">
        /// </seealso>
        public virtual string RecentModificationReply
        {
            get
            {
                if (ProcessCardBundleRequest == null)
                {
                    return null;
                }

                return ProcessCardBundleReply.RecentModification;
            }
        }

        /// <summary> Liefert die Message-ID der Nachricht (Antwort).
        /// </summary>
        /// <value> Message-ID
        /// </value>
        /// <deprecated> Im Zuge der Erweiterung des Interfaces (Version 1.0.3) * de.osci.osci12.messagetypes.ContentPackageI ersetzt durch getMessageId() </deprecated>
        public virtual string MessageIdReply
        {
            get
            {
                if (ProcessCardBundleRequest == null)
                {
                    return null;
                }
                return ProcessCardBundleReply.MessageId;
            }
        }
        /// <summary> Liefert die Message-ID der Nachricht (Antwort).
        /// </summary>
        /// <value> Message-ID
        /// </value>
        public virtual string MessageId
        {
            get
            {
                if (ProcessCardBundleRequest == null)
                {
                    return null;
                }
                return ProcessCardBundleReply.MessageId;
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
        public virtual OsciDataSource InputDataSource
        {
            get
            {
                return _inputDataSource;
            }

        }

        private static OsciDataSource _inputDataSourceImpl;
        private OsciDataSource _inputDataSource;

        public ResponseToMediateDelivery(DialogHandler dh) : base(dh)
        {
            MessageType = ResponseToMediateDelivery;
            // Weil in diesem Szenario die Rollen wechseln (der Orginator der Anfrage wird zum Addressee der Antwort
            // und umgekehrt) werden die Rollenobjekte umgebaut.
            Signer signer = null;
            Decrypter decrypter = null;
            if (DialogHandler.Client.HasSignaturePrivateKey())
            {
                signer = DialogHandler.Client.Signer;
            }
            if (DialogHandler.Client.HasCipherPrivateKey())
            {
                decrypter = DialogHandler.Client.Decrypter;
            }
            Addressee = new Addressee(signer, decrypter);
        }

        internal ResponseToMediateDelivery(MediateDelivery medDel, ResponseToProcessDelivery rspProcDel) : base(medDel.DialogHandler)
        {
            MessageType = ResponseToMediateDelivery;
            Addressee = rspProcDel.Addressee;
            Originator = rspProcDel.Originator;
            ContentContainer[] con = rspProcDel.ContentContainer;
            for (int i = 0; i < con.Length; i++)
            {
                AddContentContainer(con[i]);
            }
            EncryptedDataOsci[] enc = rspProcDel.EncryptedData;
            for (int i = 0; i < enc.Length; i++)
            {
                AddEncryptedData(enc[i]);
            }
            Attachment[] att = rspProcDel.Attachments;
            for (int i = 0; i < att.Length; i++)
            {
                AddAttachment(att[i]);
            }
            Author[] auth = rspProcDel.OtherAuthors;
            for (int i = 0; i < auth.Length; i++)
                OtherAutorsList[auth[i].Id] = auth[i];
            Reader[] read = rspProcDel.OtherReaders;
            for (int i = 0; i < read.Length; i++)
                OtherReadersList[read[i].Id] = read[i];
            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
        }

        public override void Compose()
        {
            base.Compose();
            string head = WriteFeedBack();
            if (ProcessCardBundleRequest != null)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    ProcessCardBundleRequest.WriteXml(memoryStream);
                    if (ProcessCardBundleReply != null)
                    {
                        ProcessCardBundleReply.WriteXml(memoryStream);
                    }
                    head += memoryStream.AsString();
                }
            }
            OsciH = new OsciH("responseToMediateDelivery", head);
            ImportAllCertificates();
            CreateNonIntermediaryCertificatesH();
            Body = new Body(ContentContainer, EncryptedData);
            StateOfMessage |= StateComposed;
        }

        public override void WriteXml(Stream stream)
        {
            base.WriteXml(stream);
            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(stream);
            }
            OsciH.WriteXml(stream);
            if (IntermediaryCertificatesH != null)
            {
                IntermediaryCertificatesH.WriteXml(stream);
            }
            if (NonIntermediaryCertificatesH != null)
            {
                NonIntermediaryCertificatesH.WriteXml(stream);
            }
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(stream);
            }
            CompleteMessage(stream);
        }

        internal void SetMessageIdRequest(string messageId)
        {
            ((OsciMessage)this).MessageId = messageId;
        }
    }
}