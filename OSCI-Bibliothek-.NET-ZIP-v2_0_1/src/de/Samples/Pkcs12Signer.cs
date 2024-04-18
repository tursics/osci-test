using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Helper;

namespace Osci.Samples
{
    /// <summary><p>Diese Klasse ist eine Beispiel-Implementierung der abstrakten Signer-Klasse.
    /// Für die Verwendung wird ein PKCS#12-Keystore in Form einer *.p12-Datei benötigt.
    /// Die Implementierung ist für Testzwecke bestimmt, sie greift auf den ersten
    /// verfügbaren Alias zu. Die PIN für dessen Privatschlüssel muss die gleiche sein
    /// wie die des Keystores.</p>
    /// <p>Diese einfache Implementierung hält die PIN des Keystores als Character-Array
    /// im Arbeitsspeicher, sie wird als String übergeben. Es wird Anwendern empfohlen,
    /// eigene Implementierungen zu schreiben, die die PIN in der Methode sign(...)
    /// abfragen und nach Gebrauch wieder löschen oder anderweitig für ein sicheres
    /// Pin-Cashing zu sorgen.</p> 
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
    /// <seealso cref="Signer">
    /// </seealso>
    public class Pkcs12Signer
        : Signer
    {
        private static Log _log = LogFactory.GetLog(typeof(Pkcs12Signer));
        private readonly bool _usePsSforRsAkey;
        private readonly AsymmetricKeyParameter _pKey;


        /// <summary> Liefert den Namen des Herstellers.
        /// </summary>
        /// <value> Herstellername
        /// </value>
        public override string Vendor
        {
            get
            {
                return "Governikus";
            }
        }

        /// <summary> Liefert die Versionsnummer.
        /// </summary>
        /// <value> Versionsnummer
        /// </value>
        public override string Version
        {
            get
            {
                return "2.0";
            }
        }

        /// <summary> Liefert das Signaturzertifikat.
        /// </summary>
        /// <value> Verschlüsselungszertifikat
        /// </value>
        public override X509Certificate Certificate
        {
            get;
        }

        /// <summary>
        /// Dieser Konstruktor benötigt den Pfad zu einer PKCS#12-Privatschlüsseldatei (*.p12),
        /// die dazugehörige PIN sowie den gewünschten Signaturalgorithmus. Es wird der erste
        /// in der Schlüsseldatei gefundene Schlüssel verwendet.
        /// </summary>
        /// <param name="p12FileName">Pfad zur PKCS#12-Privatschlüsseldatei (*.p12)</param>
        /// <param name="pin">PIN</param>
        public Pkcs12Signer(string p12FileName, string pin)
            : this(p12FileName, pin, false)
        {
        }

        /// <summary>
        /// Dieser Konstruktor benötigt den Pfad zu einer PKCS#12-Privatschlüsseldatei (*.p12),
        /// die dazugehörige PIN sowie den gewünschten Signaturalgorithmus. Es wird der erste
        /// in der Schlüsseldatei gefundene Schlüssel verwendet. Falls der angegebene Algorithmus
        /// nicht zu dem gefundenen Privatschlüssel passt (ECDSA/RSA), wird der Algorithmus
        /// unter Verwendung des gewählten Hash-Algorithmus entsprechend angepasst.
        /// </summary>
        /// <param name="p12FileName">Pfad zur PKCS#12-Privatschlüsseldatei (*.p12)</param>
        /// <param name="pin">PIN</param>
        /// <param name="signatureAlgorithm">Signaturalgorithmus</param>
        public Pkcs12Signer(string p12FileName, string pin, bool usePsSforRsAkey)
            : this(new FileStream(p12FileName, FileMode.Open), pin, usePsSforRsAkey)
        {
        }

        /// <summary>
        /// Dieser Konstruktor benötigt die Binärdaten zu einer PKCS#12-Privatschlüsseldatei (*.p12),
        /// die dazugehörige PIN sowie den gewünschten Signaturalgorithmus. Es wird der erste
        /// in der Schlüsseldatei gefundene Schlüssel verwendet. Falls der angegebene Algorithmus
        /// nicht zu dem gefundenen Privatschlüssel passt (ECDSA/RSA), wird der Algorithmus
        /// unter Verwendung des gewählten Hash-Algorithmus entsprechend angepasst.
        /// </summary>
        /// <param name="rawData">Pfad zur PKCS#12-Privatschlüsseldatei (*.p12)</param>
        /// <param name="pin">PIN</param>
        /// <param name="usePsSforRsAkey">Signaturalgorithmus</param>
        public Pkcs12Signer(byte[] rawData, string pin, bool usePsSforRsAkey = false)
            : this(new MemoryStream(rawData), pin, usePsSforRsAkey)
        {
        }

        private Pkcs12Signer(Stream stream, string pin, bool usePsSforRsAkey)
        {
            Pkcs12Store ks = new Pkcs12Store(stream, pin.ToCharArray());
            stream.Close();

            IEnumerator al = ks.Aliases.GetEnumerator();
            while (al.MoveNext())
            {
                if (ks.IsKeyEntry((string)al.Current))
                {
                    _pKey = ks.GetKey((string)al.Current).Key;
                    Certificate = new X509Certificate(ks.GetCertificate((string)al.Current).Certificate.GetEncoded());
                    break;
                }
            }
            if (_pKey == null)
            {
                throw new ArgumentException("No private key found in keystore !");
            }

            _usePsSforRsAkey = usePsSforRsAkey;
        }


        /// <summary> Signiert das übergebene Byte-Array.
        /// </summary>
        /// <param name="parm1"> die zu signierenden Daten
        /// </param>
        /// <param name="algorithm"> Algorithmus
        /// </param>
        /// <returns> die Signatur
        /// </returns>
        public override byte[] Sign(byte[] data, string algorithm)
        {
            string digAlg = null;
            if (algorithm.Contains("sha256"))
            {
                digAlg = "SHA-256";
            }
            else if (algorithm.Contains("sha512"))
            {
                digAlg = "SHA-512";
            }
            else if (algorithm.Contains("sha1"))
            {
                digAlg = "SHA-1";
            }
            else if (algorithm.Contains("ripemd160"))
            {
                digAlg = "RIPEMD160";
            }

            if (algorithm.Contains("ecdsa"))
            {
                ECDsaSigner signer = new ECDsaSigner();
                signer.Init(true, _pKey);
                byte[] digest = DigestUtilities.CalculateDigest(digAlg, data);
                BigInteger[] sigBigInts = signer.GenerateSignature(digest);
                int len = (((ECPrivateKeyParameters)_pKey).Parameters.N.BitLength + 7) / 8;
                byte[] s = new byte[len * 2];
                byte[] s0 = sigBigInts[0].ToByteArray();
                Array.Copy(s0, s0.Length - len, s, 0, len);
                byte[] s1 = sigBigInts[1].ToByteArray();
                Array.Copy(s1, s1.Length - len, s, len, len);
                return s;
            }
            else
            {
                IDigest id = DigestUtilities.GetDigest(digAlg);
                ISigner rsads;
                if (algorithm.Contains("MGF"))
                {
                    rsads = new PssSigner(new RsaEngine(), id);
                }
                else
                {
                    rsads = new RsaDigestSigner(id);
                }
                rsads.Init(true, _pKey);
                rsads.BlockUpdate(data, 0, data.Length);
                return rsads.GenerateSignature();
            }
        }

        public override string GetAlgorithm()
        {
            string algo = DialogHandler.SignatureAlgorithm;

            if (_pKey is ECPrivateKeyParameters)
            {
                if (algo.Contains("sha256"))
                {
                    algo = Constants.SignatureAlgorithmEcdsaSha256;
                }
                else
                {
                    algo = Constants.SignatureAlgorithmEcdsaSha512;
                }
            }
            else if (_pKey is RsaPrivateCrtKeyParameters)
            {
                if (_usePsSforRsAkey)
                {
                    if (algo.Contains("sha256"))
                    {
                        algo = Constants.SignatureAlgorithmRsaSha256Pss;
                    }
                    else if (algo.Contains("sha512"))
                    {
                        algo = Constants.SignatureAlgorithmRsaSha512Pss;
                    }
                }
                else
                {
                    if (algo.Contains("sha256"))
                    {
                        algo = Constants.SignatureAlgorithmRsaSha256;
                    }
                    else if (algo.Contains("sha512"))
                    {
                        algo = Constants.SignatureAlgorithmRsaSha512;
                    }
                }
            }
            else
            {
                throw new ArgumentException("Unsupported algorithm found, key is of type " + _pKey.GetType());
            }

            return algo;
        }
    }
}