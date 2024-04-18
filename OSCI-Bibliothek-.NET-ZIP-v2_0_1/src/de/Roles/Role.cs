using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Extensions;
using Osci.Helper;

namespace Osci.Roles
{
    /// <summary> Diese Klasse ist die Superklasse aller OSCI-Rollenobjekte.
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
    public abstract class Role
    {
        /// <summary> Ruft die Id, mit der in der OSCI-Nachricht das Verschlüsselungszertifikat
        /// referenziert wird ab, oder legt diese fest. Die Id ist ein String, der aus dem Rollennamen,
        /// ist (in Kleinschreibung, alle Leerzeichen durch Unterstriche ersetzt).
        /// bei Author- und Reader-Objekten einer lfd. Nummer, der Funktionbezeichnung 
        /// (cipher) und dem SHA-1-Hashwert über den Bytes des Zertifikats
        /// besteht 
        /// </summary>
        /// <value>Id des CipherCertificate-Elementes
        /// </value>
        /// <seealso cref="SignatureCertificateId()">
        /// </seealso>
        /// <exception cref="OsciRoleException">wenn kein Verschlüsselungszertifikat eingestellt ist
        /// </exception>
        public string CipherCertificateId
        {
            get
            {
                try
                {
                    if (!HasCipherCertificate())
                    {
                        return Id;
                    }
                    if (CipherRefId != null)
                    {
                        return CipherRefId;
                    }
                    else
                    {
                        if (_cipherHash == null)
                        {
                            _cipherHash = HashCertificate(CipherCertificate);
                        }
                        return Id + "_cipher_" + _cipherHash;
                    }
                }
                catch
                {
                    return Id;
                }
            }
        }

        /// <summary> Ruft die Id, mit der in der OSCI-Nachricht das Signaturzertifikat
        /// referenziert wird ab. Die Id ist ein String, der aus dem Rollennamen,
        /// bei Author- und Reader-Objekten einer lfd. Nummer, dem Namen des
        /// Zertifikatsherausgebers sowie der Seriennummer des Zertifikats zusammengesetzt
        /// ist (in Kleinschreibung, alle Leerzeichen durch Unterstriche ersetzt).
        /// </summary>
        /// <value>Id des SignatureCertificate-Elementes
        /// </value>
        /// <seealso cref="CipherCertificateId()">
        /// </seealso>
        /// <exception cref="OsciRoleException">wenn kein Signaturzertifikat eingestellt ist
        /// </exception>
        public string SignatureCertificateId
        {
            get
            {
                try
                {
                    if (!HasSignatureCertificate())
                    {
                        return Id;
                    }
                    if (SignatureRefId != null)
                    {
                        return SignatureRefId;
                    }
                    else
                    {
                        if (_signatureHash == null)
                        {
                            _signatureHash = HashCertificate(SignatureCertificate);
                        }

                        return Id + "_signature_" + _signatureHash;
                    }
                }
                catch
                {
                    return Id;
                }
            }
        }
        /** Stellt fest, ob ein Signatur-Privatschlüssel (Signer) verfügbar ist. 
        * @return true -> Signer wurde gesetzt 
                   */
        public bool HasSignaturePrivateKey()
        {
            return _signer != null;
        }
        /**    
              * Stellt fest, ob ein Verschlüsselungs-Privatschlüssel (Decrypter) verfügbar ist. 
              * @return true -> Decrypter wurde gesetzt 
              */
        public bool HasCipherPrivateKey()
        {
            return _decrypter != null;
        }

        /** 
         * Stellt fest, ob ein Verschlüsselungszertifikat (bzw. Decrypter) verfügbar ist. 
         * @return true -> Verschlüsselungszertifikat wurde gesetzt 
         */
        public bool HasCipherCertificate()
        {
            return (_decrypter != null) || (_cipherCertificate != null);
        }

        /** 
         * Stellt fest, ob ein Signaturzertifikat (bzw. Signer) verfügbar ist. 
         * @return true -> Signaturzertifikat wurde gesetzt 
         */
        public bool HasSignatureCertificate()
        {
            return (_signer != null) || (_signatureCertificate != null);
        }

        /**
         * Helfermethode für die Berechnung des Hashwertes von Zertfifkaten.
         * Wird vom Anwender normalerweise nicht benötigt.
         *
         * @param cert zu hashendes Zertifkat
         *
         * @return hexadezimaler Hashwert
         *
         * @throws NoSuchAlgorithmException wenn Hashalgorithmus nicht verfügbar
         * @throws CertificateEncodingException bei Codierungsfehlern
         */

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string HashCertificate(X509Certificate cert)
        {
            if (cert == null)
            {
                return "no_hash_available";
            }
            return new SHA1Managed().ComputeHash(cert.GetEncoded()).AsHexString();
        }

        /// <summary> Ruft das eingestellte Signer-Objekt ab, oder null, wenn kein Objekt an den
        /// Konstruktor übergeben wurde, oder legt dieses fest.
        /// <p>Falls bereits ein Signaturzertifikat festgelegt wurde, wird dieses nach Aufruf 
        /// dieser Methode ignoriert. Alle weiteren Aufrufe von SignatureCertificate() liefern 
        /// dann das Zertifkat des Signer-Objektes zurück. Signer(null) löscht das Signer-Objekt.</p>
        /// </summary>
        /// <value>Signer-Objekt
        /// </value>
        /// <exception cref="OsciRoleException">wenn kein Signer gesetzt wurde
        /// </exception>
        public Signer Signer
        {
            get
            {
                if (_signer == null)
                {
                    throw new OsciRoleException("Kein Signer-Objekt für Objekt " + Id + " eingestellt.", "no_signer_" + Id);
                }
                return _signer;
            }
            set
            {
                _signer = value;
            }
        }

        /// <summary>Ruft das eingestellte Decrypter-Objekt ab, oder null, wenn kein Objekt an den
        /// Konstruktor übergeben wurde, oder legt dieses fest.
        /// </summary>
        /// <value>Decrypter-Objekt
        /// </value>
        /// <exception cref="OsciRoleException">wenn kein Decrypter gesetzt wurde
        /// </exception>
        public Decrypter Decrypter
        {
            get
            {
                if (_decrypter == null)
                {
                    throw new OsciRoleException("Kein Decrypter-Objekt für Objekt " + Id + " eingestellt.", "no_decrypter_" + Id);
                }
                return _decrypter;
            }
            set
            {
                _decrypter = value;
            }
        }

        /// <summary>Ruft das eingestellte Signaturzertifikat ab, oder legt dieses fest.
        /// </summary>
        /// <value> Signaturzertifikat
        /// </value>
        /// <exception cref="OsciRoleException">wenn kein Signaturzertifikat eingestellt ist.
        /// </exception>
        public X509Certificate SignatureCertificate
        {
            get
            {
                if (_signer == null && _signatureCertificate == null)
                {
                    throw new OsciRoleException("Es wurde kein Signaturzertifikate für das Rollenobjekt vom Typ " + Id + " eingestellt.", "no_signature_cert_" + Id);
                }
                else if (_signer == null)
                {
                    return _signatureCertificate;
                }
                else
                {
                    return _signer.Certificate;
                }
            }
            set
            {
                _signatureCertificate = value;
            }
        }

        /// <summary> Ruft das eingestellte Verschlüsselungszertifikat ab, oder legt dieses fest.
        /// </summary>
        /// <value> Verschlüsselungszertifikat
        /// </value>
        /// <exception cref="OsciRoleException">wenn kein Verschlüsselungszertifikat eingestellt ist.
        /// </exception>
        public X509Certificate CipherCertificate
        {
            get
            {
                if (_decrypter == null && _cipherCertificate == null)
                {
                    throw new OsciRoleException("Es wurde kein Verschlüsselungszertifikate für das Rollenobjekt vom Typ " + Id + " eingestellt.", "no_cipher_cert_" + Id);
                }
                return _decrypter == null ? _cipherCertificate : _decrypter.Certificate;
            }
            set
            {
                _cipherCertificate = value;
            }
        }

        /// <summary> Liefert (ab Version 1.3) den Signaturalgorithmus,
        /// der von der Methode Signer.getAlgorithm() zurückgeliefert wird. 
        /// Liefert diese Methode null oder ist sie nicht implementiert, 
        /// wird der im {@link DialogHandler} eingestellte Default-Signaturalgorithmus 
        /// zurückgegeben.</summary>
        /// <returns>den Identifier des Algorithmus</returns>

        public string SignatureAlgorithm
        {
            get
            {
                if (!HasSignaturePrivateKey())
                {
                    throw new Exception("Kein Signer-Objekt gesetzt.");
                }

                try
                {
                    _signatureAlgorithm = _signer.GetAlgorithm() ?? DialogHandler.SignatureAlgorithm;
                }
                catch (Exception)
                {
                    _log.Warn("No implementaion of Signer.getAlgorithm() found in Role '" + Id
                        + "', probably old Signer implementaion (< 1.3) in use. Defaulting to "
                        + DialogHandler.SignatureAlgorithm);
                    _signatureAlgorithm = DialogHandler.SignatureAlgorithm;
                }

                return _signatureAlgorithm;
            }
        }

        internal string CipherRefId;
        internal string SignatureRefId;
        internal string Id;

        private static readonly Log _log = LogFactory.GetLog(typeof(Role));
        private Signer _signer;
        private Decrypter _decrypter;
        private X509Certificate _signatureCertificate;
        private X509Certificate _cipherCertificate;
        private string _cipherHash;
        private string _signatureHash;
        private string _signatureAlgorithm = Constants.SignatureAlgorithmRsaSha256;

        protected Role()
        {
            Id = GetType().FullName.Substring(GetType().FullName.LastIndexOf('.') + 1).ToLower().Replace('$', '_');
        }
    }
}