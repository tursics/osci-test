using System;
using System.IO;
using System.Net;
using Osci.Common;
using Osci.Interfaces;

namespace Osci.Samples
{
    /// <summary>Beispiel-Implementierung eines Transport-Interfaces.
    /// 
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    /// <seealso cref="ITransport">
    /// </seealso>
    public class HttpTransport
        : ITransport
    {
        private InputStream _inStream;
        private HttpWebRequest _hrequest;

        /// <summary> Liefert den Namen des Herstellers.
        /// </summary>
        /// <value> Herstellername
        /// </value>
        public string Vendor
        {
            get
            {
                return "Governikus";
            }
        }

        /// <summary> Liefert die Versionsnummer.
        /// </summary>
        /// <value> Versionsnummer
        /// </value>
        public string Version
        {
            get
            {
                return "0.9";
            }
        }

        public Stream ResponseStream
        {
            get
            {
                WebResponse wr = _hrequest.GetResponse();
                return new WebResponseClosingStream(wr);
            }
        }
        public ITransport NewInstance()
        {
            return new HttpTransport();
        }

        public bool IsOnline(Uri uri)
        {
            return true;
        }

        public long ContentLength
        {
            get
            {
                /**@todo Diese de.osci.osci12.extinterfaces.transport.TransportI-Methode implementieren*/
                throw new ApplicationException("Methode getContentLength() noch nicht implementiert.");
            }
        }

        public Stream GetConnection(Uri uri, long laenge)
        {
            _hrequest = (HttpWebRequest)WebRequest.Create(uri); //url als String

            /*
                 * default proxy Einstellungen vom Browser
                 * WebProxy myProxy=new WebProxy();
                 * myProxy=(WebProxy)hrequest.Proxy;
                */
            _hrequest.ProtocolVersion = HttpVersion.Version10;
            _hrequest.Method = "POST";
            _hrequest.ContentLength = laenge;
            Stream rqst = _hrequest.GetRequestStream();
            return rqst;
        }
    }
}