using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using System;
using System.IO;

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
    public class OsciH
        : HeaderEntry
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(OsciH));

        internal string Name
        {
            get;
        }

        internal string Data
        {
            get;
        }

        internal string Namespace
        {
            get;
        }

        // internal string OsciNsPrefix { get; private set; } = "osci";
        public OsciH(string name, string data)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
            Data = data;
            RefId = name.ToLower();
            Namespace = OsciNsPrefix;
        }

        public OsciH(string name, string data, string namespaceValue) : this(name, data)
        {
            Namespace = namespaceValue;
        }

        public override void WriteXml(Stream stream)
        {
            _log.Trace("RefID: " + RefId);
            stream.Write("<" + Namespace + ":" + Name);

            if (Osci2017NsPrefix.Equals(Namespace))
            {
                stream.Write(Ns2017, 0, Ns2017.Length);
            }
            else
            {
                stream.Write(Ns, 0, Ns.Length);
            }
            
            stream.Write(" Id=\"" + RefId +
                      "\" " + SoapNsPrefix + ":actor=\"http://schemas.xmlsoap.org/soap/actor/next\" " + SoapNsPrefix + ":mustUnderstand=\"1\">");
            stream.Write(Data);
            stream.Write("</" + Namespace + ":" + Name + ">");
        }
    }
}