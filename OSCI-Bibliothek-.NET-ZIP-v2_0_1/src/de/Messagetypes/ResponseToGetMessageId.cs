using Osci.Common;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>MessageId-Anforderungsantwort</H4></p>
    /// Dieses Klasse repräsentiert die Antwort des Intermediärs auf die
    /// Anforderung einer Message-ID.
    /// Clients erhalten vom Intermediär eine Instanz dieser Klasse, die eine Rückmeldung
    /// über den Erfolg der Operation (getFeedback()) sowie ggf. die angeforderte
    /// Message-Id.
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
    /// <seealso cref="GetMessageId">
    /// </seealso>
    public class ResponseToGetMessageId
        : OsciResponseTo
    {

        /// <summary> Liefert die angeforderte Message-ID. Wenn der Auftrag nicht
        /// ordnungsgemäß abgewickelt wurde, wird <b>null</b> zurückgegeben.
        /// In disem Fall sollte der Feedback-Eintrag ausgewertet werden.
        /// </summary>
        /// <value> Message-ID bzw. null im Fehlerfall
        /// </value>
        // <seealso cref="getFeedback()">
        // </seealso>
        public string MessageId
        {
            get
            {
                return ((OsciMessage)this).MessageId;
            }
        }

        internal ResponseToGetMessageId(DialogHandler dh, bool parser) : base(dh)
        {
            MessageType = ResponseToGetMessageId;
            Originator = ((Originator)dh.Client);

            if (!parser)
            {
                DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
                DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            }
        }

        internal ResponseToGetMessageId(DialogHandler dh) : this(dh, false)
        {
        }

        public override void Compose()
        {
            base.Compose();
            if (DialogHandler.Controlblock.SequenceNumber == -1)
            {
                throw new System.SystemException("Kein Squenznummer in DialogHandler eingestellt.");
            }
            if (FeedBack == null)
            {
                throw new System.SystemException("Kein Feedback eingestellt.");
            }
            System.Text.StringBuilder bd = new System.Text.StringBuilder("<");
            bd.Append(OsciNsPrefix);
            bd.Append(":responseToGetMessageId>");
            bd.Append(WriteFeedBack());

            if ((((OsciMessage)this).MessageId != null) && (((OsciMessage)this).MessageId.Length > 0))
            {
                bd.Append("<");
                bd.Append(OsciNsPrefix);
                bd.Append(":MessageId>");
                bd.Append(Base64.Encode(((OsciMessage)this).MessageId.ToByteArray()));
                bd.Append("</");
                bd.Append(OsciNsPrefix);
                bd.Append(":MessageId>");
            }
            bd.Append("</");
            bd.Append(OsciNsPrefix);
            bd.Append(":responseToGetMessageId>");
            Body = new Body(bd.ToString());
            Body.SetNamespacePrefixes(this);

            StateOfMessage |= StateComposed;
        }

        public override void WriteXml(System.IO.Stream outRenamed)
        {
            base.WriteXml(outRenamed);
            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(outRenamed);
                IntermediaryCertificatesH.WriteXml(outRenamed);
            }
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(outRenamed);
            }
            CompleteMessage(outRenamed);
        }
    }
}