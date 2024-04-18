using System.Collections.Generic;
using System.IO;
using Osci.Common;
using Osci.Extensions;
using Osci.Helper;

namespace Osci.MessageParts
{
    /// <summary> Diese Klasse bildet eine XML-Signature Referenze in der OSCI-Bibliothek ab.
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
    public class OsciSignatureReference
        : MessagePart
    {
        /// <summary> Liefert den Hashalgorithmus.
        /// </summary>
        /// <value> String-Id des Hashalgorithmus
        /// </value>
        /// <seealso cref="Constants">
        /// </seealso>
        public string DigestMethodAlgorithm
        {
            get
            {
                return digestMethodAlgorithm;
            }
            set
            {
                digestMethodAlgorithm = value;
            }
        }


        public List<string> TransformerAlgorithms
        {
            get; private set;
        }

        internal string digestMethodAlgorithm;
        internal byte[] digestValue;

        private static readonly Log _log = LogFactory.GetLog(typeof(OsciSignatureReference));

        private OsciSignatureReference()
        {
            TransformerAlgorithms = new List<string>();
        }

        internal OsciSignatureReference(MessagePart messagePart, string digestAlgorithm)
            : this()
        {
            RefId = messagePart is Attachment ? "cid:" + messagePart.RefId : "#" + messagePart.RefId;
            SetNamespacePrefixes(messagePart.SoapNsPrefix, messagePart.OsciNsPrefix, messagePart.DsNsPrefix, messagePart.XencNsPrefix, messagePart.XsiNsPrefix);
            digestMethodAlgorithm = digestAlgorithm;
            digestValue = messagePart.GetDigestValue(digestMethodAlgorithm);
            TransformerAlgorithms = messagePart.Transformers;
        }

        internal OsciSignatureReference(string refId)
            : this()
        {
            RefId = refId;
        }

        public override void WriteXml(Stream stream)
        {
            stream.Write("<" + DsNsPrefix + ":Reference URI=\"" + RefId + "\">\n");
            if (TransformerAlgorithms.Count > 0)
            {
                stream.Write("<" + DsNsPrefix + ":Transforms>\n");
                for (int j = 0; j < TransformerAlgorithms.Count; j++)
                {
                    stream.Write(TransformerAlgorithms[j]);
                }
                stream.Write("</" + DsNsPrefix + ":Transforms>\n");
            }
            stream.Write("<" + DsNsPrefix + ":DigestMethod Algorithm=\"" + digestMethodAlgorithm + "\"></" + DsNsPrefix + ":DigestMethod>\n<" + DsNsPrefix + ":DigestValue>" + Helper.Base64.Encode(digestValue) + "</" + DsNsPrefix + ":DigestValue>\n</" + DsNsPrefix + ":Reference>\n");
        }

        public override string ToString()
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    WriteXml(memoryStream);
                    return memoryStream.AsString();
                }
            }
            catch (IOException ex)
            {
                _log.Error("Fehler", ex);
                return "";
            }
        }
    }
}