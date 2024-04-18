using Osci.Common;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Bearbeitungsauftrag</H4></p>
    /// Diese Klasse repräsentiert Nachrichtenobjekte für Bearbeitungsaufträge.
    /// Der Intermediär erzeugt nach dem Erhalt eines Abwicklungsauftrags eine
    /// Instanz dieser Klasse und sendet die Nachricht an den Empfänger (hier als Supplier).
    /// Als Antwort auf diese Nachricht muß der Empfänger ein
    /// ResponseToProcessDelivery-Nachrichtenobjekt mit einer Rückmeldung (Feedback)
    /// aufbauen und an den Intermediär zurücksenden.
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
    /// <seealso cref="ResponseToProcessDelivery">
    /// </seealso>

    public class ProcessDelivery 
        : OsciRequest
        , IContentPackage
    {

        /// <summary> Diese Methode liefert den Laufzettel der Zustellung zurück oder null,
        /// wenn bei der Verarbeitung der Nachricht ein Fehler aufgetereten ist.
        /// Die Informationen im Laufzettel können auch direkt über die einzelnen
        /// getX()-Methoden ausgewertet werden.
        /// </summary>
        /// <value> Liefert den Laufzettel als ProcessCardBundle-Objekt (im Fehlerfall null).
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
                return processCardBundle;
            }
        }


        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des Eingangs
        /// des Abwicklungsauftrags beim Intermediär.
        /// </summary>
        /// <value> Zeitstempel der Einreichung beim Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public Timestamp TimestampCreation
        {
            get
            {
                return processCardBundle.Creation;
            }
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des
        /// vollständigen Aufbaus des Bearbeitungsauftrags vom Intermediär für den Empfänger.
        /// </summary>
        /// <value> Zeitstempel der Erstellung des Bearbeitungsauftrags durch den Intermediär
        /// </value>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public Timestamp TimestampForwarding
        {
            get
            {
                return processCardBundle.Forwarding;
            }
        }

        /// <summary> Liefert die Ergebnisse der Zertifikatsprüfungen in Form von Inspection-Objekten,
        /// die im ProcessCardBundle-Objekt enthalten sind.
        /// </summary>
        /// <value>Prüfergebnisse
        /// </value>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public Inspection[] Inspections
        {
            get
            {
                return processCardBundle.Inspections;
            }
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Betreff-Eintrag.
        /// </summary>
        /// <value> Betreff der Zustellung.
        /// </value>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public string Subject
        {
            get
            {
                return processCardBundle.Subject;
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
                return processCardBundle.RecentModification;
            }
        }

        /// <summary> Liefert die Message-ID der Nachricht.
        /// </summary>
        /// <value> Message-ID
        /// </value>
        /// <seealso cref="ProcessCardBundle()"> 
        /// </seealso>
        public string MessageId
        {
            get
            {
                if (processCardBundle != null)
                {
                    return processCardBundle.MessageId;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary> Diese Methode registriert (statisch) eine OSCIDataSource-Implementierung,
        /// die für die Speicherung aller eingehenden Nachrichten dieses Typs
        /// verwendet wird. Beim Empfang einer Nachricht wird vom registrierten
        /// OSCIDataSource-Objekt eine neue Instanz geholt (OSCIDataSource.newInstance())
        /// und die Nachricht (der eingehende Bytestrom) in deren OutputStream geschrieben.
        /// </summary>
        /// <value>InputDataSourceImpl
        /// </value>
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

        internal ProcessCardBundle processCardBundle;

        private static OsciDataSource _inputDataSourceImpl;
        private OsciDataSource _inputDataSource;

        internal ProcessDelivery(MediateDelivery md) 
            : base(new DialogHandler((Intermed)md.DialogHandler.Supplier, md.Addressee, null))
        {
            MessageType = ProcessDelivery;
            UriReceiver = md.UriReceiver;
            DesiredLanguagesH = new DesiredLanguagesH(md.DesiredLanguagesH.LanguageList);
            Addressee = md.Addressee;
            Originator = md.Originator;

            ContentContainer[] con = md.ContentContainer;

            for (int i = 0; i < con.Length; i++)
            {
                AddContentContainer(con[i]);
            }
            EncryptedDataOsci[] enc = md.EncryptedData;
            for (int i = 0; i < enc.Length; i++)
            {
                AddEncryptedData(enc[i]);
            }
            Attachment[] att = md.Attachments;
            for (int i = 0; i < att.Length; i++)
            {
                AddAttachment(att[i]);
            }
            for (int i = 0; i < md.OtherAuthors.Length; i++)
                OtherAutorsList[md.OtherAuthors[i].Id] = md.OtherAuthors[i];
            for (int i = 0; i < md.OtherReaders.Length; i++)
                OtherReadersList[md.OtherReaders[i].Id] = md.OtherReaders[i];
            DialogHandler.Encryption = md.DialogHandler.Encryption;
            DialogHandler.Controlblock.Response = null;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.ConversationId = null;
            DialogHandler.Controlblock.SequenceNumber = -1;
        }

        public ProcessDelivery(DialogHandler dh) : base(dh)
        {
            Addressee = ((Addressee)dh.Supplier);
            MessageType = ProcessDelivery;
        }

        public ProcessDelivery()
        {
            MessageType = ProcessDelivery;
        }

        /**
         * Liefert die vom Intermediär mitgesendete Message-ID für die Antwortnachricht.
         * @return Message-ID für Antwortnachricht oder null, wenn keine Protokollierung
         * verlangt ist.
         */
        public string GetMessageIdResponse()
        {
            return ((OsciMessage) this).MessageId;
        }
        public override void Compose()
        {
            base.Compose();
            if (processCardBundle != null)
            {
                string pdh = "<" + OsciNsPrefix + ":MessageIdResponse>"
                    + Base64.Encode(((OsciMessage) this).MessageId.ToByteArray())
                    + "</" + OsciNsPrefix + ":MessageIdResponse>" + processCardBundle.WriteToString();

                OsciH = new OsciH("processDelivery", "" + pdh);
            }
            else
            {
                OsciH = new OsciH("processDelivery", "");
            }

            if (DialogHandler.Client.HasCipherCertificate())
            {
                // Test, ob vorhanden
                if (IntermediaryCertificatesH == null)
                {
                    IntermediaryCertificatesH = new IntermediaryCertificatesH();
                }
                IntermediaryCertificatesH.CipherCertificateIntermediary = (Intermed)DialogHandler.Client;

            }

            CreateNonIntermediaryCertificatesH();

            Body = new Body(ContentContainer, EncryptedData);
            StateOfMessage |= StateComposed;
        }

        public override void WriteXml(System.IO.Stream outRenamed)
        {
            base.WriteXml(outRenamed);

            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(outRenamed);
            }

            // DesiredLanguage
            DesiredLanguagesH.WriteXml(outRenamed);
            OsciH.WriteXml(outRenamed);
            if (IntermediaryCertificatesH != null)
            {
                IntermediaryCertificatesH.WriteXml(outRenamed);
            }
            if (NonIntermediaryCertificatesH != null)
            {
                NonIntermediaryCertificatesH.WriteXml(outRenamed);
            }
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(outRenamed);
            }
            CompleteMessage(outRenamed);
        }
    }
}