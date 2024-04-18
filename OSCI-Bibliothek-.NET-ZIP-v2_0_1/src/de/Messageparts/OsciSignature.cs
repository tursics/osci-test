using System.Collections;
using System.IO;
using System.Security.Cryptography;
using Osci.Common;
using Osci.Encryption;
using Osci.Extensions;
using Osci.Helper;
using Osci.Roles;
using System.Collections.Generic;
using System;
using Osci.Exceptions;

namespace Osci.MessageParts
{
    /// <summary> Diese Klasse bildet eine die Grundlage für XML-Signature Signaturen der Bibliothek.
    /// Sie wird von Anwendungen nie direkt benötigt.
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
    public class OsciSignature
        : MessagePart
    {
        public byte[] SignedInfo
        {
            get; set;
        }

        public Dictionary<string, OsciSignatureReference> Refs
        {
            get; set;
        }

        public byte[] SignatureValue
        {
            get; set;
        }

        public string SignerId
        {
            get; set;
        }

        public string SignatureAlgorithm
        {
            get; set;
        }

        public Role Signer
        {
            get; set;
        }

        internal string EnclosingElement
        {
            get; set;
        }

        internal string SigningTime
        {
            get; set;
        }

        internal string SigningPropsId
        {
            get; set;
        }

        internal string SigningProperties
        {
            get; set;
        }

        internal Hashtable RefsHash
        {
            get; set;
        }

        internal Hashtable RefsDigestMethods
        {
            get; set;
        }

        internal static int Cnt = 0;
        private readonly static Log _log = LogFactory.GetLog(typeof(OsciSignature));


        internal OsciSignature()
        {
            Refs = new Dictionary<string, OsciSignatureReference>();
            SignatureAlgorithm = DialogHandler.SignatureAlgorithm;
        }

        internal OsciSignature(string enclosingElement)
            : this()
        {
            EnclosingElement = enclosingElement;
        }

        internal void AddSignatureTime(string time, string id, string digestAlgorithm)
        {
            if (SigningPropsId != null && Refs.ContainsKey(id))
            {
                Refs.Remove(id + SigningPropsId);
            }

            SigningPropsId = id;
            SigningTime = time;
            string nsHead = "xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:osci=\"http://www.osci.de/2002/04/osci\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xades=\"http://uri.etsi.org/01903/v1.3.2#\" xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";
            SigningProperties = "<xades:SignedProperties " + nsHead
                + " Id=\"" + SigningPropsId
                + "\"><xades:SignedSignatureProperties><xades:SigningTime>"
                + SigningTime
                + "</xades:SigningTime></xades:SignedSignatureProperties></xades:SignedProperties>";

            OsciSignatureReference osr = new OsciSignatureReference("#" + id);

            HashAlgorithm msgDigest = Crypto.CreateMessageDigest(digestAlgorithm);
            osr.digestValue = msgDigest.ComputeHash(SigningProperties.ToByteArray());
            osr.digestMethodAlgorithm = digestAlgorithm;
            osr.SetNamespacePrefixes(SoapNsPrefix, OsciNsPrefix, DsNsPrefix,
                              XencNsPrefix, XsiNsPrefix);
            AddSignatureReference(osr);
        }

        public void AddSignatureReference(OsciSignatureReference sigReference)
        {
            if (Refs.ContainsKey(sigReference.RefId))
            {
                _log.Error("Duplicated refId of signed parts!: " + sigReference.RefId);
                throw new OsciErrorException("9602");
            }
            _log.Debug("Add reference with id:" + sigReference.RefId);

            Refs.Add(sigReference.RefId, sigReference);
        }

        internal Dictionary<string, OsciSignatureReference> GetReferences()
        {
            return Refs;
        }

        public void Sign(Role signer)
        {
            Signer = signer;
            SignatureAlgorithm = signer.SignatureAlgorithm;
            if (SignedInfo == null)
            {
                CreateSignedInfo();
            }
            SignatureValue = signer.Signer.Sign(SignedInfo, SignatureAlgorithm);
            SignerId = "#" + signer.SignatureCertificateId;
        }

        /// <summary>Liefert den zu dieser Signatur gehörende Signaturzeitpunkt (ISO-8601-DateTime Format).
        /// </summary>
        /// <returns>Signaturzeitpunkt
        /// </returns>
        public string GetSigningTime()
        {
            return SigningTime;
        }

        public Hashtable GetDigests()
        {
            if (RefsHash == null)
            {
                RefsHash = new Hashtable();

                foreach (KeyValuePair<string, OsciSignatureReference> entry in Refs)
                {
                    RefsHash.Add(entry.Key, entry.Value.digestValue);
                }
            }
            return RefsHash;
        }

        public Hashtable GetDigestMethods()
        {
            if (RefsDigestMethods == null)
            {
                RefsDigestMethods = new Hashtable();

                foreach (KeyValuePair<string, OsciSignatureReference> entry in Refs)
                {
                    RefsDigestMethods.Add(entry.Key, entry.Value.digestMethodAlgorithm);
                }
            }
            return RefsDigestMethods;
        }


        private void CreateSignedInfo()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write("<" + DsNsPrefix + ":SignedInfo xmlns:" + DsNsPrefix + "=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:" + OsciNsPrefix + "=\"http://www.osci.de/2002/04/osci\" xmlns:" + SoapNsPrefix + "=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:" + XencNsPrefix + "=\"http://www.w3.org/2001/04/xmlenc#\" xmlns:" + XsiNsPrefix + "=\"http://www.w3.org/2001/XMLSchema-instance\">\n");
                memoryStream.Write("<" + DsNsPrefix + ":CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"></" + DsNsPrefix + ":CanonicalizationMethod>\n<" + DsNsPrefix + ":SignatureMethod Algorithm=\"" + SignatureAlgorithm + "\"></" + DsNsPrefix + ":SignatureMethod>\n");

                foreach (KeyValuePair<string, OsciSignatureReference> entry in Refs)
                {
                    _log.Debug("################ Referenz: " + entry.Key);
                    entry.Value.WriteXml(memoryStream);
                }
                memoryStream.Write("</" + DsNsPrefix + ":SignedInfo>");
                SignedInfo = memoryStream.ToArray();
            }
        }

        public override void WriteXml(Stream stream)
        {
            if (EnclosingElement != null)
            {
                stream.Write(EnclosingElement);
            }
            string prefix = DsNsPrefix + ":";
            int start;
            int stop;
            for (start = 0; start < SignedInfo.Length; start++)
            {
                if (SignedInfo[start] == '<')
                {
                    break;
                }
            }
            byte[] prefixBytes = prefix.ToByteArray();
            int j = 0;
            for (int i = start + 1; i < prefix.Length; i++, j++)
            {
                if (prefixBytes[j] != SignedInfo[i])
                {
                    prefix = "";
                    break;
                }
            }

            if (prefix.Length == 0)
            {
                stream.Write("<Signature " + "xmlns=\"" + MessagePartParser.DsXmlns + "\">");
            }
            else
            {
                stream.Write("<" + prefix + "Signature>");
            }
            //neu in126
            for (start = 0; start < SignedInfo.Length; start++)
            {
                if (SignedInfo[start] == 0x20)
                {
                    break;
                }
            }
            for (stop = start; stop < SignedInfo.Length; stop++)
            {
                if (SignedInfo[stop] == '>')
                {
                    break;
                }
            }
            stream.Write(SignedInfo, 0, start);
            stream.Write(SignedInfo, stop, SignedInfo.Length - stop);
            stream.Write("<" + prefix + "SignatureValue>");
            stream.Write(Helper.Base64.Encode(SignatureValue));
            //id
            stream.Write("</" + prefix + "SignatureValue><" + prefix + "KeyInfo><" + prefix + "RetrievalMethod URI=\"");
            stream.Write(SignerId);

            stream.Write("\"></" + prefix + "RetrievalMethod></" + prefix + "KeyInfo>");
            if (SigningProperties != null)
            {
                stream.Write("<" + DsNsPrefix + ":Object><xades:QualifyingProperties xmlns:xades=\"" + MessagePartParser.XadesXmlns + "\">");
                int nsStart = SigningProperties.IndexOf("<xades:SignedProperties");
                nsStart = SigningProperties.IndexOf(' ', nsStart);
                stream.Write(SigningProperties.Substring(0, nsStart));
                stream.Write(SigningProperties.Substring(SigningProperties.IndexOf(" Id=", nsStart)));
                stream.Write("</xades:QualifyingProperties></" + DsNsPrefix + ":Object>");
            }
            stream.Write("</" + prefix + "Signature>");

            if (EnclosingElement != null)
            {
                stream.Write("</" + EnclosingElement.Substring(1, (EnclosingElement.IndexOf(' ')) - (1)) + ">");
            }
        }
    }
}