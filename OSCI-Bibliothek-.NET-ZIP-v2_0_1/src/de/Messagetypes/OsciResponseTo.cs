using Osci.Common;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Resources;

namespace Osci.Messagetypes
{
    /// <summary> Diese Klasse ist die Superklasse aller OSCI-Antwortnachrichtenobjekte.
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
    public abstract class OsciResponseTo
        : OsciMessage
    {
        private FeedbackObject[] _feedbackObjects;

        /// <summary> Liefert das Kürzel der verwendeten Sprache.
        /// </summary>
        /// <value> Sprachkürzel
        /// </value>
        public string Language
        {
            get;
        }

        public void SetFeedback(string[] code)
        {
            FeedBack = new string[code.Length][];
            for (int i = 0; i < code.Length; i++)
            {
                FeedBack[i] = new string[3];
            }
            for (int i = 0; i < code.Length; i++)
            {
                FeedBack[i][0] = DialogHandler.LanguageList;
                FeedBack[i][1] = code[i];
                FeedBack[i][2] = Text.GetString(code[i]);
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
        public string[][] Feedback
        {
            set
            {
                FeedBack = value;
            }
            get
            {
                return FeedBack;
            }
        }
        public FeedbackObject[] FeedbackObjects
        {
            get
            {
                if (FeedBack == null)
                {
                    return null;
                }
                if (_feedbackObjects == null)
                {
                    _feedbackObjects = new FeedbackObject[FeedBack.Length];
                    for (int i = 0; i < FeedBack.Length; i++)
                    {
                        _feedbackObjects[i] = new FeedbackObject(FeedBack[i]);
                    }
                }
                return _feedbackObjects;
            }
        }


        protected internal string[][] FeedBack;
        internal ResourceBundle Text;

        private const string _defaultLanguage = "de";

        internal OsciResponseTo()
        {
        }

        internal OsciResponseTo(DialogHandler dialogHandler)
            : base(dialogHandler)
        {
            Language = _defaultLanguage;
            Text = ResourceBundle.GetBundle("Text", new System.Globalization.CultureInfo(_defaultLanguage));
            SupportClass.Tokenizer lang = new SupportClass.Tokenizer(DialogHandler.LanguageList.Trim(), " ");
            while (lang.HasMoreTokens())
            {
                Language = lang.NextToken();
                if (Language.IndexOf('-') == -1)
                {
                    Text = ResourceBundle.GetBundle("Text", new System.Globalization.CultureInfo(Language));
                }
                else
                {
                    Text = ResourceBundle.GetBundle("Text", new System.Globalization.CultureInfo(Language.Substring(0, 2) + "-" + Language.Substring(3)));
                }
                break;
            }
        }

        internal string WriteFeedBack()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("<");
            sb.Append(OsciNsPrefix);
            sb.Append(":Feedback>");
            for (int i = 0; i < FeedBack.Length; i++)
            {
                sb.Append("<");
                sb.Append(OsciNsPrefix);
                sb.Append(":Entry xml:lang=\"");
                sb.Append(FeedBack[i][0]);
                sb.Append("\"><");
                sb.Append(OsciNsPrefix);
                sb.Append(":Code>");
                sb.Append(FeedBack[i][1]);
                sb.Append("</");
                sb.Append(OsciNsPrefix);
                sb.Append(":Code><");
                sb.Append(OsciNsPrefix);
                sb.Append(":Text>");
                sb.Append(FeedBack[i][2]);
                sb.Append("</");
                sb.Append(OsciNsPrefix);
                sb.Append(":Text></");
                sb.Append(OsciNsPrefix);
                sb.Append(":Entry>");
            }
            sb.Append("</");
            sb.Append(OsciNsPrefix);
            sb.Append(":Feedback>");
            return sb.ToString();
        }

        /// <summary> Bringt eine Supplier-Signatur an.
        /// </summary>
        public virtual void Sign()
        {
            base.Sign(DialogHandler.Supplier);
        }
    }
}