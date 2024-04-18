using System.IO;
using Osci.Extensions;

namespace Osci.SoapHeader
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
    public class QualityOfTimestampH
        : HeaderEntry
    {

        public bool QualityCryptographic
        {
            get
            {
                return _quality;
            }
        }

        public bool ServiceReception
        {
            get
            {
                return _service;
            }
        }

        private readonly bool _service;
        private readonly bool _quality;

        public QualityOfTimestampH(bool serviceReception, bool qualityCryptographic)
            : this("qualityoftimestamp_" + (serviceReception ? "reception" : "creation"), serviceReception, qualityCryptographic)
        {
        }

        internal QualityOfTimestampH(string refdId, bool serviceReception, bool qualityCryptographic)
        {
            RefId = refdId;
            _service = serviceReception;
            _quality = qualityCryptographic;
        }

        public override void WriteXml(Stream stream)
        {
            stream.Write("<" + OsciNsPrefix + ":QualityOfTimestamp");
            stream.Write(Ns, 0, Ns.Length);
            stream.Write(" Id=\"" + RefId + "\" Quality=\"" +
                        (_quality ? "cryptographic" : "plain") + "\" Service=\"" +
                        (_service ? "reception" : "creation") +
                        "\" " + SoapNsPrefix + ":actor=\"http://schemas.xmlsoap.org/soap/actor/next\" " + SoapNsPrefix + ":mustUnderstand=\"1\"" +
                        "></" + OsciNsPrefix + ":QualityOfTimestamp>");
        }
    }
}