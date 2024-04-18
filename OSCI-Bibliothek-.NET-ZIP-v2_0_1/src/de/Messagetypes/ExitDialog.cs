using Osci.Common;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Dialogende-Nachrichtenobjekt</H4></p>
    /// Diese Klasse dient der Beendigung eines expliziten Dialogs.
    /// Clients erhalten als Antwort auf diese Nachricht vom Intermediär ein
    /// Nachrichtenobjekt, welches eine Rückmeldung über den Erfolg der Operation
    /// enthält. Diese Rückmeldung wird von der send()-Methode ausgewertet, so
    /// daß eine Behandlung der Anwortnachricht selbst nicht erforderlich ist.
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
    /// <seealso cref="DialogHandler">
    /// </seealso>
    public class ExitDialog 
        : OsciRequest
    {
        /// <summary> Legt ein Nachrichtenobjekt zur Dialogbeendigung an.
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt, welches den expliziten Dialog repräsentiert.
        /// </param>
        /// <seealso cref="DialogHandler">
        /// </seealso>
        /// 
        public ExitDialog(DialogHandler dh) 
            : base(dh)
        {
            MessageType = ExitDialog;
            Originator = (Originator)dh.Client;

            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.SequenceNumber = DialogHandler.Controlblock.SequenceNumber + 1;
        }

        internal ExitDialog()
        {
            MessageType = ExitDialog;
        }

        /// <summary> Versendet die Nachricht und liefert die Antwortnachricht zurück.
        /// Diese Methode wirft eine Exception, wenn beim Aufbau oder Versand der
        /// Nachricht ein Fehler auftritt. Fehlermeldungen vom Intermediär müssen
        /// dem Feedback der Antwortnachricht entnommen werden.
        /// </summary>
        /// <returns> Antwortnachricht-Objekt
        /// </returns>
        // --> Storestreams:
        public ResponseToExitDialog Send()
        {
            return (ResponseToExitDialog)Transmit(null, null);
        }
        // <-- Storestreams:

        public override void Compose()
        {
            base.Compose();
            if (DialogHandler.Controlblock.Challenge == null)
            {
                throw new System.SystemException("Kein Challenge-Wert in DialogHandler eingestellt.");
            }
            if (DialogHandler.Controlblock.Response == null)
            {
                throw new System.SystemException("Kein Response-Wert in DialogHandler eingestellt.");
            }
            if (DialogHandler.Controlblock.ConversationId == null)
            {
                throw new System.SystemException("Keine Conversation-ID in DialogHandler eingestellt.");
            }
            if (DialogHandler.Controlblock.SequenceNumber == -1)
            {
                throw new System.SystemException("Kein Squenznummer in DialogHandler eingestellt.");
            }
            Body = new Body("<" + OsciNsPrefix + ":exitDialog></" + OsciNsPrefix + ":exitDialog>");

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