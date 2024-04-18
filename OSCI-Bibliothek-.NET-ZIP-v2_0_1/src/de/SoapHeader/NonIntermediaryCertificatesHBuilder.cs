using System;
using System.Collections;
using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Messagetypes;
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
    public class NonIntermediaryCertificatesHBuilder
        : MessagePartParser
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(NonIntermediaryCertificatesHBuilder));

        private const int _cipherCertificateOriginator = 0;
        private const int _cipherCertificateOtherAuthor = 1;
        private const int _cipherCertificateAddressee = 2;
        private const int _cipherCertificateOtherReader = 3;
        private const int _signatureCertificateOriginator = 4;
        private const int _signatureCertificateOtherAuthor = 5;
        private const int _signatureCertificateAddressee = 6;

        // für das Array relevant sind die oben aufgeführten statischen Variablen wie z.B. CIPHER_CERTIFICATE_ORIGINATOR
        private readonly int[] _check;
        private readonly NonIntermediaryCertificatesH _nic;
        private int _typ;
        private Originator _originator;
        private Addressee _addressee;
        private readonly ArrayList _reader;
        private readonly ArrayList _authors;
        private string _tmpId, _refId;
        private readonly bool _changeOrgsAndAdds;

        /// <summary> Constructor for the NonIntermediaryCertificatesHBuilder object
        /// </summary>
        /// <param name="xmlReader">
        /// </param>
        /// <param name="parentHandler">
        /// </param>
        /// <param name="attributes">
        /// </param>
        /// <param name="check">
        /// </param>

        public NonIntermediaryCertificatesHBuilder(OsciMessageBuilder parentHandler, Attributes attributes, int[] check)
            : base(parentHandler)
        {
            parentHandler.SignatureRelevantElements.AddElement("NonIntermediaryCertificates", OsciXmlns, attributes);

            _check = check;
            _reader = new ArrayList();
            _authors = new ArrayList();

            string id = attributes.GetValue("Id");
            _nic = id == null
                ? new NonIntermediaryCertificatesH()
                : new NonIntermediaryCertificatesH(id);

            int msgTyp = Msg.MessageType;

            if ((msgTyp == OsciMessage.ResponseToFetchDelivery) ||
                (msgTyp == OsciMessage.ResponseToProcessDelivery) ||
                (msgTyp == OsciMessage.ResponseToMediateDelivery) ||
                (msgTyp == OsciMessage.ResponseToPartialFetchDelivery))
            {
                _changeOrgsAndAdds = true;
            }
            _nic.SetNamespacePrefixes(Msg);

        }

        /// <summary> 
        /// </summary>
        /// <param name="uri">
        /// </param>
        /// <param name="localName">
        /// </param>
        /// <param name="qName">
        /// </param>
        /// <param name="attributes">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start-Element: " + qName);
            if (localName.Equals("CipherCertificateOriginator") && uri.Equals(OsciXmlns))
            {
                _typ = _cipherCertificateOriginator;
            }
            else if (localName.Equals("CipherCertificateOtherAuthor") && uri.Equals(OsciXmlns))
            {
                _typ = _cipherCertificateOtherAuthor;
            }
            else if (localName.Equals("CipherCertificateAddressee") && uri.Equals(OsciXmlns))
            {
                _typ = _cipherCertificateAddressee;
            }
            else if (localName.Equals("CipherCertificateOtherReader") && uri.Equals(OsciXmlns))
            {
                _typ = _cipherCertificateOtherReader;
            }
            else if (localName.Equals("SignatureCertificateOriginator") && uri.Equals(OsciXmlns))
            {
                _typ = _signatureCertificateOriginator;
            }
            else if (localName.Equals("SignatureCertificateOtherAuthor") && uri.Equals(OsciXmlns))
            {
                _typ = _signatureCertificateOtherAuthor;
            }
            else if (localName.Equals("SignatureCertificateAddressee") && uri.Equals(OsciXmlns))
            {
                _typ = _signatureCertificateAddressee;
            }
            else if (localName.Equals("X509Certificate") && uri.Equals(DsXmlns))
            {
                CurrentElement = new System.Text.StringBuilder();
            }
            else if (!localName.Equals("X509Data") && uri.Equals(DsXmlns))
            {
                throw new SaxException("Unerwartetes Element im NonIntermediaryCertificate-Header: " + qName);
            }
            if (attributes.GetValue("Id") != null)
            {
                _refId = attributes.GetValue("Id");
                if (_refId.IndexOf("_") >= 0)
                {
                    _tmpId = _refId.Substring(0, _refId.IndexOf('_'));
                }
            }
            if (_check[_typ] == 0)
            {
                throw new SaxException("Unerlaubtes Zertifikat im NonIntermediaryCertificate-Header, Typ: " + _typ);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="uri">
        /// </param>
        /// <param name="localName">
        /// </param>
        /// <param name="qName">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End-Element: " + qName);
            if (localName.Equals("NonIntermediaryCertificates") && uri.Equals(OsciXmlns))
            {
                for (int i = 0; i < _check.Length; i++)
                {
                    if (_check[i] > 0)
                    {
                        throw new SaxException("Fehlendes Zertifikat im NonIntermediaryCertificate-Header, Typ: " + i);
                    }
                }
                _nic.CipherCertificatesOtherReaders = (Reader[])_reader.ToArray(typeof(Reader));

                ArrayList vec = new ArrayList();
                for (int i = 0; i < _authors.Count; i++)
                {
                    if (((Role)_authors[i]).HasCipherCertificate())
                    {
                        object dumm = ((Role)_authors[i]).CipherCertificate;
                        vec.Add(_authors[i]);
                    }
                }
                _nic.CipherCertificatesOtherAuthors = (Author[])vec.ToArray(typeof(Author));

                vec = new ArrayList();
                for (int i = 0; i < _authors.Count; i++)
                {
                    if (((Role)_authors[i]).HasSignatureCertificate())
                    {
                        object dumm = ((Role)_authors[i]).SignatureCertificate;
                        vec.Add(_authors[i]);
                    }
                }
                _nic.SignatureCertificatesOtherAuthors = (Author[])vec.ToArray(typeof(Author));
                Msg.AddRole(_originator);
                Msg.AddRole(_addressee);

                if (Msg is ResponseToAcceptDelivery)
                {
                    if (_addressee != null)
                    {
                        Msg.DialogHandler.Supplier = _addressee;
                    }
                }
                else if (Msg is ResponseToProcessDelivery)
                {
                    if (_originator != null)
                    {
                        if (_originator.HasSignatureCertificate())
                        {
                            Msg.DialogHandler.Supplier.SignatureCertificate = _originator.SignatureCertificate;
                        }
                        if (_originator.HasCipherCertificate())
                        {
                            Msg.DialogHandler.Supplier.CipherCertificate = _originator.CipherCertificate;
                        }
                    }
                }
                else if (!_changeOrgsAndAdds && !(Msg is AcceptDelivery) && !(Msg is ProcessDelivery) && (_originator != null))
                {
                    Msg.DialogHandler.Client = _originator;
                }

                for (int i = 0; i < _authors.Count; i++)
                {
                    Msg.AddRole((Author)_authors[i]);
                }
                for (int i = 0; i < _reader.Count; i++)
                {
                    Msg.AddRole((Reader)_reader[i]);
                }
                Msg.NonIntermediaryCertificatesH = _nic;

                XmlReader.ContentHandler = ParentHandler;
            }
            else if (localName.Equals("X509Certificate") && uri.Equals(DsXmlns))
            {
                X509Certificate cert = Tools.CreateCertificateFromBase64String(CurrentElement.ToString());

                if (_typ == _cipherCertificateOriginator)
                {

                    if (!_changeOrgsAndAdds && (Msg.DialogHandler.Client is Originator))
                    {
                        _originator = (Originator)Msg.DialogHandler.Client;
                    }
                    if (_originator != null)
                    {
                        if (!_originator.HasCipherCertificate())
                        {
                            _originator.CipherCertificate = cert;
                        }
                        if (!_originator.CipherCertificate.Equals(cert))
                        {
                            throw new ArgumentException("Nicht passendes Zertifikat gefunden für Originator.");
                        }

                    }
                    else
                    {
                        _originator = new Originator(null, cert);
                    }
                    if (_tmpId != null)
                    {
                        _originator.Id = _tmpId;
                    }
                    _originator.CipherRefId = _refId;

                    _nic.CipherCertificateOriginator = _originator;
                    _check[_cipherCertificateOriginator] = 0;
                }
                else if (_typ == _cipherCertificateOtherAuthor)
                {
                    Author au = new Author(null, cert);
                    if (_tmpId != null)
                    {
                        au.Id = _tmpId;
                    }
                    au.CipherRefId = _refId;
                    _authors.Add(au);
                    Msg.AddRole(au);
                }
                else if (_typ == _cipherCertificateAddressee)
                {

                    if (!_changeOrgsAndAdds && (Msg.DialogHandler.Client is Addressee))
                    {
                        _addressee = (Addressee)Msg.DialogHandler.Supplier;
                    }

                    if (_addressee != null)
                    {
                        if (_addressee.HasCipherCertificate())
                        {
                            _addressee.CipherCertificate = cert;
                        }
                        else if (!_addressee.CipherCertificate.Equals(cert))
                        {
                            throw new ArgumentException("Nicht apssendes Zertifikat gefunden für Addressee.");
                        }
                    }
                    else
                    {
                        _addressee = new Addressee(null, cert);
                    }

                    if (_tmpId != null)
                    {
                        _addressee.Id = _tmpId;
                    }
                    _addressee.CipherRefId = _refId;
                    _nic.CipherCertificateAddressee = _addressee;
                    _check[_cipherCertificateAddressee] = 0;
                }
                else if (_typ == _cipherCertificateOtherReader)
                {
                    Reader rd = new Reader(cert);
                    if (_tmpId != null)
                    {
                        rd.Id = _tmpId;
                    }
                    rd.CipherRefId = _refId;
                    _reader.Add(rd);
                    Msg.AddRole(rd);
                }
                else if (_typ == _signatureCertificateOriginator)
                {
                    if (_originator == null)
                    {
                        if (!_changeOrgsAndAdds && (Msg.DialogHandler.Client is Originator))
                        {
                            _originator = (Originator)Msg.DialogHandler.Client;
                        }
                        else
                        {
                            _originator = new Originator(cert, null);
                        }
                    }
                    else
                    {
                        _originator.SignatureCertificate = cert;
                    }
                    if (_tmpId != null)
                    {
                        _originator.Id = _tmpId;
                    }
                    _originator.SignatureRefId = _refId;
                    _nic.SignatureCertificateOriginator = _originator;
                    _check[_signatureCertificateOriginator] = 0;
                }
                else if (_typ == _signatureCertificateOtherAuthor)
                {
                    int i;
                    for (i = 0; i < _authors.Count; i++)
                    {

                        if (((Role)_authors[i]).Id.Equals(_tmpId))
                        {
                            try
                            {
                                // Test, ob bereits ein Author-Objekt mit dieser Vorwahl mit einem Signaturzertifikat 
                                // versehen wurde (sollte nur beim Import von Rollen aus anderen Nachrichten vorkommen) 
                                X509Certificate x5 = ((Author)_authors[i]).SignatureCertificate;

                                // Role.id wird nur noch intern verwendet 
                                if (_tmpId != null)
                                {
                                    _tmpId += '0';
                                }
                            }
                            catch (OsciRoleException)
                            {
                                ((Author)_authors[i]).SignatureCertificate = cert;
                                ((Author)_authors[i]).SignatureRefId = _refId;
                                break;
                            }
                        }
                    }
                    if (i == _authors.Count)
                    {
                        Author au = new Author(cert, null);
                        if (_tmpId != null)
                        {
                            au.Id = _tmpId;
                        }
                        au.SignatureRefId = _refId;
                        _authors.Add(au);
                        Msg.AddRole(au);
                    }
                }
                else if (_typ == _signatureCertificateAddressee)
                {
                    if (_addressee == null)
                    {
                        if (!_changeOrgsAndAdds && (Msg.DialogHandler.Supplier is Addressee))
                        {
                            _addressee = (Addressee)Msg.DialogHandler.Supplier;
                            _addressee.SignatureCertificate = cert;
                        }
                        _addressee = new Addressee(cert, null);
                    }
                    else
                    {
                        _addressee.SignatureCertificate = cert;
                    }

                    if (_tmpId != null)
                    {
                        _addressee.Id = _tmpId;
                    }
                    _addressee.SignatureRefId = _refId;
                    _nic.SignatureCertificateAddressee = _addressee;
                    _check[_signatureCertificateAddressee] = 0;
                }
                _tmpId = null;
            }
            _typ = -1;
            CurrentElement = null;
        }
    }
}