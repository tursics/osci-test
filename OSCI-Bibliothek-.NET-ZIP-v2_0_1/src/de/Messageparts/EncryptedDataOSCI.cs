using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Encryption;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.Messagetypes;
using Osci.Roles;
using Osci.Signature;

namespace Osci.MessageParts
{
    /// <summary> <p>Die EncryptedDataOSCI-Klasse stellt einen Datencontainer für verschlüsselte
    /// Daten in einer OSCI-Nachricht dar. Ein EncryptedDataOSCI-Objekt wird in eine
    /// OSCI-Nachricht eingestellt oder in einen Content-Conatiner(bei Merhrfachverschlüsselung)
    /// und  kann selbst entweder ein Content-Container enthalten (Normalfall) oder ein
    /// EncryptedData-Objekt (für den Parser).</p>
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
    /// <seealso cref="ContentContainer">
    /// </seealso>
    /// <seealso cref="Attachment">
    /// </seealso>
    public class EncryptedDataOsci
        : MessagePart
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(EncryptedDataOsci));

        /// <summary> Liefert die eingestellten Attachment-Objekte des ContentContainer.
        /// </summary>
        /// <value> Array der zugehörigen Attachments
        /// </value>
        public Attachment[] Attachments => _attachments.ToArray();

        /// <summary> Liefert die eingestellten Role-Objekte des EncryptedData-Objektes,
        /// welche für die Signatur und/oder Verschlüsselung verwendet wurden.
        /// </summary>
        /// <value> Array der verwendeten Role-Objekte.
        /// </value>
        public Role[] Roles => _roles.ToArray();

        /// <summary> Liefert die eingestellten Role-Objekte des EncryptedData-Objektes,
        /// mit dem die Daten verschlüsselt wurden.
        /// </summary>
        /// <value>Array der Verschlüsselungs-Role-Objekte.
        /// </value>
        public Role[] Readers => _roles.ToArray();

        public string Namespace
        {
            get; set;
        }

        internal EncryptedData EncryptedDataObject
        {
            get;
        }

        public string SymEncryptionMethod
        {
            get { return EncryptedDataObject.EncryptionMethodAlgorithm; }
        }

        private readonly int _keyIDs = 0;

        private enum DataType
        {
            Unknown,
            Attachment,
            ContentContainer
        }
        private readonly DataType _dataType = DataType.Unknown;

        private enum ObjectState
        {
            Unknown,
            Encrypted,
            Decrypted
        }
        private ObjectState _stateOfObject = ObjectState.Unknown;

        private readonly Hashtable _encryptedKeyList;
        private readonly MessagePart _content;
        private readonly OsciMessage _msg;
        private readonly SecretKey _secretKey;
        private readonly List<Role> _roles;
        private readonly List<Attachment> _attachments;
        private readonly List<Role> _readers;

        #region c'tor

        private EncryptedDataOsci()
        {
            _roles = new List<Role>();
            _readers = new List<Role>();
            _attachments = new List<Attachment>();
            _encryptedKeyList = new Hashtable();
        }

        /**
         * Interner Konstruktor für das Hinzufügen von verschlüsselten Attachments zur Nachricht
         * @param attachment
         * @throws IllegalArgumentException
         * @throws IOException
         */
        internal EncryptedDataOsci(Attachment attachment)
            : this()
        {
            if (attachment == null)
            {
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString(DialogHandler.ResourceBundle.GetString("invalid_thirdargument") + " attachment = null"));
            }

            _dataType = DataType.Attachment;
            _attachments.Add(attachment);

            if (!attachment.Encrypted)
            {
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString(DialogHandler.ResourceBundle.GetString("error_unencrypted_attachment")));
            }
            
            _log.Debug("Secret-Key des Attachments wird verwendet.");

            CipherReference refRenamed = new CipherReference("cid:" + attachment.RefId);
            RefId = "Attachment" + attachment.RefId;
            EncryptedDataObject = new EncryptedData(refRenamed, RefId, attachment.SecretKey, attachment.IvLength);
            _stateOfObject = ObjectState.Encrypted;

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.MgmtData = Helper.Base64.Encode(attachment.SecretKey.Key);
            EncryptedDataObject.KeyInfo = keyInfo;
            _content = attachment;
        }

        /**
         * Legt ein EncryptedData-Objekt für AES-256-Verschlüsselung an, welches
         * als zu verschlüsselnden Inhalt das übergebenen ContentContainer-Objekt
         * enthält. Es wird für die symmetrische Verschlüsselung ein
         * TripleDES-Schlüssel erzeugt.
         *
         * @param coco Inhaltsdatencontainer mit den zu verschlüsselnden Daten
         * @throws NoSuchAlgorithmException wenn ein nicht unterstützter Algorothmus
         * übergeben wurde
         */
        public EncryptedDataOsci(ContentContainer coco)
            : this(new SecretKey(Constants.DefaultSymmetricCipherAlgorithm), Constants.DefaultGcmIVLength, coco)
        {
        }

        public EncryptedDataOsci(SymmetricCipherAlgorithm symmetricCipherAlgorithm, int ivLength, ContentContainer coco)
            : this(new SecretKey(symmetricCipherAlgorithm), ivLength, coco)
        {
        }

        public EncryptedDataOsci(SymmetricCipherAlgorithm symmetricCipherAlgorithm, ContentContainer coco)
            : this(new SecretKey(symmetricCipherAlgorithm), Constants.DefaultGcmIVLength, coco)
        {
        }

        /// <summary> Interner Konstruktor für das Hinzufügen von verschlüsselten Attachments zur Nachricht
        /// </summary>
        /// <param name="secretKey">geheimer Schlüssel
        /// </param>
        /// <param name="attachment">attachment 
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        internal EncryptedDataOsci(SecretKey secretKey, Attachment attachment)
            : this()
        {
            if (attachment == null)
            {
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString("invalid_thirdargument") + " attachment is null");
            }
            _dataType = DataType.Attachment;
            _attachments.Add(attachment);
            if (!attachment.Encrypted)
            {
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString("invalid_thirdargument") + " attachment must be encrypted");
            }

            _secretKey = attachment.SecretKey;
            _log.Debug("Secret-Key Des Attachment wird verwendet. Info: " + new string(secretKey.Key.ToString().ToCharArray()));
            CipherReference refRenamed = new CipherReference("cid:" + attachment.RefId);
            RefId = "Attachment" + attachment.RefId;//pr mai
            EncryptedDataObject = new EncryptedData(refRenamed, RefId, secretKey, attachment.IvLength);
            _stateOfObject = ObjectState.Encrypted;
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.MgmtData = Helper.Base64.Encode(attachment.SecretKey.Key);
            EncryptedDataObject.KeyInfo = keyInfo;
            _content = attachment;
        }

        /// <summary> Legt eine EncryptedData-Objekt an, welches als zu verschlüsselnden Inhalt
        /// das übergebene MessagePart-Objekt enthält.
        /// </summary>
        /// <param name="secretKey">geheimer Schlüssel zum Verschlüsseln der Daten, der
        /// wiederum in verschlüsselter Form in die EncryptedKey-Elemente eingetragen
        /// wird. Wird als dritter Parameter ein Attachment-Objekt übergeben, so wird
        /// dieser erste Parameter ignoriert und der geheime Schlüssel aus dem
        /// Attachment-Objekt übernommen.
        /// </param>
        /// <param name="coco">Inheltsdatenobjekt mit den zu verschlüsselnden Daten vom
        /// Typ ContentContainer, EncryptedData oder Attachment
        /// </param>
        /// <exception cref="System.Exception">im Fehlerfall
        /// </exception>
        public EncryptedDataOsci(SecretKey secretKey, ContentContainer coco)
            : this(secretKey, Constants.DefaultGcmIVLength, coco)
        {
        }

        /// <summary> Legt eine EncryptedData-Objekt an, welches als zu verschlüsselnden Inhalt
        /// das übergebene MessagePart-Objekt enthält.
        /// </summary>
        /// <param name="secretKey">geheimer Schlüssel zum Verschlüsseln der Daten, der
        /// wiederum in verschlüsselter Form in die EncryptedKey-Elemente eingetragen
        /// wird. Wird als dritter Parameter ein Attachment-Objekt übergeben, so wird
        /// dieser erste Parameter ignoriert und der geheime Schlüssel aus dem
        /// Attachment-Objekt übernommen.
        /// </param>
        /// <param name="ivLength">Länge des IV in Bytes
        /// </param>
        /// <param name="coco">Inhaltsdatenobjekt mit den zu verschlüsselnden Daten vom
        /// Typ ContentContainer, EncryptedData oder Attachment
        /// </param>
        /// <exception cref="System.Exception">im Fehlerfall
        /// </exception>
        public EncryptedDataOsci(SecretKey secretKey, int ivLength, ContentContainer coco)
            : this()
        {
            Transformers.Add(Canonizer);
            if (secretKey == null)
            {
                secretKey = new SecretKey();
            }
            if (coco == null)
            {
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString("invalid_thirdargument") + " coco should not be 'null'");
            }
            _dataType = DataType.ContentContainer;
            _secretKey = secretKey;
            _content = coco;
            CipherValue valueRenamed = new CipherValue(coco, ivLength);
            EncryptedDataObject = new EncryptedData(valueRenamed, RefId, _secretKey, ivLength);
            Attachment[] atts = ((ContentContainer)_content).Attachments;
            if ((atts != null) && (atts.Length > 0))
            {
                for (int i = 0; i < atts.Length; i++)
                {
                    _attachments.Add(atts[i]);
                }
            }
            foreach (Role role in coco.Roles)
            {
                _roles.Add(role);
            }
            EncryptedDataObject.Namespace = coco.Ns.AsString();
        }

        /// <summary> Dieser Konstruktor wird beim parsen aufgerufen
        /// </summary>
        /// <param name="encryptedData">encryptedData
        /// </param>
        internal EncryptedDataOsci(EncryptedData encryptedData, OsciMessage osciMsg)
            : this()
        {
            EncryptedDataObject = encryptedData;
            _stateOfObject = ObjectState.Encrypted;
            RefId = encryptedData.Id;
            _msg = osciMsg;
            EncryptedKey[] encKeys = encryptedData.KeyInfo.EncryptedKeys;
            for (int i = 0; i < encKeys.Length; i++)
            {
                string uri = encKeys[i].KeyInfo.RetrievalMethod.Uri;
                if (uri.StartsWith("#"))
                {
                    uri = uri.Substring(1);
                }
                Role role = osciMsg.GetRoleForRefId(uri);
                if (role == null)
                {
                    throw new IOException("Das Role Object mit der RefId " + uri + " konnte in der OSCIMessage nicht gefunden werden ");
                }
                _roles.Add(role);
                _readers.Add(role);
            }
        }

        #endregion

        public void SetRefId(string refId)
        {
            RefId = refId;
        }

        /// <summary><p>Diese Methode entschlüsselt das EncryptedDataOSCI Objekt mit dem übergebenem Role Objekt.</p>
        /// <p>Die Verschlüsselung wird aufgehoben, die Entschlüsselten Informationen geparst und ein
        /// ContentContainer Objekt aufgebau. Decryptet das EncryptedData Object und liefert ein entschlüsselten 
        /// ContentContainer als Return-Wert. </p>
        /// </summary>
        /// <param name="reader">enthält die Entschlüsselungsinformationen. Das Role-Objekt muss auf
        /// jeden Fall ein Decrypter Objekt zur Aufhebung der Verschlüsselung enthalten.
        /// </param>
        /// <returns> Liefert den decrypted ContentContainer als Antwort
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        /// <exception cref="System.SystemException">
        /// </exception>
        public ContentContainer Decrypt(Role reader)
        {
            if (reader == null)
            {
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString("wrong_role_cipher_obj"));
            }
            string rdId = null;

            for (int i = 0; i < _roles.Count; i++)
            {
                if (reader.CipherCertificate.Equals(_roles[i].CipherCertificate))
                {
                    rdId = _roles[i].CipherCertificateId;
                    break;
                }
            }

            if (rdId == null)
            {
                throw new IllegalArgumentException(DialogHandler.ResourceBundle.GetString("no_encryption_for_role"));
            }

            if (_stateOfObject == ObjectState.Decrypted)
            {
                throw new SystemException(DialogHandler.ResourceBundle.GetString("invalid_stateofobject") + " Object is decrypted.");
            }
            _log.Trace("Start");
            if (rdId.IndexOf("_") >= 0)
            {
                rdId = rdId.Substring(rdId.IndexOf('_'));
            }
            EncryptedKey encKey = EncryptedDataObject.FindEncrypedKey(rdId);

            _log.Trace("Anzahl der Empfänger: " + _encryptedKeyList.Count);
            if ((encKey == null) || !reader.HasCipherPrivateKey())
            {
                IEnumerator enumRenamed = _encryptedKeyList.Keys.GetEnumerator();
                while (enumRenamed.MoveNext())
                {
                    _log.Trace("Verschlüsselt für: " + ((Role)enumRenamed.Current).CipherCertificateId);
                }
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString("wrong_role_cipher_obj"));
            }
            _log.Debug("key: " + encKey.KeyInfo.RetrievalMethod.Uri);
            // es wurde ein EncryptedKey gefunden
            // entschlüsseln des symmetrischen Schlüssels
            Stream keyIn = encKey.CipherData.CipherValue.CipherValueBuffer.InputStream;
            keyIn.Seek(0, SeekOrigin.Begin);

            Base64InputStream base64In = new Base64InputStream(keyIn);
            byte[] dt = base64In.ToArray();
            byte[] decryptedKey;

            if (encKey.EncryptionMethodAlgorithm.Equals(Constants.AsymmetricCipherAlgorithmRsaOaep))
            {
                decryptedKey = reader.Decrypter.Decrypt(dt, encKey.MgfAlgorithm, encKey.DigestAlgorithm);
            }
            else
            {
                decryptedKey = reader.Decrypter.Decrypt(dt);
            }

            // Unterscheidung ob es sich um eine CipherRef oder Cipher Value handelt
            if (EncryptedDataObject.CipherData.CipherReference != null)
            {
                CipherReference cipherRef = EncryptedDataObject.CipherData.CipherReference;
                string refId = EncryptedDataObject.Id;
                if (refId.StartsWith("Attachment"))
                {
                    _log.Error("EncryptedData object is an attachment.");
                    throw new OsciCipherException(DialogHandler.ResourceBundle.GetString("decryption_error") + " EncryptedData object is an attachment.");
                }
                else if (refId.StartsWith("encOSCI_Msg"))
                {
                    _log.Trace("Es handelt sich um eine verschlüsselte OSCI-Nachricht.");
                    throw new OsciCipherException(DialogHandler.ResourceBundle.GetString("decryption_error") + " EncryptedData object is a OSCI-Message.");
                }
                else
                {
                    _log.Trace("Es handelt sich um eine CipherReference unbekannter herkunft.");
                    throw new OsciCipherException(DialogHandler.ResourceBundle.GetString("decryption_error") + " EncryptedData object is a unknown CipherReferenz.");
                }
            }
            else
            {
                _log.Trace("Es handelt sich um eine CipherValue.");

				if (!EncryptedDataObject.IVLengthParsed)
				{
					// kein IV-Length-Element geparsed? Dann benutze alten IV-Standard 16 Byte /128 Bit
					EncryptedDataObject.IVLength = 16;
				}

				Base64InputStream bin = new Base64InputStream(EncryptedDataObject.CipherData.CipherValue.CipherValueBuffer.InputStream);
                SymCipherInputStream cin = new SymCipherInputStream(bin, new SecretKey(decryptedKey, EncryptedDataObject.EncryptionMethodAlgorithm), EncryptedDataObject.IVLength);
                _stateOfObject = ObjectState.Decrypted;
                return ParseInputStream(cin);
            }
        }

        /// <summary> Diese Methode erstellt die EncryptedData Strukturen ohne den symetrischen Schlüssel nocheinmal
        /// zu verschlüsseln. Dies kann bei vielen Verschlüsselungen zu einer schnelleren Verarbeitung mit wenig
        /// PIN eingaben führen.
        /// </summary>
        /// <param name="encryptedSymKey">verschlüsselter symmetrischer Schlüssel.
        /// </param>
        /// <param name="reader">Role-Objekt, welches den Leser repräsentiert.
        /// </param>
        public void Encrypt(byte[] encryptedSymKey, Role reader)
        {
            Encrypt(encryptedSymKey, reader, Constants.DefaultAsymmetricCipherAlgorithm);
        }

        /// <summary> Diese Methode erstellt die EncryptedData Strukturen ohne den symetrischen Schlüssel nocheinmal
        /// zu verschlüsseln. Dies kann bei vielen Verschlüsselungen zu einer schnelleren Verarbeitung mit wenig
        /// PIN eingaben führen.
        /// </summary>
        /// <param name="encryptedSymKey">verschlüsselter symmetrischer Schlüssel.
        /// </param>
        /// <param name="reader">Role-Objekt, welches den Leser repräsentiert.
        /// </param>
        public void Encrypt(byte[] encryptedSymKey, Role reader, AsymmetricCipherAlgorithm algorithm)
        {
            if (!_readers.Contains(reader))
            {
                _roles.Add(reader);
                _readers.Add(reader);
            }
            _stateOfObject = ObjectState.Encrypted;
            _log.Trace("Encrypted-Data Methode encrypt mit :" + reader.CipherCertificateId); //+ " encryptedSymKey: " +
            KeyInfo keyInfo;
            if (_content == null)
            {
                keyInfo = new KeyInfo("#" + reader.CipherCertificate);
            }
            else
            {
                keyInfo = new KeyInfo("#" + reader.CipherCertificateId);
            }
            CipherValue cipherValue = new CipherValue(encryptedSymKey);
            cipherValue.UseSecretKey(false);
            EncryptedKey encryptedKey = new EncryptedKey(algorithm, cipherValue);
            _log.Trace("Encrypted Base64 CipherValue: " + new string(cipherValue.CipherValueBytes.Select(_ => (char)_).ToArray()));
            encryptedKey.Id = "EncData_" + RefId + "_" + _keyIDs;
            encryptedKey.KeyInfo = keyInfo;
            if (EncryptedDataObject.KeyInfo == null)
            {
                EncryptedDataObject.KeyInfo = new KeyInfo();
            }
            EncryptedDataObject.KeyInfo.AddEncryptedKey(encryptedKey);
            _encryptedKeyList.Put(reader, encryptedKey);
            if (_content is ContentContainer)
            {
                CreateEncryptedAttachments((ContentContainer)_content);
            }
            _log.Trace("Fertig mit Encrypted-Data Methode encrypt mit.");
        }

        /// <summary> Diese Methode parst den soeben entschlüsselten XML-Strom und liefert das Ergebnisobjekt zurück.
        /// </summary>
        /// <param name="in">Stream, der geparst wird.
        /// </param>
        /// <param name="reader">reader
        /// </param>
        /// <returns>  Erstelltes Object (derzeit nur coco)
        /// </returns>
        /// <exception cref="OsciCipherException">
        /// </exception>
        private ContentContainer ParseInputStream(Stream stream)
        {
            Canonizer can = new Canonizer(stream, null, GlobalSettings.IsDuplicateIdCheckEnabled);
            XmlReader xmlReader = new XmlReader();
            ContentPackageBuilder packageBuilder = new ContentPackageBuilder(xmlReader, _msg, can);
            xmlReader.ContentHandler = packageBuilder;
            xmlReader.Parse(can);
            stream.Close();
            if (packageBuilder.LastCreatedObject is ContentContainer)
            {
                ContentContainer contentContainer = (ContentContainer)packageBuilder.LastCreatedObject;
                DecryptAttachments(contentContainer);
                return contentContainer;
            }
            else
            {
                throw new OsciCipherException("sax_exception");
            }
        }
        private void DecryptAttachments(ContentContainer cc)
        {
            EncryptedDataOsci[] encDatas = cc.EncryptedData;

            for (int i = 0; i < encDatas.Length; i++)
            {
                if (encDatas[i].EncryptedDataObject.CipherData.CipherReference != null)
                {
                    string uri = encDatas[i].EncryptedDataObject.CipherData.CipherReference.Uri;
                    if (uri.StartsWith("cid:"))
                    {
                        uri = uri.Substring(4);
                    }
                    encDatas[i].RefId = uri;

                    Hashtable msgAtts = _msg.AttachmentList;
                    Attachment attachment = (Attachment)msgAtts[uri];
                    attachment.StateOfAttachment = Attachment.StateOfAttachmentEncrypted;
                    attachment.Encrypted = true;

                    _log.Trace("das Attachment: " + attachment + " URI: " + uri);
                    attachment.SecretKey = new SecretKey(Helper.Base64.Decode(encDatas[i].EncryptedDataObject.KeyInfo.MgmtData), encDatas[i].EncryptedDataObject.EncryptionMethodAlgorithm);
                    attachment.IvLength = encDatas[i].EncryptedDataObject.IVLength;
                    cc.AddAttachment(attachment);
                    cc.RemoveEncryptedData(encDatas[i], false);
                }
            }

            Content[] cnts = cc.Contents;
            for (int i = 0; i < cnts.Length; i++)
            {
                if (cnts[i].ContentType == ContentType.ContentContainer)
                {
                    DecryptAttachments(cnts[i].ContentContainer);
                }
            }
        }

        /// <summary> Fügt beim Verschlüsseln die encryptedData Elemente der Attachments hinzu.
        /// </summary>
        private void CreateEncryptedAttachments(ContentContainer cc)
        {
            Attachment[] atts = cc.Attachments;
            if (atts != null)
            {
                ContentContainer contentContainer = (ContentContainer)_content;
                contentContainer.StateOfObject = ContentContainer.StateOfObjectParsing;

                HashSet<String> encDataRefIds = new HashSet<String>();

                EncryptedDataOsci[] encryptedDatas = contentContainer.EncryptedData;

                for (int i = 0; i < encryptedDatas.Length; i++)
                {
                    encDataRefIds.Add(encryptedDatas[i].RefId);
                }

                for (int i = 0; i < atts.Length; i++)
                {
                    _log.Trace("Ein weiteres Attachment.");

                    // fix for double attachment RefIDs in nested cocos
                    if (!encDataRefIds.Add("Attachment" + atts[i].RefId))
                    {
                        continue;
                    }

                    EncryptedDataOsci encData = new EncryptedDataOsci(atts[i]);
                    encData.SetNamespacePrefixes(contentContainer.SoapNsPrefix, contentContainer.OsciNsPrefix, contentContainer.DsNsPrefix, contentContainer.XencNsPrefix, contentContainer.XsiNsPrefix);
                    encData.Namespace = contentContainer.Ns.AsString();
                    contentContainer.AddEncryptedData(encData);
                }
                Role[] roles = ((ContentContainer)_content).Roles;
                for (int i = 0; i < roles.Length; i++)
                {
                    if (!_roles.Contains(roles[i]))
                    {
                        _roles.Add(roles[i]);
                    }
                }
            }

            foreach (Content content in cc.Contents)
            {
                if (content.ContentType == ContentType.ContentContainer)
                {
                    CreateEncryptedAttachments(content.ContentContainer);
                }
            }
        }

        /// <summary> Verschlüsselt den geheimen Schlüssel und fügt ihn als EncryptedKey-Element
        /// dem EncyptedData-Element hinzu. Außerdem wird das Rollen-Objekt der Parent-
        /// Nachricht (des MessagePart-Objektes) hinzugefügt, so dass das
        /// Verschlüsselungszertifikat in der Nachricht enthalten ist.
        /// </summary>
        /// <param name="reader">Rollen-Objekt, für welches verschlüsselt werden soll.
        /// </param>
        /// <exception cref="OsciCipherException">wenn dem Rollen-Objekt das erforderliche
        /// Verschlüsselungszertifikat fehlt.
        /// </exception>
        public void Encrypt(Role reader)
        {
            Encrypt(reader, Constants.DefaultAsymmetricCipherAlgorithm);
        }

        /// <summary> Verschlüsselt den geheimen Schlüssel und fügt ihn als EncryptedKey-Element
        /// dem EncyptedData-Element hinzu. Außerdem wird das Rollen-Objekt der Parent-
        /// Nachricht (des MessagePart-Objektes) hinzugefügt, so dass das
        /// Verschlüsselungszertifikat in der Nachricht enthalten ist.
        /// </summary>
        /// <param name="reader">Rollen-Objekt, für welches verschlüsselt werden soll.
        /// </param>
        /// <param name="algorithm">Asymmetrischer Verschlüsselungsalgorithmus
        /// </param>
        /// <exception cref="OsciCipherException">wenn dem Rollen-Objekt das erforderliche
        /// Verschlüsselungszertifikat fehlt.
        /// </exception>
        public void Encrypt(Role reader, AsymmetricCipherAlgorithm algorithm)
        {
            if (!_readers.Contains(reader))
            {
                _roles.Add(reader);
                _readers.Add(reader);
            }
            _stateOfObject = ObjectState.Encrypted;
            _log.Trace("Encrypted-Data Methode encrypt mit :" + reader.CipherCertificateId + " Es handelt sich um ein EncryptedData Objekt vom Typ:" + _dataType);
            if (_secretKey == null)
            {
                throw new OsciCipherException(DialogHandler.ResourceBundle.GetString("encryption_error") + " no secretKey");
            }
            _log.Trace("Secret Key: " + new string(_secretKey.Key.ToString().ToCharArray()));
            byte[] encryptedSymKey = Crypto.Encrypt(reader.CipherCertificate, _secretKey.Key, algorithm);
            Encrypt(encryptedSymKey, reader, algorithm);
        }

        public override void WriteXml(Stream outRenamed)
        {
            WriteXml(outRenamed, true);
        }

        internal void WriteXml(Stream outRenamed, bool inner)
        {
            if (_stateOfObject == ObjectState.Unknown)
            {
                throw new SystemException(DialogHandler.ResourceBundle.GetString("invalid_stateofobject") + " not encrypted.");
            }
            EncryptedDataObject.Id = RefId;
            EncryptedDataObject.WriteXml(outRenamed, inner);
        }
    }
}