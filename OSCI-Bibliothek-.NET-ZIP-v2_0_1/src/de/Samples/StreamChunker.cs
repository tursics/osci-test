using Osci.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Osci.Helper
{
    internal class StreamChunker : IDisposable
    {
        private Stream input;

        private byte[] buffer;

        private static readonly int bufferSize = 1024;

        // Takes an input stream and writes it chunk for chunk to output streams.
        //
        // @param input the input stream
        // @param chunkSize the size of the chunk in bytes
        public StreamChunker(Stream input)
        {
            this.input = input;
            this.buffer = new byte[bufferSize];
        }

        // @return true if there are still bytes in the input stream available to be written to a new chunk.
        public bool ChunkRemaining()
        {
            return input.Position < input.Length;
        }

        // Writes bytes to the output stream till the chunk size is reached or no bytes are available anymore in the
        // input stream.
        //
        // @param out the output stream
        // @return the number of bytes written to the output stream
        public long WriteChunk(Stream output, long chunkSize)
        {
            long bytesWritten = 0;
            int bytesRead;
            while (bytesWritten < chunkSize)
            {
                bytesRead = input.Read(buffer, 0, bufferSize);
                if (bytesRead > 0)
                {
                    bytesWritten += bytesRead;
                    output.Write(buffer, 0, bytesRead);
                }
                else
                {
                    break;
                }
            }
            return bytesWritten;
        }

        public void Dispose()
        {
            ((IDisposable)input).Dispose();
        }
    }
}