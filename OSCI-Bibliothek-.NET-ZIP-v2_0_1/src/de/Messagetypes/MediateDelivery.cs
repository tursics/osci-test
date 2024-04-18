using System;
using Osci.Common;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Abwicklungsauftrag</H4></p>
    /// Mit dieser Klasse werden Nachrichtenobjekte für Abwicklungsaufträge
    /// angelegt. Clients erhalten als Antwort auf diese Nachricht vom Intermediär
    /// ein ResponseToMediateDelivery-Nachrichtenobjekt, welches eine Rückmeldung über den
    /// Erfolg der Operation (getFeedback()) und ggf. Inhaltsdaten vom Empfänger enthält.
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
    /// <seealso cref="ResponseToMediateDelivery">
    /// </seealso>
    public class MediateDelivery 
        : OsciRequest
        , IContentPackage
    {
        private string _subject;

        /// <summary>Ruft die gewünschte Qualität des Zeitstempels, 
        /// mit dem der Intermediär den Eingang des Auftrags im Laufzettel protokolliert ab,
        /// oder setzt diese.
        /// </summary>
        /// <value> <b>true</b>: kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst. 
        /// <p><b>false</b>: Einfacher Zeitstempel, default
        /// (lokale Rechnerzeit des Intermedärs).</p>
        /// </value>
        /// <seealso cref="QualityOfTimeStampCreation()">
        /// </seealso>
        public bool QualityOfTimeStampCreation
        {
            get
            {
                return QualityOfTimestampTypeCreation.QualityCryptographic;
            }
            set
            {
                QualityOfTimestampTypeCreation = new QualityOfTimestampH(false, value);
            }
        }

        /// <summary>Ruft die gewünschte Qualität des Zeitstempels, 
        /// mit dem der Intermediär den Empfang der Bearbeitungsantwort im Laufzettel protokolliert ab, 
        /// oder setzt diese.
        /// </summary>
        /// <value> <b>true</b>: kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst. 
        /// <p><b>false</b>: Einfacher Zeitstempel, default
        /// (lokale Rechnerzeit des Intermedärs).</p>
        /// </value>
        /// <seealso cref="QualityOfTimeStampReception()">
        /// </seealso>
        public bool QualityOfTimeStampReception
        {
            get
            {
                return QualityOfTimestampTypeReception.QualityCryptographic;
            }
            set
            {
                QualityOfTimestampTypeReception = new QualityOfTimestampH(true, value);
            }
        }

        /// <summary> Ruft die Adresse des Empfängers der Inhaltsdaten ab, oder setzt diese.
        /// </summary>
        /// <value> Adresse (URI)
        /// </value>
        public string ContentReceiver
        {
            get
            {
                return UriReceiver;
            }
            set
            {
                UriReceiver = value;
            }
        }

        /// <summary> Ruft den Betreff-Eintrag im Laufzettel ab, oder setzt diesen.
        /// </summary>
        /// <value> Betreff der Zustellung
        /// </value>
        public string Subject
        {
            get
            {
                return _subject;
            }
            set
            {
                _subject = value;
            }
        }

        /// <summary> Ruftt die Message-ID der Nachricht ab, oder setzt diese.
        /// </summary>
        /// <value>Message-ID
        /// </value>
        virtual public string MessageId
        {
            get
            {
                return ((OsciMessage) this).MessageId;
            }
            set
            {
                ((OsciMessage) this).MessageId = value;
            }
        }

        /// <summary> Legt ein Nachrichtenobjekt für einen Abwicklungsauftrag an.
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt des Dialogs, innerhalb dessen die Nachricht
        /// versendet werden soll.
        /// </param>
        /// <seealso cref="DialogHandler">
        /// </seealso>
        public MediateDelivery(DialogHandler dh, Addressee addressee, string uriReceiver) : base(dh)
        {
            MessageType = MediateDelivery;
            Originator = ((Originator)dh.Client);
            Addressee = addressee;
            this.UriReceiver = uriReceiver;

            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.SequenceNumber = DialogHandler.Controlblock.SequenceNumber + 1;
            QualityOfTimeStampCreation = false;
            QualityOfTimeStampReception = false;
        }

        internal MediateDelivery()
        {
            MessageType = MediateDelivery;
        }

        /// <summary> Versendet die Nachricht und liefert die Antwortnachricht zurück.
        /// Diese Methode wirft eine Exception, wenn beim Aufbau oder Versand der
        /// Nachricht ein Fehler auftritt. Fehlermeldungen vom Intermediär müssen
        /// dem Feedback der Antwortnachricht entnommen werden.
        /// </summary>
        /// <returns> das Antwortnachricht-Objekt
        /// </returns>
        // --> Storestreams:
        public virtual ResponseToMediateDelivery Send()
        {
            return (ResponseToMediateDelivery)Transmit(null, null);
        }

        public virtual ResponseToMediateDelivery Send(System.IO.Stream storeOutput, System.IO.Stream storeInput)
        {
            return (ResponseToMediateDelivery)Transmit(storeOutput, storeInput);
        }
        
        public override void Compose()
        {
            base.Compose();
            if (DialogHandler.Controlblock.Challenge == null)
            {
                throw new SystemException("Kein Challenge-Wert in DialogHandler eingestellt.");
            }
            if (DialogHandler.Controlblock.Response == null)
            {
                {
                    throw new SystemException("Kein Response-Wert in DialogHandler eingestellt.");
                }
            }
            if (DialogHandler.Controlblock.ConversationId == null)
            {
                throw new SystemException("Keine Conversation-ID in DialogHandler eingestellt.");
            }
            if (DialogHandler.Controlblock.SequenceNumber == -1)
            {
                throw new SystemException("Kein Squenznummer in DialogHandler eingestellt.");
            }

            ImportAllCertificates();
            CreateNonIntermediaryCertificatesH();

            if (UriReceiver == null)
            {
                throw new SystemException("Kein URI für ContentReceiver eingestellt.");
            }
            string head = "<" + OsciNsPrefix + ":ContentReceiver URI=\"" + UriReceiver +
                "\"></" + OsciNsPrefix + ":ContentReceiver>";
            if (((OsciMessage) this).MessageId != null)
            {
                head += "<" + OsciNsPrefix + ":MessageId>" + Base64.Encode(((OsciMessage) this).MessageId.ToByteArray()) + "</" + OsciNsPrefix + ":MessageId>";
                if (_subject != null)
                {
                    head += "<" + OsciNsPrefix + ":Subject>" + _subject + "</" + OsciNsPrefix + ":Subject>";
                }
            }
            else
            {
                QualityOfTimestampTypeCreation = null;
                QualityOfTimestampTypeReception = null;
            }

            OsciH = new OsciH("mediateDelivery", head);
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
            if (((OsciMessage) this).MessageId != null)
            {
                MessagePartsFactory.WriteXml(QualityOfTimestampTypeCreation, outRenamed);
                MessagePartsFactory.WriteXml(QualityOfTimestampTypeReception, outRenamed);
            }
            OsciH.WriteXml(outRenamed);
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