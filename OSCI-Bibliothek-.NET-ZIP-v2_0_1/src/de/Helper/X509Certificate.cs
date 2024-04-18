using System.IO;

namespace Osci.Helper
{
    /// <summary>
    /// Zusammenfassung für X509Certificate.
    /// </summary>
    public class X509Certificate 
        : Org.BouncyCastle.X509.X509Certificate
    {
        private readonly IssuerDn _mIssuerDn;

        public X509Certificate(byte[] data)
            : base(Org.BouncyCastle.Asn1.X509.X509CertificateStructure.GetInstance(new Org.BouncyCastle.Asn1.Asn1InputStream(data).ReadObject()))
        {
            _mIssuerDn = new IssuerDn(base.IssuerDN.ToString());
        }

        public X509Certificate(Stream data)
            : base(Org.BouncyCastle.Asn1.X509.X509CertificateStructure.GetInstance(new Org.BouncyCastle.Asn1.Asn1InputStream(data).ReadObject()))
        {
            _mIssuerDn = new IssuerDn(base.IssuerDN.ToString());
            data.Close();
        }

        public byte[] GetRawCertData()
        {
            return GetEncoded();
        }

        // Testen
        public string GetIssuerName()
        {
            return _mIssuerDn.Name();
        }

        // Testen
        public string GetSerialNumber()
        {
            return SerialNumber.ToString();
        }

        // Testen   
        public IssuerDn IssuerDn()
        {
            return _mIssuerDn;
        }

        //Testen

        public string GetName()
        {
            return SubjectDN.ToString();
        }


    } // class

    public class IssuerDn
    {
        private readonly string _name;

        public IssuerDn(string name)
        {
            _name = name;
        }

        public string Name()
        {
            return _name;
        }
    } // class

} // namespace
