using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Messagetypes;
using Osci.Roles;

namespace Osci.SoapHeader
{
    /// <exclude/>
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class ControlBlockHBuilder
        : MessagePartParser
    {
        /// <summary> Gets the controlBlock attribute of the ControlBlockHBuilder object
        /// </summary>
        /// <value>The controlBlock value
        /// </value>
        public ControlBlockH ControlBlock
        {
            get
            {
                return _cb;
            }

        }
        private static readonly Log _log = LogFactory.GetLog(typeof(ControlBlockHBuilder));

        //  Array steht für {Response,challenge,conversationID,SequenzNumber}
        /*
         *  private int[][] checkRequest = {{0, 0, 0, 0},    // undef
         *  {0, 1, 0, 0},    // initdialog
         *  {1, 1, 1, 1},    // exitdialog
         *  {-1, 1, -1, 1},  // getmsgid
         *  {-1, 1, -1, 1},  // storedel
         *  {1, 1, 1, 1},    // fetchdel
         *  {1, 1, 1, 1},    // fetchproccard
         *  {-1, 1, -1, 1},  // forwarddel
         *  {0, 1, 0, 0},    // acceptdel
         *  {1, 1, 1, 1},    // mediatedel
         *  {0, 1, 0, 0}};   // processdel
         *  private int[][] checkResponse = {{0, 0, 0, 0},   // resptoundef
         *  {1, 1, 1, 0},    // resptoinitdialog
         *  {1, 0, 1, 1},    // resptoexitdialog
         *  {1, -1, 1, 1},   // resptogetmsgid
         *  {1, -1, 1, 1},   // resptostoredel
         *  {1, 1, 1, 1},    // resptofetchdel
         *  {1, 1, 1, 1},    // resptofetchproccard
         *  {1, -1, 1, 1},   // resptoforwarddel
                                             geändert, weil ConvId in impliziten Dialogen unsinnig
         *  {1, -1, -1, 1},   // resptoforwarddel
         *  {1, -1, 1, 1},   // resptoacceptdel
                                             geändert, weil ConvId in impliziten Dialogen unsinnig
         *  {1, -1, 0, 0},   // resptoacceptdel
         *  {1, 1, 1, 1},    // resptomediatedel
         *  {1, -1, -1, 1}};   // resptoprocessdel
         */
        private readonly int[] _check;

        private readonly ControlBlockH _cb;

        /// <summary> Constructor for the ControlBlockHBuilder object
        /// </summary>
        /// <param name="parent">
        /// </param>
        /// <param name="attributes">
        /// </param>
        /// <param name="check">
        /// </param>
        public ControlBlockHBuilder(OsciMessageBuilder parent, Attributes attributes, int[] check)
            : base(parent)
        {
            parent.SignatureRelevantElements.AddElement("ControlBlock", OsciXmlns, attributes);

            _check = check;

            _cb = attributes.GetValue("Id") != null
                ? new ControlBlockH(attributes.GetValue("Id"))
                : new ControlBlockH();

            _cb.ConversationId = attributes.GetValue("ConversationId");
            _cb.SetNamespacePrefixes(Msg);
            if (attributes.GetValue("SequenceNumber") != null)
            {
                _cb.SequenceNumber = int.Parse(attributes.GetValue("SequenceNumber"));
            }
        }

        /// <summary> 
        /// </summary>
        /// <param name="uri">
        /// </param>
        /// <param name="localName">
        /// </param>
        /// <param name="qName">
        /// </param>
        /// <param name="attributes">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("init ControlBlock  Element: " + localName);
            if (localName.Equals("Response") && uri.Equals(OsciXmlns) || localName.Equals("Challenge") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new System.Text.StringBuilder();
            }
            else
            {
                throw new SaxException("Unerwartetes Element im ControlBlock: " + localName);
            }
        }

        /// <summary> 
        /// </summary>
        /// <param name="uri">
        /// </param>
        /// <param name="localName">
        /// </param>
        /// <param name="qName">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End-Element Von ControlBlock: " + localName);
            if (localName.Equals("Response") && uri.Equals(OsciXmlns))
            {
                _cb.Response = CurrentElement.ToString();
            }
            else if (localName.Equals("Challenge") && uri.Equals(OsciXmlns))
            {
                _cb.Challenge = CurrentElement.ToString();
            }
            else if (localName.ToUpper().Equals("ControlBlock".ToUpper()) && uri.Equals(OsciXmlns))
            {
                if ((_check[0] == 1) && (_cb.Response == null))
                {
                    throw new SaxException("Erforderlicher Response-Wert für das Element ControlBlock nicht vorhanden!");
                }
                else if ((_check[0] == 0) && (_cb.Response != null))
                {
                    throw new SaxException("Nicht erlaubter Response-Wert für das Element ControlBlock vorhanden!");
                }

                if ((_check[1] == 1) && (_cb.Challenge == null))
                {
                    throw new SaxException("Erforderlicher Challenge-Wert für das Element ControlBlock nicht vorhanden !");
                }
                else if ((_check[1] == 0) && (_cb.Challenge != null))
                {
                    _log.Debug("\n\n " + _check[0] + _check[1] + _check[2] + _check[3]);
                    throw new SaxException("Nicht erlaubter Challenge-Wert für das Element ControlBlock vorhanden !");
                }

                if ((_check[2] == 1) && (_cb.ConversationId == null))
                {
                    throw new OsciErrorException("9802", Msg);
                }
                else if ((_check[2] == 0) && (_cb.ConversationId != null))
                {
                    throw new SaxException("Nicht erlaubte Conversation-Id für das Element ControlBlock vorhanden !");
                }

                if ((_check[3] == 1) && (_cb.SequenceNumber == -1))
                {
                    throw new SaxException("Erforderliche Sequence-Number für das Element ControlBlock nicht vorhanden !");
                }
                else if ((_check[3] == 0) && (_cb.SequenceNumber != -1))
                {
                    throw new SaxException("Nicht erlaubte Sequence-Number für das Element ControlBlock vorhanden !");
                }
                DialogHandler dh = Msg.DialogHandler;
                if ((_check[0] & _check[1] & _check[2] & _check[3]) < 0)
                {
                    dh = new DialogHandler(null, new Intermed(null, null, null), null);
                    dh.Controlblock.ConversationId = _cb.ConversationId;
                    dh.Controlblock.Challenge = _cb.Challenge;
                    dh.Controlblock.Response = _cb.Response;
                    dh.Controlblock.SequenceNumber = _cb.SequenceNumber;
                }
                else
                {
                    if (dh == null)
                    {
                        if (_cb.ConversationId != null)
                        {
                            dh = DialogHandler.FindDialog(_cb);
                        }
                        else
                        {
                            dh = new DialogHandler(null, (Intermed)null, null);
                            _log.Trace("!!!!!!NEUER CB!!!!!!!!!! " + _cb.SequenceNumber);
                        }
                    }
                    // An dieser Stelle wird die Sequenz-Nummer auf der Supplierseite hochgezählt
                    if (Msg is OsciRequest)
                    {
                        if (_cb.SequenceNumber > -1)
                        {
                            dh.Controlblock.SequenceNumber = dh.Controlblock.SequenceNumber + 1;
                        }
                    }
                    dh.CheckControlBlock(_cb);
                }
                Msg.DialogHandler = dh;
                Msg.ControlBlock = _cb;
                XmlReader.ContentHandler = ParentHandler;
            }
            CurrentElement = null;
        }
    }
}