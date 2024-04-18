using System.IO;
using Osci.Extensions;

namespace Osci.Encryption
{
    /// <exclude/>
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class CipherData
    {
        private CipherReference _cipherReference;
        private CipherValue _cipherValue;
        private bool _referenzedData;

        public CipherReference CipherReference
        {
            get
            {
                return _cipherReference;
            }
            set
            {
                if (_cipherValue != null)
                {
                    throw new OsciCipherException("CipherValue Element wurde bereits eingestellt");
                }
                _referenzedData = true;
                _cipherReference = value;
            }

        }

        public CipherValue CipherValue
        {
            get
            {
                return _cipherValue;
            }

            set
            {
                if (_cipherReference != null)
                {
                    throw new OsciCipherException("CipherReference Element wurde bereits eingestellt");
                }
                _cipherValue = value;
            }

        }

        public bool ReferenzedData
        {
            get
            {
                return _referenzedData;
            }

        }

        public CipherData()
        {
        }

        public CipherData(CipherValue cipherValue)
        {
            _referenzedData = false;
            _cipherValue = cipherValue;
        }

        public CipherData(CipherReference cipherRef)
        {
            _referenzedData = true;
            _cipherReference = cipherRef;
        }

        public virtual void WriteXml(Stream stream, string ds, string xenc)
        {
            stream.Write("<" + xenc + ":CipherData>");
            if (_referenzedData)
            {
                _cipherReference.WriteXml(stream, ds, xenc);
            }
            else
            {
                _cipherValue.WriteXml(stream, xenc);
            }
            stream.Write("</" + xenc + ":CipherData>");
        }
    }
}