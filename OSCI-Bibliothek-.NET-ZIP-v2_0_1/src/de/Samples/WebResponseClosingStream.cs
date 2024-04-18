using System.IO;
using System.Net;
using Osci.Common;

namespace Osci.Samples
{
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
        /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
        /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N. B�ngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    /// 
    // Notwendig, damit die Schnittstelle zum Schlie�en des WebRequests nicht ge�ndert werden musste.
    public class WebResponseClosingStream : InputStream
    {
        private readonly Stream _respStream;
        readonly WebResponse _wr;

        public WebResponseClosingStream(WebResponse wr)
        {
            _wr = wr;
            _respStream = _wr.GetResponseStream();
        }

        public override int ReadByte()
        {
            return _respStream.ReadByte();
        }

        public override int Read(byte[] b, int off, int len)
        {
            return _respStream.Read(b, off, len);
        }

        public override void Close()
        {
            _wr.Close();
        }

        public override void Flush()
        {
            _respStream.Flush();
        }
    }
}