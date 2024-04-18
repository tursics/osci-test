using System;
using System.IO;
using Osci.Common;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

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
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    /// <seealso cref="FetchDelivery">
    /// </seealso>

    public class ResponseToFetchDelivery
        : ResponseToFetchAbstract
        , IContentPackage
    {

        internal ResponseToFetchDelivery(FetchDelivery fetchDel, StoreDelivery storeDel)
            : base(fetchDel.DialogHandler)
        {

            //@todo: hier werden die Rollenobjekte getauscht...
            MessageType = ResponseToFetchDelivery;

            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);

            SelectionMode = fetchDel.SelectionMode;
            SelectionRule = fetchDel.SelectionRule;

            if (storeDel == null)
            {
                SetFeedback(new[] { "9803" });
            }
            else
            {
                Addressee = storeDel.Addressee;
                Originator = storeDel.Originator;

                ContentContainer[] con = storeDel.ContentContainer;
                for (int i = 0; i < con.Length; i++)
                {
                    AddContentContainer(con[i]);
                }
                EncryptedDataOsci[] enc = storeDel.EncryptedData;
                for (int i = 0; i < enc.Length; i++)
                {
                    AddEncryptedData(enc[i]);
                }
                Attachment[] att = storeDel.Attachments;
                for (int i = 0; i < att.Length; i++)
                {
                    AddAttachment(att[i]);
                }
                for (int i = 0; i < storeDel.OtherAuthors.Length; i++)
                {
                    OtherAutorsList[storeDel.OtherAuthors[i].Id] = storeDel.OtherAuthors[i];
                }
                for (int i = 0; i < storeDel.OtherReaders.Length; i++)
                {
                    OtherReadersList[storeDel.OtherReaders[i].Id] = storeDel.OtherReaders[i];
                }
            }
        }

        internal ResponseToFetchDelivery(DialogHandler dh) : base(dh)
        {
            Originator = ((Originator)dh.Client);
            MessageType = ResponseToFetchDelivery;
        }

        public override void Compose()
        {
            base.Compose();
            CreateNonIntermediaryCertificatesH();
            string selection;

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
            if (ProcessCardBundle == null)
            {
                OsciH = new OsciH("responseToFetchDelivery", WriteFeedBack() + selection);
            }
            else
            {
                MemoryStream outRenamed = new MemoryStream();
                ProcessCardBundle.WriteXml(outRenamed);

                char[] tmpChar;
                byte[] tmpByte;
                tmpByte = outRenamed.GetBuffer();
                tmpChar = new char[outRenamed.Length];
                Array.Copy(tmpByte, 0, tmpChar, 0, tmpChar.Length);
                OsciH = new OsciH("responseToFetchDelivery", WriteFeedBack() + selection + new string(tmpChar));
            }

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