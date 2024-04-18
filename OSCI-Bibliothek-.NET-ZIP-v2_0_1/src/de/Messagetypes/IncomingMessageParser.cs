using System;
using System.Collections;
using System.IO;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Encryption;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary><H4>Streamparser</H4>
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
    public abstract class IncomingMessageParser
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(IncomingMessageParser));

        protected abstract OsciEnvelopeBuilder GetParser(XmlReader reader, DialogHandler dialogHandler);
        protected static Addressee[] DefaultSupplier;

        private OsciMessage Parse(Stream inputStream, DialogHandler dialogHandler, StoreInputStream storeInStream)
        {
            XmlReader reader = new XmlReader();
            OsciEnvelopeBuilder builder = GetParser(reader, dialogHandler);
            reader.ContentHandler = builder;
            builder.HashNCanStream = new Canonizer(inputStream, storeInStream);
            reader.Parse(builder.HashNCanStream);
            return builder.MessageBuilder.Msg;
        }

        internal OsciMessage ParseStream(Stream inputStream, DialogHandler dialogHandler, bool request, Stream storeStream)
        {
            return ParseStream(inputStream, dialogHandler, request, false, storeStream);
        }

        private OsciMessage ParseStream(Stream inputStream, DialogHandler dialogHandler, bool request, bool decryptedStream, Stream storeStream)
        {
            MimeParser incomingMsg;

            StoreInputStream storeInputStream = null;
            _log.Trace("InputStream: " + inputStream);

            if (storeStream != null)
            {
                storeInputStream = new StoreInputStream(inputStream, storeStream);
                incomingMsg = new MimeParser(storeInputStream);
            }
            else
            {
                incomingMsg = new MimeParser(inputStream);
            }

            MimePartInputStream mimeStream = incomingMsg.GetNextMimePart();
            OsciMessage msg = Parse(mimeStream, dialogHandler, storeInputStream);
            msg.BoundaryString = incomingMsg.MimeHeaders.Boundary;

            _log.Trace("Fertig mit Parsen des Transport Objektes. Msgtype: " + msg.GetType());

            if (msg.MessageType == OsciMessage.SoapMessageEncrypted)
            {
                EncryptedData encryptedData = ((SoapMessageEncrypted)msg).EncryptedData;
                mimeStream = incomingMsg.GetNextMimePart();

                string s;
                _log.Trace("Mime ID:  " + mimeStream.ContentId + " Encrypted ID: " + encryptedData.CipherData.CipherReference.Uri);

                if (!(s = "cid:" + mimeStream.ContentId).Equals(encryptedData.CipherData.CipherReference.Uri))
                {
                    throw new ArgumentException("Falsche Attachment-Id: " + s);
                }

                Role role = null;

                if (dialogHandler == null)
                {
                    for (int i = 0; i < DefaultSupplier.Length; i++)
                    {
                        if (DefaultSupplier[i].CipherCertificate.Equals(encryptedData.KeyInfo.EncryptedKeys[0].KeyInfo.X509Data.X509Certificate))
                        {
                            role = DefaultSupplier[i];
                            break;
                        }
                    }
                    if (role == null)
                    {
                        throw new SoapClientException("9200");
                    }
                }
                else
                {
                    try
                    {
                        X509Certificate cert;
                        if (request)
                        {
                            cert = dialogHandler.Supplier.CipherCertificate;
                        }
                        else
                        {
                            cert = dialogHandler.Client.CipherCertificate;
                        }
                        string s1 = cert.GetEncoded().AsString();
                        string s2 = encryptedData.KeyInfo.EncryptedKeys[0].KeyInfo.X509Data.X509Certificate.GetEncoded().AsString();

                        if (s1.Equals(s2))
                        {
                            if (request)
                            {
                                role = dialogHandler.Supplier;
                            }
                            else
                            {
                                role = dialogHandler.Client;
                            }
                        }
                    }
                    catch (OsciRoleException)
                    {
                        // wird unten abgefangen
                    }
                }

                if (role == null)
                {
                    throw new OsciRoleException("Kein Rollenobjekt mit dem Verschlüsselungszertifikat der Nachricht im DialogHandler gefunden.");
                }

                if (!encryptedData.KeyInfo.EncryptedKeys[0].EncryptionMethodAlgorithm.Equals(Constants.AsymmetricCipherAlgorithmRsa15)
                     && !encryptedData.KeyInfo.EncryptedKeys[0].EncryptionMethodAlgorithm.Equals(Constants.AsymmetricCipherAlgorithmRsaOaep))
                {
                    throw new Exception("Falscher asymmetrischer Verschlüsselungsalgorithmus: " + encryptedData.KeyInfo.EncryptedKeys[0].EncryptionMethodAlgorithm);
                }

                _log.Trace("ENC-ALGO: " + encryptedData.EncryptionMethodAlgorithm);

                Stream inKey = encryptedData.KeyInfo.EncryptedKeys[0].CipherData.CipherValue.CipherValueBuffer.InputStream;
                inKey.Seek(0, SeekOrigin.Begin);

                Base64InputStream base64InKey = new Base64InputStream(inKey);

                byte[] decryptedKey;
                using (MemoryStream bos2 = new MemoryStream())
                {
                    base64InKey.CopyToStream(bos2);

                    if (encryptedData.KeyInfo.EncryptedKeys[0].EncryptionMethodAlgorithm.Equals(Constants.AsymmetricCipherAlgorithmRsaOaep))
                    {
                        decryptedKey = role.Decrypter.Decrypt(bos2.ToArray(), encryptedData.KeyInfo.EncryptedKeys[0].MgfAlgorithm, encryptedData.KeyInfo.EncryptedKeys[0].DigestAlgorithm);
                    }
                    else
                    {
                        decryptedKey = role.Decrypter.Decrypt(bos2.ToArray());
                    }
                }

                _log.Trace("Symetrischer Schlüssel: " + decryptedKey.AsString());

                Stream inStream;
                bool b64 = mimeStream.ContentTransferEncoding.ToLower().Equals("base64") || mimeStream.ContentType.ToLower().Equals("text/base64");
                if (b64)
                {
                    inStream = new Base64InputStream(mimeStream);
                }
                else
                {
                    inStream = mimeStream;
                }

                SecretKey secretKey = new SecretKey(decryptedKey, encryptedData.EncryptionMethodAlgorithm);
                SymCipherInputStream cipherInputStream = new SymCipherInputStream(inStream, secretKey, encryptedData.IVLength);
                _log.Trace("#################### Encrypted OSCI-Msg wurde komplett verarbeitet, nun wird der Transportumschlag geöffnet und die eigentliche OSCI-Nachricht betrachtet ####################");

                msg = ParseStream(cipherInputStream, dialogHandler, request, true, storeStream);
                msg.Base64Encoding = b64;
                msg.DialogHandler.Encryption = true;

                string symEncMethod = encryptedData.EncryptionMethodAlgorithm;
                FeatureDescriptionH featureDesc = msg.FeatureDescription;

                // Wird der veraltete CBC-Padding-Modus verwendet, stelle wenn moeglich auf GCM um
                if (symEncMethod.EndsWith("-cbc") && featureDesc != null && featureDesc.SupportedFeatures != null
                    && featureDesc.SupportedFeatures.Contains(OsciFeatures.GCMPaddingModus) && !"false".Equals(System.Environment.GetEnvironmentVariable("de.osci.SwitchToGCM")))
                {
                    if (_log.IsEnabled(LogLevel.Debug))
                    {
                        _log.Trace("GCM wird in aktueller OSCI-Kommunikation unterstützt, benutze GCM für symmetrische Transportverschlüsselung");
                    }

                    msg.DialogHandler.SymmetricCipherAlgorithm = SymmetricCipherAlgorithm.Aes256Gcm;
                }
                else
                {
                    msg.DialogHandler.SymmetricCipherAlgorithm = secretKey.AlgorithmType;
                }
                
                msg.DialogHandler.AsymmetricCipherAlgorithm = encryptedData.KeyInfo.EncryptedKeys[0].AsymmetricCipherAlgorithm;

                return msg;
            }
            else
            {
                // Check, ob eine Antwort auf einen verschlüsselten request auch verschlüsselt war. 
                if (!request && msg.DialogHandler.Encryption && !decryptedStream && msg.MessageType != OsciMessage.SoapFaultMessage)
                {
                    throw new OsciCipherException(DialogHandler.ResourceBundle.GetString("unencrypted_msg"), null);
                }

                msg.DialogHandler.Encryption = false;
                // es handelt sich um eine nicht verschlüsselte Nachricht
                // die Attachments werden der Nachricht hinzugefügt
                _log.Trace("Es handelte sich um eine nicht Verschlüsselte Nachricht");
                // ## Lesen der Attachments
                _log.Trace("Nächster Schritt: Verarbeitung der Attachments");
                ReadAttachment(msg, incomingMsg);
                _log.Trace("Die Signaturen werden überprüft.");

                // Nachricht signiert?
                if (msg.DialogHandler.CheckSignatures && msg.IsSigned)
                {
                    bool sigErg = CheckMessageHashes(msg);
                    string str;

                    if (sigErg)
                    {
                        _log.Trace("Die Signaturen der XML-OSCI-Daten sind in Ordnung.");

                        IEnumerator hashes = msg.HashableMsgPart.Keys.GetEnumerator();
                        str = "Noch nicht geprüfte Signature: ";

                        while (hashes.MoveNext())
                        {
                            str += (string)hashes.Current + ", ";
                        }
                        _log.Trace(str);
                    }
                    else
                    {
                        _log.Error("Die Signaturen der XML-OSCI-Daten sind fehlerhaft.");
                        throw new OsciErrorException("9601", msg);
                    }
                }
                else
                {
                    _log.Debug("Unsignierte-Nachricht");
                }
            }

            _log.Trace("Alles ist Fertig");
            return msg;
        }

        internal bool CheckMessageHashes(OsciMessage message)
        {
            try
            {
                if (!message.SignatureRelevantElements.IsSubsetOf(message.HashableMsgPart.Keys))
                {
                    // Not all signature relevant elements are hashed
                    _log.Error("Signature check failed.");
                    throw new OsciErrorException("9602", message);
                }

                OsciSignature sig = message.SignatureHeader;
                
                // ### check the signature references and parsed hashes
                Hashtable refsHash = sig.GetDigests();
                // check the sizes of hashes and references
                if (refsHash.Count != message.HashableMsgPart.Count)
                {
                    _log.Error("The number of references and hashed parts are not equil");
                    return false;
                }                

                foreach (String key in refsHash.Keys)
                {
                    if (message.HashableMsgPart[key] == null)
                    {
                        _log.Error("Element zur Signatur-Referenz '" + key + "' nicht in Nachricht gefunden.");

                        return false;
                    }
                    else if (!Tools.CompareByteArrays((byte[])message.HashableMsgPart[key], (byte[])refsHash[key]))
                    {
                        _log.Error("Hashwerte der Signatur-Referenz '" + key + "' stimmen nicht überein.");

                        return false;
                    }
                    else
                    {
                        message.HashableMsgPart.Remove(key);
                    }
                }

                if (message.HashableMsgPart.Count > 0)
                {
                    _log.Error("Nachricht enthält " + message.HashableMsgPart.Count + " unsignierte(s) Element(e).");
                    throw new OsciErrorException("9602", message);
                }

                X509Certificate c;
                if (message is OsciRequest)
                {
                    c = message.DialogHandler.Client.SignatureCertificate;
                }
                else
                {
                    c = message.DialogHandler.Supplier.SignatureCertificate;
                }

                message.SignerCertificate = c;

                if ((c.GetKeyUsage() != null) && !c.GetKeyUsage()[0] && !c.GetKeyUsage()[1])
                {
                    _log.Error("Signature certificate has wrong key usage.");
                    return false;
                }


                bool res = Crypto.CheckSignature(c, sig.SignedInfo, sig.SignatureValue, sig.SignatureAlgorithm);

                if (!res)
                {
                    return false;
                }

                _log.Trace("Nach check Signature");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Es ist ein Fehler beim überprüfen der Hashwerte aufgetreten.", ex);
                return false;
            }
        }

        private static void ReadAttachment(OsciMessage msg, MimeParser incomingMsg)
        {
            MimePartInputStream mimeStream;
            Attachment[] atts = msg.Attachments;
            while ((mimeStream = incomingMsg.GetNextMimePart()) != null)
            {
                Attachment att = null;
                string refId = mimeStream.ContentId;
                _log.Debug("Attachment RefId: " + refId);
                bool b64 = mimeStream.ContentTransferEncoding.ToLower().Equals("base64") || mimeStream.ContentType.ToLower().Equals("text/base64");

                for (int i = 0; i < atts.Length; i++)
                {
                    if (atts[i].RefId.Equals(refId))
                    {
                        _log.Trace("Vorbereitetes Attachment gefunden. Der Stream wird nun hinzugefügt.");
                        att = atts[i];
                        att.IsBase64Encoded = b64;
                        att.BoundaryString = incomingMsg.MimeHeaders.Boundary;
                        MessagePartsEntry.AttachmentSetState(att, Attachment.StateOfAttachmentParsing, false);
                        break;
                    }
                }

                string digestMethod = null;
                if (msg.IsSigned)
                {
                    digestMethod = (string)msg.SignatureHeader.GetDigestMethods()["cid:" + refId];
                }

                if (att == null)
                {
                    // ## Verschlüsselte Attachachments
                    _log.Trace("Verschlüsseltes Attachment gefunden.");
                    if (b64)
                    {
                        att = MessagePartsEntry.Attachment(new Base64InputStream(mimeStream), refId, mimeStream.Length, digestMethod);
                    }
                    else
                    {
                        att = MessagePartsEntry.Attachment(mimeStream, refId, mimeStream.Length, digestMethod);
                    }

                    att.IsBase64Encoded = b64;
                    att.BoundaryString = incomingMsg.MimeHeaders.Boundary;
                    msg.AddAttachment(att);
                    MessagePartsEntry.AttachmentSetState(att, Attachment.StateOfAttachmentEncrypted, true);

                }
                else
                {
                    // ## Attachments wurden der Nachricht bereits hinzugefügt
                    _log.Trace("Unverschlüsseltes Attachment gefunden.");
                    if (b64)
                    {
                        MessagePartsEntry.AttachmentSetStream(att, new Base64InputStream(mimeStream), false, mimeStream.Length, digestMethod);
                    }
                    else
                    {
                        MessagePartsEntry.AttachmentSetStream(att, mimeStream, false, mimeStream.Length, digestMethod);
                    }
                }

                // ## Allgemeiner Part für Attachments
                _log.Debug("Es wurde ein Attachment hinzugefügt!RefID: " + att.RefId);
                att.MimeHeaders = mimeStream.MimeHeaders.GetHashtable();
                if (msg.IsSigned)
                {
                    msg.HashableMsgPart.Put("cid:" + att.RefId, att.EncryptedDigestValues[digestMethod]);
                }
            }
        }
    }
}