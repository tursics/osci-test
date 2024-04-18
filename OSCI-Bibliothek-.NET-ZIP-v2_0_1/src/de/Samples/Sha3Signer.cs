using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Helper;

namespace Osci.Samples
{
    public class Sha3Signer
        : Signer
    {
        public override string Version
        {
            get
            {
                return "1.0";
            }
        }

        public override string Vendor
        {
            get
            {
                return "Governikus";
            }
        }

        public override X509Certificate Certificate
        {
            get
            {
                return _certificate;
            }
        }


        private readonly X509Certificate _certificate;
        private readonly AsymmetricKeyParameter _asymmetricKeyParameter;
        private readonly Dictionary<string, string> _supportedDigestAlgorithms = new Dictionary<string, string>
        {
            { Constants.SignatureAlgorithmRsaSha3With256Bit, NamespaceMap.GetBouncyCastleName(Constants.DigestAlgorithmSha3With256Bit) },
            { Constants.SignatureAlgorithmRsaSha3With384Bit, NamespaceMap.GetBouncyCastleName(Constants.DigestAlgorithmSha3With384Bit) },
            { Constants.SignatureAlgorithmRsaSha3With512Bit, NamespaceMap.GetBouncyCastleName(Constants.DigestAlgorithmSha3With512Bit) },
        };



        public Sha3Signer(byte[] certificate, string password)
            : this(new MemoryStream(certificate), password, true)
        {
        }

        public Sha3Signer(Stream certificate, string password)
            : this(certificate, password, false)
        {
        }

        private Sha3Signer(Stream certificate, string password, bool disposeStream)
        {
            GetCertificateData(certificate, password, disposeStream, out _certificate, out _asymmetricKeyParameter);
        }


        private static void GetCertificateData(Stream stream, string password, bool disposeStream, out X509Certificate certificate, out AsymmetricKeyParameter asymmetricKeyParameter)
        {
            Pkcs12Store pkcs12Store = new Pkcs12Store(stream, password.ToCharArray());
            if (disposeStream)
            {
                stream.Dispose();
            }

            IEnumerator enumerator = pkcs12Store.Aliases.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (pkcs12Store.IsKeyEntry((string)enumerator.Current))
                {
                    asymmetricKeyParameter = pkcs12Store.GetKey((string)enumerator.Current).Key;
                    certificate = new X509Certificate(pkcs12Store.GetCertificate((string)enumerator.Current).Certificate.GetEncoded());
                    return;
                }
            }

            throw new ArgumentException("No private key found in keystore !");
        }


        public override byte[] Sign(byte[] data, string algorithm)
        {
            if (!_supportedDigestAlgorithms.ContainsKey(algorithm))
            {
                throw new NotSupportedException("Algorithm is not supported: " + algorithm);
            }

            IDigest digest = DigestUtilities.GetDigest(_supportedDigestAlgorithms[algorithm]);
            PssSigner signer = new PssSigner(new RsaEngine(), digest);
            Console.WriteLine("AlgorithmName: " + signer.AlgorithmName);

            signer.Init(true, _asymmetricKeyParameter);
            signer.BlockUpdate(data, 0, data.Length);
            return signer.GenerateSignature();
        }

        public override string GetAlgorithm()
        {
            return _supportedDigestAlgorithms.Keys.First(); // default
        }
    }
}
