using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Osci.MessageParts
{
    public struct CheckInstance
    {

        public static CheckInstance PartialStoreDelivery = new CheckInstance(true, true, true, false, true);

        public static CheckInstance ResponsePartialStoreDelivery = new CheckInstance(false, true, false, true, true);

        public static CheckInstance PartialFetchDelivery = new CheckInstance(true, true, false, true, false);

        public static CheckInstance ResponsePartialFetchDelivery = new CheckInstance(false, true, true, false, true);

        private bool _chunkSizeCheck;

        private bool _chunkNumberCheck;

        private bool _totalMessageSizeCheck;

        private bool _totalChunkNumberCheck;

        private bool _receivedChunksCheck;

        CheckInstance(bool chunkSizeCheck,
              bool chunkNumberCheck,
              bool totalMessageSizeCheck,
              bool receivedChunksCheck,
              bool totalChunkNumberCheck)
        {
            _chunkSizeCheck = chunkSizeCheck;
            _chunkNumberCheck = chunkNumberCheck;
            _totalMessageSizeCheck = totalMessageSizeCheck;
            _receivedChunksCheck = receivedChunksCheck;
            _totalChunkNumberCheck = totalChunkNumberCheck;
        }

        /**
         * @return True sobald das Attribut ChunkSize Pflicht ist.
         */
        public bool IsChunkSizeCheck()
        {
            return _chunkSizeCheck;
        }

        /**
         * @return True sobald das Attribut ChunkNumber Pflicht ist.
         */
        public bool IsChunkNumberCheck()
        {
            return _chunkNumberCheck;
        }

        /**
         * @return True sobald das Attribut TotalMessageSize Pflicht ist.
         */
        public bool IsTotalMessageSizeCheck()
        {
            return _totalMessageSizeCheck;
        }

        /**
         * @return True sobald das Attribut ReceivedChunks Pflicht ist.
         */
        public bool IsReceivedChunksCheck()
        {
            return _receivedChunksCheck;
        }

        /**
         * @return True sobald das Attribut TotalChunkNumber Pflicht ist.
         */
        public bool IsTotalChunkNumberCheck()
        {
            return _totalChunkNumberCheck;
        }
    }
}
