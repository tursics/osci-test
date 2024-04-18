using System;
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
    public class IntermediaryCertificatesHBuilder
        : MessagePartParser
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(IntermediaryCertificatesHBuilder));

        private const int _cipherCertificateIntermediary = 0;
        private const int _signatureCertificateIntermediary = 1;
        private readonly int[] _check;
        private readonly IntermediaryCertificatesH _ich;
        private int _typ;
        private string _refId;

        private readonly Intermed _intermed;

        /// <summary> Constructor for the IntermediaryCertificatesHBuilder object
        /// </summary>
        /// <param name="xmlReader">
        /// </param>
        /// <param name="parentHandler">
        /// </param>
        /// <param name="attributes">
        /// </param>
        /// <param name="check">
        /// </param>
        public IntermediaryCertificatesHBuilder(OsciMessageBuilder parentHandler, Attributes attributes, int[] check)
            : base(parentHandler)
        {
            parentHandler.SignatureRelevantElements.AddElement("IntermediaryCertificates", OsciXmlns, attributes);

            _check = check;
            _ich = new IntermediaryCertificatesH();
            OsciMessage msg = parentHandler.OsciMessage;
            _ich.SetNamespacePrefixes(msg);

            if (parentHandler.OsciMessage.DialogHandler.Supplier is Intermed)
            {
                _intermed = (Intermed)parentHandler.OsciMessage.DialogHandler.Supplier;
            }
            else
            {
                _intermed = (Intermed)parentHandler.OsciMessage.DialogHandler.Client;
                if (_intermed == null)
                {
                    _intermed = new Intermed(null, null, null);
                    parentHandler.OsciMessage.DialogHandler.Client = _intermed;
                }
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
        /// <param name="attributes">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start-Element: " + localName);
            if (localName.Equals("CipherCertificateIntermediary") && uri.Equals(OsciXmlns))
            {
                _typ = _cipherCertificateIntermediary;
            }
            else if (localName.Equals("SignatureCertificateIntermediary") && uri.Equals(OsciXmlns))
            {
                _typ = _signatureCertificateIntermediary;
            }
            else if (localName.Equals("X509Certificate") && uri.Equals(DsXmlns))
            {
                CurrentElement = new System.Text.StringBuilder();
            }
            else if (!(localName.Equals("X509Data") && uri.Equals(DsXmlns)))
            {
                throw new SaxException("Unerwartetes Element im IntermediaryCertificate-Header: " + localName);
            }
            if (_check[_typ] == 0)
            {
                throw new SaxException("Unerlaubtes Zertifikat im IntermediaryCertificate-Header, Typ: " + _typ);
            }
            if (attributes.GetValue("Id") != null)
            {
                _refId = attributes.GetValue("Id");
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
            _log.Trace("End-Element: " + localName);
            if (localName.Equals("IntermediaryCertificates") && uri.Equals(OsciXmlns))
            {
                _log.Trace("Setze IntermedCertificates");
                Msg.IntermediaryCertificatesH = _ich;
                XmlReader.ContentHandler = ParentHandler;
            }
            else if (localName.Equals("X509Certificate") && uri.Equals(DsXmlns))
            {
                int i;

                X509Certificate cert = Tools.CreateCertificateFromBase64String(CurrentElement.ToString());

                if (_typ == _cipherCertificateIntermediary)
                {
                    if (_intermed.HasCipherCertificate())
                    {
                        if (!_intermed.CipherCertificate.Equals(cert))
                        {
                            throw new SystemException("Verschlüsselungszertifikat des Intermediärs in der Nachricht stimmt nicht mit dem vorhandenen Zertifikat überein.");
                        }
                    }
                    else
                        _intermed.CipherCertificate = cert;

                    _intermed.CipherRefId = _refId;
                    _ich.CipherCertificateIntermediary = _intermed;
                }
                else if (_typ == _signatureCertificateIntermediary)
                {
                    if (_intermed.HasSignatureCertificate())
                    {
                        if (!_intermed.SignatureCertificate.Equals(cert))
                        {
                            throw new SystemException("Signaturzertifikat des Intermediärs in der Nachricht stimmt nicht mit dem vorhandenen Zertifikat überein.");
                        }
                    }
                    else
                        _intermed.SignatureCertificate = cert;

                    _intermed.SignatureRefId = _refId;
                    _ich.SignatureCertificateIntermediary = _intermed;
                }
            }
            _typ = -1;
            CurrentElement = null;
        }
    }
}