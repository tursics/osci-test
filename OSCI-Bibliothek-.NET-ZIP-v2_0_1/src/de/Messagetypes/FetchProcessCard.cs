using System.IO;
using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;

namespace Osci.Messagetypes
{
    /// <summary><p><H4>Laufzettelabholauftrag</H4></p>
    /// Mit dieser Klasse werden Nachrichtenobjekte für Laufzettelabholaufträge
    /// angelegt. Clients können hiermit Laufzettel eingegangener Nachrichten vom Intermediär
    /// abrufen.  Als Antwort auf diese Nachricht erhalten sie vom Intermediär
    /// ein ResponseToFetchProcessCard-Nachrichtenobjekt, welches eine Rückmeldung über den
    /// Erfolg der Operation und ggf. die gewünschten Laufzettel enthält.
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
    /// <seealso cref="ResponseToFetchProcessCard">
    /// </seealso>
    public class FetchProcessCard
        : OsciRequest
    {
        private int _roleForSelection = -1;
        private bool _selectNoReceptionOnly;
        private int _selectionMode = -1;
        private long _quantityLimit = -1;

        /// <summary> Ruft die Auswahlregel für die abzuholende Nachricht ab, oder setzt diese. Der Inhalt des
        /// übergebenen Strings hängt vom gewählten Auswahlmodus ab und kann
        /// entweder in einer Base64-codierten Message-Id oder einem Datum bestehen.
        /// Mehrere Message-Ids können durch "&amp;" getrennt aneinandergereiht werden.
        /// Das Format eines Datums muß dem XML-Schema nach
        /// <a href="http://www.w3.org/TR/xmlschema-2/#dateTime">
        /// http://www.w3.org/TR/xmlschema-2/#dateTime</a> (ISO 8601-Format) entsprechen.
        /// </summary>
        /// <value>Message-Id oder Datum
        /// </value>
        /// <seealso cref="SelectionMode()">
        /// </seealso>
        public string SelectionRule
        {
            get; set;
        }

        // <summary> BEZOGEN AUF getSelectionMode: Liefert den gesetzten Auswahlmodus.
        // </summary>
        // <returns> den Auswahlmodus SELECT_BY_MESSAGE_ID, SELECT_BY_DATE_OF_RECEPTION,
        // SELECT_BY_RECENT_MODIFICATION oder NO_SELECTION_RULE
        // </returns>
        // <seealso cref="SelectionRule(String)">
        // </seealso>
        // <seealso cref="SelectionMode()">
        // </seealso>

        /// <summary> Ruft den Auswahlmodus ab, oder setzt diesen. Mögliche Werte sind
        /// SELECT_BY_MESSAGE_ID, SELECT_BY_DATE_OF_RECEPTION,
        /// SELECT_BY_RECENT_MODIFICATION oder NO_SELECTION_RULE (default).
        /// </summary>
        /// <value>Auswahlmodus
        /// </value>
        /// <seealso cref="SelectionRule()">
        /// </seealso>
        public int SelectionMode
        {
            get
            {
                return _selectionMode;
            }
            set
            {
                if ((value != SelectByMessageId) && (value != SelectByDateOfReception) && (value != SelectByRecentModification) && (value != NoSelectionRule))
                {
                    throw new System.ArgumentException("Ungültiger Auswahlmodus. " + "Es sind nur die Werte SELECT_BY_MESSAGE_ID, SELECT_BY_DATE_OF_RECEPTION, " + "SELECT_BY_RECENT_MODIFICATION und NO_SELECTION_RULE erlaubt.");
                }
                _selectionMode = value;
            }
        }

        /// <summary> Ruft die maximale Anzahl zurückzugebender Laufzettel ab, oder setzt diese.
        /// </summary>
        /// <value> gesetzte maximale Anzahl.
        /// </value>
        public long QuantityLimit
        {
            get
            {
                return _quantityLimit;
            }
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentException("Limit für die Zahl der Laufzettel muß größer 0 sein.");
                }
                _quantityLimit = value;
            }
        }

        /// <summary> Legt ein Nachrichtenobjekt für einen Laufzettelabholauftrag an.
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt des Dialogs, innerhalb dessen die Nachricht
        /// versendet werden soll.
        /// </param>
        /// <seealso cref="DialogHandler">
        /// </seealso>
        public FetchProcessCard(DialogHandler dh)
            : base(dh)
        {
            Originator = (Originator)dh.Client;
            MessageType = FetchProcessCard;
            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.SequenceNumber = DialogHandler.Controlblock.SequenceNumber + 1;
        }

        internal FetchProcessCard()
        {
            MessageType = FetchProcessCard;
        }

        /// <summary> Versendet die Nachricht und liefert die Antwortnachricht zurück.
        /// Diese Methode wirft eine Exception, wenn beim Aufbau oder Versand der
        /// Nachricht ein Fehler auftritt. Fehlermeldungen vom Intermediär müssen
        /// dem Feedback der Antwortnachricht entnommen werden.
        /// </summary>
        /// <returns> das Antwortnachricht-Objekt
        /// </returns>
        // --> Storestreams:
        public virtual ResponseToFetchProcessCard Send()
        {
            return (ResponseToFetchProcessCard)Transmit(null, null);
        }

        public virtual ResponseToFetchProcessCard Send(Stream storeOutput, Stream storeInput)
        {
            return (ResponseToFetchProcessCard)Transmit(storeOutput, storeInput);
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
        /// <seealso href="SELECT_ORIGINATOR">
        /// </seealso>
        /// <seealso href="SELECT_ADDRESSEE">
        /// </seealso>
        /// <seealso href="SELECT_ALL">
        /// </seealso>
        /// <seealso cref="SelectionRule">
        /// </seealso>
        /// <seealso cref="SelectionMode">
        /// </seealso>
        public int RoleForSelection
        {
            get
            {
                return _roleForSelection;
            }
            set
            {
                if ((value != SelectOriginator) && (value != SelectAddressee) && (value != SelectAll))
                {
                    throw new IllegalArgumentException(DialogHandler.ResourceBundle.GetString("invalid_firstargument") + value);
                }
                _roleForSelection = value;
            }
        }

        /// <summary> Legt fest, ob nur Laufzettel von Nachrichten zurückgegeben werden sollen,
        /// für die keine Empfangsbestätigung vom Empfänger vorliegt.</summary>
        /// <value>noReceptionOnly true -> es werden nur Laufzettel für nicht zugestellte
        /// Nachrichten zurückgegeben. false -> es werden alle Laufzettel zurückgegeben
        /// (default). <b>Diese Einstellung ist nur in den Selection-Modes
        /// SELECT_BY_DATE_OF_RECEPTION und SELECT_BY_RECENT_MODIFICATION wirksam.</b>
        /// </value>
        /// <seealso cref="SelectionRule">
        /// </seealso>
        /// <seealso cref="SelectionMode">
        /// </seealso>
        /// <seealso cref="RoleForSelection">
        /// </seealso>
        public bool SelectNoReceptionOnly
        {
            set
            {
                _selectNoReceptionOnly = value;
            }
        }

        /// <summary> Legt fest, ob nur Laufzettel von Nachrichten zurückgegeben werden sollen,
        /// für die keine Empfangsbestätigung vom Empfänger vorliegt.
        /// Gibt an, ob Liefert den Identifier für das Auswahlkriterium, ob nur Laufzettel von
        /// Nachrichten zurückgegeben werden sollen, die an den oder vom Absender des
        /// Laufzettelabholauftrags geschickt wurden.
        /// </summary>
        /// <seealso cref="RoleForSelection">
        /// </seealso>
        public bool IsSelectNoReception()
        {
            return _selectNoReceptionOnly;
        }

        public override void Compose()
        {
            base.Compose();
            if (DialogHandler.Controlblock.Challenge == null)
            {
                throw new System.SystemException("Kein Challenge-Wert in DialogHandler eingestellt.");
            }

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
                throw new System.SystemException("Kein Squenznummer in DialogHandler eingestellt.");
            }

            //System.String selection = "";
            string selectionAttributes = "";

            if (_selectNoReceptionOnly)
            {
                selectionAttributes += " NoReception=\"true\"";
            }

            if (_roleForSelection == SelectAddressee)
            {
                selectionAttributes += " Role=\"Addressee\"";
            }
            else if (_roleForSelection == SelectOriginator)
            {
                selectionAttributes += " Role=\"Originator\"";
            }

            System.Text.StringBuilder selection = new System.Text.StringBuilder("<");
            selection.Append(OsciNsPrefix);
            selection.Append(":SelectionRule>");
            if (_selectionMode == SelectByMessageId)
            {
                string[] msgIds = SelectionRule.Split('&');
                for (int i = 0; i < msgIds.Length; i++)
                {
                    selection.Append("<" + OsciNsPrefix + ":MessageId>");
                    selection.Append(Base64.Encode(msgIds[i].ToByteArray()));
                    selection.Append("</" + OsciNsPrefix + ":MessageId>");
                }
                selection.Append("</");
            }
            else if (_selectionMode == SelectByDateOfReception)
            {
                selection.Append("<" + OsciNsPrefix + ":ReceptionOfDelivery" + selectionAttributes + ">" + SelectionRule + "</" + OsciNsPrefix + ":ReceptionOfDelivery></");
            }
            else if (_selectionMode == SelectByRecentModification)
            {
                selection.Append("<" + OsciNsPrefix + ":RecentModification" + selectionAttributes + ">" + SelectionRule + "</" + OsciNsPrefix + ":RecentModification></");
            }
            else
                selection.Remove(0, selection.Length);

            if (selection.Length > 0)
            {
                selection.Append(OsciNsPrefix);
                selection.Append(":SelectionRule>");
            }
            if (_quantityLimit >= 0)
            {
                selection.Append("<" + OsciNsPrefix + ":Quantity Limit=\"" + _quantityLimit + "\"></" + OsciNsPrefix + ":Quantity>");
            }
            Body = new Body("<" + OsciNsPrefix + ":fetchProcessCard>" + selection + "</" + OsciNsPrefix + ":fetchProcessCard>");
            StateOfMessage |= StateComposed;
            Body.SetNamespacePrefixes(this);
        }

        public override void WriteXml(Stream stream)
        {
            base.WriteXml(stream);

            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(stream);
            }

            // DesiredLanguage
            DesiredLanguagesH.WriteXml(stream);

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
    }
}