using System.IO;
using Osci.Common;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Messagetypes;

namespace Osci.MessageParts
{
    public class MessagePartsFactory
    {
        public static ContentPackageBuilder CreateContentPackageBuilder(OsciMessageBuilder omb)
        {
            return new ContentPackageBuilder(omb);
        }

        public static OsciSignatureBuilder CreateOsciSignatureBuilder(XmlReader xmlReader, DefaultHandler parentHandler, Attributes atts)
        {
            return new OsciSignatureBuilder(xmlReader, parentHandler, atts, false);
        }

        public static ProcessCardBundle CreateProcessCardBundle(string name, string messageId, string recentModification, Timestamp creation, Timestamp forwarding, Timestamp reception, string subject, Inspection[] inspections)
        {
            return new ProcessCardBundle(name, messageId, recentModification, creation, forwarding, reception, subject, inspections);
        }

        public static FeedbackObject CreateFeedbackObject(string[] feedback)
        {
            return new FeedbackObject(feedback);
        }

        public static Attachment CreateAttachment(Stream ins, string refId, long length, string digestAlgorithm)
        {
            return new Attachment(ins, refId, length, digestAlgorithm);
        }

        public static void AttachmentSetStream(Attachment attachment, Stream ins, bool encrypt, long length, string digestAlgorithm)
        {
            attachment.setInputStream(ins, encrypt, length, digestAlgorithm);
        }

        public static void AttachmentSetState(Attachment att, int newState, bool encrypted)
        {
            att.StateOfAttachment = newState;
            att.Encrypted = encrypted;
        }

        public static void WriteXml(MessagePart mp, Stream outRenamed)
        {
            mp.WriteXml(outRenamed);
        }

        public static ChunkInformation createChunkInformation(CheckInstance chunkInstance)
        {
            return new ChunkInformation(chunkInstance);
        }
    }
}