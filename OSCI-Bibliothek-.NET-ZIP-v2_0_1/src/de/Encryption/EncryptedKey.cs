using System.IO;
using Osci.Common;
using Osci.Extensions;
using Osci.Helper;

namespace Osci.Encryption
{

    /// <summary> Ein EncryptedKey für jeden Reader
    /// <EncryptedKey xmlns="http://www.w3.org/2001/04/xmlenc#" Id="job-encrypted-key">
    /// <EncryptionMethod Algorithm="http://www.w3.org/2001/04/xmlenc#rsa-1_5" />
    /// <KeyInfo xmlns="http://www.w3.org/2000/09/xmldsig#" Id="job-key-info">
    /// <X509Data>
    /// <X509Certificate>
    /// MIICiDCCAfGgAwIBAgIGAO6OKE3CMA0GCSqGSIb3DQEBBQUAMG4xCzAJBgNVBAYT
    /// AklFMQ8wDQYDVQQIEwZEdWJsaW4xJDAiBgNVBAoTG0JhbHRpbW9yZSBUZWNobm9s
    /// b2dpZXMgTHRkLjERMA8GA1UECxMIWC9TZWN1cmUxFTATBgNVBAMTDFRyYW5zaWVu
    /// dCBDQTAeFw0wMjA2MjAxNTMzNDJaFw0xMjA2MjAxNTMzMzhaMGUxCzAJBgNVBAYT
    /// AklFMQ8wDQYDVQQIEwZEdWJsaW4xJDAiBgNVBAoTG0JhbHRpbW9yZSBUZWNobm9s
    /// b2dpZXMgTHRkLjERMA8GA1UECxMIWC9TZWN1cmUxDDAKBgNVBAMTA0pvYjCBnzAN
    /// BgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEA3heEqT6jgWd2Q2fFPwjck1fMwoXp2YzA
    /// jJ8PcBDEW4vdCZJYwBcuSPbuMgscqE0pNxlxRc9ZGYXrX2UPHTrxqOdRg43x1D93
    /// 7jLqb5206e+iPGaxdjKFrL1K/UQoltV6w/ibawQRIjNOnDraw4WpELFxdL4jQdjo
    /// Z6u38t5l3fECAwEAAaM6MDgwDgYDVR0PAQH/BAQDAgUgMBEGA1UdDgQKBAiFdmau
    /// 1AD5BjATBgNVHSMEDDAKgAiA4IWwNLDjUTANBgkqhkiG9w0BAQUFAAOBgQBqP1ZE
    /// btWnLPC/8kNvWFq/VHqMsALkks44QM2QOnzDF86EOVXgwkxSeOUii1UIKTwmeMev
    /// 9Kx0fNEpuvQUNP89V0fLNTqVN2BGJL12XgaKpAwoAu3SpUfTEN2WXi3taFKbrIVW
    /// 47YWz1BoHJewZdcO4dfftCHcDGcXg6W3NxW88A==
    /// </X509Certificate>
    /// </X509Data>
    /// </KeyInfo>
    /// <CipherData>
    /// <CipherValue>
    /// DmmgQWTqFhQoUkiPVh3yHpy6CMv/leHFU8qZOMuAztLrDfSTqkKFECtgdeshs4lo
    /// d1lyQ3QuZLXL6nalZHpayP6IZQJiRip54i8JPjZzhkK3sOnrmqwewSqz2CHBe7gv
    /// QWVNcNrV1q/KrjlR3S5JT/9FPQJ6tOyywt9QrQIgbZw=
    /// </CipherValue>
    /// </CipherData>
    /// </EncryptedKey>
    /// </summary>

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
    public class EncryptedKey
        : EncryptedType
    {
        public string Recipient
        {
            get; set;
        }

        public string CarriedKeyName
        {
            get; set;
        }

        public string ReferenceList
        {
            get; set;
        }

        public string MgfAlgorithm
        {
            get; set;
        }

        public string DigestAlgorithm
        {
            get; set;
        }

        public AsymmetricCipherAlgorithm AsymmetricCipherAlgorithm
        {
            get; set;
        }

        private static string _xenc11 = "http://www.w3.org/2009/xmlenc11#";
        private readonly string _xenc11NsPrefix = "xenc11";
        private SupportClass.KeySupport _key = null;

        /// <summary> Für den Parser
        /// </summary>
        /// <param name="encryptionMethodAlgorithm">
        /// </param>
        internal EncryptedKey()
        {
        }

        /// <summary> Erstellt ein neues EncryptedKey-Object.
        /// </summary>
        /// <param name="encryptionMethodAlgorithm">
        /// </param>
        /// <param name="cipherValue">
        /// </param>
        public EncryptedKey(AsymmetricCipherAlgorithm encryptionMethodAlgorithm, CipherValue cipherValue)
        {
            AsymmetricCipherAlgorithm = encryptionMethodAlgorithm;
            EncryptionMethodAlgorithm = encryptionMethodAlgorithm.GetXmlName();

            if (encryptionMethodAlgorithm.Equals(AsymmetricCipherAlgorithm.RsaOaep))
            {
                // MGF und Hashfunktionen nach DialogHandler
                if (DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha512)
                    || DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha3With512Bit)
                    || DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha3With384Bit))
                {
                    MgfAlgorithm = Constants.MaskGenerationFunction1Sha512;
                    DigestAlgorithm = Constants.DigestAlgorithmSha512;
                }
                else if (DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha256)
                        || DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha3With256Bit))
                {
                    MgfAlgorithm = Constants.MaskGenerationFunction1Sha256;
                    DigestAlgorithm = Constants.DigestAlgorithmSha256;
                }
                else // default
                {
                    MgfAlgorithm = Constants.MaskGenerationFunction1Sha256;
                    DigestAlgorithm = Constants.DigestAlgorithmSha256;
                }
            }
            CipherData cipherData = new CipherData(cipherValue);
            CipherData = cipherData;
        }

        public void WriteXml(Stream stream, string ds, string xenc)
        {
            stream.Write("<" + xenc + ":EncryptedKey>");
            stream.Write("<" + xenc + ":EncryptionMethod Algorithm=\"" + EncryptionMethodAlgorithm + "\">");

            if (AsymmetricCipherAlgorithm == AsymmetricCipherAlgorithm.RsaOaep)
            {
                stream.Write("<" + _xenc11NsPrefix + ":MGF xmlns:" + _xenc11NsPrefix + "=\"" + _xenc11 + "\" Algorithm=\"" + MgfAlgorithm + "\"></" + _xenc11NsPrefix + ":MGF>");
                stream.Write("<" + ds + ":DigestMethod Algorithm=\"" + DigestAlgorithm + "\"></" + ds + ":DigestMethod>");
            }

            stream.Write("</" + xenc + ":EncryptionMethod>");

            KeyInfo.WriteXml(stream, ds, xenc);
            CipherData.WriteXml(stream, ds, xenc);
            stream.Write("</" + xenc + ":EncryptedKey>");
        }
    }
}