using Osci.Common;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Weiterleitungsauftrag</H4></p>
    /// Mit dieser Klasse werden Nachrichtenobjekte für Weiterleitungsaufträge
    /// angelegt. Clients erhalten als Antwort auf diese Nachricht vom Intermediär
    /// ein ResponseToForwardDelivery-Nachrichtenobjekt, welches eine Rückmeldung über den
    /// Erfolg der Operation (getFeedback()) und ggf. den Laufzettel zur gesendeten
    /// Nachricht enthält.
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
    /// <seealso cref="ResponseToForwardDelivery">
    /// </seealso>
    public class ForwardDelivery 
        : OsciRequest
        , IContentPackage
    {

        private static readonly Log _log = LogFactory.GetLog(typeof(ForwardDelivery));

        /// <summary> Ruft die gewünschte Qualität des Zeitstempels ab, 
        /// mit dem der Intermediär den Eingang des Auftrags im Laufzettel protokolliert, 
        /// oder setzt diese. 
        /// </summary>
        /// <value><b>true</b>: kryptographischer Zeitstempel von einem
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

        /// <summary>Ruft die gewünschte Qualität des Zeitstempels ab, 
        /// mit dem der Intermediär den Empfang der Annahmeantwort im Laufzettel protokolliert,
        /// oder setzt diese.
        /// </summary>
        /// <value><b>true</b>: kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst. 
        /// <p><b>false</b>: Einfacher Zeitstempel, default
        /// (lokale Rechnerzeit des Intermedärs).</p>
        /// </value>
        /// <seealso cref="QualityOfTimeStampReception()">
        /// </seealso>
        virtual public bool QualityOfTimeStampReception
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

        /// <summary>Ruft die Adresse des Empfängers der Inhaltsdaten ab, oder setzt diese.
        /// </summary>
        /// <value>Adresse
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

        /// <summary>Ruft den Betreff-Eintrag im Laufzettel ab, oder setzt diesen.
        /// </summary>
        /// <value> Betreff der Zustellung
        /// </value>
        public string Subject { get; set; }

        /// <summary> Ruft die Message-ID der Nachricht ab, oder setzt diese.
        /// </summary>
        public string MessageId
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

        /// <summary> Legt ein Nachrichtenobjekt für einen Weiterleitungsauftrag an.
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt des Dialogs, innerhalb dessen die Nachricht
        /// versendet werden soll.
        /// </param>
        /// <seealso cref="DialogHandler">
        /// </seealso>
        public ForwardDelivery(DialogHandler dh, Addressee addressee, string uriReceiver, string messageId)
            : base(dh)
        {
            if (!DialogHandler.ExplicitDialog)
            {
                DialogHandler.ResetControlBlock();
            }

            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.SequenceNumber = DialogHandler.Controlblock.SequenceNumber + 1;

            MessageType = ForwardDelivery;
            Originator = ((Originator)dh.Client);
            Addressee = addressee;
            this.UriReceiver = uriReceiver;
            ((OsciMessage) this).MessageId = messageId;

            QualityOfTimeStampCreation = false;
            QualityOfTimeStampReception = false;
        }

        /// <summary> Legt ein Nachrichtenobjekt für einen Weiterleitungsauftrag an.
        /// Dieser Konstruktor wird nur von dem Parser benutzt.
        /// </summary>
        internal ForwardDelivery(DialogHandler dh)
        {
            MessageType = ForwardDelivery;
            DialogHandler = dh;
        }

        /// <summary> Versendet die Nachricht und liefert die Antwortnachricht zurück.
        /// Diese Methode wirft eine Exception, wenn beim Aufbau oder Versand der
        /// Nachricht ein Fehler auftritt. Fehlermeldungen vom Intermediär müssen
        /// dem Feedback der Antwortnachricht entnommen werden.
        /// </summary>
        /// <returns> das Antwortnachricht-Objekt
        /// </returns>
        // --> Storestreams:		
        public virtual ResponseToForwardDelivery Send()
        {
            return (ResponseToForwardDelivery)Transmit(null, null);
        }

        public virtual ResponseToForwardDelivery Send(System.IO.Stream storeOutput, System.IO.Stream storeInput)
        {
            return (ResponseToForwardDelivery)Transmit(storeOutput, storeInput);
        }

        public override void Compose()
        {
            base.Compose();
            if (DialogHandler.Controlblock.Challenge == null)
            {
                throw new System.SystemException("Kein Challenge-Wert in DialogHandler eingestellt.");
            }
            if (DialogHandler.Controlblock.SequenceNumber == -1)
            {
                throw new System.SystemException("Kein Squenznummer in DialogHandler eingestellt.");
            }

            ImportAllCertificates();
            if (NonIntermediaryCertificatesH == null)
            {
                CreateNonIntermediaryCertificatesH();
            }
            if (UriReceiver == null)
            {
                throw new System.SystemException("Kein URI für ContentReceiver eingestellt.");
            }
            if (((OsciMessage) this).MessageId == null)
            {
                throw new System.SystemException("Keine MessageId eingestellt.");
            }

            string head = "<" + OsciNsPrefix + ":ContentReceiver URI=\"" + UriReceiver +
                "\"></" + OsciNsPrefix + ":ContentReceiver><" + OsciNsPrefix + ":MessageId>"
                + Base64.Encode(
                ((OsciMessage) this).MessageId, Constants.CharEncoding) + "</" + OsciNsPrefix + ":MessageId>";

            if (Subject != null)
            {
                head += ("<" + OsciNsPrefix + ":Subject>" + Subject + "</" + OsciNsPrefix + ":Subject>");
            }
            OsciH = new OsciH("forwardDelivery", head);
            Body = new Body(ContentContainer, EncryptedData);
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

            // DesiredLanguage
            DesiredLanguagesH.WriteXml(outRenamed);
            QualityOfTimestampTypeCreation.WriteXml(outRenamed);
            QualityOfTimestampTypeReception.WriteXml(outRenamed);
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