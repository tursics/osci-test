using Osci.Common;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Zustellungsauftrag</H4></p>
    /// Mit dieser Klasse werden Nachrichtenobjekte für Zustellungsaufträge
    /// angelegt. Die Inhaltsdaten werden in Form von ContentContainer-Objekten
    /// oder (im verschlüsselten Fall) EncryptedData-Objekten in die Nachricht
    /// eingestellt. Clients erhalten als Antwort auf diese Nachricht
    /// vom Intermediär ein ResponseToStoreDelivery-Nachrichtenobjekt,
    /// welches eine Rückmeldung über den Erfolg der Operation und ggf. den
    /// über den Erfolg der Operation (getFeedback()) Laufzettel der Zustellung
    /// enthält.
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
    /// <seealso cref="ResponseToStoreDelivery">
    /// </seealso>
    public class StoreDelivery 
        : OsciRequest
        , IContentPackage
    {
        private string _subject;

        /// <summary> Ruft die gewünschte Qualität des Zeitstempels, mit dem der Intermediär
        /// den Eingang des Auftrags im Laufzettel protokolliert ab, oder legt diese fest.
        /// </summary>
        /// <value><b>true</b>: kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst.
        /// <p><b>false</b>: Einfacher Zeitstempel, default
        /// (lokale Rechnerzeit des Intermedärs).</p>
        /// </value>
        /// <seealso cref="QualityOfTimeStampCreation()">
        /// </seealso>
        public virtual bool QualityOfTimeStampCreation
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

        /// <summary> Ruft die gewünschte Qualität des Zeitstempels, mit dem der Intermediär die
        /// Empfangbestätigung der Zustellung durch den Empfänger im Laufzettel protokolliert ab,
        /// oder legt diese fest.
        /// <p>Die Empfangsbestätigung besteht in einem weiteren Auftrag, den der Empfänger
        /// nach Erhalt der Zustellungsabholantwort innerhalb desselben expliziten Dialogs
        /// an den Intermediär schickt.</p>
        /// </summary>
        /// <value><b>true</b>: kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst.
        /// <p><b>false</b>: Einfacher Zeitstempel, default
        /// (lokale Rechnerzeit des Intermedärs).</p>
        /// </value>
        /// <seealso cref="QualityOfTimeStampReception()">
        /// </seealso>
        public virtual bool QualityOfTimeStampReception
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

        /// <summary> Ruft den im Laufzettel enthaltenen Betreff-Eintrag ab, oder legt diesen fest.
        /// </summary>
        /// <value> Betreff der Zustellung.
        /// </value>
        public virtual string Subject
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

        /// <summary> Ruft die Message-ID der Nachricht ab, oder legt diese fest.
        /// </summary>
        /// <value> Message-ID
        /// </value>
        public virtual string MessageId
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

        internal StoreDelivery()
        {
            MessageType = StoreDelivery;
        }

        /// <summary> Legt ein Nachrichtenobjekt für einen Zustellungsauftrag an.
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt des Dialogs, innerhalb dessen die Nachricht
        /// versendet werden soll.
        /// </param>
        /// <seealso cref="DialogHandler">
        /// </seealso>
        public StoreDelivery(DialogHandler dh, Addressee addressee, string messageId) 
            : base(dh)
        {
            MessageType = StoreDelivery;
            Originator = ((Originator)dh.Client);
            Addressee = addressee;
            // Check, ob ein Cipherzert eingestellt wurde
            object test = addressee.CipherCertificate;

            ((OsciMessage) this).MessageId = messageId;

            if (!DialogHandler.ExplicitDialog)
            {
                DialogHandler.ResetControlBlock();
            }

            DialogHandler.Controlblock.Response = DialogHandler.Controlblock.Challenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.SequenceNumber = DialogHandler.Controlblock.SequenceNumber + 1;

            QualityOfTimeStampCreation = false;
            QualityOfTimeStampReception = false;
        }

        
        /// <summary> Versendet die Nachricht und liefert die Antwortnachricht zurück.
        /// Diese Methode wirft eine Exception, wenn beim Aufbau oder Versand der
        /// Nachricht ein Fehler auftritt. Fehlermeldungen vom Intermediär müssen
        /// dem Feedback der Antwortnachricht entnommen werden.
        /// </summary>
        /// <returns>das Antwortnachricht-Objekt
        /// </returns>
        // --> Storestreams:
        public virtual ResponseToStoreDelivery Send()
        {
            return (ResponseToStoreDelivery)Transmit(null, null);
        }
        // <-- Storestreams:

        public virtual ResponseToStoreDelivery Send(System.IO.Stream storeOutput, System.IO.Stream storeInput)
        {
            return (ResponseToStoreDelivery)Transmit(storeOutput, storeInput);
        }

        public override void Compose()
        {
            base.Compose();

            if (((OsciMessage) this).MessageId == null)
            {
                throw new System.SystemException("Keine MessageId eingestellt.");
            }
            if (DialogHandler.Controlblock.Challenge == null)
            {
                throw new System.SystemException("Kein Challenge-Wert im Control-Block eingestellt.");
            }
            if (DialogHandler.Controlblock.SequenceNumber == -1)
            {
                throw new System.SystemException("Keine Sequenznummer im Control-Block eingestellt.");
            }
            if (QualityOfTimestampTypeCreation == null)
            {
                throw new System.SystemException("Keine QualityOfTimeStamp für 'Creation' eingestellt.");
            }
            if (QualityOfTimestampTypeReception == null)
            {
                throw new System.SystemException("Keine QualityOfTimeStamp für 'Reception' eingestellt.");
            }

            string head = "<" + OsciNsPrefix + ":MessageId>" + Base64.Encode(((OsciMessage) this).MessageId.ToByteArray()) + "</" + OsciNsPrefix + ":MessageId>";
            if (_subject != null)
            {
                head += "<" + OsciNsPrefix + ":Subject>" + _subject + "</" + OsciNsPrefix + ":Subject>";
            }
            OsciH = new OsciH("storeDelivery", head);
            ImportAllCertificates();
            CreateNonIntermediaryCertificatesH();
            Addressee generatedAux = NonIntermediaryCertificatesH.CipherCertificateAddressee; // Test
            Body = new Body(ContentContainer, EncryptedData);

            StateOfMessage |= StateComposed;
        }

        public override void WriteXml(System.IO.Stream stream)
        {
            base.WriteXml(stream);

            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(stream);
            }

            // DesiredLanguage
            DesiredLanguagesH.WriteXml(stream);

            QualityOfTimestampTypeCreation.WriteXml(stream);
            QualityOfTimestampTypeReception.WriteXml(stream);
            OsciH.WriteXml(stream);
            NonIntermediaryCertificatesH.WriteXml(stream);
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(stream);
            }

            CompleteMessage(stream);
        }
    }
}