using Osci.Common;
using Osci.MessageParts;

namespace Osci.Messagetypes
{
    /// <summary>Diese Klasse dient dazu, von außerhalb des Packages auf Konstruktoren und
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
    /// <seealso cref="ContentContainer"></seealso>
    /// <seealso cref="Attachment"></seealso>
    public class MessageTypesEntry
    {

        public MessageTypesEntry()
        {
            ;
        }

        public static ResponseToInitDialog ResponseToInitDialog(InitDialog id)
        {
            return new ResponseToInitDialog(id);
        }

        public static ResponseToExitDialog ResponseToExitDialog(DialogHandler dH)
        {
            return new ResponseToExitDialog(dH);
        }

        public static ResponseToGetMessageId ResponseToGetMessageId(DialogHandler dH)
        {
            return new ResponseToGetMessageId(dH);
        }

        public static ResponseToStoreDelivery ResponseToStoreDelivery(DialogHandler dH)
        {
            return new ResponseToStoreDelivery(dH, false);
        }

        public static ResponseToFetchDelivery ResponseToFetchDelivery(FetchDelivery fd, StoreDelivery sd)
        {
            return new ResponseToFetchDelivery(fd, sd);
        }

        public static ResponseToFetchProcessCard ResponseToFetchProcessCard(FetchProcessCard fpc)
        {
            return new ResponseToFetchProcessCard(fpc);
        }

        public static ResponseToMediateDelivery ResponseToMediateDelivery(MediateDelivery md, ResponseToProcessDelivery rpd)
        {
            return new ResponseToMediateDelivery(md, rpd);
        }

        public static ResponseToForwardDelivery ResponseToForwardDelivery(AcceptDelivery ad, ResponseToAcceptDelivery rad)
        {
            return new ResponseToForwardDelivery(ad, rad);
        }

        public static ProcessDelivery ProcessDelivery(MediateDelivery md)
        {
            return new ProcessDelivery(md);
        }

        public static AcceptDelivery AcceptDelivery(ForwardDelivery fd)
        {
            return new AcceptDelivery(fd);
        }

        public static void SetFeedBack(OsciResponseTo rsp, string[] codes)
        {
            rsp.SetFeedback(codes);
        }

        public static void Sign(OsciMessage msg)
        {
            if (msg is OsciRequest)
            {
                ((OsciRequest) msg).Sign();
            }
            else
            {
                ((OsciResponseTo)msg).Sign();
            }
        }

        public static void WriteXml(OsciMessage rsp, System.IO.Stream out0)
        {
            rsp.WriteXml(out0);
        }
        public static void AddContentContainer(OsciMessage msg, ContentContainer coco)
        {
            msg.AddContentContainer(coco);
        }
    }
}
