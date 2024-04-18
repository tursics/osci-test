using System.Collections.Generic;
using System.IO;
using Osci.Common;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;

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
    public class CipherReference
    {
        public Stream ReferencedStream
        {
            get
            {
                return _swapBuffer.InputStream;
            }
            set
            {
                _log.Debug("Der Referenced Stream wurde gesetzt.");
                value.CopyToStream(_swapBuffer.OutputStream);
            }
        }

        public string Uri
        {
            get; set;
        }

        public bool EncryptedStream
        {
            get; set;
        }

        private static readonly Log _log = LogFactory.GetLog(typeof(CipherReference));
        private readonly List<string> _transformList;
        private readonly OsciDataSource _swapBuffer;

        public CipherReference(string uri)
        {
            _transformList = new List<string>();
            Uri = uri;
            _log.Trace("Konstruktor");
            _swapBuffer = DialogHandler.NewDataBuffer;
        }

        public void AddTransform(string transform)
        {
            _transformList.Add(transform);
        }

        public string GetTransform(int i)
        {
            return _transformList[i];
        }

        public void WriteXml(Stream stream, string ds, string xenc)
        {
            stream.Write(string.Format("<{0}:CipherReference URI=\"{3}\"><{0}:Transforms><{1}:Transform Algorithm=\"{2}\"></{1}:Transform></{0}:Transforms></{0}:CipherReference>", xenc, ds, Constants.Base64Decoder, Uri));
        }
    }
}