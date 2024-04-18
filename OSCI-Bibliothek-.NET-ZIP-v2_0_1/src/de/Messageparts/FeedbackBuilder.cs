using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Messagetypes;
using System.Collections.Generic;
using System.Text;

namespace Osci.MessageParts
{
    /// <summary><H4>Feedback-Parser</H4>
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

    internal class FeedbackBuilder
        : MessagePartParser
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(FeedbackBuilder));
        private readonly List<string[]> _feedbacks = new List<string[]>();
        private string[] _entry;
        private bool isInsideFeedback = false;

        public FeedbackBuilder(OsciMessageBuilder parent)
            : base(parent)
        {
        }

        /// <summary> Constructor for the FeedbackBuilder object
        /// </summary>
        public FeedbackBuilder(OsciMessageBuilder parent, bool isInsideFeedback)
            : base(parent)
        {
            this.isInsideFeedback = isInsideFeedback;
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start Element in Feedback: " + localName);

            if (localName.Equals("Entry") && uri.Equals(OsciXmlns))
            {
                _entry = new string[3];
                _entry[0] = attributes.GetValue("xml:lang");
            }
            else if (localName.Equals("Code") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new StringBuilder();
            }
            else if (localName.Equals("Text") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new StringBuilder();
            }
            else
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
            }
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End Element in Feedback: " + localName);

            if (isInsideFeedback && localName.Equals("InsideFeedback") && uri.Equals(Osci2017Xmlns))
            {
                ParentHandler.EndElement(uri, localName, qName);
                XmlReader.ContentHandler = ParentHandler;
            }
            else if (localName.Equals("Feedback") && uri.Equals(OsciXmlns))
            {
                string[][] fd = new string[_feedbacks.Count][];
                for (int i = 0; i < _feedbacks.Count; i++)
                {
                    fd[i] = new string[3];
                    fd[i][0] = _feedbacks[i][0];
                    fd[i][1] = _feedbacks[i][1];
                    fd[i][2] = _feedbacks[i][2];
                }
                ((OsciResponseTo)Msg).FeedBack = fd;

                XmlReader.ContentHandler = ParentHandler;
            }
            else if (localName.Equals("Entry") && uri.Equals(OsciXmlns))
            {
                _feedbacks.Add(_entry);
            }
            else if (localName.Equals("Code") && uri.Equals(OsciXmlns))
            {
                _entry[1] = CurrentElement.ToString();
            }
            else if (localName.Equals("Text") && uri.Equals(OsciXmlns))
            {
                _entry[2] = CurrentElement.ToString();
            }
            else
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
            }

            CurrentElement = null;
        }

        public string[][] GetFeedback()
        {
            string[][] fd = new string[_feedbacks.Count][];
            for (int i = 0; i < _feedbacks.Count; i++)
            {
                fd[i] = new string[3];
                fd[i][0] = _feedbacks[i][0];
                fd[i][1] = _feedbacks[i][1];
                fd[i][2] = _feedbacks[i][2];
            }
            return fd;
        }
    }
}