using System.IO;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Bearbeitungsantwort</H4></p>
    /// Mit dieser Klasse werden Nachrichtenobjekte für Bearbeitungsantworten
    /// angelegt. Ein passiver Client, der als Supplier fungiert, muß nach Empfang
    /// eines Bearbeitungsauftrags eine Instanz dieser Klasse aufbauen und an den Intermediär
    /// zurücksenden. Die Nachricht enthält eine Rückmeldung über
    /// den Empfang der Nachricht (Feedback) sowie ggf. verschlüsselte bzw.
    /// unverschlüsselte Inhaltsdaten.
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
    /// <seealso cref="ProcessDelivery">
    /// </seealso>
    ///<konvert>ContentPackageI zz nicht nötig</konvert>	
    public class ResponseToProcessDelivery
        : OsciResponseTo
        , IContentPackage
    {
        /// <summary> Liefert den im Laufzettel enthaltenen Betreff-Eintrag.
        /// </summary>
        /// <returns> den Betreff der Zustellung
        /// </returns>
        /// <summary>  Setzt den Betreff-Eintrag im Laufzettel
        /// </summary>
        /// <param name="subject"> der Betreff
        /// </param>
        public string Subject
        {
            get; set;
        }

        /// <summary> Setzt die Rückmeldungen (Fehler und Warnungen) auf Auftragsebene
        /// </summary>
        /// <param name="code">Array mit Fehlercodes
        /// </param>
        /// <summary> Liefert die Qualität des Zeitstempels, mit dem der Intermediär den
        /// Eingang des Auftrags im Laufzettel protokolliert.
        /// </summary>
        /// <returns> Qualität des Zeitstempels: <b>true</b> - kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst.<b>false</b> - Einfacher Zeitstempel
        /// (lokale Rechnerzeit des Intermedärs).
        /// </returns>
        /// <seealso>
        ///     <cref>setQualityOfTimeStampCreation(boolean)</cref>
        /// </seealso>
        /// <summary> Setzt die gewünschte Qualität des Zeitstempels, mit dem der Intermediär
        /// den Eingang des Auftrags im Laufzettel protokolliert.
        /// </summary>
        /// <param name="cryptographic"><b>true</b>: kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst.<b>false</b>: Einfacher Zeitstempel
        /// (lokale Rechnerzeit des Intermedärs).
        /// </param>
        /// <seealso cref="QualityOfTimeStampCreation()">#getQualityOfTimeStampCreation()
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

        /// <summary> Liefert die geforderte Qualität des Zeitstempels, mit dem der Intermediär den
        /// Empfang der Annahmeantwort im Laufzettel protokolliert.
        /// </summary>
        /// <returns> Qualität des Zeitstempels: <b>true</b> - kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst.<b>false</b> - Einfacher Zeitstempel
        /// (lokale Rechnerzeit des Intermedärs).
        /// </returns>
        /// <seealso>
        ///     <cref>setQualityOfTimeStampReception(boolean)</cref>
        /// </seealso>
        /// <summary> Setzt die gewünschte Qualität des Zeitstempels, mit dem der Intermediär die
        /// Empfangbestätigung der Zustellung durch den Empfänger im Laufzettel protokolliert.
        /// Die Empfangsbestätigung besteht in einem weiteren Auftrag, den der Empfänger
        /// nach Erhalt der Bearbeitungsantwort innerhalb desselben expliziten Dialogs
        /// an den Intermediär schickt.
        /// </summary>
        /// <param name="cryptographic"><b>true</b>: kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst.<b>false</b>: Einfacher Zeitstempel
        /// (lokale Rechnerzeit des Intermedärs).
        /// </param>
        /// <seealso cref="QualityOfTimeStampReception()">#getQualityOfTimeStampReception()
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


        /// <summary> Liefert die Message-ID der Nachricht.
        /// </summary>
        /// <returns> die Message-ID
        /// </returns>

        /// <summary> Setzt die Message-ID der Nachricht.
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

        internal ResponseToProcessDelivery(DialogHandler dh) : base(dh)
        {
            MessageType = ResponseToProcessDelivery;
        }

        /// <summary> Legt ein Nachrichtenobjekt für eine Bearbeitungsantwort an.
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt des Dialogs, innerhalb dessen die Nachricht
        /// versendet werden soll.
        /// </param>
        /// <seealso cref="DialogHandler">
        /// </seealso>
        public ResponseToProcessDelivery(ProcessDelivery procDel) 
            : base(procDel.DialogHandler)
        {
            MessageType = ResponseToProcessDelivery;
            // Weil in diesem Szenario die Rollen wechseln (der Orginator der Anfrage wird zum Addressee der Antwort
            // und umgekehrt) werden die Rollenobjekte umgebaut. Damit passen die Rollenobjekte des DialogHandlers nicht mehr so richtig,
            // aber in einem impliziten Dialog kann man das wohl riskieren.
            Signer signer = null;
            Decrypter decrypter = null;
            if (DialogHandler.Supplier.HasSignaturePrivateKey())
            {
                signer = DialogHandler.Supplier.Signer;
            }

            if (DialogHandler.Supplier.HasCipherPrivateKey())
            {
                decrypter = DialogHandler.Supplier.Decrypter;
            }

            Originator = new Originator(signer, decrypter);

            X509Certificate signerCert = null;
            X509Certificate cipherCert = null;

            if (procDel.Originator.HasSignatureCertificate())
            {
                signerCert = procDel.Originator.SignatureCertificate;
            }

            if (procDel.Originator.HasCipherCertificate())
            {
                cipherCert = procDel.Originator.CipherCertificate;
            }

            Addressee = new Addressee(signerCert, cipherCert);

            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = null;
            DialogHandler.Controlblock.ConversationId = null;
            DialogHandler.Controlblock.SequenceNumber = -1;
        }

        /// <summary> Bringt eine Supplier-Signatur an.
        /// @throws IOException bei Schreib-/Leseproblemen
        /// @throws OSCIRoleException wenn dem Rollenobjekt, das als Client fungiert
        /// kein Signer-Objekt zugeordnet wurde.
        /// @throws java.security.SignatureException bei Signatur-Problemen
        /// @throws de.osci.osci12.common.OSCICancelledException bei Abbruch durch den
        /// Benutzer
        /// </summary>
        public override void Sign()
        {
            base.Sign(Originator);
        }

        public override void Compose()
        {
            string head = WriteFeedBack();
            if (((OsciMessage) this).MessageId != null)
            {
                head += "<" + OsciNsPrefix + ":MessageId>" + Base64.Encode(((OsciMessage) this).MessageId.ToByteArray()) + "</" + OsciNsPrefix + ":MessageId>";
                if (Subject != null)
                {
                    head += "<" + OsciNsPrefix + ":Subject>" + Subject + "</" + OsciNsPrefix + ":Subject>";
                }
            }
            else
            {
                QualityOfTimestampTypeCreation = null;
                QualityOfTimestampTypeReception = null;
            }

            OsciH = new OsciH("responseToProcessDelivery", head);
            CreateNonIntermediaryCertificatesH();
            Body = new Body(ContentContainer, EncryptedData);

            StateOfMessage |= StateComposed;
        }

        public override void WriteXml(Stream stream)
        {
            base.WriteXml(stream);

            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(stream);
            }

            if (QualityOfTimestampTypeCreation != null)
            {
                QualityOfTimestampTypeCreation.WriteXml(stream);
            }
            if (QualityOfTimestampTypeReception != null)
            {
                QualityOfTimestampTypeReception.WriteXml(stream);
            }
            OsciH.WriteXml(stream);

            if (NonIntermediaryCertificatesH != null)
            {
                NonIntermediaryCertificatesH.WriteXml(stream);
            }
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(stream);
            }
            CompleteMessage(stream);
        }

        /// <summary> Serialisiert und schreibt die Nachricht - ggf. verschlüsselt - in den übergebenen Stream.
        /// Die ausgehende Nachricht kann zu Debug- oder Archivierungszwecken (in jedem Fall
        /// unverschlüsselt) in den zweiten übergebenen Stream geschrieben werden.
        /// Dieser Parameter kann null sein.
        /// @param out Stream, in den die Antwortnachricht geschrieben werden soll
        /// @param storeOutput Stream, in dem die (unverschlüsselte) Antwortnachricht
        /// gespeichert werden soll
        /// Algorithmus nicht unterstützt wird
        /// </summary>
        public virtual void WriteToStream(Stream outStream, Stream storeStream)
        {
            if (DialogHandler.Encryption)
            {
                new SoapMessageEncrypted(this, storeStream).WriteXml(outStream);
            }
            else
            {
                if (storeStream != null)
                {
                    StoreOutputStream sos = new StoreOutputStream(outStream, storeStream);
                    WriteXml(sos);
                    sos.Close();
                }
                else
                {
                    WriteXml(outStream);
                }
            }
        }
    }
}