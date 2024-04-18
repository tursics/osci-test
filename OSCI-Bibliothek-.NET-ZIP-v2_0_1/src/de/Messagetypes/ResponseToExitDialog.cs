using Osci.Common;
using Osci.MessageParts;
using Osci.Roles;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Dialogendeantwort-Nachrichtenobjekt</H4></p>
    /// Dieses Klasse repräsentiert die Antwortnachricht auf die Beendigung
    /// eines expliziten Dialogs dar.
    /// Clients erhalten vom Intermediär eine Instanz dieser Klasse als Antwort auf
    /// eine an den Intermediär gesendeten ExitDialog-Objekt.
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
    /// <seealso cref="ExitDialog">
    /// </seealso>
    /// <seealso cref="DialogHandler">
    /// </seealso>

    public class ResponseToExitDialog
        : OsciResponseTo
    {
        internal ResponseToExitDialog(DialogHandler dh) : base(dh)
        {
            MessageType = ResponseToExitDialog;
            Originator = (Originator)dh.Client;
            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
        }

        internal ResponseToExitDialog()
        {
            MessageType = ResponseToExitDialog;
            DialogHandler.ExplicitDialog = false;
        }

        public override void Compose()
        {
            base.Compose();
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
            DialogHandler.Controlblock.Challenge = null;
            DialogHandler.ExplicitDialog = false;
            if (FeedBack == null)
            {
                throw new System.SystemException("Kein Feedback eingestellt.");
            }
            Body = new Body("<" + OsciNsPrefix + ":responseToExitDialog>" +
                WriteFeedBack() + "</" + OsciNsPrefix + ":responseToExitDialog>");
            StateOfMessage |= StateComposed;
        }

        public override void WriteXml(System.IO.Stream outRenamed)
        {
            base.WriteXml(outRenamed);
            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(outRenamed);
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