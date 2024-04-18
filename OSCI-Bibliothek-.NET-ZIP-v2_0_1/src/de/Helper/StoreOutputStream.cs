using Osci.Common;

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
    public class StoreOutputStream 
        : OutputStream
    {
        private readonly System.IO.Stream _baseStream; // InputStream
        private readonly System.IO.Stream _copyStream; // OutputStream

        public StoreOutputStream(System.IO.Stream outStream, System.IO.Stream copyStream)
        {
            _baseStream = outStream;
            _copyStream = copyStream;
        }

        public void Write(int b)
        {
            _baseStream.WriteByte((byte)b);
            _copyStream.WriteByte((byte)b);
        }

        public override void Write(byte[] b, int off, int len)
        {
            _baseStream.Write(b, off, len);
            _copyStream.Write(b, off, len);
        }

        public override void Flush()
        {
            _baseStream.Flush();
            _copyStream.Flush();
        }

        public override void Close()
        {
            Flush();
            _baseStream.Close();
            _copyStream.Close();
        }
    }
}