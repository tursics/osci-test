using Osci.Common;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>MessageId-Anforderungsauftrag</H4></p>
    /// Mit dieser Klasse werden Nachrichtenobjekte zur Anforderung einer
    /// MessageId angelegt. Clients erhalten als Antwort auf diese Nachricht
    /// vom Intermediär ein Nachrichtenobjekt, welches eine Rückmeldung über den
    /// Erfolg der Operation und ggf. die angeforderte MessageId enthält.
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
    /// <seealso cref="ResponseToGetMessageId">
    /// </seealso>
    public class GetMessageId
        : OsciRequest
    {
        /// <summary> Legt ein Nachrichtenobjekt zur Anforderung einer MessageId an.
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt des Dialogs, innerhalb dessen die Nachricht
        /// versendet werden soll.
        /// </param>
        /// <seealso cref="DialogHandler">
        /// </seealso>
        public GetMessageId(DialogHandler dh) 
            : base(dh)
        {
            MessageType = GetMessageId;
            Originator = (Originator)dh.Client;
            if (!DialogHandler.ExplicitDialog)
            {
                DialogHandler.ResetControlBlock();
            }
            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.SequenceNumber = DialogHandler.Controlblock.SequenceNumber + 1;
        }

        internal GetMessageId()
        {
            MessageType = GetMessageId;
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
            if (DialogHandler.Client.HasCipherCertificate())
            {
                object dumm = DialogHandler.Client.CipherCertificate;
                if (NonIntermediaryCertificatesH == null)
                {
                    NonIntermediaryCertificatesH = new NonIntermediaryCertificatesH();
                }
                NonIntermediaryCertificatesH.CipherCertificateOriginator = (Originator)DialogHandler.Client;
            }

            Body = new Body("<" + OsciNsPrefix + ":getMessageId></" + OsciNsPrefix + ":getMessageId>");
            StateOfMessage |= StateComposed;
        }

        /// <summary> Versendet die Nachricht und liefert die Antwortnachricht zurück.
        /// Diese Methode wirft eine Exception, wenn beim Aufbau oder Versand der
        /// Nachricht ein Fehler auftritt. Fehlermeldungen vom Intermediär müssen
        /// dem Feedback der Antwortnachricht entnommen werden.
        /// </summary>
        /// <returns> das Antwortnachricht-Objekt
        /// </returns>
        // --> Storestreams:
        public ResponseToGetMessageId Send(System.IO.Stream outp, System.IO.Stream inp)
        {
            return (ResponseToGetMessageId)Transmit(outp, inp);
        }
        public ResponseToGetMessageId Send()
        {
            return (ResponseToGetMessageId)Transmit(null, null);
        }
        // <-- Storestreams:

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