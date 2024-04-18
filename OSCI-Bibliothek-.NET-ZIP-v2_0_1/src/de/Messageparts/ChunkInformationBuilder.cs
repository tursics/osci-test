using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Messagetypes;
using System.Collections.Generic;
using System.Text;

namespace Osci.MessageParts
{
    internal class ChunkInformationBuilder : MessagePartParser
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(ChunkInformationBuilder));
        private ChunkInformation _chunkInformation = null;

        public ChunkInformationBuilder(XmlReader xmlReader, DefaultHandler parentHandler, CheckInstance checkInstance)
            : base(xmlReader, parentHandler)
        {
            _chunkInformation = new ChunkInformation(checkInstance);
            OsciMessage msg = ((OsciMessageBuilder)parentHandler).OsciMessage;
            _chunkInformation.SetNamespacePrefixes(msg);
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start Element" + localName);
            if (localName.Equals("ChunkInformation") && uri.Equals(Osci2017Xmlns))
            {
                if (attributes.GetValue("ChunkNumber") != null)
                {
                    _chunkInformation.ChunkNumber = int.Parse(attributes.GetValue("ChunkNumber"));
                }
                if (attributes.GetValue("TotalChunkNumbers") != null)
                {
                    _chunkInformation.TotalChunkNumbers = int.Parse(attributes.GetValue("TotalChunkNumbers"));
                }
                if (attributes.GetValue("TotalMessageSize") != null)
                {
                    _chunkInformation.TotalMessageSize = long.Parse(attributes.GetValue("TotalMessageSize"));
                }
                if (attributes.GetValue("ChunkSize") != null)
                {
                    _chunkInformation.ChunkSize = long.Parse(attributes.GetValue("ChunkSize"));
                }
                if (attributes.GetValue("ReceivedChunks") != null)
                {
                    _chunkInformation.ReceivedChunks = ParseReceivedChunks(attributes.GetValue("ReceivedChunks"));
                }
            }
            else
            {
                throw new SaxException("Unerwartetes Element im ContentContainer: " + localName);
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
            _log.Debug("End-Element: " + qName);

            if (localName.Equals("ChunkInformation") && uri.Equals(Osci2017Xmlns))
            {
                ParentHandler.EndElement(uri, localName, qName);
                XmlReader.ContentHandler = ParentHandler;
            }
            else
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
            }

            CurrentElement = null;
        }

        private List<int> ParseReceivedChunks(string receivedChunks)
        {
            if (receivedChunks == null)
            {
                return null;
            }
            List<int> arrayChunks = new List<int>();
            foreach (string item in receivedChunks.Split(' '))
            {
                arrayChunks.Add(int.Parse(item));
            }
            return arrayChunks;
        }

        public ChunkInformation GetChunkInformationObject()
        {
            return _chunkInformation;
        }
    }
}