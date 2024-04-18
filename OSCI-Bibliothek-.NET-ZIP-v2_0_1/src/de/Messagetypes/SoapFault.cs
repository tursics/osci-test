using Osci.Common;
using Osci.Extensions;

namespace Osci.Messagetypes
{
    /// <summary>Diese Klasse repräsentiert eine SOAP-Fehlermeldung auf Nachrichtenebene.
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
    public class SoapFault
        : OsciResponseTo
    {
        private static readonly string _soapFaultIntro = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:osci=\"http://www.osci.de/2002/04/osci\"><soap:Body><soap:Fault><faultcode>soap:";
        private static readonly string _soapFaultExtro = "</osci:Code></detail></soap:Fault></soap:Body></soap:Envelope>";

        private readonly string _oscicode;
        private string _soapfault;

        /// <summary>Legt ein SOAP-Fehlerobjekt für den genannten OSCI-Code an.
        /// </summary>
        /// <param name="oscicode">OSCI-Fehlercode (s. Spezifikation)
        /// </param>
        public SoapFault(string oscicode)
        {
            _oscicode = oscicode;
            MessageType = ResponseToExitDialog;
        }

        protected new void Compose()
        {
            _soapfault = _soapFaultIntro;
            if (_oscicode.Equals("9000") || _oscicode.Equals("9503"))
            {
                _soapfault += "Server";
            }
            else
            {
                _soapfault += "Client";
            }
            _soapfault += "</faultcode><faultstring>" + DialogHandler.ResourceBundle.GetString(_oscicode) + "</faultstring><detail><osci:Code>" + _oscicode + _soapFaultExtro;

            StateOfMessage |= StateComposed;
        }

        protected void WriteXml(OutputStream stream)
        {
            Compose();
            stream.Write("\r\nMIME-Version: 1.0\r\nContent-Type: Multipart/Related; boundary=" + DialogHandler.Boundary + "; type=text/xml\r\n");
            stream.Write("\r\n--" + DialogHandler.Boundary + "\r\nContent-Type: text/xml; charset=UTF-8\r\n");
            stream.Write("Content-Transfer-Encoding: 8bit\r\nContent-ID: <" + ContentId + ">\r\n\r\n");
            stream.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n\r\n");
            stream.Write(_soapfault);
            stream.Write("\r\n--" + DialogHandler.Boundary + "--\r\n");
        }

        /// <summary>Mit dieser Methode können passive Empfänger SOAP-Fehlernachrichten an
        /// den Intermediär zurückschicken.
        /// </summary>
        /// <param name="out0">OutputStream
        /// </param>
        public void WriteToStream(OutputStream out0)
        {
            WriteXml(out0);
        }
    }
}
