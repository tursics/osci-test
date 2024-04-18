using System.IO;
using Osci.Extensions;
using Osci.Helper;
using Osci.Roles;

namespace Osci.SoapHeader
{
    /// <exclude/>
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class IntermediaryCertificatesH
        : CertificateH
    {

        public Intermed CipherCertificateIntermediary
        {
            get
            {
                return _cipherCertificateIntermediary;
            }
            set
            {
                _log.Debug("CipherCertificate Intermed wurde gesetzt");
                _cipherCertificateIntermediary = value;
            }
        }

        public Intermed SignatureCertificateIntermediary
        {
            get; set;
        }

        private static readonly Log _log = LogFactory.GetLog(typeof(IntermediaryCertificatesH));
        private Intermed _cipherCertificateIntermediary;

        public override void WriteXml(Stream stream)
        {
            stream.Write("<" + OsciNsPrefix + ":IntermediaryCertificates");
            stream.Write(Ns, 0, Ns.Length);
            stream.Write(" Id=\"intermediarycertificates\"  " + SoapNsPrefix + ":actor=\"http://www.w3.org/2001/12/soap-envelope/actor/none\"  " + SoapNsPrefix + ":mustUnderstand=\"1\">");

            if (_cipherCertificateIntermediary != null)
            {
                AddCipherCertificate(_cipherCertificateIntermediary, stream);
            }
            if (SignatureCertificateIntermediary != null)
            {
                AddSignatureCertificate(SignatureCertificateIntermediary, stream);
            }
            stream.Write("</" + OsciNsPrefix + ":IntermediaryCertificates>");
        }
    }
}