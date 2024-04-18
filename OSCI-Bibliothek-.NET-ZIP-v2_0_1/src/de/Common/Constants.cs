using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Osci.Common
{
    /// <summary> <p>In dieser Klasse sind Konstanten der Bibliothek definiert.</p>
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
    public class Constants
    {

        /// <summary> Character-Encoding
        /// </summary>
        public const string CharEncoding = "UTF-8";

        /// <summary> Algorithmus für Zufallszahlenerstellung wird auf SHA1 Pseudo Random Number Generation gestellt
        /// </summary>
        public const string SecureRandomAlgorithmSha1 = "SHA1PRNG";

        /// <summary>Ablaufdatum für 1024 Bit RSA-Schlüssellänge
        /// </summary>
        public static DateTime OutDateKeysize1024 = new DateTime(2008, 7, 1);

        /// <summary>Default-Länge in Byte des Initialisierungsvektors (IV) für die Ver- und Entschlüsselung mit AES-GCM.
        /// </summary>
        public static int DefaultGcmIVLength = 12;

        /// <summary>Default-Gültigkeitsdatum für Algorithmusprüfungen
        /// </summary>
        public static DateTime DefaultDateForAlgorithmCheck = new DateTime(2011, 7, 1);

        #region Signature Algorithms

        // TODO: Enum

        /// <summary> Algorithmus für Signaturerstellung, Konstante für SHA1withRSA
        /// </summary>
        public const string SignatureAlgorithmRsaSha1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

        /// <summary> Algorithmus für Signaturerstellung, Konstante für SHA256withRSA
        /// </summary>
        public const string SignatureAlgorithmRsaSha256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        /// <summary> Algorithmus für Signaturerstellung, Konstante für SHA512withRSA
        /// </summary>
        public const string SignatureAlgorithmRsaSha512 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";

        /// <summary> Algorithmus für Signaturerstellung, Konstante für SHA256withRSAandMGF1 (PSS)
        /// </summary>
        public const string SignatureAlgorithmRsaSha256Pss = "http://www.w3.org/2007/05/xmldsig-more#sha256-rsa-MGF1";

        /// <summary> Algorithmus für Signaturerstellung, Konstante für SHA512withRSAandMGF1 (PSS)
        /// </summary>
        public const string SignatureAlgorithmRsaSha512Pss = "http://www.w3.org/2007/05/xmldsig-more#sha512-rsa-MGF1";

        /// <summary> Algorithmus für Signaturerstellung, Konstante für RIPEMD160withRSA
        /// </summary>
        public const string SignatureAlgorithmRsaRipemd160Rfc4051 = "http://www.w3.org/2001/04/xmldsig-more/rsa-ripemd160";

        /// <summary> Für Abwärtskompatibilität nach Korrektur der Konstante für RIPEMD160withRSA (Version 1.5, typo in RFC 4051)
        /// </summary>
        public const string SignatureAlgorithmRsaRipemd160 = "http://www.w3.org/2001/04/xmldsig-more#rsa-ripemd160";

        /// <summary> Algorithmus für Signaturerstellung, Konstante für SHA256withECDSA
        /// </summary>
        public const string SignatureAlgorithmEcdsaSha256 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256";

        /// <summary> Algorithmus für Signaturerstellung, Konstante für SHA512withECDSA
        /// </summary>
        public const string SignatureAlgorithmEcdsaSha512 = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha512";

        /// <summary> Algorithmus für Signaturerstellung, Konstante für SHA3_256withRSA
        /// </summary>
        public const string SignatureAlgorithmRsaSha3With256Bit = "http://www.w3.org/2007/05/xmldsig-more#sha3-256-rsa-MGF1";

        /// <summary> Algorithmus für Signaturerstellung, Konstante für SHA3_384withRSA
        /// </summary>
        public const string SignatureAlgorithmRsaSha3With384Bit = "http://www.w3.org/2007/05/xmldsig-more#sha3-384-rsa-MGF1";

        /// <summary> Algorithmus für Signaturerstellung, Konstante für SHA3_512withRSA
        /// </summary>
        public const string SignatureAlgorithmRsaSha3With512Bit = "http://www.w3.org/2007/05/xmldsig-more#sha3-512-rsa-MGF1";

        public static IEnumerable<string> GetAllSignatureAlgorithms()
        {
            yield return SignatureAlgorithmRsaSha1;
            yield return SignatureAlgorithmRsaSha256;
            yield return SignatureAlgorithmRsaSha512;
            yield return SignatureAlgorithmRsaSha256Pss;
            yield return SignatureAlgorithmRsaSha512Pss;
            yield return SignatureAlgorithmRsaRipemd160Rfc4051;
            yield return SignatureAlgorithmRsaRipemd160;
            yield return SignatureAlgorithmEcdsaSha256;
            yield return SignatureAlgorithmEcdsaSha512;
            yield return SignatureAlgorithmRsaSha3With256Bit;
            yield return SignatureAlgorithmRsaSha3With384Bit;
            yield return SignatureAlgorithmRsaSha3With512Bit;
        }

        #endregion Signature Algorithms

        #region Digest Algorithms

        /// <summary> Algorithmus für Hashwerterstellung, Konstante für SHA1
        /// </summary>
        public const string DigestAlgorithmSha1 = "http://www.w3.org/2000/09/xmldsig#sha1";

        /// <summary> Algorithmus für Hashwerterstellung, Konstante für SHA256
        /// </summary>
        public const string DigestAlgorithmSha256 = "http://www.w3.org/2001/04/xmlenc#sha256";

        /// <summary> Algorithmus für Hashwerterstellung, Konstante für SHA512
        /// </summary>
        public const string DigestAlgorithmSha512 = "http://www.w3.org/2001/04/xmlenc#sha512";

        /// <summary> Algorithmus für Hashwerterstellung, Konstante für RIPEMD160
        /// </summary>
        public const string DigestAlgorithmRipemd160 = "http://www.w3.org/2001/04/xmlenc#ripemd160";

        /// <summary> Algorithmus für Hashwerterstellung, Konstante für SHA3-256
        /// </summary>
        public const string DigestAlgorithmSha3With256Bit = "http://www.w3.org/2007/05/xmldsig-more#sha3-256";

        /// <summary> Algorithmus für Hashwerterstellung, Konstante für SHA3-384
        /// </summary>
        public const string DigestAlgorithmSha3With384Bit = "http://www.w3.org/2007/05/xmldsig-more#sha3-384";

        /// <summary> Algorithmus für Hashwerterstellung, Konstante für SHA3-512
        /// </summary>
        public const string DigestAlgorithmSha3With512Bit = "http://www.w3.org/2007/05/xmldsig-more#sha3-512";

        #endregion Digest Algorithms

        #region Cipher Algorithms

        public const SymmetricCipherAlgorithm DefaultSymmetricCipherAlgorithm = SymmetricCipherAlgorithm.Aes256Gcm;

        /// <summary> Algorithmus für symmetrische Verschlüsselung Two-Key-Triple-DES (CBC-Mode)
        /// </summary>
        public const string SymmetricCipherAlgorithmTripleDesCbc = "http://www.w3.org/2001/04/xmlenc#tripledes-cbc";

        /// <summary> Algorithmus für symmetrische Verschlüsselung AES-128 CBC
        /// </summary>
        public const string SymmetricCipherAlgorithmAes128Cbc = "http://www.w3.org/2001/04/xmlenc#aes128-cbc";

        /// <summary> Algorithmus für symmetrische Verschlüsselung AES-192 CBC
        /// </summary>
        public const string SymmetricCipherAlgorithmAes192Cbc = "http://www.w3.org/2001/04/xmlenc#aes192-cbc";

        /// <summary> Algorithmus für symmetrische Verschlüsselung AES-256 CBC
        /// </summary>
        public const string SymmetricCipherAlgorithmAes256Cbc = "http://www.w3.org/2001/04/xmlenc#aes256-cbc";
        
        /// <summary> Algorithmus für asymmetrische Verschlüsselung RSAES-PKCS1-v1_5
        /// </summary>
        public const string AsymmetricCipherAlgorithmRsa15 = "http://www.w3.org/2001/04/xmlenc#rsa-1_5";

        /// <summary> Algorithmus für asymmetrische Verschlüsselung RSA-OAEP
        /// </summary>
        public const string AsymmetricCipherAlgorithmRsaOaep = "http://www.w3.org/2009/xmlenc11#rsa-oaep";

        /// <summary> Default-Algorithmus für asymmetrische Verschlüsselung
        /// </summary>
        public const AsymmetricCipherAlgorithm DefaultAsymmetricCipherAlgorithm = AsymmetricCipherAlgorithm.RsaOaep;

        /// <summary> Algorithmus für symmetrische Verschlüsselung AES-128 GCM
        /// </summary>
        public const string SymmetricCipherAlgorithmAes128Gcm = "http://www.w3.org/2009/xmlenc11#aes128-gcm";

        /// <summary> Algorithmus für symmetrische Verschlüsselung AES-192 GCM
        /// </summary>
        public const string SymmetricCipherAlgorithmAes192Gcm = "http://www.w3.org/2009/xmlenc11#aes192-gcm";

        /// <summary> Algorithmus für symmetrische Verschlüsselung AES-256 GCM
        /// </summary>
        public const string SymmetricCipherAlgorithmAes256Gcm = "http://www.w3.org/2009/xmlenc11#aes256-gcm";

        #endregion Cipher Algorithms

        /// <summary> Algorithmus für Signaturerstellung, Konstante für Bas64-encoding
        /// </summary>
        public const string Base64Decoder = "http://www.w3.org/2000/09/xmldsig#base64";

        /// <summary> Algorithmus für Maskengenerierung (RSA-OAEP) mit MGF1 und SHA-256
        /// </summary>
        public const string MaskGenerationFunction1Sha256 = "http://www.w3.org/2009/xmlenc11#mgf1sha256";

        /// <summary> Algorithmus für Maskengenerierung (RSA-OAEP) mit MGF1 und SHA-512
        /// </summary>
        public const string MaskGenerationFunction1Sha512 = "http://www.w3.org/2009/xmlenc11#mgf1sha512";

        public const string TypeContent = "http://www.w3.org/2001/04/xmlenc#Content";

        // Optional (wird nicht benutzt)
        /// <summary> Kanonisierung (optional)
        /// </summary>
        public const string Canonicalization = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";

        public const string ExcCanonicalization = "http://www.w3.org/2001/10/xml-exc-c14n#";

        // Transforms
        /// <summary> XSLT-Transformation
        /// </summary>
        public const string TransformXslt = "http://www.w3.org/TR/1999/REC-xslt-19991116";

        /// <summary> Kanonisierung
        /// </summary>
        public const string TransformCanonicalization = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";

        /// <summary> Tranformer für Base64-Decoder
        /// </summary>
        public const string TransformBase64 = "http://www.w3.org/2000/09/xmldsig#base64";

        internal const string DefaultNamespaces = "xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:osci=\"http://www.osci.de/2002/04/osci\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";

        internal const string DefaultNamespaces2017 = "xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:osci=\"http://www.osci.de/2002/04/osci\" xmlns:osci2017=\"http://xoev.de/transport/osci12/7\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";
    }
}