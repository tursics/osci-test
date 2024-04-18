using Osci.Common;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;
using Osci.Messagetypes;
using Osci.Roles;
using Osci.SoapHeader;
using System.IO;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Antwort auf einen paketierten Zustellungsauftrag</H4></p>
    /// Instanzen dieser Klasse werden als Antworten auf paketierte Zustellungsaufträge
    /// zurückgegeben. Das Nachrichtenobjekt enthält eine Rückmeldung über den Erfolg
    /// der Operation (getFeedback()) sowie ggf. den Laufzettel der Zustellung.
    ///
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    ///
    /// <p>Author: R.Lindemann, A.Mergenthal</p>
    /// <p>Version: 2.0.1</p>
    /// </summary>
    /// <seealso cref="PartialStoreDelivery">
    /// </seealso>
    /// <seealso cref="ProcessCardBundle()">
    /// </seealso>
    public class ResponseToPartialStoreDelivery
        : OsciResponseTo
    {
        protected internal string[][] InsideFeedBack;

        private FeedbackObject[] insideFeedbackObjects;
        public ChunkInformation chunkInformation { get; set; } = new ChunkInformation(CheckInstance.ResponsePartialStoreDelivery);

        internal ResponseToPartialStoreDelivery(DialogHandler dh)
            : this(dh, null, false)
        {
        }

        internal ResponseToPartialStoreDelivery(DialogHandler dh, bool parser)
            : this(dh, null, parser)
        {
        }

        internal ResponseToPartialStoreDelivery(DialogHandler dh, ProcessCardBundle processCardBundle, bool parser)
        : base(dh)
        {
            MessageType = ResponseToPartialStoreDelivery;
            Originator = ((Originator)dh.Client);
            ProcessCardBundle = processCardBundle;
            if (!parser)
            {
                DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
                DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            }

            Body = new Body("");
            Body.SetNamespacePrefixes(this);
        }

        public void SetInsideFeedback(string[] code)
        {
            InsideFeedBack = new string[code.Length][];

            for (int i = 0; i < code.Length; i++)
            {
                InsideFeedBack[i] = new string[3];
            }
            for (int i = 0; i < code.Length; i++)
            {
                InsideFeedBack[i][0] = DialogHandler.LanguageList;
                InsideFeedBack[i][1] = code[i];
                InsideFeedBack[i][2] = Text.GetString(code[i]);
            }
        }

        /// <summary> Liefert die Rückmeldung (Feedback-Eintrag) als String-Array zurück.
        /// Der erste Index des Arrays entspricht dem Index des Entry-Elementes.
        /// Beim zweiten Index bezeichnet
        /// <p>0 - das Sprachkürzel (z.B. "de", "en-US", optional)</p>
        /// <p>1 - den Code</p>
        /// <p>2 - den Text</p>
        /// </summary>
        /// <value> Rückmeldung
        /// </value>
        public string[][] InsideFeedback
        {
            set
            {
                InsideFeedBack = value;
            }
            get
            {
                return InsideFeedBack;
            }
        }

        public FeedbackObject[] InsideFeedbackObjects
        {
            get
            {
                if (InsideFeedback == null)
                {
                    return null;
                }
                if (insideFeedbackObjects == null)
                {
                    insideFeedbackObjects = new FeedbackObject[InsideFeedback.Length];
                    for (int i = 0; i < InsideFeedback.Length; i++)
                    {
                        insideFeedbackObjects[i] = new FeedbackObject(InsideFeedback[i]);
                    }
                }
                return insideFeedbackObjects;
            }
        }

        /// <summary> Diese Methode liefert den Laufzettel der Zustellung zurück oder null,
        /// wenn bei der Verarbeitung der Nachricht ein Fehler aufgetereten ist.
        /// Die Informationen im Laufzettel können auch direkt über die einzelnen
        /// getX()-Methoden ausgewertet werden.
        /// </summary>
        /// <returns> den Laufzettel als ProcessCardBundle-Objekt, im Fehlerfall null
        /// </returns>
        /// <seealso cref="TimestampCreation()">
        /// </seealso>
        /// <seealso cref="Inspections()">
        /// </seealso>
        /// <seealso cref="Subject()">
        /// </seealso>
        /// <seealso cref="RecentModification()">
        /// </seealso>
        /// <seealso cref="MessageId()">
        /// </seealso>
        public ProcessCardBundle ProcessCardBundle
        {
            get; internal set;
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Zeitstempel vom Zeitpunkt des Eingangs
        /// des Zustellungsauftrags beim Intermediär.
        /// </summary>
        /// <returns> Zeitstempel der Einreichung beim Intermediär
        /// </returns>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public Timestamp TimestampCreation
        {
            get
            {
                return ProcessCardBundle.Creation;
            }
        }

        /// <summary> Liefert die Ergebnisse der Zertifikatsprüfungen in Form von Inspection-Objekten,
        /// die im ProcessCardBundle-Objekt enthalten sind.
        /// </summary>
        /// <returns> inspections die Prüfergebnisse
        /// </returns>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public Inspection[] Inspections
        {
            get
            {
                return ProcessCardBundle.Inspections;
            }
        }

        /// <summary> Liefert den im Laufzettel enthaltenen Betreff-Eintrag.
        /// </summary>
        /// <returns> den Betreff der Zustellung
        /// </returns>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public string Subject
        {
            get
            {
                return ProcessCardBundle.Subject;
            }
        }

        /// <summary> Liefert das Datum der letzten Änderung des Laufzettels. Das Format
        /// entspricht dem XML-Schema nach http://www.w3.org/TR/xmlschema-2/#dateTime
        /// </summary>
        /// <returns> Datum der letzten Änderung.
        /// </returns>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public string RecentModification
        {
            get
            {
                return ProcessCardBundle.RecentModification;
            }
        }

        /// <summary> Liefert die Message-ID der Nachricht.
        /// </summary>
        /// <returns> die Message-ID
        /// </returns>
        /// <seealso cref="ProcessCardBundle()">
        /// </seealso>
        public string MessageId
        {
            get
            {
                return ProcessCardBundle.MessageId;
            }
        }

        /// <summary> Diese Methode registriert (statisch) eine OSCIDataSource-Implementierung,
        /// die für die Speicherung aller eingehenden Nachrichten dieses Typs
        /// verwendet wird. Beim Empfang einer Nachricht wird vom registrierten
        /// OSCIDataSource-Objekt eine neue Instanz geholt (OSCIDataSource.newInstance())
        /// und die Nachricht (der eingehende Bytestrom) in deren OutputStream geschrieben.
        /// </summary>
        /// <param name="dataSource">
        /// </param>
        /// <seealso cref="OsciDataSource">
        /// </seealso>
        public static OsciDataSource InputDataSourceImpl
        {
            set
            {
                _inputDataSourceImpl = value;
            }
        }

        /// <summary> Liefert die Instanz des registrierten OSCIDataSource-Objektes, welches
        /// für die Speicherung der Nachricht beim Empfang verwendet wurde.
        /// Die Methode liefert null, wenn keine OSCIDataSource-Implementierung
        /// registriert wurde.
        /// </summary>
        /// <returns> Instanz von OSCIDataSource
        /// </returns>
        public OsciDataSource InputDataSource
        {
            get
            {
                return _inputDataSource;
            }
        }

        private static OsciDataSource _inputDataSourceImpl;
        private OsciDataSource _inputDataSource;

        public override void Compose()
        {
            base.Compose();
            if (DialogHandler.Controlblock.Response == null)
            {
                throw new System.SystemException("Kein Response-Wert in DialogHandler eingestellt.");
            }
            if (DialogHandler.Controlblock.ConversationId == null)
            {
                throw new System.SystemException("Keine Conversation-ID in DialogHandler eingestellt.");
            }
            if (DialogHandler.Controlblock.SequenceNumber == -1)
            {
                throw new System.SystemException("Keine Sequenznummer in DialogHandler eingestellt.");
            }
            if (FeedBack == null)
            {
                throw new System.SystemException("Kein Feedback eingestellt.");
            }


            string chunkInfoString = "";
            string insideFeedback = "";

            if (chunkInformation != null)
            {
                MemoryStream chunkOut = new MemoryStream();
                MessagePartsFactory.WriteXml(chunkInformation, chunkOut);
                StreamReader reader = new StreamReader(chunkOut);
                chunkInfoString = reader.ReadToEnd();
            }

            if(InsideFeedback != null && !(InsideFeedback.Length == 0))
            {
                insideFeedback = WriteInsideFeedBack();
            }


            if (ProcessCardBundle == null)
            {
                OsciH = new OsciH("responseToPartialStoreDelivery", insideFeedback + WriteFeedBack() + chunkInfoString);
            }
            else
            {
                MemoryStream outRenamed = new System.IO.MemoryStream();
                ProcessCardBundle.WriteXml(outRenamed);
                char[] tmpChar;
                byte[] tmpByte;
                tmpByte = outRenamed.GetBuffer();
                tmpChar = new char[outRenamed.Length];
                System.Array.Copy(tmpByte, 0, tmpChar, 0, tmpChar.Length);
                OsciH = new OsciH("responseToPartialStoreDelivery", insideFeedback+ WriteFeedBack() + new string(tmpChar) + chunkInfoString);
            }
            //if (featureDescription != null && dialogHandler.isSendFeatureDescription())
            //{
            //    messageParts.add(featureDescription);
            //}
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
            OsciH.WriteXml(outRenamed);
            if (IntermediaryCertificatesH != null)
            {
                IntermediaryCertificatesH.WriteXml(outRenamed);
            }
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(outRenamed);
            }
            CompleteMessage(outRenamed);
        }


        internal string WriteInsideFeedBack()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("<");
            sb.Append(OsciNsPrefix);
            sb.Append(":InsideFeedback>");
            for (int i = 0; i < InsideFeedBack.Length; i++)
            {
                sb.Append("<");
                sb.Append(OsciNsPrefix);
                sb.Append(":Entry xml:lang=\"");
                sb.Append(InsideFeedBack[i][0]);
                sb.Append("\"><");
                sb.Append(OsciNsPrefix);
                sb.Append(":Code>");
                sb.Append(InsideFeedBack[i][1]);
                sb.Append("</");
                sb.Append(OsciNsPrefix);
                sb.Append(":Code><");
                sb.Append(OsciNsPrefix);
                sb.Append(":Text>");
                sb.Append(InsideFeedBack[i][2]);
                sb.Append("</");
                sb.Append(OsciNsPrefix);
                sb.Append(":Text></");
                sb.Append(OsciNsPrefix);
                sb.Append(":Entry>");
            }
            sb.Append("</");
            sb.Append(OsciNsPrefix);
            sb.Append(":InsideFeedback>");
            return sb.ToString();
        }
    }
}