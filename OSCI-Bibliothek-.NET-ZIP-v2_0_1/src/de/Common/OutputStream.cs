using System;
using System.IO;

namespace Osci.Common
{
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: unbekannt</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public abstract class OutputStream 
        : Stream
    {
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get
            {
                throw new InvalidOperationException("Nicht implementiert");
            }
        }

        public override long Position
        {
            get
            {
                throw new InvalidOperationException("Nicht implementiert");
            }
            set
            {
                throw new InvalidOperationException("Nicht implementiert");
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException("Nicht implementiert");
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Nicht implementiert");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Nicht implementiert");
        }

        public virtual void Write(byte[] buffer)
        {
            BeginWrite(buffer, 0, buffer.Length, null, null);
        }

        public virtual void Write(int buffer)
        {
            WriteByte((byte)buffer);
        }
    }
}