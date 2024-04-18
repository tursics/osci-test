using System.Collections;
using System.IO;
using Osci.Common;
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
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class NonIntermediaryCertificatesH
        : CertificateH
    {

        public Addressee CipherCertificateAddressee
        {
            get
            {
                return _cipherCertificateAddressee;
            }
            set
            {
                Certificates.Put(value.CipherCertificateId, value.CipherCertificate);
                _cipherCertificateAddressee = value;
            }
        }

        public Originator CipherCertificateOriginator
        {
            get
            {
                return _cipherCertificateOriginator;
            }
            set
            {
                Certificates.Put(value.CipherCertificateId, value.CipherCertificate);
                _cipherCertificateOriginator = value;
            }
        }

        public Author[] CipherCertificatesOtherAuthors
        {
            get
            {
                return _cipherCertificatesOtherAuthors;
            }
            set
            {
                Hashtable hs = Hashtable.Synchronized(new Hashtable());

                for (int i = 0; i < value.Length; i++)
                {
                    // Nach Import von Rollen aus anderen Nachrichten könnte es vorkommen,
                    // dass verschiedene Authors gleiche Ids haben.
                    if (hs.ContainsKey(value[i].CipherCertificateId))
                    {
                        if (!hs[value[i].CipherCertificateId].Equals(value[i].CipherCertificate))
                        {
                            throw new OsciRoleException(DialogHandler.ResourceBundle.GetString("id_conflict_cipher_author") + " " + value[i].CipherCertificateId);
                        }
                    }
                    else
                    {
                        hs[value[i].CipherCertificateId] = value[i];
                        Certificates[value[i].CipherCertificateId] = value[i].CipherCertificate;
                    }
                }
                _cipherCertificatesOtherAuthors = (Author[])SupportClass.CollectionSupport.ToArray(hs.Values, new Author[hs.Count]);
            }
        }

        public Reader[] CipherCertificatesOtherReaders
        {
            get
            {
                return _cipherCertificatesOtherReaders;
            }
            set
            {
                Hashtable hs = Hashtable.Synchronized(new Hashtable());

                for (int i = 0; i < value.Length; i++)
                {
                    // Nach Import von Rollen aus anderen Nachrichten könnte es vorkommen,
                    // dass verschiedene Readers gleiche Ids haben.
                    if (hs.ContainsKey(value[i].CipherCertificateId))
                    {
                        if (!hs[value[i].CipherCertificateId].Equals(value[i].CipherCertificate))
                        {
                            throw new OsciRoleException(DialogHandler.ResourceBundle.GetString("id_conflict_reader") + " " + value[i].CipherCertificateId);
                        }
                    }
                    else
                    {
                        hs[value[i].CipherCertificateId] = value[i];
                        Certificates[value[i].CipherCertificateId] = value[i].CipherCertificate;
                    }
                }

                _cipherCertificatesOtherReaders = (Reader[])SupportClass.CollectionSupport.ToArray(hs.Values, new Reader[hs.Count]);
            }
        }

        public Originator SignatureCertificateOriginator
        {
            get
            {
                return _signatureCertificateOriginator;
            }
            set
            {
                object dumm = value.SignatureCertificate;
                Certificates.Put(value.SignatureCertificateId, value.SignatureCertificate);
                _signatureCertificateOriginator = value;
            }
        }

        public Addressee SignatureCertificateAddressee
        {
            get
            {
                return _signatureCertificateAddressee;
            }
            set
            {
                object dumm = value.SignatureCertificate;
                Certificates.Put(value.SignatureCertificateId, value.SignatureCertificate);
                _signatureCertificateAddressee = value;
            }
        }

        public Author[] SignatureCertificatesOtherAuthors
        {
            get
            {
                return _signatureCertificatesOtherAuthors;
            }
            set
            {
                Hashtable hs = Hashtable.Synchronized(new Hashtable());

                for (int i = 0; i < value.Length; i++)
                {
                    // Nach Import von Rollen aus anderen Nachrichten könnte es vorkommen,
                    // dass verschiedene Authors gleiche Ids haben.
                    if (hs.ContainsKey(value[i].SignatureCertificateId))
                    {
                        if (!hs[value[i].SignatureCertificateId].Equals(value[i].SignatureCertificate))
                        {
                            throw new OsciRoleException(DialogHandler.ResourceBundle.GetString("id_conflict_signer_author") + " " + value[i].SignatureCertificateId);
                        }
                    }
                    else
                    {
                        hs[value[i].SignatureCertificateId] = value[i];
                        Certificates[value[i].SignatureCertificateId] = value[i].SignatureCertificate;
                    }
                }

                _signatureCertificatesOtherAuthors = (Author[])SupportClass.CollectionSupport.ToArray(hs.Values, new Author[hs.Count]);
            }
        }

        private Originator _cipherCertificateOriginator;
        private Author[] _cipherCertificatesOtherAuthors;
        private Addressee _cipherCertificateAddressee;
        private Reader[] _cipherCertificatesOtherReaders;
        private Originator _signatureCertificateOriginator;
        private Addressee _signatureCertificateAddressee;
        private Author[] _signatureCertificatesOtherAuthors;

        /// <summary> Creates a new NonIntermediaryCertificatesH object.
        /// </summary>
        /// <param name="parentMessage">
        /// </param>
        public NonIntermediaryCertificatesH()
        {
        }

        internal NonIntermediaryCertificatesH(string refId)
        {
            RefId = refId;
        }

        public override void WriteXml(Stream stream)
        {
            stream.Write("<" + OsciNsPrefix + ":NonIntermediaryCertificates");
            stream.Write(Ns, 0, Ns.Length);
            stream.Write(" Id=\"nonintermediarycertificates\" " + SoapNsPrefix + ":actor=\"http://www.w3.org/2001/12/soap-envelope/actor/none\" " + SoapNsPrefix + ":mustUnderstand=\"1\">");

            if (_cipherCertificateOriginator != null)
            {
                AddCipherCertificate(_cipherCertificateOriginator, stream);
            }
            if (_cipherCertificatesOtherAuthors != null)
            {
                for (int i = 0; i < _cipherCertificatesOtherAuthors.Length; i++)
                {
                    AddCipherCertificate(_cipherCertificatesOtherAuthors[i], stream);
                }
            }
            if (_cipherCertificateAddressee != null)
            {
                AddCipherCertificate(_cipherCertificateAddressee, stream);
            }
            if (_cipherCertificatesOtherReaders != null)
            {
                for (int i = 0; i < _cipherCertificatesOtherReaders.Length; i++)
                {
                    AddCipherCertificate(_cipherCertificatesOtherReaders[i], stream);
                }
            }
            if (_signatureCertificateOriginator != null)
            {
                AddSignatureCertificate(_signatureCertificateOriginator, stream);
            }
            if (_signatureCertificateAddressee != null)
            {
                AddSignatureCertificate(_signatureCertificateAddressee, stream);
            }
            if (_signatureCertificatesOtherAuthors != null)
            {
                for (int i = 0; i < _signatureCertificatesOtherAuthors.Length; i++)
                {
                    AddSignatureCertificate(_signatureCertificatesOtherAuthors[i], stream);
                }
            }
            stream.Write("</" + OsciNsPrefix + ":NonIntermediaryCertificates>");
        }
    }
}