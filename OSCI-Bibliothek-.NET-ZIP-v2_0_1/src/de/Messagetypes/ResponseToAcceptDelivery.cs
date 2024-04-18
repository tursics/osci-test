using Osci.Common;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Annahmeantwort</H4></p>
    /// Mit dieser Klasse werden Nachrichtenobjekte für Annahmeantworten
    /// angelegt. Ein passiver Client, der als Supplier fungiert, muß nach Empfang
    /// eines Annahmeauftrags eine Instanz dieser Klasse aufbauen und an den Intermediär
    /// zurücksenden. Die Nachricht enthält inhaltlich lediglich eine Rückmeldung über
    /// den Empfang der Nachricht (Feedback).
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
    /// <seealso cref="AcceptDelivery">
    /// </seealso>
    public class ResponseToAcceptDelivery
        : OsciResponseTo
    {
        /// <summary> Legt ein Nachrichtenobjekt für einen Annahmeantwort an.
        /// </summary>
        /// <param name="req">Auftragsnachricht</param>
        public ResponseToAcceptDelivery(AcceptDelivery req)
            : base(req.DialogHandler)
        {
            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            // Hier wurde die Spec korrigiert, in der vorigen Zeile sollte auch null gesetzt werden
            DialogHandler.Controlblock.ConversationId = null;
            DialogHandler.Controlblock.SequenceNumber = -1;
            MessageType = ResponseToAcceptDelivery;
        }

        internal ResponseToAcceptDelivery(DialogHandler dh)
            : base(dh)
        {
            Addressee = ((Addressee)dh.Supplier);
            MessageType = ResponseToAcceptDelivery;
        }

        public override void Compose()
        {
            base.Compose();
            OsciH = new OsciH("responseToAcceptDelivery", WriteFeedBack());
            Body = new Body("");
			CreateNonIntermediaryCertificatesH();
            StateOfMessage |= StateComposed;
            Body.SetNamespacePrefixes(this);
            StateOfMessage |= StateComposed;
        }

        public virtual void GetMessageAsStream(System.IO.Stream outRenamed)
        {
            WriteXml(outRenamed);
        }

        /// <summary> Bringt eine Supplier-Signatur an.
        /// </summary>
        /// @throws IOException bei Schreib-/Leseproblemen 
        /// @throws OSCIRoleException wenn dem Rollenobjekt, das als Client fungiert
        /// kein Signer-Objekt zugeordnet wurde.
        /// @throws java.security.SignatureException bei Signatur-Problemen
        /// @throws de.osci.osci12.common.OSCICancelledException bei Abbruch durch den
        /// Benutzer
        public override void Sign()
        {
            base.Sign(DialogHandler.Supplier);
        }

        public override void WriteXml(System.IO.Stream outRenamed)
        {
            base.WriteXml(outRenamed);
            // Supplier-Signatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(outRenamed);
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

        /// <summary> Serialisiert und schreibt die Nachricht - ggf. verschlüsselt - in den übergebenen Stream.
        /// Die ausgehende Nachricht kann zu Debug- oder Archivierungszwecken (in jedem Fall
        /// unverschlüsselt) in den zweiten übergebenen Stream geschrieben werden.
        /// Dieser Parameter kann null sein.
        /// </summary>
        /// <param name="out_Renamed">Stream, in den die Antwortnachricht geschrieben werden soll.
        /// </param>
        /// <param name="storeOutput">Stream, in dem die (unverschlüsselte) Antwortnachricht gespeichert werden soll.
        /// (NOT AVAILABLE!)
        /// </param>
        /// @throws OSCIRoleException wenn erforderliche Zertifikate fehlen
        /// @throws IOException bei Schreibproblemen
        /// @throws java.security.NoSuchAlgorithmException wenn ein benötigter
        /// Algorithmus nicht unterstützt wird
        public virtual void WriteToStream(System.IO.Stream outRenamed, System.IO.Stream storeOutput)
        {
            if (DialogHandler.Encryption)
            {
                new SoapMessageEncrypted(this, storeOutput).WriteXml(outRenamed);
            }
            else
                if (storeOutput != null)
            {
                StoreOutputStream sos = new StoreOutputStream(outRenamed, storeOutput);
                WriteXml(sos);
                sos.Close();
            }
            else
            {
                WriteXml(outRenamed);
            }
        }
    }
}