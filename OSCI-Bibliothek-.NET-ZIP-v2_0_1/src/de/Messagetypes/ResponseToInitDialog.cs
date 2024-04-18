using Osci.Common;
using Osci.Helper;
using Osci.MessageParts;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Dialoginitialisierungsantwort-Nachrichtenobjekt</H4></p>
    /// Diese Klasse repräsentiert die Antwortnachricht auf die Initialisierung
    /// eines expliziten Dialogs dar.
    /// Clients erhalten vom Intermediär eine Instanz dieser Klasse als Antwort auf
    /// eine an den Intermediär gesendeten InitDialog-Objekt. Da der an den Konstruktor
    /// des InitDialog-Objektes übergebene DialogHandler bereits beim Empfang der
    /// Antwortnachricht mit den empfangenen Parametern aktualisiert wird, ist dieses
    /// Objekt für Client-Anwendungen normalerweise ohne Bedeutung.
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
    /// <seealso cref="InitDialog">
    /// </seealso>
    /// <seealso cref="DialogHandler">
    /// </seealso>

    public class ResponseToInitDialog
        : OsciResponseTo
    {
        protected internal ResponseToInitDialog(DialogHandler dh) 
            : base(dh)
        {
            MessageType = ResponseToInitDialog;
            DialogHandler.ExplicitDialog = true;
        }

        protected internal ResponseToInitDialog(InitDialog iD) 
            : base(iD.DialogHandler)
        {
            MessageType = ResponseToInitDialog;
            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.SequenceNumber = -1;
            SetFeedback(new[] { "0801" });
        }

        public override void Compose()
        {
            base.Compose();
            if (FeedBack == null)
            {
                throw new System.SystemException("Kein Feedback eingestellt.");
            }
            System.Text.StringBuilder bd = new System.Text.StringBuilder("<");
            bd.Append(OsciNsPrefix);
            bd.Append(":responseToInitDialog>");
            bd.Append(WriteFeedBack());
            bd.Append("</");
            bd.Append(OsciNsPrefix);
            bd.Append(":responseToInitDialog>");
            Body = new Body(bd.ToString());
            Body.SetNamespacePrefixes(this);
            DialogHandler.ExplicitDialog = true;
            StateOfMessage |= StateComposed;
        }

        public override void WriteXml(System.IO.Stream stream)
        {
            base.WriteXml(stream);
            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(stream);
                IntermediaryCertificatesH.WriteXml(stream);
            }
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(stream);
            }

            CompleteMessage(stream);
        }
    }
}