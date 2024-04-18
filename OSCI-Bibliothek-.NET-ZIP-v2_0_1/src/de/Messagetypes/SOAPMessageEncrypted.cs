using System.IO;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Encryption;
using Osci.Extensions;
using Osci.Helper;

namespace Osci.Messagetypes
{
    // Storestreams

    /// <summary>Diese Klasse entspricht einer Verschlüsselten OSCI-Nachricht.
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
    public class SoapMessageEncrypted
        : OsciMessage
    {
        public new EncryptedData EncryptedData
        {
            get;
            set;
        }

        private readonly OsciMessage _msg;
        private readonly SecretKey _symKey;
        private int ivLength = Constants.DefaultGcmIVLength;
        private byte[] _encSymKey;
        private IOException _ioex;
        private readonly Stream _storeStream;

        private static byte[] _xml0;
        private static byte[] _xml_1A1;
        private static byte[] _xml_1A2;
        private static byte[] _xml_1B;
        private static byte[] _xml2;
        private static byte[] _xml3;
        private byte[] _algo;
        private byte[] _asymAlgo;
        private byte[] ivLengthElement;
        private int _length;
        private X509Certificate _cipherCert;


        public SoapMessageEncrypted(OsciMessage msg, Stream storeStream)
        {
            SetFragm();
            _storeStream = storeStream;

            MessageType = SoapMessageEncrypted;
            _msg = msg;
            if (msg != null)
            {
                _symKey = new SecretKey(msg.DialogHandler.SymmetricCipherAlgorithm);
                ivLength = msg.DialogHandler.IvLength;
            }
        }

        public override void Compose()
        {
            if ((_msg.StateOfMessage & StateComposed) == 0)
            {
                _msg.Compose();
            }

            if (_msg is OsciRequest)
            {
                _cipherCert = _msg.DialogHandler.Supplier.CipherCertificate;
            }
            else
            {
                _cipherCert = _msg.DialogHandler.Client.CipherCertificate;
            }

            _encSymKey = Base64.Encode(Crypto.Encrypt(_cipherCert, _symKey.Key, _msg.DialogHandler.AsymmetricCipherAlgorithm)).ToByteArray();
            _algo = _msg.DialogHandler.SymmetricCipherAlgorithm.GetXmlName().ToByteArray();
            _asymAlgo = ConstructEncryptionAlgo();
            _length = _xml0.Length + _algo.Length + _xml_1A1.Length + _xml_1A2.Length + _asymAlgo.Length + _xml_1B.Length + _xml2.Length + _encSymKey.Length + _xml3.Length;

            // nur einsetzen, wenn ungleich altem Default-Wert (16), um Abwärtskompatibilität zu wahren
            if (ivLength != 16)
            {
                ivLengthElement = ("<osci128:IvLength xmlns:osci128=\""+ Namespace.Osci128 + "\" Value=\"" + ivLength + "\"></osci128:IvLength>").ToByteArray();
                _length += ivLengthElement.Length;
            }

            StateOfMessage |= StateComposed;
        }

        private byte[] ConstructEncryptionAlgo()
        {
            string ret;

            if (_msg.DialogHandler.AsymmetricCipherAlgorithm == AsymmetricCipherAlgorithm.RsaOaep)
            {
                if (DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha512)
                    || DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha3With512Bit)
                    || DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha3With384Bit))
                {
                    ret = Constants.AsymmetricCipherAlgorithmRsaOaep +
                          "\"><xenc11:MGF xmlns:xenc11=\"http://www.w3.org/2009/xmlenc11#\" Algorithm=\"" +
                          Constants.MaskGenerationFunction1Sha512 + "\"></xenc11:MGF>";
                    ret += "<ds:DigestMethod Algorithm=\"" + Constants.DigestAlgorithmSha512 + "\"></ds:DigestMethod>";

                }
                else if (DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha256)
                    || DialogHandler.DigestAlgorithm.Equals(Constants.DigestAlgorithmSha3With256Bit))
                {
                    ret = Constants.AsymmetricCipherAlgorithmRsaOaep +
                          "\"><xenc11:MGF xmlns:xenc11=\"http://www.w3.org/2009/xmlenc11#\" Algorithm=\"" +
                          Constants.MaskGenerationFunction1Sha256 + "\"></xenc11:MGF>";
                    ret += "<ds:DigestMethod Algorithm=\"" + Constants.DigestAlgorithmSha256 + "\"></ds:DigestMethod>";
                }
                else // default
                {
                    ret = Constants.AsymmetricCipherAlgorithmRsaOaep +
                          "\"><xenc11:MGF xmlns:xenc11=\"http://www.w3.org/2009/xmlenc11#\" Algorithm=\"" +
                          Constants.MaskGenerationFunction1Sha256 + "\"></xenc11:MGF>";
                    ret += "<ds:DigestMethod Algorithm=\"" + Constants.DigestAlgorithmSha256 + "\"></ds:DigestMethod>";
                }
            }
            else
            {
                ret = Constants.AsymmetricCipherAlgorithmRsa15 + "\">";
            }

            return ret.ToByteArray();
        }

        public override void WriteXml(Stream stream)
        {
            if ((StateOfMessage & StateComposed) == 0)
            {
                Compose();
            }

            stream.Write("MIME-Version: 1.0\r\nContent-Type: Multipart/Related; boundary=" + _msg.BoundaryString + "; type=text/xml\r\n");
            stream.Write("\r\n--" + _msg.BoundaryString + "\r\nContent-Type: text/xml; charset=UTF-8\r\n");
            stream.Write("Content-Transfer-Encoding: 8bit\r\nContent-ID: <" + ContentId + ">\r\n");
            stream.Write("Content-Length: " + _length + "\r\n\r\n");

            stream.Write(_xml0, 0, _xml0.Length);
            stream.Write(_algo, 0, _algo.Length);
            stream.Write(_xml_1A1, 0, _xml_1A1.Length);
            if (ivLengthElement != null)
            {
                stream.Write(ivLengthElement, 0, ivLengthElement.Length);
            }
            stream.Write(_xml_1A2, 0, _xml_1A2.Length);

            byte[] tmp = ConstructEncryptionAlgo();
            stream.Write(tmp, 0, tmp.Length);
            stream.Write(_xml_1B, 0, _xml_1B.Length);

            if (_msg is OsciRequest)
            {
                stream.Write(Base64.Encode(_msg.DialogHandler.Supplier.CipherCertificate.GetEncoded()));
            }
            else
            {
                stream.Write(Base64.Encode(_msg.DialogHandler.Client.CipherCertificate.GetEncoded()));
            }

            var tempByteArray6 = _xml2;
            stream.Write(tempByteArray6, 0, tempByteArray6.Length);
            var tempByteArray7 = _encSymKey;
            stream.Write(tempByteArray7, 0, tempByteArray7.Length);
            var tempByteArray8 = _xml3;
            stream.Write(tempByteArray8, 0, tempByteArray8.Length);

            stream.Write("\r\n\r\n--" + _msg.BoundaryString + "\r\nContent-Type: ");
            stream.Write(_msg.Base64Encoding ? "text/base64" : "application/octet-stream");
            stream.Write("\r\nContent-Transfer-Encoding: ");
            stream.Write(_msg.Base64Encoding ? "7bit" : "binary");
            stream.Write("\r\nContent-ID: <osci_enc>\r\n\r\n");

            Base64OutputStream b64Out = null;
            SymCipherOutputStream tdesOut;
            if (_msg.Base64Encoding)
            {
                b64Out = new Base64OutputStream(stream, false);
                tdesOut = new SymCipherOutputStream(b64Out, _symKey, ivLength, true);
            }
            else
            {
                tdesOut = new SymCipherOutputStream(stream, _symKey, ivLength, true);
            }

            if (_storeStream == null)
            {
                _msg.WriteXml(tdesOut);
                tdesOut.Close();
            }
            else
            {
                StoreOutputStream sos = new StoreOutputStream(tdesOut, _storeStream);
                _msg.WriteXml(sos);
                sos.Close();
            }

            if (_msg.Base64Encoding)
            {
                b64Out.Flush();
            }

            stream.Write("\r\n--" + _msg.BoundaryString + "--\r\n");
        }



        protected void SetFragm()
        {
            _xml0 = ("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n\r\n<soap:Envelope xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://schemas.xmlsoap.org/soap/envelope/ soapMessageEncrypted.xsd http://www.w3.org/2000/09/xmldsig# oscisig.xsd http://www.w3.org/2001/04/xmlenc# oscienc.xsd\"><soap:Body><xenc:EncryptedData Id=\"Attachment\" MimeType=\"text/xml\"><xenc:EncryptionMethod Algorithm=\"").ToByteArray();
            _xml_1A1 = ("\">").ToByteArray();
            _xml_1A2 = ("</xenc:EncryptionMethod><ds:KeyInfo><xenc:EncryptedKey><xenc:EncryptionMethod Algorithm=\"").ToByteArray();
            _xml_1B = ("</xenc:EncryptionMethod><" + DsNsPrefix + ":KeyInfo><" + DsNsPrefix + ":X509Data><" + DsNsPrefix + ":X509Certificate>").ToByteArray();
            _xml2 = ("</ds:X509Certificate></ds:X509Data></ds:KeyInfo><xenc:CipherData><xenc:CipherValue>").ToByteArray();
            _xml3 = ("</xenc:CipherValue></xenc:CipherData></xenc:EncryptedKey></" + DsNsPrefix + ":KeyInfo><xenc:CipherData><xenc:CipherReference URI=\"cid:osci_enc\"><xenc:Transforms><ds:Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#base64\"></" + DsNsPrefix + ":Transform></xenc:Transforms></xenc:CipherReference></xenc:CipherData></xenc:EncryptedData></soap:Body></soap:Envelope>").ToByteArray();
        }
    }
}
