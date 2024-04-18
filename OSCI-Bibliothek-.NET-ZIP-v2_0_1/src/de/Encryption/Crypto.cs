using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;

namespace Osci.Encryption
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
    public class Crypto
    {
        private static readonly Log _log =  LogFactory.GetLog(typeof(Crypto));


        #region Decryption

        public static byte[] Decrypt(AsymmetricKeyParameter asymmetricKeyParameter, byte[] data, string mgfAlgorithm, string digestAlgorithm)
        {
            if (mgfAlgorithm == null)
            {
                return Decrypt(asymmetricKeyParameter, data, AsymmetricCipherAlgorithm.Rsa15);
            }

            IDigest digest = null;
            if (digestAlgorithm.Equals(Constants.DigestAlgorithmSha256))
            {
                digest = new Sha256Digest();
            }
            else if (digestAlgorithm.Equals(Constants.DigestAlgorithmSha512))
            {
                digest = new Sha512Digest();
            }

            IDigest mgfDigest = null;
            if (mgfAlgorithm.Equals(Constants.MaskGenerationFunction1Sha256))
            {
                mgfDigest = new Sha256Digest();
            }
            else if (mgfAlgorithm.Equals(Constants.MaskGenerationFunction1Sha512))
            {
                mgfDigest = new Sha512Digest();
            }

            return Decrypt(asymmetricKeyParameter, data, AsymmetricCipherAlgorithm.RsaOaep, digest, mgfDigest);
        }

        public static byte[] Decrypt(AsymmetricKeyParameter asymmetricKeyParameter, byte[] data, AsymmetricCipherAlgorithm asymmetricCipherAlgorithm)
        {
            IDigest digest = null;
            if (AsymmetricCipherAlgorithm.RsaOaep == asymmetricCipherAlgorithm)
            {
                if (DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha512))
                {
                    digest = new Sha512Digest();
                }
                else
                {
                    digest = new Sha256Digest();
                }
            }
            return Decrypt(asymmetricKeyParameter, data, asymmetricCipherAlgorithm, digest, digest);
        }

        private static byte[] Decrypt(AsymmetricKeyParameter asymmetricKeyParameter, byte[] data, AsymmetricCipherAlgorithm asymmetricCipherAlgorithm, IDigest digest, IDigest mgfDigest)
        {
            if (asymmetricKeyParameter == null)
            {
                throw new ArgumentNullException("asymmetricKeyParameter");
            }
            if (data == null || data.Length <= 0)
            {
                throw new ArgumentNullException("data");
            }

            switch (asymmetricCipherAlgorithm)
            {
                case AsymmetricCipherAlgorithm.Rsa15:
                    Pkcs1Encoding pkcs12 = new Pkcs1Encoding(new RsaEngine());
                    pkcs12.Init(false, asymmetricKeyParameter);
                    return pkcs12.ProcessBlock(data, 0, data.Length);

                case AsymmetricCipherAlgorithm.RsaOaep:
                    OaepEncoding oaep = new OaepEncoding(new RsaEngine(), digest, mgfDigest, new byte[0]);
                    oaep.Init(false, asymmetricKeyParameter);
                    return oaep.ProcessBlock(data, 0, data.Length);

                default:
                    throw new NotSupportedException("Algorithm is not supported: " + asymmetricCipherAlgorithm);

            }
        }

        #endregion


        #region Encryption

        public static byte[] Encrypt(X509Certificate encryptionCertificate, byte[] data, AsymmetricCipherAlgorithm asymmetricCipherAlgorithm)
        {
            if (encryptionCertificate == null)
            {
                throw new ArgumentNullException("encryptionCertificate");
            }
            if (data == null || data.Length <= 0)
            {
                throw new ArgumentNullException("data");
            }

            switch (asymmetricCipherAlgorithm)
            {
                case AsymmetricCipherAlgorithm.Rsa15:
                    Pkcs1Encoding pkcs12 = new Pkcs1Encoding(new RsaEngine());
                    _log.Trace(pkcs12.AlgorithmName);
                    pkcs12.Init(true, encryptionCertificate.GetPublicKey());
                    return pkcs12.ProcessBlock(data, 0, data.Length);

                case AsymmetricCipherAlgorithm.RsaOaep:
                    IDigest digest = DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha512) 
                        ? (IDigest) new Sha512Digest() 
                        : new Sha256Digest(); 

                    OaepEncoding oaep = new OaepEncoding(new RsaEngine(), digest, digest, null);
                    _log.Trace(oaep.AlgorithmName);
                    oaep.Init(true, encryptionCertificate.GetPublicKey());
                    return oaep.ProcessBlock(data, 0, data.Length);

                default:
                    throw new NotSupportedException("Algorithm is not supported: " + asymmetricCipherAlgorithm);
            }
        }

        #endregion



        /// <summary>
        /// Diese Methode prüft, ob mit der verwendeten Schlüssellänge und dem im DialogHandler eingestellten
        /// Hashalgorithmus eine Verschlüsselung möglich ist und liefert ggf. den Identifier für SHA-256 statt
        /// für SHA-512. Da das Problem z.Zt. (Version 1.6) nur mit den nicht mehr zulässigen 1024-Bit-Schlüsseln
        /// in Verbindung mit SHA-512 auftritt, wird diese Methode von der Bibliothek selbst nicht verwendet.
        /// </summary>
        /// <param name="encryptionCert">Verschlüsselungszertifikat</param>
        /// <param name="symmetricCipherAlgo">Symmetrischer Verschlüsselungsalgorithmus</param>
        /// <returns></returns>      
        [Obsolete]
        public static string GetDigestMethodForOaep(X509Certificate encryptionCert, string symmetricCipherAlgo)
        {
            int dataLen = 24;
            if (symmetricCipherAlgo.Contains("aes"))
            {
                dataLen = int.Parse(symmetricCipherAlgo.Substring(symmetricCipherAlgo.IndexOf("#aes") + 4).Substring(0, symmetricCipherAlgo.Length - symmetricCipherAlgo.IndexOf("-cbc") - 1));
            }

            int keyLength = ((RsaKeyParameters)encryptionCert.GetPublicKey()).Modulus.BitLength;

            string digAlgo = DialogHandler.DigestAlgorithm;

            if (keyLength < dataLen + 2 + 2 * (int.Parse(digAlgo.Substring(digAlgo.IndexOf("#sha") + 4))))
            {
                digAlgo = Constants.DigestAlgorithmSha256;
            }

            return digAlgo;
        }

        public static bool IsWeak(X509Certificate certificate, DateTimeOffset date)
        {
            return IsWeak(certificate.GetPublicKey(), certificate.GetKeySize(), date);
        }

        public static bool IsWeak(AsymmetricKeyParameter asymmetricKeyParameter, int keySize, DateTimeOffset date)
        {
            if (asymmetricKeyParameter is ECPublicKeyParameters)
            {
                if (keySize < 224)
                {
                    _log.Info("Signature key (EC) has insufficient key size: " + keySize);
                    return true;
                }
            }
            else
            {
                if (keySize < 1024)
                {
                    _log.Info("Signature key (RSA) has insufficient key size: " + keySize);
                    return true;
                }

                if (date == default(DateTimeOffset))
                {
                    date = Constants.DefaultDateForAlgorithmCheck;
                }

                if (keySize < 2048 && date.CompareTo(Constants.OutDateKeysize1024) >= 0)
                {
                    _log.Info("Signature key (RSA) has insufficient key size: " + keySize);
                    return true;
                }
            }
            return false;
        }

        private static readonly Dictionary<string, DateTime> _outDates = new Dictionary<string, DateTime>
        {
            {Constants.DigestAlgorithmSha1, new DateTime(2008, 7, 1)},
            {Constants.SignatureAlgorithmRsaSha1, new DateTime(2008, 7, 1)},
            {Constants.DigestAlgorithmRipemd160, new DateTime(2011, 1, 1)},
            {Constants.SignatureAlgorithmRsaRipemd160, new DateTime(2011, 1, 1)},
            {Constants.SignatureAlgorithmRsaRipemd160Rfc4051, new DateTime(2011, 1, 1)}
        };

        public static bool IsWeak(OsciSignature osciSignature, DateTimeOffset date)
        {
            if (_outDates.ContainsKey(osciSignature.SignatureAlgorithm) && date.CompareTo(_outDates[osciSignature.SignatureAlgorithm]) >= 0)
            {
                return true;
            }
            IEnumerator digMeths = osciSignature.GetDigestMethods().Values.GetEnumerator();
            while (digMeths.MoveNext())
            {
                string digest = digMeths.Current as string;

                if (digest != null && _outDates.ContainsKey(digest) && date.CompareTo(_outDates[digest]) >= 0)
                {
                    return true;
                }
            }
            return false;
        }


        #region Signature check

        private static readonly Dictionary<string, string> _digestForSignatureMap = new Dictionary<string, string>
        {
            { Constants.SignatureAlgorithmRsaSha1, Constants.DigestAlgorithmSha1 },

            { Constants.SignatureAlgorithmRsaSha256, Constants.DigestAlgorithmSha256 },
            { Constants.SignatureAlgorithmRsaSha256Pss, Constants.DigestAlgorithmSha256 },
            { Constants.SignatureAlgorithmEcdsaSha256, Constants.DigestAlgorithmSha256 },

            { Constants.SignatureAlgorithmRsaSha512, Constants.DigestAlgorithmSha512 },
            { Constants.SignatureAlgorithmRsaSha512Pss, Constants.DigestAlgorithmSha512 },
            { Constants.SignatureAlgorithmEcdsaSha512, Constants.DigestAlgorithmSha512 },

            { Constants.SignatureAlgorithmRsaRipemd160, Constants.DigestAlgorithmRipemd160 },
            { Constants.SignatureAlgorithmRsaRipemd160Rfc4051, Constants.DigestAlgorithmRipemd160 },

            { Constants.SignatureAlgorithmRsaSha3With256Bit, Constants.DigestAlgorithmSha3With256Bit },
            { Constants.SignatureAlgorithmRsaSha3With384Bit, Constants.DigestAlgorithmSha3With384Bit },
            { Constants.SignatureAlgorithmRsaSha3With512Bit, Constants.DigestAlgorithmSha3With512Bit }
        };


        public static bool CheckSignature(X509Certificate certificate, byte[] data, byte[] signature, string algorithm)
        {
            AsymmetricKeyParameter asymmetricKeyParameter = certificate.GetPublicKey();
            Func<string> getDigestAlgorithm = () =>
            {
                if (_digestForSignatureMap.ContainsKey(algorithm))
                {
                    return NamespaceMap.GetBouncyCastleName(_digestForSignatureMap[algorithm]);
                }
                throw new NotSupportedException("Signature algorithm is not supported: " + algorithm);
            };

            if (asymmetricKeyParameter is ECPublicKeyParameters)
            {
                // ECDSA
                byte[] digest = DigestUtilities.CalculateDigest(getDigestAlgorithm(), data);
                BigInteger r = new BigInteger(1, signature, 0, signature.Length / 2);
                BigInteger s = new BigInteger(1, signature, signature.Length / 2, signature.Length / 2);

                ECDsaSigner signer = new ECDsaSigner();
                signer.Init(false, asymmetricKeyParameter);
                return signer.VerifySignature(digest, r, s);
            }
            else
            {
                // RSA
                IDigest digest = DigestUtilities.GetDigest(getDigestAlgorithm());

                ISigner signer;
                if (algorithm.Contains("MGF"))
                {
                    signer = new PssSigner(new RsaEngine(), digest);
                }
                else
                {
                    signer = new RsaDigestSigner(digest);
                }
                signer.Init(false, asymmetricKeyParameter);
                signer.BlockUpdate(data, 0, data.Length);
                return signer.VerifySignature(signature);
            }
        }

        #endregion


        #region MessageDigest

        private static readonly Dictionary<string, Func<HashAlgorithm>> _digestMap = new Dictionary<string, Func<HashAlgorithm>>
        {
            { Constants.DigestAlgorithmSha1, () => new SHA1Managed() },
            { Constants.DigestAlgorithmSha256, () => new SHA256Managed() },
            { Constants.DigestAlgorithmSha512, () => new SHA512Managed() },
            { Constants.DigestAlgorithmRipemd160, () => new RIPEMD160Managed() },
            { Constants.DigestAlgorithmSha3With256Bit, () => new Sha3Managed(256) },
            { Constants.DigestAlgorithmSha3With384Bit, () => new Sha3Managed(384) },
            { Constants.DigestAlgorithmSha3With512Bit, () => new Sha3Managed(512) }
        };

        public static HashAlgorithm CreateMessageDigest(string algorithm)
        {
            if (_digestMap.ContainsKey(algorithm))
            {
                return _digestMap[algorithm]();
            }
            throw new NotSupportedException("Algorithm is not supported: " + algorithm);
        }

        #endregion
    }
}