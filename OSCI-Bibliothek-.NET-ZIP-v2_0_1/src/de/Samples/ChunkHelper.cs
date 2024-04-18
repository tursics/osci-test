using Osci.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Osci.Samples
{
    //This is a part of the demo application for an asynchronous communication
    //scenario with partial messages according to the OSCI 1.2-transport
    //specification. The main method needs the intermediary's URL as parameter.
    // <p>
    //Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany
    //</p>
    //<p>
    //Erstellt von Governikus GmbH & Co. KG
    //</p>
    //<p>
    //Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    ///Public Licence genutzt werden.
    //</p>
    //Die Lizenzbestimmungen können unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.
    //</p>
    //@author J. Buckelo, A. Mergenthal
    //@version 0.9
    //@since 1.8.0
    public class ChunkHelper
    {
        public enum Mode
        {
            CONSTANTSIZE, OPTIMIZEDSTORE
        }

        private List<FileInfo> chunks;

        public bool chunksDeleted = false;

        private long chunkSizeBytes;

        // @param in InputStream that will be chunked
        // @param chunkDirectory Directoriy for the chunks
        // @param chunkName Name of the chunk files
        // @param chunkSize Desired size of the chunk files in KB
        public ChunkHelper(Stream input, String chunkDirectory, String chunkName, long chunkSize)
        {
            chunkSizeBytes = chunkSize * 1024;
            int numberOfChunks = (int)((input.Length / chunkSizeBytes) + 1);
            chunks = new List<FileInfo>(numberOfChunks);
            WriteToFiles(input, Path.Combine(chunkDirectory, chunkName), chunkSizeBytes);
        }

        // @param in InputStream that will be chunked
        // @param chunkDirectory Directoriy for the chunks
        // @param chunkName Name of the chunk files
        // @param chunkNumber Name of the chunk files
        // @param totalSize Size of the whole input stream in KB
        // @param mode The mode decides how the input stream should be chunked
        public ChunkHelper(Stream input,
                    String chunkDirectory,
                    String chunkName,
                    int chunkNumber,
                    long totalSize,
                    Mode mode)
        {
            chunks = new List<FileInfo>(chunkNumber);
            WriteToFiles(input, Path.Combine(chunkDirectory, chunkName), chunkNumber, totalSize * 1024, mode);
        }

        public int GetNumberOfChunks()
        {
            return chunks.Count;
        }

        public FileInfo GetChunkFile(int num)
        {
            if (!chunksDeleted && num <= chunks.Count() && num > 0)
            {
                // index starts at 0
                return chunks[num - 1];
            }
            return null;
        }

        public FileStream GetChunkStream(int num)
        {
            if (!chunksDeleted && num <= chunks.Count() && num > 0)
            {
                // index starts at 0
                return chunks[num - 1].OpenRead();
            }
            return null;
        }

        public long GetChunkSize()
        {
            return chunkSizeBytes / 1024;
        }

        public void DeleteFiles()
        {
            if (!chunksDeleted)
            {
                foreach (FileInfo file in chunks)
                {
                    file.Delete();
                }
                chunksDeleted = true;
                chunks = null;
            }
        }

        private void WriteToFiles(Stream input, String filePath, long chunkSize)
        {
            using (StreamChunker sc = new StreamChunker(input))
            {
                String fileName = filePath + ".chunk";

                for (int i = 1; sc.ChunkRemaining(); i++)
                {
                    FileInfo file = new FileInfo(fileName + i);

                    using (FileStream output = file.Create())
                    {
                        sc.WriteChunk(output, chunkSize);
                    }
                    chunks.Add(file);
                }
            }
        }

        private long WriteToFiles(Stream input, String filePath, int chunkNumber, long totalSize, Mode mode)
        {
            chunkSizeBytes = (totalSize / chunkNumber) + 128;

            if (mode == Mode.OPTIMIZEDSTORE)
            {
                chunkSizeBytes = (totalSize - (chunkSizeBytes / 3)) / (chunkNumber - 1);
            }

            WriteToFiles(input, filePath, chunkSizeBytes);

            return chunkSizeBytes;
        }
    }
}