using System;
using System.Collections;
using System.IO;
using Osci.Common;
using Osci.Extensions;

namespace Osci.Helper
{
    /// <summary> <p> Einfacher MIME-Parser. Die Funktion beschränkt
    /// sich auf die Anforderungen der OSCI 1.2 Transportbibliothek.</p>
    /// 
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class MimeParser
    {
        internal Stream Stream;

        internal byte[] Buffer;
        internal int BufferPointer;

        internal MimeHeaders MimeHeaders
        {
            get; private set;
        }

        private static readonly Log _log = new Log(typeof(MimeParser));
        private MimePartInputStream _currentStream;

        public MimeParser(Stream stream)
        {
            Stream = stream;

            MimeHeaders = new MimeHeaders(ReadHeaders());

            if (!string.IsNullOrEmpty(MimeHeaders.Version) && (!MimeHeaders.Version.Equals("1.0")))
            {
                throw new IOException(DialogHandler.ResourceBundle.GetString("msg_format_error"));
            }

            if (string.IsNullOrEmpty(MimeHeaders.Boundary))
            {
                throw new IOException(DialogHandler.ResourceBundle.GetString("msg_format_error"));
            }
        }

        public MimePartInputStream GetNextMimePart()
        {
            long length = -1;

            string bound;
            while ((bound = ReadLine().Trim()).Equals(""))
            {
            }

            if (("--" + MimeHeaders.Boundary + "--").Equals(bound))
            {
                Stream.Close();
                return null;
            }

            if (!("--" + MimeHeaders.Boundary).Equals(bound))
            {
                throw new IOException(DialogHandler.ResourceBundle.GetString("msg_format_error"));
            }

            Hashtable header = ReadHeaders();
            if (header == null)
            {
                Stream.Close();
                return null;
            }

            MimeHeaders partHeaders = new MimeHeaders(header);

            if (string.IsNullOrEmpty(partHeaders.ContentType))
            {
                throw new IOException(DialogHandler.ResourceBundle.GetString("msg_format_error"));
            }

            if (partHeaders.Encoding != null && !partHeaders.Encoding.ToUpper().Equals(Constants.CharEncoding))
            {
                throw new Exception(DialogHandler.ResourceBundle.GetString("invalid_charset") + partHeaders.Encoding);
            }

            _currentStream = new MimePartInputStream(this, partHeaders);

            return _currentStream;
        }

        private Hashtable ReadHeaders()
        {
            bool end = false;
            Hashtable headers = new Hashtable();
            string header;
            string nextHeader = null;

            try
            {
                while ((header = ReadLine().Trim()).Equals(""))
                {
                }

                while (!header.Equals(""))
                {
                    nextHeader = ReadLine();

                    while ((nextHeader.Length > 0) && char.IsWhiteSpace(nextHeader[0]))
                    {
                        header = header + nextHeader;
                        nextHeader = ReadLine();
                    }
                    int i = header.IndexOf(':');
                    headers.Put(header.Substring(0, i).ToLower().Trim(), header.Substring(i + 1).Trim());
                    header = nextHeader.Trim();
                }
            }
            catch(IndexOutOfRangeException ex)
            {
                // do nothing
                _log.Warn("unexpected mime structure!");
                if (nextHeader != null)
                    header = nextHeader.Trim();
            }
            if (headers.Count == 0)
            {
                return null;
            }

            return headers;
        }

        private string ReadLine()
        {
            byte[] buffer;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] ba = new byte[1];
                bool end = false;

                while (!end)
                {
                    int b = ReadFromInput();

                    if (b == -1)
                    {
                        return null;
                    }

                    if (b == 0x0D)
                    {
                        b = ReadFromInput();

                        if (b == 0x0A)
                        {
                            end = true;
                        }
                        else if (char.IsWhiteSpace((char)(b)))
                        {
                            continue;
                        }
                    }

                    ba[0] = (byte)b;
                    if (!end)
                    {
                        memoryStream.Write(ba, 0, 1);
                    }
                }
                string txt = memoryStream.AsString();
                _log.Trace("ZEILE: " + txt);

                return txt;
            }
        }

        private int ReadFromInput()
        {
            if (Buffer != null && BufferPointer < Buffer.Length)
            {
                int value = Buffer[BufferPointer++];

                if (BufferPointer >= Buffer.Length)
                {
                    Buffer = null;
                    BufferPointer = 0;
                }
                return value;
            }
            else
            {
                byte[] buf = new byte[1];
                if (Stream.Read(buf, 0, 1) == 0)
                {
                    return -1;
                }
                else
                {
                    return buf[0];
                }
            }
        }
    }
}