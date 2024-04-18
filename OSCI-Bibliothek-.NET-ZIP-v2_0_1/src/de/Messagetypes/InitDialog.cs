using Osci.Common;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Dialoginitialisierungs-Nachrichtenobjekt</H4></p>
    /// Diese Klasse dient der Initialisierung eines expliziten Dialogs. Clients
    /// erhalten als Antwort auf diese Nachricht vom Intermediär ein
    /// Nachrichtenobjekt, welches in seinem ControlBlock die angeforderte
    /// ConversationId enthält. Diese Id wird beim Empfang der Antwort an das
    /// verwendete DialogHandler-Objekt übergeben. Der Client muß lediglich dieses
    /// DialogHandler-Objekt für alle weiteren Nachrichten verwenden, die innerhalb
    /// dieses Dialogs behandelt werden sollen.
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

    public class InitDialog 
        : OsciRequest
    {
        /// <summary> Legt ein Nachrichtenobjekt zur Dialoginitialisierung an.
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt, welches für die
        /// folgenden Nachrichten initialisiert werden soll.
        /// </param>
        /// <exception cref="OsciRoleException">wenn dem im übergebenen DialogHandler
        /// eingestellten Originator-Objekt das Verschlüsselungszertifikat
        /// fehlt.
        /// </exception>
        /// <seealso cref="DialogHandler">
        /// </seealso>		
        public InitDialog(DialogHandler dh) 
            : base(dh)
        {
            dh.Controlblock.Challenge = Tools.CreateRandom(10);
            dh.Controlblock.ConversationId = null;
            dh.Controlblock.SequenceNumber = -1;
            dh.Controlblock.Response = null;
            MessageType = InitDialog;
            Originator = (Originator)dh.Client;
        }

        /// <summary> Dieser Konstruktor wird nur für die parser benötigt
        /// </summary>
        internal InitDialog()
        {
            MessageType = InitDialog;
        }

        /// <summary> Versendet die Nachricht und liefert die Antwortnachricht zurück.
        /// Diese Methode wirft eine Exception, wenn beim Aufbau oder Versand der
        /// Nachricht ein Fehler auftritt. Fehlermeldungen vom Intermediär müssen
        /// dem Feedback der Antwortnachricht entnommen werden.
        /// </summary>
        /// <returns> das Antwortnachricht-Objekt
        /// </returns>
        // --> Storestreams:
        public virtual ResponseToInitDialog Send()
        {
            return (ResponseToInitDialog)Transmit(null, null);
        }

        public override void Compose()
        {
            base.Compose();
            if (NonIntermediaryCertificatesH == null)
            {
                NonIntermediaryCertificatesH = new NonIntermediaryCertificatesH();
            }
            NonIntermediaryCertificatesH.CipherCertificateOriginator = (Originator)DialogHandler.Client;

            Body = new Body("<" + OsciNsPrefix + ":initDialog></" + OsciNsPrefix + ":initDialog>");
            // Zur Sicherheit nochmal aufräumen
            if (DialogHandler.Controlblock.Challenge == null)
            {
                throw new System.SystemException("Kein Challenge-Wert in DialogHandler eingestellt.");
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
            NonIntermediaryCertificatesH.WriteXml(outRenamed);
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(outRenamed);
            }

            CompleteMessage(outRenamed);
        }
    }
}