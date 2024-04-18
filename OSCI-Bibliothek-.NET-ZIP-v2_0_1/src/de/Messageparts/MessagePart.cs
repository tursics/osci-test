using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Osci.Common;
using Osci.Encryption;
using Osci.Extensions;
using Osci.Messagetypes;

namespace Osci.MessageParts
{
    /// <summary> <p>Diese Klasse ist die Basisklasse für sämtliche Header-Element und alle weiteren
    /// Elemente welche in einer OSCI-Nachricht signiert werden (Attachment, ContentContainer,
    /// Content... ).</p>
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
    public abstract class MessagePart
    {
        /// <summary> Das refID-Attribut des MessagePart-Objekts.
        /// </summary>
        /// <value> refID-String
        /// </value>
        public string RefId { get; protected set; }

        public byte[] Ns { get; set; }

        public byte[] Ns2017 { get; set; }

        internal string DsNsPrefix { get; private set; } = "ds";

        internal string SoapNsPrefix { get; private set; } = "soap";

        internal string OsciNsPrefix { get; private set; } = "osci";

        internal string Osci2017NsPrefix { get; private set; } = "osci2017";

        internal string Osci128NsPrefix { get; private set; } = "osci128";

        internal string XencNsPrefix { get; private set; } = "xenc";

        internal string XsiNsPrefix { get; private set; } = "xsi";

        internal string Base64 { get; private set; }

        internal string Canonizer { get; private set; }

        internal List<string> Transformers { get; set; } = new List<string>();

        internal Hashtable DigestValues { get; set; } = new Hashtable();

        internal Hashtable Xmlns = new Hashtable();

        protected MessagePart()
        {
            Base64 = "<" + DsNsPrefix + ":Transform Algorithm=\"" + Constants.TransformBase64 + "\"></" + DsNsPrefix + ":Transform>";
            Canonizer = "<" + DsNsPrefix + ":Transform Algorithm=\"" + Constants.TransformCanonicalization + "\"></" + DsNsPrefix + ":Transform>";

            RefId = string.Format("{0}_{1}", GetType().Name.ToLowerInvariant(), Guid.NewGuid().ToString("N"));

            Ns = (" " + Constants.DefaultNamespaces).ToByteArray();
            Ns2017 = (" " + Constants.DefaultNamespaces2017).ToByteArray();
        }

        public abstract void WriteXml(Stream stream);

        /// <summary> Fügt der Liste von Transformer-Objekten ein weiteres hinzu. Die Transformer
        /// werden in der Reihenfolge des Hinzufügens beim Signieren dieses Objektes
        /// verwendet.
        /// </summary>
        /// <param name="transform">Transformer
        /// </param>
        public virtual void AddTransformerForSignature(string transform)
        {
            Transformers.Add(transform);
        }

        /// <summary> Liefert den Hashwert des Message-Parts.
        /// </summary>
        /// <value> Hashwert
        /// </value>
        public virtual byte[] GetDigestValue(string digestAlgorithm)
        {
            if (DigestValues[digestAlgorithm] != null)
            {
                return (byte[])DigestValues[digestAlgorithm];
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                HashAlgorithm msgDigest = Crypto.CreateMessageDigest(digestAlgorithm);
                using (CryptoStream outStream = new CryptoStream(memoryStream, msgDigest, CryptoStreamMode.Write))
                {
                    WriteXml(outStream);
                }
                DigestValues.Add(digestAlgorithm, msgDigest.Hash);
            }
            return (byte[])DigestValues[digestAlgorithm];
        }

        public void SetNamespacePrefixes(OsciMessage msg)
        {
            SoapNsPrefix = msg.SoapNsPrefix;
            OsciNsPrefix = msg.OsciNsPrefix;
            Osci2017NsPrefix = msg.Osci2017NsPrefix;
            Osci128NsPrefix = msg.Osci128NsPrefix;
            DsNsPrefix = msg.DsNsPrefix;
            XencNsPrefix = msg.XencNsPrefix;
            XsiNsPrefix = msg.XsiNsPrefix;
            Ns = msg.Ns.ToByteArray();
        }

        public void SetNamespacePrefixes(string soap, string osci, string ds, string xenc, string xsi)
        {
            SoapNsPrefix = soap;
            OsciNsPrefix = osci;
            DsNsPrefix = ds;
            XencNsPrefix = xenc;
            XsiNsPrefix = xsi;
        }
    }
}