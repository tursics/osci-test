using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Osci.Common;
using Osci.Encryption;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Roles;
using Osci.Signature;

namespace Osci.MessageParts
{
    /// <summary> <p><H4>ContentContainer</H4></p>
    /// <p>Die ContentContainer-Klasse stellt einen OSCI-Inhaltsdatenscontainer dar. 
    /// Ein ContentContainer kann einen oder mehrere Content-oder EncryptedData-Objekte enthalten. 
    /// Attachments werden als Contents eingestellt, die eine Referenz auf das Attachment enthalten.</p>
    /// <p>Ein Content-Container wird als eine Einheit signiert und / oder verschlüsselt.</p>
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
    public class ContentContainer
        : MessagePart
    {
        public OsciSignature[] Signatures => (OsciSignature[])SignerList.ToArray(typeof(OsciSignature));

        /// <summary> Liefert die eingestellten Attachment-Objekte des ContentContainer.
        /// </summary>
        /// <value> Array der referenzierten Attachments
        /// </value>
        public Attachment[] Attachments
        {
            get
            {
                _log.Debug("Anzahl der Attachments: " + _attachments.Count);
                Attachment[] atts = null;
                if (_attachments.Count > 0)
                {
                    atts = new Attachment[_attachments.Count];
                    IEnumerator enumerator = _attachments.GetEnumerator();
                    int count = 0;
                    while (enumerator.MoveNext())
                    {
                        atts[count] = (Attachment)(((DictionaryEntry)enumerator.Current)).Value;
                        count++;
                    }
                }
                return atts;
            }
        }

        /// <summary> Liefert die eingestellten Rollen-Objekte des ContentContainer, welche für die
        /// Signatur sowie untergeordnete Verschlüsselungen und Signaturen verwendet wurden.
        /// </summary>
        /// <value> Array der Rollenobjekte
        /// </value>
        public Role[] Roles => _roles.ToArray();

        /// <summary> Liefert die Rollenobjekte, von denen die Signaturen angebracht wurden.
        /// </summary>
        /// <value> Array der Rollenobjekte.
        /// </value>
        public Role[] Signers
        {
            get
            {
                Role[] roles = new Role[SignerList.Count];
                for (int i = 0; i < roles.Length; i++)
                {
                    roles[i] = ((OsciSignature)SignerList[i]).Signer;
                }
                return roles;
            }

        }

        /// <summary> Liefert die im ContentContainer enthaltenen Content-Objekte.
        /// </summary>
        /// <value>Array der enthaltenen Content-Objekt
        /// </value>
        /// <seealso cref="Content">
        /// </seealso>
        public Content[] Contents => _contentList.ToArray();

        /// <summary> Liefert die im ContentContainer enthaltenen verschlüsselten Daten als
        /// EncryptedData-Objekte.
        /// </summary>
        /// <value>Array der enthaltenen EncryptedData-Objekte
        /// </value>
        /// <seealso cref="EncryptedDataOsci">
        /// </seealso>
        public EncryptedDataOsci[] EncryptedData => (EncryptedDataOsci[])_encryptedDataList.ToArray(typeof(EncryptedDataOsci));

        private static readonly Log _log = LogFactory.GetLog(typeof(ContentContainer));
        internal static bool StateOfObjectConstruction = true;
        internal static bool StateOfObjectParsing = false;
        internal bool StateOfObject;

        /// <summary>
        /// lfdNr für die ContentContainer
        /// </summary>
        private static int _idNr = -1;

        /// <summary>
        /// lfdNr für die signed signature properties Ids
        /// </summary>
        private int _signedSigPropNr;

        /// <summary>
        ///  Vector für die Signer dieses ContentContainers
        /// </summary>
        internal ArrayList SignerList;

        /// <summary>
        /// Vector für alle Roles des ContentContainer. 
        /// Beinhalten auch die Rollen Objekte eventueller Encrypted Data Childs
        /// </summary>
        private readonly List<Role> _roles;

        /// <summary>
        /// Vector für die Attachments
        /// </summary>
        private readonly Hashtable _attachments;

        /// <summary>
        /// Vector für die Contents
        /// </summary>
        private readonly List<Content> _contentList;

        /// <summary>
        /// Vector für die EncryptedData Objekte
        /// </summary>
        private readonly ArrayList _encryptedDataList;

        private OsciDataSource _signatureData = null;

        /// <summary>Legt ein ContentContainer-Objekt an.
        /// </summary>
        public ContentContainer()
        {
            StateOfObject = StateOfObjectConstruction;
            SignerList = new ArrayList();
            _roles = new List<Role>();
            _attachments = new Hashtable();
            _contentList = new List<Content>();
            _encryptedDataList = new ArrayList();
        }

        public ContentContainer(string refdId)
            : this()
        {
            RefId = refdId;
        }

        /// <summary> Überprüft die Signatur zu dem übergebenen Role Objekt. Zur Installation
        /// von ggf. erforderlichen Transformern s. checkAllSignatures().
        /// Bevor eine Signaturprüfung an dem ContentContainer-Objekt durchgeführt
        /// werden kann, müssen denjenigen Content-Objekten, die unter Anwendung von
        /// Transformationen signiert wurden, die transformierten Daten übergeben werden.
        /// Welche Transformationen erforderlich sind, kann (bei Content-Objekten mit
        /// Inhaltsdaten) mit Hilfe der Methode getTransformerForSignature()
        /// abgefragt werden.
        /// </summary>
        /// <param name="signtureRole">Rollen-Objekt mit dem Zertifikat zur Signatur.
        /// </param>
        /// <returns> true, wenn die Prüfung positiv ausgefallen ist.
        /// </returns>
        public bool CheckSignature(Role signatureRole)
        {
            _log.Debug("(start) checkSignature (...) ");
            if (CheckContainsSigner(signatureRole))
            {
                List<OsciSignature> signatures = FindSignatureObjects(signatureRole);
                for (int j = 0; j < signatures.Count; j++)
                {
                    _log.Trace("Signature Objekt: " + signatures[j]);
                    Hashtable newHashes = new Hashtable();
                    for (int i = 0; i < _contentList.Count; i++)
                    {
                        Content co = _contentList[i];
                        newHashes.Put("#" + co.RefId, co);
                    }
                    for (int i = 0; i < _encryptedDataList.Count; i++)
                    {
                        EncryptedDataOsci encData = (EncryptedDataOsci)_encryptedDataList[i];
                        newHashes.Put("#" + encData.RefId, encData);
                    }
                    IEnumerator enumerator = _attachments.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        Attachment att = (Attachment)((DictionaryEntry)enumerator.Current).Value;
                        newHashes.Put("cid:" + att.RefId, att);
                    }

                    Dictionary<string, OsciSignatureReference> sigRefs = signatures[j].GetReferences();

                    if (_log.IsEnabled(LogLevel.Debug))
                    {
                        foreach (string refId in sigRefs.Keys)
                        {
                            _log.Trace("Reference: " + refId);
                        }
                    }

                    _log.Trace("Anzahl contents und encData: " + newHashes.Count + " OSCISIGReferenzen: " + sigRefs.Count);

                    ArrayList checkedList = new ArrayList();
                    MessagePart mp;
                    byte[] digest;
                    byte[] newDigest;

                    int sigRefCount = sigRefs.Count;

                    if (signatures[j].SigningTime != null)
                    {
                        sigRefCount--;
                        _log.Info("Add signing time to count list");
                    }

                    if (sigRefCount != newHashes.Count)
                    {
                        _log.Error("The number of references and hashed parts are not equal");
                        return false;
                    }


                    // sind die Hashwerte gleich?
                    foreach (OsciSignatureReference signatureRef in sigRefs.Values)
                    {
                        string id = signatureRef.RefId;
                        _log.Trace("ID die kontrolliert wird: " + id);

                        if (id.Equals("#" + signatures[j].SigningPropsId))
                        {
                            HashAlgorithm msgDigest = Crypto.CreateMessageDigest(signatureRef.DigestMethodAlgorithm);
                            newDigest = msgDigest.ComputeHash(signatures[j].SigningProperties.ToByteArray());
                        }
                        else
                        {
                            mp = (MessagePart)newHashes[id];
                            newDigest = mp.GetDigestValue(signatureRef.DigestMethodAlgorithm);
                        }

                        digest = signatureRef.digestValue;

                        if (newDigest == null)
                        {
                            _log.Trace("Der aktuelle Digest für die RefID: " + id + " konnte nicht gefunden werden");
                            return false;
                        }
                        if (!Tools.CompareByteArrays(digest, newDigest))
                        {
                            _log.Error("Der Digest für die RefID: " + id + " ist falsch! Eingestellter Digeste:" + digest.AsHexString() + " neuer Digest: " + newDigest.AsHexString());
                            return false;
                        }
                        else
                        {
                            _log.Trace("Der Digest ist richtig.");
                        }

                        checkedList.Add(id);
                    }

                    string key;
                    IEnumerator enum1 = newHashes.Keys.GetEnumerator();
                    while (enum1.MoveNext())
                    {
                        _log.Trace("Reference: " + enum1.Current);
                        key = (string)enum1.Current;
                        if (!checkedList.Contains(key))
                        {
                            _log.Error("Unsigniertes Containerelement gefunden: " + RefId);
                            return false;
                        }
                    }

                    X509Certificate c = signatureRole.SignatureCertificate;

                    bool res = Crypto.CheckSignature(signatureRole.SignatureCertificate, signatures[j].SignedInfo, signatures[j].SignatureValue, signatures[j].SignatureAlgorithm);
                    if (!res)
                    {
                        return false;
                    }
                    else
                    {
                        _log.Trace("Die Signaturprüfung wurde erfolgreich abgeschlossen.");
                    }
                }
            }
            else
            {
                _log.Warn("Content-Signatur konnte nicht überprüft werden (Falsches Role Objekt).");
                throw new ArgumentException("Content-Signatur konnte nicht überprüft werden (Falsches Role Objekt).");
            }
            _log.Trace("(ende) findSignaurObject");
            return true;
        }

        /// <summary> Liefert das zum Role Objekt gehörige XML-Signature Objekt.
        /// </summary>
        /// <param name="roleToCheck">Rollenobjekt, für das die Signatur-Objekt übergeben werden soll.
        /// </param>
        /// <returns> OSCISignature Objekt, oder null sobald kein korospondierendes Objekt exitstiert
        /// </returns>
        private List<OsciSignature> FindSignatureObjects(Role roleToCheck)
        {
            _log.Debug("(start) findSignaurObject (...) ");

            _log.Debug("Anzahl SignerList: " + SignerList.Count);

            List<OsciSignature> signatures = new List<OsciSignature>();

            IEnumerator enumerator = SignerList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                OsciSignature signature = enumerator.Current as OsciSignature;
                if (signature != null && signature.Signer.SignatureCertificate.Equals(roleToCheck.SignatureCertificate))
                {
                    signatures.Add(signature);
                }
            }

            return signatures;
        }

        /// <summary> Fügt ein Signature() XML-Signature Element dem Objekt hinzu (wird vom Parser aufgerufen)
        /// </summary>
        /// <param name="signature">OSCISignature Objekt
        /// </param>
        internal void AddSignature(OsciSignature signature)
        {
            SignerList.Add(signature);
            _roles.Add(signature.Signer);
        }

        /// <summary> Überprüft, ob das Role Objekt für die Signaturprüfung verwendet werden kann.
        /// </summary>
        /// <param name="roleToCheck">Role Objekt
        /// </param>
        /// <returns> true, sobald das Zertifikat des Role Objektes zur Signaturprüfung benutzt werden kann.
        /// </returns>
        private bool CheckContainsSigner(Role roleToCheck)
        {
            _log.Debug("(start) checkContainsRole (...) ");
            Role[] signer = Signers;
            for (int i = 0; i < signer.Length; i++)
            {
                _log.Debug("Role Object: " + signer[i].Id);
                if (signer[i].SignatureCertificate.Equals(roleToCheck.SignatureCertificate))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary> Durchsucht Inhaltsdatensignaturen nach den verwendeten
        /// Algorithmen. Es wird true zurückgegeben, wenn Referenzen der 
        /// XML-Signatur oder die Signatur selbst mit Algorithmen erzeugt
        /// wurden, die zu dem übergebenen Prüfzeitpunkt als unsicher 
        /// eingestuft wurden. 
        /// </summary>
        /// <param name="date"> Prüfzeitpunkt </param>
        /// <returns> true, wenn unsichere Algorithmen zur Signatur verwendet wurden,
        /// andernfalls false </returns>
        public bool HasWeakSignature(Role signer, DateTimeOffset date)
        {
            if (CheckContainsSigner(signer) == false)
            {
                throw new Exception("Message is not signed by given role object " + signer.Id + ".");
            }

            if (Crypto.IsWeak(signer.SignatureCertificate, date))
            {
                return true;
            }

            List<OsciSignature> signatures = FindSignatureObjects(signer);
            return signatures.Any(osciSignature => Crypto.IsWeak(osciSignature, date));
        }

        /// <summary>Überprüft alle Signaturen in dem ContentContainer. Die Hinweise zu
        /// transformierten Daten (s. checkSignature(Role)) sind zu beachten.
        /// </summary>
        /// <returns> true, sobald alle Prüfungen positiv ausgefallen sind
        /// </returns>
        /// <seealso cref="CheckSignature">
        /// </seealso>
        public bool CheckAllSignatures()
        {
            if (Signers.Length == 0)
            {
                throw new OsciSignatureException("no_signature");
            }
            return Signers.ToList().TrueForAll(CheckSignature);
        }

        /// <summary> Diese Methode signiert mit dem angegebenen Rollen-Objekt den kompletten
        /// ContentContainer. Es wird der im DialogHandler festgelegte Standardhashalgorithmus 
        /// verwendet (zur Berechnung der Hashwerte der Nachrichtenbestandteile, die in das 
        /// SignedInfo-Element eingetragen werden). Der Signaturalgorithmus wird von der 
        /// verwendeten Signer-Implementierung festgelegt. 
        /// </summary>
        /// <param name="signer">Role-Objekt mit dem Signer-Objekt
        /// </param>
        /// <exception cref="OsciRoleException">wenn für das übergebene Rollenobjekt kein
        /// Signer-Objekt gesetzt wurde oder diesem das erforderliche
        /// Signaturzertifikat fehlt.
        /// </exception>
        /// <exception cref="System.Exception">bei Schreib-/Lesefehlern
        /// </exception>
        public void Sign(Role signer)
        {
            Sign(signer, DialogHandler.DigestAlgorithm);
        }

        /// <summary> Diese Methode signiert mit dem angegebenen Rollen-Objekt den kompletten
        /// ContentContainer.
        /// <b>Hinweis:</b> Wenn der ContentContainer einen verschlüsselten 
        /// ContentContainer (EncryptedDataOSCI) enthält, so werden die (ebenfalls 
        /// verschlüsselten) Attachments, die ggf. in diesem Container referenziert
        /// sind, nicht mit signiert. Der Grund ist, dass bei einer Signaturprüfung
        /// Refenzen auf Attachments im verschlüsselten ContentContainer nicht
        /// überprüft werden können.	
        /// </summary>
        /// <param name="signer">Role-Objekt mit dem Signer-Objekt
        /// </param>
        /// <exception cref="OsciRoleException">wenn für das übergebene Rollenobjekt kein
        /// Signer-Objekt gesetzt wurde oder diesem das erforderliche
        /// Signaturzertifikat fehlt.
        /// </exception>
        /// <exception cref="System.Exception">bei Schreib-/Lesefehlern
        /// </exception>
        public void Sign(Role signer, string digestAlgorithm)
        {
            Sign(signer, digestAlgorithm, null);
        }

		/// <summary> Diese Methode signiert mit dem angegebenen Rollen-Objekt den kompletten
		/// ContentContainer. Der Signaturzeitpunkt kann im ISO-8601-Format übergeben 
		/// werden. 
		/// <b>Hinweis:</b> Wenn der ContentContainer einen verschlüsselten 
		/// ContentContainer (EncryptedDataOSCI) enthält, so werden die (ebenfalls 
		///verschlüsselten) Attachments, die ggf. in diesem Container referenziert
		///sind, nicht mit signiert. Der Grund ist, dass bei einer Signaturprüfung
		///Refenzen auf Attachments im verschlüsselten ContentContainer nicht
		/// überprüft werden können.	
		/// <b>Die Verwendung dieser Methode führt zur Inkompatibilität der
		/// erzeugten Nachrichten mit älteren Versionen der OSCI 1.2-Transportbibliothek 
		/// (vor 1.4). Sie sollte nur in Szenarien eingesetzt werden, in denen sichergestellt 
		/// ist, dass alle beteiligten Kommunikationspartner aktuelle Implementierungen 
		/// verwenden.</b>
		/// </summary>
		/// <param name="signer">Role-Objekt mit dem Signer-Objekt
		/// </param>
		/// <param name="digestAlgorithm">Zu verwendender Hashalgorithmus
		/// </param>
		/// <param name="time">Signaturzeitpunkt im ISO 8601-Format
		/// </param>
		/// <exception cref="OsciRoleException">wenn für das übergebene Rollenobjekt kein
		/// Signer-Objekt gesetzt wurde oder diesem das erforderliche
		/// Signaturzertifikat fehlt.
		/// </exception>
		/// <exception cref="System.Exception">bei Schreib-/Lesefehlern
		/// </exception>
		[Obsolete]
		public void Sign(Role signer, string digestAlgorithm, string time)
        {
            if (!_roles.Contains(signer))
            {
                _roles.Add(signer);
            }
            if (!(signer is Author) && !(signer is Originator))
            {
                throw new OsciRoleException("wrong_role_sign_cont");
            }
            int ret = 0;
            OsciSignature sig = new OsciSignature();
            if (SignerList.Count == 0)
            {
                _log.Debug("Anzahl der Contents: " + _contentList.Count);
                Content cnt;
                for (int i = 0; i < _contentList.Count; i++)
                {
                    // Contents und EncryptedData müssen dieselben NS-Prefixes verwenden
                    cnt = _contentList[i];
                    SetNamespacePrefixes(cnt.SoapNsPrefix, cnt.OsciNsPrefix, cnt.DsNsPrefix, cnt.XencNsPrefix, cnt.XsiNsPrefix);
                    AddSignatureReference(sig, cnt, digestAlgorithm);
                }
                AddAttachmentSigRefs(sig, this, digestAlgorithm);
                EncryptedDataOsci enc;
                for (int i = 0; i < _encryptedDataList.Count; i++)
                {
                    enc = (EncryptedDataOsci)_encryptedDataList[i];
                    SetNamespacePrefixes(enc.SoapNsPrefix, enc.OsciNsPrefix, enc.DsNsPrefix, enc.XencNsPrefix, enc.XsiNsPrefix);
                    AddSignatureReference(sig, enc, digestAlgorithm);
                }
            }
            else
            {
                foreach (OsciSignatureReference sigRef in ((OsciSignature)SignerList[0]).Refs.Values)
                {
                    _log.Trace("RefID: " + sigRef.RefId);
                    // Hinzufügen der signature references nur sobald es nicht die Timestamp reference ist
                    if (!sigRef.RefId.StartsWith("#" + RefId + "TS"))
                    {
                        sig.AddSignatureReference(sigRef);
                    }
                }
            }

            if (time != null)
            {
                sig.AddSignatureTime(time, RefId + "TS" + (_signedSigPropNr++), digestAlgorithm);
            }

            sig.Sign(signer);
            SignerList.Add(sig);
        }

        internal void AddAttachment(Attachment attachment)
        {
            _attachments.Put(attachment.RefId, attachment);
        }


        /**
          * Fügt Attachment-Signaturreferenzen rekursiv hinzu.
          * @param sig
          * @param coco
          * @throws OSCIException
          * @throws IOException
          * @throws NoSuchAlgorithmException
          */
        private void AddAttachmentSigRefs(OsciSignature sig, ContentContainer coco, string digestAlgorithm)
        {
            Content cnt;
            for (int i = 0; i < coco._contentList.Count; i++)
            {
                cnt = coco._contentList[i];
                if (cnt.ContentType == ContentType.AttachmentReference)
                {
                    if (!sig.GetReferences().ContainsKey("cid:" + cnt.Attachment.RefId))
                    {
                        AddSignatureReference(sig, cnt.Attachment, digestAlgorithm);
                    }
                }
                else if (cnt.ContentType == ContentType.ContentContainer)
                {
                    AddAttachmentSigRefs(sig, cnt.ContentContainer, digestAlgorithm);
                }
            }
        }

        private void AddSignatureReference(OsciSignature sig, MessagePart mp, string algo)
        {
            sig.AddSignatureReference(new OsciSignatureReference(mp, algo));
        }

        /// <summary> Fügt dem ContentContainer ein Content-Objekt hinzu.
        /// </summary>
        /// <param name="content">das hinzuzufügende Content-Objekt
        /// </param>
        /// <seealso cref="Content">
        /// </seealso>
        public void AddContent(Content content)
        {
            AddContent(content, false);
        }

        internal void AddContent(Content content, bool allowDuplicateRefId)
        {
            if ((SignerList.Count > 0) && (StateOfObject == StateOfObjectConstruction))
            {
                throw new SystemException("Content-Container wurde bereits signiert.");
            }

            if (content == null || _contentList.Contains(content))
            {
                return;
            }

            if (!allowDuplicateRefId && _contentList.Any(_ => string.Equals(content.RefId, _.RefId)))
            {
                throw new IllegalArgumentException(string.Format("Content mit Ref-ID '{0}' ist bereit vorhanden.", content.RefId));
            }

            _contentList.Add(content);
            if (content.Attachment != null)
            {
                _attachments.Put(content.Attachment.RefId, content.Attachment);
            }
            if (content.ContentContainer != null)
            {
                Role[] roles = content.ContentContainer.Roles;
                for (int i = 0; i < roles.Length; i++)
                {
                    _roles.Add(roles[i]);
                }
                Attachment[] atts = content.ContentContainer.Attachments;
                if (atts != null)
                {
                    for (int i = 0; i < atts.Length; i++)
                    {
                        _attachments.Put(atts[i].RefId, atts[i]);
                    }
                }
            }
        }

        /// <summary> Entfernt ein Content-Objekt aus dem ContentContainer.
        /// </summary>
        /// <param name="content">das zu entfernende Content-Objekt
        /// </param>
        /// <seealso cref="Content">
        /// </seealso>
        public void RemoveContent(Content content)
        {
            if ((SignerList.Count > 0) && (StateOfObject == StateOfObjectConstruction))
            {
                throw new SystemException("Content-Container wurde bereits signiert.");
            }

            _log.Debug("start remove");
            _contentList.Remove(content);
            if (content.Attachment != null)
            {
                SupportClass.HashtableRemove(_attachments, content.Attachment.RefId);
            }
        }

        /// <summary> Fügt dem ContentContainer ein EncryptedData-Objekt hinzu.
        /// </summary>
        /// <param name="encryptedDataElement">das hinzuzufügende EncryptedData-Objekt
        /// </param>
        /// <seealso cref="EncryptedDataOsci">
        /// </seealso>
        public void AddEncryptedData(EncryptedDataOsci encryptedDataElement)
        {
            if ((SignerList.Count > 0) && (StateOfObject == StateOfObjectConstruction))
            {
                throw new SystemException("Content-Container wurde bereits signiert.");
            }
            if (!_encryptedDataList.Contains(encryptedDataElement))
            {
                _log.Trace("Encrypted-Data Element wird hinzugefügt.");
                _encryptedDataList.Add(encryptedDataElement);
            }
            Attachment[] atts = encryptedDataElement.Attachments;
            for (int i = 0; i < atts.Length; i++)
            {
                if (!_attachments.Contains(atts[i]))
                {
                    _attachments.Put(atts[i].RefId, atts[i]);
                }
            }
            Role[] roles = encryptedDataElement.Roles;
            for (int i = 0; i < roles.Length; i++)
            {
                _roles.Add(roles[i]);
            }
            _log.Trace("Anzahl der neuen Roles: " + roles.Length);
        }

        /// <summary> Entfernt ein EncryptedData-Objekt aus dem ContentContainer.
        /// Der zweite Parameter gibt an, ob bei EncryptedData-Objekten, die ein
        /// verschlüsseltes Attachment referenzieren, dieses aus dem ContentContainer
        /// entfernt wird.
        /// Dies ist von Bedeutung, wenn ein Attachment in mehreren
        /// EncryptedData-Objekten referenziert wird.
        /// </summary>
        /// <param name="encryptedDataElement">das zu entfernende EncryptedData-Objekt
        /// </param>
        /// <param name="removeAttachment"><b>true</b> -> Attachments, welche im EncryptedData-Objekt
        /// referenziert sind, werden ebenfalls aus dem ContentContainer entfernt.
        /// </param>
        /// <seealso cref="EncryptedDataOsci">
        /// </seealso>
        public void RemoveEncryptedData(EncryptedDataOsci encryptedDataElement, bool removeAttachment)
        {
            if ((SignerList.Count > 0) && (StateOfObject == StateOfObjectConstruction))
            {
                throw new SystemException("Content-Container wurde bereits signiert.");
            }
            _encryptedDataList.Remove(encryptedDataElement);
            if (removeAttachment)
            {
                foreach (Attachment encryptedAttachment in encryptedDataElement.Attachments)
                {
                    if (_attachments.ContainsKey(encryptedAttachment.RefId))
                    {
                        SupportClass.HashtableRemove(_attachments, encryptedAttachment);
                    }
                }
            }
        }

        public override void WriteXml(Stream outRenamed)
        {
            WriteXml(outRenamed, true);
        }

        private void WriteXml(Stream stream, bool inner)
        {
            int i;
            stream.Write("<" + OsciNsPrefix + ":ContentContainer");
            if (stream is SymCipherOutputStream || (!(stream is CryptoStream) && !inner)) //pr nov5
            {
                stream.Write(Ns, 0, Ns.Length);
            }
            if (!string.IsNullOrEmpty(RefId))
            {
                stream.Write(" Id=\"" + RefId + "\"");
            }
            stream.Write(">");

            if (SignerList.Count > 0)
            {
                for (i = 0; i < SignerList.Count; i++)
                {
                    ((OsciSignature)SignerList[i]).WriteXml(stream);
                }
            }
            for (i = 0; i < _contentList.Count; i++)
            {
                _contentList[i].WriteXml(stream, inner);
            }
            for (i = 0; i < _encryptedDataList.Count; i++)
            {
                ((EncryptedDataOsci)_encryptedDataList[i]).WriteXml(stream, inner);
            }
            stream.Write("</" + OsciNsPrefix + ":ContentContainer>");
        }

        /// <summary> Serialisiert den ContentContainer.
        /// </summary>
        /// <returns> Xml-String des ContentConatiners
        /// </returns>
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
            catch (Exception ex)
            {
                _log.Error("Error", ex);
                return "";
            }
        }
    }
}