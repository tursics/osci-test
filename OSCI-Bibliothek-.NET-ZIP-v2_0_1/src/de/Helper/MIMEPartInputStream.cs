using System;
using System.IO;
using Osci.Common;
using Osci.Extensions;

namespace Osci.Helper
{
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class MimePartInputStream
        : InputStream
    {
        public string ContentId
        {
            get;
        }

        public override long Length
        {
            get;
        }

        public string ContentType
        {
            get; private set;
        }

        public string ContentTransferEncoding
        {
            get; private set;
        }

        public MimeHeaders MimeHeaders
        {
            get; private set;
        }

        private bool _closed;
        private readonly byte[] _end;
        private readonly MimeParser _parser;


        internal MimePartInputStream(MimeParser parser, MimeHeaders headers)
        {
            _parser = parser;
            MimeHeaders = headers;

            ContentType = headers.ContentType;
            ContentTransferEncoding = headers.ContentTransferEncoding;
            ContentId = headers.ContentId;
            Length = headers.ContentLength;
            _end = ("\r\n--" + parser.MimeHeaders.Boundary).ToByteArray();
        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            int count;

            if (_closed)
            {
                return 0;
            }

            if (_parser.Buffer != null)
            {
                if ((_parser.BufferPointer + length) <= _parser.Buffer.Length)
                {
                    Array.Copy(_parser.Buffer, _parser.BufferPointer, buffer, offset, length);
                    _parser.BufferPointer += length;
                    count = length;
                }
                else
                {
                    int bufBytes = _parser.Buffer.Length - _parser.BufferPointer;
                    Array.Copy(_parser.Buffer, _parser.BufferPointer, buffer, offset, bufBytes);

                    count = _parser.Stream.CopyTo(buffer, offset + bufBytes, length - bufBytes);


                    if (count == -1)
                    {
                        count = bufBytes;
                    }
                    else
                    {
                        count += bufBytes;
                    }

                    _parser.Buffer = null;
                    _parser.BufferPointer = 0;
                }
            }
            else
            {
                count = _parser.Stream.CopyTo(buffer, offset, length);
                if (count == -1)
                {
                    count = 0;
                }
            }

            if (count == 0)
            {
                _closed = true;
            }
            else
            {
                int i;

                for (i = offset; i < offset + count; i++)
                {
                    if ((buffer[i] == _end[0]))
                    {
                        int tmp;
                        int patternIndex;
                        for (tmp = i, patternIndex = 0; ((patternIndex < _end.Length) && (tmp < (offset + count))); patternIndex++, tmp++)
                        {
                            if (buffer[tmp] != _end[patternIndex])
                            {
                                break;
                            }
                        }

                        if (patternIndex == _end.Length)
                        {
                            // Falls keine Daten im Buffer waren, restliche Daten in neuem Buffer speichern,
                            // sonst den bufferPointer zurücksetzen.
                            if (_parser.Buffer == null)
                            {
                                _parser.Buffer = new byte[count - i + offset];
                                Array.Copy(buffer, i, _parser.Buffer, 0, _parser.Buffer.Length);
                                _parser.BufferPointer = 0;
                            }
                            else
                            {
                                _parser.BufferPointer -= length - (i - offset);
                            }
                            count = i - offset;
                            _closed = true;

                            break;
                        }
                        else if (tmp >= (offset + count))
                        {
                            int rest = _end.Length - patternIndex;
                            if ((_parser.Buffer == null) || ((_parser.Buffer.Length - _parser.BufferPointer) < rest))
                            {
                                int bufferIndex = 0;
                                if (_parser.Buffer == null)
                                {
                                    _parser.Buffer = new byte[rest];
                                }
                                else
                                {
                                    // Buffer erweitern, falls notwendig 
                                    bufferIndex = _parser.Buffer.Length - _parser.BufferPointer;
                                    byte[] newBuffer = new byte[rest];
                                    Array.Copy(_parser.Buffer, _parser.BufferPointer, newBuffer, 0, _parser.Buffer.Length - _parser.BufferPointer);
                                    _parser.Buffer = newBuffer;
                                }

                                while (bufferIndex < _parser.Buffer.Length)
                                {
                                    if (_parser.Stream.Read(_parser.Buffer, bufferIndex, 1) > 0)
                                    {
                                        bufferIndex++;
                                    }
                                    else
                                    {
                                        throw new IOException("EOS before marker.");
                                    }
                                }
                                _parser.BufferPointer = 0;
                            }
                            for (tmp = _parser.BufferPointer; patternIndex < _end.Length; patternIndex++, tmp++)
                            {
                                if (_parser.Buffer[tmp] != _end[patternIndex])
                                {
                                    break;
                                }
                            }

                            if (patternIndex == _end.Length)
                            {
                                count = i - offset;
                                byte[] newBuffer = new byte[(_end.Length + _parser.Buffer.Length) - tmp];
                                Array.Copy(_end, 0, newBuffer, 0, _end.Length);
                                Array.Copy(_parser.Buffer, tmp, newBuffer, _end.Length, _parser.Buffer.Length - tmp);
                                _parser.Buffer = newBuffer;
                                _parser.BufferPointer = 0;
                                _closed = true;
                            }
                        }
                    }
                }
            }
            return count;
        }

        public override void Close()
        {
            //	Bis zum Ende lesen....
            byte[] tmp = new byte[1024];

            while (this.CopyTo(tmp, 0, tmp.Length) > 0)
            {
            }
            _closed = true;
        }
    }
}