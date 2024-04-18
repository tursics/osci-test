using System;
using Osci.Common;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Annahmeauftrag</H4></p>
    /// Diese Klasse repräsentiert Nachrichtenobjekte für Annahmeaufträge.
    /// Der Intermediär erzeugt nach dem Erhalt eines Weiterleitungsauftrags eine
    /// Instanz dieser Klasse und sendet die Nachricht an den Empfänger (hier als Supplier).
    /// Als Antwort auf diese Nachricht muß der Empfänger ein
    /// ResponseToAcceptDelivery-Nachrichtenobjekt mit einer Rückmeldung (Feedback)
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
    /// <seealso cref="ResponseToAcceptDelivery">
    /// </seealso>
    public class AcceptDelivery 
        : OsciRequest
        , IContentPackage
    {

        /// <summary> Diese Methode liefert den Laufzettel der Zustellung zurück oder null,
        /// wenn bei der Verarbeitung der Nachricht ein Fehler aufgetreten ist.
        /// Die Informationen im Laufzettel können auch direkt über die einzelnen
        /// getX()-Methoden ausgewertet werden.
        /// </summary>
        /// <value> Der Laufzettel als ProcessCardBundle-Objekt (im Fehlerfall null)
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
        public virtual ProcessCardBundle ProcessCardBundle
        {
            get;
            internal set;
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des Eingangs
        /// des Weiterleitungsauftrags beim Intermediär.
        /// </summary>
        /// <value> Der Zeitstempel der Einreichung beim Intermediär.
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public virtual Timestamp TimestampCreation
        {
            get
            {
                return ProcessCardBundle.Creation;
            }
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des
        /// vollständigen Aufbaus des Annahmeauftrags vom Intermediär für den Empfänger.
        /// </summary>
        /// <value> Der Zeitstempel der Erstellung des Annahmeauftrags durch den Intermediär.
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public virtual Timestamp TimestampForwarding
        {
            get
            {
                return ProcessCardBundle.Forwarding;
            }
        }

        /// <summary> Liefert die Ergebnisse der Zertifikatsprüfungen in Form von Inspection-Objekten,
        /// die im ProcessCardBundle-Objekt enthalten sind.
        /// </summary>
        /// <value> Die Prüfergebnisse.
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public virtual Inspection[] Inspections
        {
            get
            {
                return ProcessCardBundle.Inspections;
            }
        }

        /// <summary> Liefert den Betreff der Nachricht.
        /// </summary>
        /// <value> Der Betreff der Zustellung.
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public virtual string Subject
        {
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
        public virtual string RecentModification
        {
            get
            {
                return ProcessCardBundle.RecentModification;
            }
        }

        /// <summary> Liefert die Message-ID der Nachricht.
        /// </summary>
        /// <value> Die Message-ID.
        /// </value>
        public virtual string MessageId
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
        /// <value>inputDataSourceImpl
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
        /// <value> Instanz von OSCIDataSource.
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
        public DialogHandler FwdDh;

        /// <summary> Konstruktor für den Supplier Parser
        /// </summary>
        /// <param name="dh">dh
        /// </param>
        internal AcceptDelivery(DialogHandler dh) : base(dh)
        {
            MessageType = AcceptDelivery;
            Addressee = (Addressee)dh.Supplier;
        }

        internal AcceptDelivery()
        {
            MessageType = AcceptDelivery;
        }

        /// <summary> Konstruktor für den Intermediär
        /// </summary>
        /// <param name="fd">ForwardDelivery
        /// </param>
        internal AcceptDelivery(ForwardDelivery fd)
            : base(new DialogHandler((Intermed)fd.DialogHandler.Supplier, fd.Addressee, null))
        {
            MessageType = AcceptDelivery;
            UriReceiver = fd.UriReceiver;
            Addressee = fd.Addressee;
            Originator = fd.Originator;
            DesiredLanguagesH = new DesiredLanguagesH(fd.DesiredLanguagesH.LanguageList);

            ContentContainer[] con = fd.ContentContainer;
            for (int i = 0; i < con.Length; i++)
            {
                AddContentContainer(con[i]);
            }

            EncryptedDataOsci[] enc = fd.EncryptedData;
            for (int i = 0; i < enc.Length; i++)
            {
                AddEncryptedData(enc[i]);
            }

            Attachment[] att = fd.Attachments;
            for (int i = 0; i < att.Length; i++)
            {
                AddAttachment(att[i]);
            }
            for (int i = 0; i < fd.OtherAuthors.Length; i++)
            {
                OtherAutorsList[fd.OtherAuthors[i].Id] = fd.OtherAuthors[i];
            }
            for (int i = 0; i < fd.OtherReaders.Length; i++)
            {
                OtherReadersList[fd.OtherReaders[i].Id] = fd.OtherReaders[i];
            }

            DialogHandler.Encryption = fd.DialogHandler.Encryption;
            DialogHandler.Controlblock.Response = null;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.ConversationId = null;
            DialogHandler.Controlblock.SequenceNumber = -1;
            FwdDh = fd.DialogHandler;
        }

        /// <summary> Versendet die Nachricht und liefert die Antwortnachricht zurück.
        /// Diese Methode wirft eine Exception, wenn beim Aufbau oder Versand der
        /// Nachricht ein Fehler auftritt. Fehlermeldungen vom Intermediär müssen
        /// dem Feedback der Antwortnachricht entnommen werden.
        /// </summary>
        /// <returns> das Antwortnachricht-Objekt
        /// </returns>
        // <-- Storestreams:
        internal virtual ResponseToAcceptDelivery Send()
        {
            return (ResponseToAcceptDelivery)Transmit(null, null);
        }

        public override void Compose()
        {
            base.Compose();
            if (ProcessCardBundle == null)
            {
                throw new SystemException("Kein Laufzettel eingestellt.");
            }
            System.IO.MemoryStream outRenamed = new System.IO.MemoryStream();
            ProcessCardBundle.WriteXml(outRenamed);

            byte[] tmpByte = outRenamed.GetBuffer();
            char[] tmpChar = new char[outRenamed.Length];
            Array.Copy(tmpByte, 0, tmpChar, 0, tmpChar.Length);
            OsciH = new OsciH("acceptDelivery", new string(tmpChar));
            if (DialogHandler.Client.HasCipherCertificate())
            {
                if (IntermediaryCertificatesH == null)
                {
                    IntermediaryCertificatesH = new IntermediaryCertificatesH();
                }
                IntermediaryCertificatesH.CipherCertificateIntermediary = (Intermed)DialogHandler.Client;
            }
            CreateNonIntermediaryCertificatesH();

            if (Body == null)
            {
                Body = new Body(ContentContainer, EncryptedData);
            }
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
            if(FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(outRenamed);
            }
            CompleteMessage(outRenamed);
        }
    }
}