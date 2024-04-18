using Osci.Common;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;
using System;
using System.IO;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Zustellungsabholantwort-Nachrichtenobjekt</H4></p>
    /// Diese Klasse repräsentiert die Antwort des Intermediärs auf einen
    /// Zustellungsabholauftrag.
    /// Clients erhalten vom Intermediär eine Instanz dieser Klasse, die eine Rückmeldung
    /// über den Erfolg der Operation (getFeedback()) sowie ggf. die angeforderten verschlüsselten
    /// und/oder unverschlüsselten Inhaltsdaten einschl. des zugehörigen Laufzettels enthält.
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
    /// <seealso cref="PartialFetchDelivery">
    /// </seealso>

    public class ResponseToPartialFetchDelivery
        : ResponseToFetchAbstract
        , IContentPackage
    {
        public ChunkInformation ChunkInformation { get; set; } = MessagePartsFactory.createChunkInformation(CheckInstance.ResponsePartialFetchDelivery);

        internal ResponseToPartialFetchDelivery(FetchDelivery fetchDel, Addressee addressee, Originator originator, Attachment chunkAttachment)
            : base(fetchDel.DialogHandler)
        {
            //@todo: hier werden die Rollenobjekte getauscht...
            MessageType = ResponseToPartialFetchDelivery;

            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);

            SelectionMode = fetchDel.SelectionMode;
            SelectionRule = fetchDel.SelectionRule;
            SetChunkBlob(chunkAttachment);

            if (addressee == null || originator == null)
            {
                SetFeedback(new[] { "9803" });
            }
            else
            {
                Addressee = addressee;
                Originator = originator;
            }
        }

        internal ResponseToPartialFetchDelivery(FetchDelivery fetchDel, Addressee addressee, Originator originator, InputStream chunkBlob)
            : this(fetchDel, addressee, originator, new Attachment(chunkBlob, "ChunkBlobStoreDelivery"))
        {
        }

        internal ResponseToPartialFetchDelivery(DialogHandler dh) : base(dh)
        {
            Originator = ((Originator)dh.Client);
            MessageType = ResponseToPartialFetchDelivery;
        }

        public Stream GetChunkBlob()

        {
            ContentContainer coco = (ContentContainer)GetContentContainerByRefId("ChunkContentContainer");
            Attachment att = coco.Contents[0].Attachment;
            return att.Stream;
        }

        private void SetChunkBlob(Attachment chunkAttachment)
        {
            ContentContainer container = new ContentContainer("ChunkContentContainer");
            chunkAttachment.IsBase64Encoded = false;
            Content content = new Content("ChunkContent", chunkAttachment);
            container.AddContent(content);
            base.AddContentContainer(container);
        }

        public override void Compose()
        {
            base.Compose();
            CreateNonIntermediaryCertificatesH();
            string selection = "";
            string msgIdElement = "";

            MemoryStream parHeader = new MemoryStream();
            MessagePartsFactory.WriteXml(ChunkInformation, parHeader);

            if (SelectionMode == SelectByMessageId)
            {
                selection = "<" + OsciNsPrefix + ":fetchDelivery><" + OsciNsPrefix + ":SelectionRule><" + OsciNsPrefix + ":MessageId>" +
                    Base64.Encode(SelectionRule)
                    + "</" + OsciNsPrefix + ":MessageId></" + OsciNsPrefix + ":SelectionRule></" + OsciNsPrefix + ":fetchDelivery>";
            }
            else if (SelectionMode == SelectByDateOfReception)
            {
                selection = "<" + OsciNsPrefix + ":fetchDelivery><" + OsciNsPrefix + ":SelectionRule><" + OsciNsPrefix + ":ReceptionOfDelivery>"
                    + SelectionRule +
                    "</" + OsciNsPrefix + ":ReceptionOfDelivery></" + OsciNsPrefix + ":SelectionRule></" + OsciNsPrefix + ":fetchDelivery>";
            }
            else
            {
                selection = "<" + OsciNsPrefix + ":fetchDelivery></" + OsciNsPrefix + ":fetchDelivery>";
            }


            if (MessageId != null)
            {
                msgIdElement = "<" + OsciNsPrefix + ":MessageId>"
                               + Base64.Encode(MessageId) + "</"
                               + OsciNsPrefix + ":MessageId>";
            }

            OsciH = new OsciH("responseToPartialFetchDelivery", WriteFeedBack() + selection + parHeader.AsString() + msgIdElement, Osci2017NsPrefix);

            //if (featureDescription != null && dialogHandler.isSendFeatureDescription())
            //{
            //    messageParts.add(featureDescription);
            //}

            Body = new Body(ContentContainer, EncryptedData);
            StateOfMessage |= StateComposed;
        }

        public override void WriteXml(Stream outRenamed)
        {
            base.WriteXml(outRenamed);

            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(outRenamed);
            }
            OsciH.WriteXml(outRenamed);

            if (IntermediaryCertificatesH != null)
            {
                IntermediaryCertificatesH.WriteXml(outRenamed);
            }
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