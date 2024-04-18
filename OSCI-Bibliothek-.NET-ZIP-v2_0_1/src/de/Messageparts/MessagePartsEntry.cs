using Osci.Common;
using Osci.Cryptographic;
using Osci.Helper;
using Osci.Messagetypes;

namespace Osci.MessageParts
{
    /// <summary> Diese Klasse dient dazu, von außerhalb des Packages auf Konstruktoren und
    /// Methoden zuzugreifen, die dort nicht sichtbar sind, um die API für den Anwender
    /// übersichtlich zu halten. Die Klasse sollte deshalb nicht dokumentiert werden.
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
    /// <seealso cref="ContentContainer">
    /// </seealso>
    /// <seealso cref="MessageParts.Attachment">
    /// </seealso>
    public class MessagePartsEntry
    {

        private static Iso8601DateTimeFormat _isodate;

        public static EncryptedDataOsci EncryptedDataOsci(SecretKey secretKey, string encryptionMethodAlgorithm, Attachment att)
        {
            return new EncryptedDataOsci(secretKey, att);
        }

        public static ProcessCardBundle ProcessCardBundle(string name, string messageId, string recentModification, Timestamp creation, Timestamp forwarding, Timestamp reception, string subject, Inspection[] inspections)
        {
            return new ProcessCardBundle(name, messageId, recentModification, creation, forwarding, reception, subject, inspections);
        }

        public static Timestamp Timestamp(int nameId, string algorithm, string timeStamp)
        {
            return new Timestamp(nameId, algorithm, timeStamp);
        }

        public static void SetTimestampCreation(ProcessCardBundle pcb, Timestamp tm)
        {
            pcb.RecentModification = SupportClass.FormatDateTime(_isodate.MakeFormat(), System.DateTime.Now);
            pcb.Creation = tm;
        }

        public static void SetTimestampForwarding(ProcessCardBundle pcb, Timestamp tm)
        {
            pcb.RecentModification = SupportClass.FormatDateTime(_isodate.MakeFormat(), System.DateTime.Now);
            pcb.Forwarding = tm;
        }

        public static void SetTimestampReception(ProcessCardBundle pcb, Timestamp tm)
        {
            pcb.RecentModification = SupportClass.FormatDateTime(_isodate.MakeFormat(), System.DateTime.Now);
            pcb.Reception = tm;
        }

        public static OsciSignature OsciSignature(string enclosingElement)
        {
            return new OsciSignature(enclosingElement);
        }

        public static OsciSignatureReference OsciSignatureReference(MessagePart mp, string algo)
        {
            return new OsciSignatureReference(mp, algo);
        }

        public static Attachment Attachment(System.IO.Stream ins, string refId, long length, string digAlgo)
        {
            return new Attachment(ins, refId, length, digAlgo);
        }

        public static void AttachmentSetStream(Attachment attachment, System.IO.Stream ins, bool encrypt, long length, string digAlgo)
        {
            attachment.setInputStream(ins, encrypt, length, digAlgo);
        }

        public static void AttachmentSetState(Attachment att, int newState, bool encrypted)
        {
            att.StateOfAttachment = newState;
            att.Encrypted = encrypted;
        }

        static MessagePartsEntry()
        {
            _isodate = new Iso8601DateTimeFormat();
        }
        public static AcceptDelivery NewAcceptDelivery(DialogHandler dh)
        {
            return new AcceptDelivery(dh);
        }
        public static void Sign(OsciMessage msg)
        {
            if (msg is OsciRequest)
                ((OsciRequest)msg).Sign();
            else
                ((OsciResponseTo)msg).Sign();
        }
        public static Attachment NewAttachment(string refid, SecretKey key, System.IO.Stream in1, string algo)
        {
            Attachment att = new Attachment(null, refid, 0, algo);
            att.setInputStream(in1, true, 0, algo);
            att.StateOfAttachment = MessageParts.Attachment.StateOfAttachmentEncrypted;
            att.SecretKey = key;
            return att;
        }
        public static void SetAttachmentSecretKey(Attachment atta, SecretKey sk)
        {
            atta.SecretKey = sk;
        }

    }
}