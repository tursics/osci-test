using System;
using System.Collections;
using System.IO;
using Osci.Common;
using Osci.Extensions;
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
    public abstract class CertificateH
        : HeaderEntry
    {
        public Hashtable Certificates
        {
            get; private set;
        }

        public CertificateH()
        {
            Certificates = new Hashtable();
        }

        protected void AddCipherCertificate(Role role, Stream stream)
        {
            if (role == null)
            {
                throw new OsciRoleException("Role-Objekt darf nicht null sein.", "");
            }
            string name = role.GetType().FullName.Substring(role.GetType().FullName.LastIndexOf('.') + 1);
            if (role.CipherCertificate == null)
            {
                throw new OsciRoleException("Kein Verschlüsselungszertifikat für " + name + " eingestellt.", null);
            }
            if (role is Intermed)
            {
                name = "Intermediary";
            }
            else if (role is Originator)
            {
                name = "Originator";
            }
            else if (role is Addressee)
            {
                name = "Addressee";
            }
            else if (role is Reader)
            {
                name = "OtherReader";
            }
            else if (role is Author)
            {
                name = "OtherAuthor";
            }
            else
            {
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString("invalid_firstargument") + role);
            }

            stream.Write("<" + OsciNsPrefix + ":CipherCertificate" + name + " Id=\"" + role.CipherCertificateId + "\"><" + DsNsPrefix + ":X509Data><" + DsNsPrefix + ":X509Certificate>");
            Base64OutputStream base64Out = new Base64OutputStream(stream, true);
            base64Out.Write(role.CipherCertificate.GetEncoded(), 0, role.CipherCertificate.GetEncoded().Length);
            base64Out.Flush();
            stream.Write("</" + DsNsPrefix + ":X509Certificate></" + DsNsPrefix + ":X509Data></" + OsciNsPrefix + ":CipherCertificate" + name + ">");
        }

        protected void AddSignatureCertificate(Role role, Stream stream)
        {
            string name = role.GetType().FullName.Substring(role.GetType().FullName.LastIndexOf('.') + 1);
            if ((role == null) || (role.SignatureCertificate == null))
            {
                throw new OsciRoleException("Kein Signaturzertifikat für " + name + "eingestellt.", null);
            }

            if (role is Intermed)
            {
                name = "Intermediary";
            }
            else if (role is Originator)
            {
                name = "Originator";
            }
            else if (role is Addressee)
            {
                name = "Addressee";
            }
            else if (role is Reader)
            {
                name = "OtherReader";
            }
            else if (role is Author)
            {
                name = "OtherAuthor";
            }
            else
            {
                throw new ArgumentException(DialogHandler.ResourceBundle.GetString("invalid_firstargument") + role);
            }

            stream.Write("<" + OsciNsPrefix + ":SignatureCertificate" + name + " Id=\"" + role.SignatureCertificateId + "\"><" + DsNsPrefix + ":X509Data><" + DsNsPrefix + ":X509Certificate>");
            try
            {
                Base64OutputStream base64Out = new Base64OutputStream(stream, true);
                base64Out.Write(role.SignatureCertificate.GetEncoded(), 0, role.SignatureCertificate.GetEncoded().Length);
                base64Out.Flush();
            }
            catch (Exception ex)
            {
                throw new OsciRoleException("Problem beim Codieren des Zertifikats: " + ex + ". " + ex.Message, null);
            }

            stream.Write("</" + DsNsPrefix + ":X509Certificate></" + DsNsPrefix + ":X509Data></" + OsciNsPrefix + ":SignatureCertificate" + name + ">");
        }
    }
}