using System.IO;
using Osci.Extensions;

namespace Osci.SoapHeader
{
    /// <exclude/>
    /// <summary> <p> Die Elemente eines ControlBlocks.</p>
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
    public class ControlBlockH
        : HeaderEntry
    {
        public ControlBlockH()
        {
        }

        internal ControlBlockH(string refdId)
        {
            RefId = refdId;
        }

        /// <summary>Challenge der Nachricht. 
        /// </summary>
        private string _challenge;

        /// <summary>Response der Nachricht. 
        /// </summary>
        private string _response;

        /// <summary>Sequence Nummer der Nachricht. Sequenz beginnt mit dem Value 0 
        /// </summary>
        private int _sequenceNumber = -1;

        /// <summary>Die durch den Supplier vergebene DialogID. 
        /// </summary>
        private string _conversationId;


        public virtual string Challenge
        {
            get
            {
                return _challenge;
            }
            set
            {
                _challenge = value;
            }
        }

        public virtual string ConversationId
        {
            get
            {
                return _conversationId;
            }
            set
            {
                _conversationId = value;
            }
        }

        public virtual string Response
        {
            get
            {
                return _response;
            }
            set
            {
                _response = value;
            }
        }

        public virtual int SequenceNumber
        {
            get
            {
                return _sequenceNumber;
            }
            set
            {
                _sequenceNumber = value;
            }
        }

        public override byte[] GetDigestValue(string digestAlgorithm)
        {
            // Im ControlBlockH dürfen keine temporären Buffer verwendet werden, weil dieses
            // Objekt als Bestandteil des DialogHandlers immer wieder verändert wird.		
            DigestValues.Remove(digestAlgorithm);
            return base.GetDigestValue(digestAlgorithm);
        }

        public override void WriteXml(Stream stream)
        {
            stream.Write("<" + OsciNsPrefix + ":ControlBlock");
            stream.Write(Ns, 0, Ns.Length);
            if (_conversationId != null)
            {
                stream.Write(" ConversationId=\"" + _conversationId + "\"");
            }
            stream.Write(" Id=\"controlblock\"");

            if (_sequenceNumber != -1)
            {
                stream.Write(" SequenceNumber=\"" + _sequenceNumber + "\"");
            }

            stream.Write(" " + SoapNsPrefix + ":actor=\"http://schemas.xmlsoap.org/soap/actor/next\" " + SoapNsPrefix + ":mustUnderstand=\"1\"");
            stream.Write(">");

            if (_response != null)
            {
                stream.Write("<" + OsciNsPrefix + ":Response>" + _response + "</" + OsciNsPrefix + ":Response>");
            }
            if (_challenge != null)
            {
                stream.Write("<" + OsciNsPrefix + ":Challenge>" + _challenge + "</" + OsciNsPrefix + ":Challenge>");
            }
            stream.Write("</" + OsciNsPrefix + ":ControlBlock>");
        }

    }
}