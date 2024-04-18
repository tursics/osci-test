using System.Collections;
using System.IO;
using System.Text;
using Osci.Extensions;
using Osci.Helper;
using Osci.SoapHeader;

namespace Osci.MessageParts
{
    /// <summary> Diese Klasse stellt einen OSCI-Body (soap:Body) dar. 
    /// Sie muss von Anwendungen nie direkt verwendet werden.
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
    public class Body
        : HeaderEntry
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(Body));
        internal string Data;
        private readonly ContentContainer[] _contentContainers;
        private readonly EncryptedDataOsci[] _encryptedData;

        private Body()
        {
            RefId = "body";
        }

        /// <summary> Setzt den Body initial mit einem selber vorbereiteten String.
        /// </summary>
        /// <param name="data">Inhalt
        /// </param>
        public Body(string data)
            : this()
        {
            Data = data;
        }

        /// <summary>Legt ein Body-Objekt mit Inhaltsdaten an.
        /// </summary>
        /// <param name="contentContainers">ContentContainer mit unverschlüsselten Inhaltsdaten.
        /// </param>
        /// <param name="encryptedData">EncryptedData mit unverschlüsselten Inhaltsdaten.
        /// </param> 
        public Body(ContentContainer[] contentContainers, EncryptedDataOsci[] encryptedData)
            : this()
        {
            _contentContainers = contentContainers;
            _encryptedData = encryptedData;
            Hashtable nsp = new Hashtable();
            nsp.Add(OsciNsPrefix, MessagePartParser.OsciXmlns);
            nsp.Add(SoapNsPrefix, MessagePartParser.SoapXmlns);
            nsp.Add(DsNsPrefix, MessagePartParser.DsXmlns);
            nsp.Add(XencNsPrefix, MessagePartParser.XencXmlns);
            nsp.Add(XsiNsPrefix, MessagePartParser.XsiXmlns);

            for (int i = 0; i < contentContainers.Length; i++)
            {
                if (!nsp.Contains(contentContainers[i].OsciNsPrefix))
                {
                    nsp.Add(contentContainers[i].OsciNsPrefix, MessagePartParser.OsciXmlns);
                }
                if (!nsp.Contains(contentContainers[i].OsciNsPrefix))
                {
                    nsp.Add(contentContainers[i].SoapNsPrefix, MessagePartParser.SoapXmlns);
                }
                if (!nsp.Contains(contentContainers[i].OsciNsPrefix))
                {
                    nsp.Add(contentContainers[i].DsNsPrefix, MessagePartParser.DsXmlns);
                }
                if (!nsp.Contains(contentContainers[i].OsciNsPrefix))
                {
                    nsp.Add(contentContainers[i].XencNsPrefix, MessagePartParser.XencXmlns);
                }
                if (!nsp.Contains(contentContainers[i].OsciNsPrefix))
                {
                    nsp.Add(contentContainers[i].XsiNsPrefix, MessagePartParser.XsiXmlns);
                }
            }

            for (int i = 0; i < encryptedData.Length; i++)
            {
                if (!nsp.Contains(encryptedData[i].OsciNsPrefix))
                {
                    nsp.Add(encryptedData[i].OsciNsPrefix, MessagePartParser.OsciXmlns);
                }

                if (!nsp.Contains(encryptedData[i].OsciNsPrefix))
                {
                    nsp.Add(encryptedData[i].SoapNsPrefix, MessagePartParser.SoapXmlns);
                }
                if (!nsp.Contains(encryptedData[i].OsciNsPrefix))
                {
                    nsp.Add(encryptedData[i].DsNsPrefix, MessagePartParser.DsXmlns);
                }
                if (!nsp.Contains(encryptedData[i].OsciNsPrefix))
                {
                    nsp.Add(encryptedData[i].XencNsPrefix, MessagePartParser.XencXmlns);
                }
                if (!nsp.Contains(encryptedData[i].OsciNsPrefix))
                {
                    nsp.Add(encryptedData[i].XsiNsPrefix, MessagePartParser.XsiXmlns);
                }
            }

            ArrayList keySet = new ArrayList(nsp.Keys);
            keySet.Sort();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < keySet.Count; i++)
            {
                sb.Append(" xmlns:");
                sb.Append(keySet[i]);
                sb.Append("=\"");
                sb.Append(nsp[keySet[i]]);
                sb.Append("\"");
            }
            Ns = sb.ToString().ToByteArray();
            _log.Trace("NS: " + sb);
        }

        /// <summary>
        /// Es wird der eingestellte Body serialisiert.
        /// </summary>
        /// <param name="stream">Outputstream, in den geschrieben werden soll
        /// </param>
        public override void WriteXml(Stream stream)
        {
            stream.Write("<" + SoapNsPrefix + ":Body");
            stream.Write(Ns, 0, Ns.Length);
            stream.Write(" Id=\"" + RefId + "\">");

            if (Data != null)
            {
                stream.Write(Data);
            }
            else
            {
                stream.Write("<" + OsciNsPrefix + ":ContentPackage>");
                for (int i = 0; i < _encryptedData.Length; i++)
                {
                    _encryptedData[i].WriteXml(stream, false);
                }
                for (int i = 0; i < _contentContainers.Length; i++)
                {
                    _contentContainers[i].WriteXml(stream);
                }
                stream.Write("</" + OsciNsPrefix + ":ContentPackage>");
            }
            stream.Write("</" + SoapNsPrefix + ":Body>");
        }
    }
}