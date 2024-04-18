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
    public class StoreInputStream
        : InputStream
    {
        private readonly Stream _baseStream; // InputStream
        private readonly Stream _copyStream; // OutputStream
        private MemoryStream _buffer;
        private bool _closed;
        private bool _save;

        public StoreInputStream(Stream inStream, Stream copyStream)
        {
            _baseStream = inStream;
            _copyStream = copyStream;
            _buffer = new MemoryStream();
        }

        public bool Save
        {
            set
            {
                lock (_baseStream)
                {
                    _save = value;
                    if (value)
                    {
                        _buffer.Close();
                        byte[] tempByteArray = _buffer.ToArray();
                        _copyStream.Write(tempByteArray, 0, tempByteArray.Length);
                    }
                    _buffer = null;
                }
            }
        }

        public int Read()
        {
            if (_closed)
            {
                return -1;
            }
            int s = _baseStream.ReadByte();
            if (s == -1)
            {
                return -1;
            }
            else
            {
                if (_buffer != null)
                {
                    _buffer.WriteByte((byte)s);
                }
                else if (_save)
                {
                    _copyStream.WriteByte((byte)s);
                }
            }
            return s;
        }

        public override int Read(byte[] b, int off, int len)
        {
            if (_closed)
            {
                return -1;
            }
            int s = _baseStream.CopyTo(b, off, len);

            if (s == -1)
            {
                return -1;
            }
            else
            {
                if (_buffer != null)
                {
                    _buffer.Write(b, off, s);
                }
                else if (_save)
                {
                    _copyStream.Write(b, off, s);
                }
            }
            return s;
        }

        public override void Close()
        {
            if (_closed)
            {
                return;
            }
            _baseStream.Close();
            if (_buffer != null)
            {
                _buffer.Close();
                byte[] tempByteArray = _buffer.ToArray();
                _copyStream.Write(tempByteArray, 0, tempByteArray.Length);
            }
            _copyStream.Close();
            _closed = true;
        }
    }
}