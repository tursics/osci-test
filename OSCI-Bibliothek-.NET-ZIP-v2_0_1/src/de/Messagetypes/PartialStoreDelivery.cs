using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;

using Osci.MessageParts;

using Osci.Messagetypes;
using Osci.Roles;

using Osci.Roles;

using Osci.SoapHeader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Paketierter Zustellungsauftrag</H4></p>
    /// Mit dieser Klasse werden Nachrichtenobjekte für paketierte Zustellungsaufträge
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
    /// <p>Author: R. Lindemann, A. Mergenthal</p>
    /// <p>Version: 2.0.1</p>
    /// </summary>
    /// <seealso cref="ResponseToPartialStoreDelivery">
    /// </seealso>
    public class PartialStoreDelivery
        : OsciRequest
        , IContentPackage
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(OsciMessage));

        public bool IsInfoOnly { get; set; } = false;

        private ChunkInformation _chunkInformation;

        internal PartialStoreDelivery() : base()
        {
            MessageType = PartialStoreDelivery;
            Base64Encoding = false;
        }

        /// <summary> Legt ein Nachrichtenobjekt für einen paketierten Zustellungsauftrag an.
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt des Dialogs, innerhalb dessen die Nachricht
        /// versendet werden soll.
        /// </param>
        /// <param name="addressee"> Rollenobjekt des Empfängers</param>
        /// <param name="chunkInformation">Informationen zu dem aktuellem Chunk</param>
        /// <param name="messageId">Message-ID der originalen StoreDelivery Nachricht. Diese Klasse hängt selbstständig den Postfix '_Partial' an.</param>
        /// <seealso cref="DialogHandler">
        /// </seealso>
        public PartialStoreDelivery(DialogHandler dh, Addressee addressee, ChunkInformation chunkInformation, string messageId)
            : base(dh)
        {
            MessageType = PartialStoreDelivery;
            Originator = ((Originator)dh.Client);
            Addressee = addressee;
            // Check, ob ein Cipherzert eingestellt wurde
            object test = addressee.CipherCertificate;

            ChunkInformationObject = chunkInformation;

            if (!messageId.EndsWith("_Partial"))
            {
                MessageId = messageId + "_Partial";
            }
            else
            {
                MessageId = messageId;
            }
            Base64Encoding = false;
            if (!DialogHandler.ExplicitDialog)
            {
                DialogHandler.ResetControlBlock();
            }
            QualityOfTimeStampCreation = false;
            DialogHandler.Controlblock.Response = DialogHandler.Controlblock.Challenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.SequenceNumber = DialogHandler.Controlblock.SequenceNumber + 1;
        }

        /// <summary> Legt ein Nachrichtenobjekt für einen paketierten Zustellungsauftrag an. Es wird lediglich eine PartialStoreDelivery Nachricht mit 'InfoOnly' aufgebaut!
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt des Dialogs, innerhalb dessen die Nachricht
        /// versendet werden soll.
        /// </param>
        /// <param name="addressee"> Rollenobjekt des Empfängers</param>
        /// <param name="infoOnly">Schalter, um  auszugeben</param>
        /// <param name="messageId">Message-ID der originalen StoreDelivery Nachricht. Diese Klasse hängt selbstständig den Postfix '_Partial' an.</param>
        /// <seealso cref="DialogHandler">
        /// </seealso>
        public PartialStoreDelivery(DialogHandler dh,
                             Addressee addressee,
                            bool infoOnly,
                             String messageId) : this(dh, addressee, null, messageId)

        {
            if (!infoOnly)
            {
                _log.Error("Error create constructor with infoOnly set to false");
                throw new IllegalArgumentException("Error create constructor with infoOnly set to false ");
            }
            IsInfoOnly = true;
        }

        /// <summary> Ruft den im Laufzettel enthaltenen Betreff-Eintrag ab, oder legt diesen fest.
        /// </summary>
        /// <value> Betreff der Zustellung.
        /// </value>
        public string Subject
        {
            get; set;
        }

        /// <summary> Ruft die Message-ID der Nachricht ab, oder legt diese fest.
        /// </summary>
        /// <value> Message-ID
        /// </value>
        public string MessageId
        {
            get
            {
                return base.MessageId;
            }
            set
            {
                base.MessageId = value;
            }
        }

        public ChunkInformation ChunkInformationObject
        {
            get
            {
                return _chunkInformation;
            }
            set
            {
                if (IsInfoOnly)
                {
                    throw new IllegalArgumentException("No chunkInformation allowed in the case of InfoOnly");
                }
                _chunkInformation = value;
            }
        }

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

        /// <summary> Setzt den aktuellen zu übertragen Chunk der kompletten StoreDelivery Nachricht.
        /// Dieser Stream wird in PartialStoreDelivery Nachricht als Attachment eingefügt.
        /// </summary>
        /// <param name="chunkBlob">InputStream von einem Teil(Chunk) der großen Store Delivery Nachricht.</param>
        public void setChunkBlob(Stream chunkBlob)
        {
            ContentContainer container = new ContentContainer("ChunkContentContainer");
            Attachment atta = new Attachment(chunkBlob, "ChunkBlobStoreDelivery");
            atta.IsBase64Encoded = false;
            Content content = new Content("ChunkContent", atta);
            container.AddContent(content);
            AddContentContainer(container);
        }

        /// <summary> Versendet die Nachricht und liefert die Antwortnachricht zurück.
        /// Diese Methode wirft eine Exception, wenn beim Aufbau oder Versand der
        /// Nachricht ein Fehler auftritt. Fehlermeldungen vom Intermediär müssen
        /// dem Feedback der Antwortnachricht entnommen werden.
        /// </summary>
        /// <returns>das Antwortnachricht-Objekt
        /// </returns>
        public virtual ResponseToPartialStoreDelivery Send()
        {
            if (Base64Encoding)
            {
                throw new IllegalStateException("Base64 should be disabled!");
            }
            return (ResponseToPartialStoreDelivery)Transmit(null, null);
        }
        

        public virtual ResponseToPartialStoreDelivery Send(System.IO.Stream storeOutput, System.IO.Stream storeInput)
        {
            if (Base64Encoding)
            {
                throw new IllegalStateException("Base64 should be disabled!");
            }
            return (ResponseToPartialStoreDelivery)Transmit(storeOutput, storeInput);
        }

        public override void Compose()
        {
            base.Compose();
            if (DialogHandler.Controlblock.Challenge == null)
            {
                throw new IllegalStateException("Fehlender Eintrag: Challenge");
            }

            if (DialogHandler.Controlblock.SequenceNumber == -1)
            {
                throw new IllegalStateException("Fehlender Eintrag: SequenceNumber");
            }

            ImportAllCertificates();
            MemoryStream partialHeader = new MemoryStream();
            partialHeader.Write($"<{OsciNsPrefix}:MessageId>{Base64.Encode(MessageId.ToByteArray())}</"
                              + OsciNsPrefix + ":MessageId>");
            if (Subject != null)
                partialHeader.Write("<" + OsciNsPrefix + ":Subject>" + Subject + "</" + OsciNsPrefix
                             + ":Subject>");
            if (IsInfoOnly)
            {
                partialHeader.Write("<" + Osci2017NsPrefix + ":InfoOnly></" + Osci2017NsPrefix
                             + ":InfoOnly>");
            }
            else
            {
                MessagePartsFactory.WriteXml(_chunkInformation, partialHeader);
            }

            OsciH = new OsciH("partialStoreDelivery", partialHeader.AsString(), Osci2017NsPrefix);
            CreateNonIntermediaryCertificatesH();

            if (IsInfoOnly)
            {
                Body = new Body(new ContentContainer[] { }, new EncryptedDataOsci[0]);
            }
            else
            {
                if (ContentContainer == null || ContentContainer.Length != 1)
                {
                    throw new IllegalStateException("Wrong count of ContentContainer objects.");
                }
                if (EncryptedData != null && EncryptedData.Length > 0)
                {
                    throw new IllegalStateException("Wrong count of EncryptedData objects.");
                }
                Body = new Body(ContentContainer, EncryptedData);
            }
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