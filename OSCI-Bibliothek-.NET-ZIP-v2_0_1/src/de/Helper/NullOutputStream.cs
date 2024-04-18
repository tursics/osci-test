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
    public class NullOutputStream 
        : OutputStream
    {
        //private static Log log = LogFactory.getLog(NullOutputStream.class);
        private long _count;
        public NullOutputStream()
        {
            ;
        }
        public override void Write(int b)
        {
            _count++;
        }
        public override void Write(byte[] b, int off, int len)
        {
            _count += len;
        }
        public long GetLength()
        {
            return _count;
        }
    }
}
