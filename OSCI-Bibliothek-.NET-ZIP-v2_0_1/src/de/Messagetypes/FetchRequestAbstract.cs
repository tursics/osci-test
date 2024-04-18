using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;
using System;
using System.IO;

namespace Osci.Messagetypes
{
    // Storestreams

    /// <summary><p>Die Klasse ist die Superklasse der AbholAuftrags-Objekte.</p>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    ///
    /// <p>Author: A. Mergenthal</p>
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public abstract class FetchRequestAbstract
        : OsciRequest
    {
        private int _selectionMode = -1;

        protected FetchRequestAbstract()
        { }

        public FetchRequestAbstract(DialogHandler dh) : base(dh)
        {
        }

        /// <summary> Ruft die Auswahlregel für die abzuholende Nachricht ab, oder legt diese fest. Der Inhalt des
        /// übergebenen Strings hängt vom gewählten Auswahlmodus ab und kann
        /// entweder in einer Message-Id oder einem Datum bestehen.
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
            get;
            set;
        }

        /// <summary> Ruft den Auswahlmodus ab, oder setzt diesen. Mögliche Werte sind
        /// SELECT_BY_MESSAGE_ID, SELECT_BY_DATE_OF_RECEPTION oder NO_SELECTION_RULE
        /// (default).
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
                if ((value != SelectByMessageId) &&
                    (value != SelectByDateOfReception) &&
                    (value != NoSelectionRule))
                {
                    throw new ArgumentException("Ungültiger Auswahlmodus. " + "Es sind nur die Werte SELECT_BY_MESSAGE_ID, SELECT_BY_DATE_OF_RECEPTION und NO_SELECTION_RULE erlaubt.");
                }

                _selectionMode = value;
            }
        }
        

        protected string GetSelectionRuleString()
        {
            // todo: Evtl. Plausis Selection-Mode / Selection-Rule-Format checken (Datum/MsgID)
            if (_selectionMode == SelectByMessageId || _selectionMode == SelectByDateOfReception)
            {
                if (SelectionRule == null)
                {
                    throw new IllegalStateException(DialogHandler.ResourceBundle.GetString("missing_entry") + ": SelectionRule");
                }

                System.Text.StringBuilder selection = new System.Text.StringBuilder("<");
                selection.Append(OsciNsPrefix);
                selection.Append(":SelectionRule><");
                selection.Append(OsciNsPrefix);

                if (_selectionMode == SelectByMessageId)
                {
                    selection.Append(":MessageId>");
                    selection.Append(Base64.Encode(SelectionRule.ToByteArray()));
                    selection.Append("</");
                    selection.Append(OsciNsPrefix);
                    selection.Append(":MessageId></");
                }
                else if (_selectionMode == SelectByDateOfReception)
                {
                    selection.Append(":ReceptionOfDelivery>");
                    selection.Append(SelectionRule);
                    selection.Append("</");
                    selection.Append(OsciNsPrefix);
                    selection.Append(":ReceptionOfDelivery></");
                }
                else
                {
                    selection.Remove(0, selection.Length);
                }

                if (selection.Length > 0)
                {
                    selection.Append(OsciNsPrefix);
                    selection.Append(":SelectionRule>");
                }

                return selection.ToString();
            }
            else { return ""; }
        }
    }
}