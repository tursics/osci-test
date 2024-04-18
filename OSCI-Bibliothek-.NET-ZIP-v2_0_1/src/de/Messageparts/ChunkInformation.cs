using Osci.Common;
using Osci.Exceptions;
using Osci.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Osci.MessageParts
{
    public class ChunkInformation
        : MessagePart
    {

        private long _chunkNumber = 0;

        public long _chunkSize = 0;

        private long _totalChunkNumbers = 0;

        private long _totalMessageSize = 0;

        private List<int> _receivedChunks = new List<int>();


        public long ChunkNumber
        {
            get { return _chunkNumber; }
            set
            {
                if (!CheckInstance.IsChunkNumberCheck())
                {
                    throw new IllegalArgumentException(DialogHandler.ResourceBundle.GetString("invalid_firstargument") + value);
                }
                _chunkNumber = value;
            }
        }

        public long ChunkSize
        {
            get { return _chunkSize; }
            set
            {
                if (!CheckInstance.IsChunkSizeCheck())
                {
                    throw new IllegalArgumentException(DialogHandler.ResourceBundle.GetString("invalid_firstargument") + value);
                }
                _chunkSize = value;
            }
        }

        public long TotalChunkNumbers
        {
            get { return _totalChunkNumbers; }
            set
            {
                if (!CheckInstance.IsTotalChunkNumberCheck())
                {
                    throw new IllegalArgumentException(DialogHandler.ResourceBundle.GetString("invalid_firstargument") + value);
                }
                _totalChunkNumbers = value;
            }
        }


        public long TotalMessageSize
        {
            get { return _totalMessageSize; }
            set
            {
                if (!CheckInstance.IsTotalMessageSizeCheck())
                {
                    throw new IllegalArgumentException(DialogHandler.ResourceBundle.GetString("invalid_firstargument") + value);
                }
                _totalMessageSize = value;
            }
        }
        

        public List<int> ReceivedChunks
        {
            get { return _receivedChunks; }
            set
            {
                if (!CheckInstance.IsReceivedChunksCheck())
                {
                    throw new IllegalArgumentException(DialogHandler.ResourceBundle.GetString("invalid_firstargument") + value);
                }
                _receivedChunks = value;
            }
        }

        public CheckInstance CheckInstance { get; set; }

        public ChunkInformation(CheckInstance checkInstance)
        {
            CheckInstance = checkInstance;
        }

        public ChunkInformation(long chunkSize, int chunkNumber, long totalMessageSize, int totalChunkNumbers)
        {
            CheckInstance = CheckInstance.PartialStoreDelivery;
            ChunkSize = chunkSize;
            ChunkNumber = chunkNumber;
            TotalMessageSize = totalMessageSize;
            TotalChunkNumbers = totalChunkNumbers;
        }

        public ChunkInformation(long chunkSize, int chunkNumber, List<int> receivedChunks)
        {
            CheckInstance = CheckInstance.PartialFetchDelivery;
            ChunkSize = chunkSize;
            ChunkNumber = chunkNumber;
            if (receivedChunks == null || receivedChunks.Count == 0)
            {
                ReceivedChunks = new List<int>() { -1 };
            }
            else
            {
                ReceivedChunks = receivedChunks;
            }
        }

        public override void WriteXml(Stream stream)
        {
            stream.Write("<" + Osci2017NsPrefix + ":ChunkInformation");
            if (ChunkNumber != 0)
            {
                stream.Write(" ChunkNumber=\"" + ChunkNumber + "\"");
            }
            if (ChunkSize != 0)
            {
                stream.Write(" ChunkSize=\"" + ChunkSize + "\"");
            }
            if (ReceivedChunks.Any())
            {
                stream.Write(" ReceivedChunks=\"");
                for (int i = 0; i < ReceivedChunks.Count - 1; i++)
                {
                    stream.Write(ReceivedChunks[i] + " ");
                }
                stream.Write(ReceivedChunks[ReceivedChunks.Count - 1] + "\"");
            }
            if (TotalChunkNumbers != 0)
            {
                stream.Write(" TotalChunkNumbers=\"" + TotalChunkNumbers + "\"");
            }
            if (TotalMessageSize != 0)
            {
                stream.Write(" TotalMessageSize=\"" + TotalMessageSize + "\"");
            }
            stream.Write("></" + Osci2017NsPrefix + ":ChunkInformation>");
        }
    }
}