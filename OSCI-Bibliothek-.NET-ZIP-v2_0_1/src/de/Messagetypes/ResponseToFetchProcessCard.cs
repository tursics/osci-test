using Osci.Common;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Laufzettelabholauftrag-Nachrichtenobjekt</H4></p>
    /// Diese Klasse repräsentiert die Antwort des Intermediärs auf einen
    /// Laufzettelabholauftrag.
    /// Clients erhalten vom Intermediär eine Instanz dieser Klasse, die eine Rückmeldung
    /// über den Erfolg der Operation (getFeedback()) sowie ggf. die
    /// angeforderten Laufzettel enthält.
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
    /// <seealso cref="FetchProcessCard">
    /// </seealso>

    public class ResponseToFetchProcessCard
        : OsciResponseTo
    {
        /// <summary> Liefert die angeforderten Laufzettel als Array von ProcessCardBundle-Objekten.
        /// </summary>
        /// <value> Laufzettel
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        public ProcessCardBundle[] ProcessCardBundles
        {
            get
            {
                return processCardBundles;
            }
        }

        private string _selectionRule;

        /// <summary> Liefert die gesetzte Auswahlregel. Der Inhalt des zruückgegebenen
        /// Strings hängt vom gesetzten Auswahlmodus ab und kann
        /// entweder in einer oder mehrerer Message-Id oder einem Datum bestehen
        /// Das Format eines Datums entspricht dem XML-Schema nach
        /// <a href="http://www.w3.org/TR/xmlschema-2/#dateTime">
        /// http://www.w3.org/TR/xmlschema-2/#dateTime</a>.
        /// Wurde keine Regel gesetzt, wird als default null zurückgegeben.
        /// Mehrere Message-Ids werden aneinandergereiht durch "&amp;" getrennt zurückgegeben.
        /// </summary>
        /// <value> die Auswahlregel (Message-Id oder Datum)
        /// </value>
        /// <seealso cref="_selectionRule">
        /// </seealso>
        public string SelectionRule
        {
            get
            {
                return _selectionRule;
            }
            internal set
            {
                _selectionRule = value;
            }
        }

        private int _selectionMode = -1;

        /// <summary> Liefert den gesetzten Auswahlmodus.
        /// </summary>
        /// <value> Auswahlmodus SELECT_BY_MESSAGE_ID, SELECT_BY_DATE_OF_RECEPTION,
        /// SELECT_BY_RECENT_MODIFICATION oder NO_SELECTION_RULE
        /// </value>
        /// <seealso cref="_selectionMode">
        /// </seealso>
        ///<getset></getset> 
        public int SelectionMode
        {
            get
            {
                return _selectionMode;
            }
            internal set
            {
                _selectionMode = value;
            }
        }

        private long _quantityLimit = -1;

        /// <summary> Liefert die maximale Anzahl angeforderter Laufzettel.
        /// </summary>
        /// <value> maximale Anzahl
        /// </value>
        /// <getset></getset>
        public long quantityLimit
        {
            get
            {
                return _quantityLimit;
            }
        }

        internal long QuantityLimit
        {
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentException("Limit für die Zahl der Laufzettel muß größer 0 sein.");
                }

                _quantityLimit = value;
            }
            get
            {
                return _quantityLimit;
            }
        }

        /// <summary>Liefert den Identifier für das Auswahlkriterium, ob nur Laufzettel von
        /// Nachrichten zurückgegeben werden sollen, die an den oder vom Absender des
        /// Laufzettelabholauftrags geschickt wurden.
        /// Mögliche Werte sind
        /// <ul><li>SELECT_ORIGINATOR - für Nachrichten, die vom Absender dieses
        /// Auftrags gesendet wurden.</li><li>SELECT_ADDRESSEE - für Nachrichten, die
        /// an den Absender dieses Auftrags gesendet wurden.</li><li>SELECT_ALL -
        /// für alle Nachrichten (default).</li></ul> <b>Diese Einstellung ist nur in
        /// den Selection-Modes SELECT_BY_DATE_OF_RECEPTION und
        /// SELECT_BY_RECENT_MODIFICATION wirksam.</b>
        /// </summary>
        /// <value>int Code for the Selection Role
        /// </value>
        private int _roleForSelection = -1;

        public int RoleForSelection
        {
            get
            {
                return _roleForSelection;
            }
            internal set
            {
                _roleForSelection = value;
            }
        }

        /** 
         * Gibt an, ob Liefert den Identifier für das Auswahlkriterium, ob nur Laufzettel von 
         * Nachrichten zurückgegeben werden sollen, die an den oder vom Absender des 
         * Laufzettelabholauftrags geschickt wurden. 
         * 
         * @see FetchProcessCard#setRoleForSelection(int) 
         * @return Modus 
         */
        public bool SelectNoReceptionOnly
        {
            get;
            internal set;
        }

        internal ProcessCardBundle[] processCardBundles = new ProcessCardBundle[0];

        internal ResponseToFetchProcessCard(DialogHandler dh) : base(dh)
        {
            MessageType = ResponseToFetchProcessCard;
            Originator = (Originator)DialogHandler.Client;
        }

        internal ResponseToFetchProcessCard(FetchProcessCard fpc) : base(fpc.DialogHandler)
        {
            Originator = (Originator)DialogHandler.Client;
            MessageType = ResponseToFetchProcessCard;
            _selectionMode = fpc.SelectionMode;
            _selectionRule = fpc.SelectionRule;
            _roleForSelection = fpc.RoleForSelection;
            SelectNoReceptionOnly = fpc.IsSelectNoReception();

            if (fpc.QuantityLimit > 0)
            {
                QuantityLimit = fpc.QuantityLimit;
            }
            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;

            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
        }

        public override void Compose()
        {
            base.Compose();
            System.Text.StringBuilder sb = new System.Text.StringBuilder("<" + OsciNsPrefix + ":responseToFetchProcessCard>");
            sb.Append(WriteFeedBack());
            sb.Append("<" + OsciNsPrefix + ":fetchProcessCard>");
            string selectionAttributes = "";
            if (SelectNoReceptionOnly)
                selectionAttributes += " NoReception=\"true\"";

            if (_roleForSelection == SelectAddressee)
                selectionAttributes += " Role=\"Addressee\"";
            else if (_roleForSelection == SelectOriginator)
                selectionAttributes += " Role=\"Originator\"";
            System.Text.StringBuilder selection = new System.Text.StringBuilder("<");
            selection.Append(OsciNsPrefix);
            selection.Append(":SelectionRule>");
            if (SelectionMode == SelectByMessageId)
            {
                string[] msgIds = SelectionRule.Split('&');
                for (int i = 0; i < msgIds.Length; i++)
                {
                    selection.Append("<");
                    selection.Append(OsciNsPrefix);
                    selection.Append(":MessageId>");
                    selection.Append(Base64.Encode(msgIds[i].ToByteArray()));
                    selection.Append("</");
                    selection.Append(OsciNsPrefix);
                    selection.Append(":MessageId>");
                }
                selection.Append("</");
            }
            else if (SelectionMode == SelectByDateOfReception)
            {
                selection.Append("<");
                selection.Append(OsciNsPrefix);
                selection.Append(":ReceptionOfDelivery" + selectionAttributes + ">" + SelectionRule +
                    "</");
                selection.Append(OsciNsPrefix);
                selection.Append(":ReceptionOfDelivery></");
            }
            else if (SelectionMode == SelectByRecentModification)
            {
                selection.Append("<");
                selection.Append(OsciNsPrefix);
                selection.Append(":RecentModification" + selectionAttributes + ">" + SelectionRule + "</");
                selection.Append(OsciNsPrefix);
                selection.Append(":RecentModification></");
            }
            if (quantityLimit >= 0)
            {
                selection.Append("<");
                selection.Append(OsciNsPrefix);
                selection.Append(":Quantity Limit=\"" + quantityLimit + "\"></");
                selection.Append(OsciNsPrefix);
                selection.Append(":Quantity>");
            }
            for (int i = 0; i < processCardBundles.Length; i++)
            {
                System.IO.MemoryStream outRenamed = new System.IO.MemoryStream();
                processCardBundles[i].WriteXml(outRenamed);
                char[] tmpChar;
                byte[] tmpByte;
                tmpByte = outRenamed.GetBuffer();
                tmpChar = new char[outRenamed.Length];
                System.Array.Copy(tmpByte, 0, tmpChar, 0, tmpChar.Length);
                sb.Append(new string(tmpChar));
            }
            sb.Append("</" + OsciNsPrefix + ":responseToFetchProcessCard>");
            Body = new Body(sb.ToString());
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
    }
}