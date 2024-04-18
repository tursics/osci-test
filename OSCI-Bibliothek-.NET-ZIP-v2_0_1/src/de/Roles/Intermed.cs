using System;
using Osci.Cryptographic;
using Osci.Helper;

namespace Osci.Roles
{
    /// <summary> Diese Klasse stellt einen OSCI-Intermediär dar.
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
    public class Intermed
        : Role
    {
        /// <summary> Ruft die URI des Intermediärs ab, oder legt diese fest.
        /// </summary> 
        /// <value> URI
        /// </value>
        public Uri Uri
        {
            get; set;
        }

        /// <summary> Konstruktor für ein Intermediärs-Objekt (intermediärsseitig).
        /// </summary>
        /// <param name="signer">Signer-Objekt, welches die Signaturen der Nachrichten
        /// erstellen soll (null, wenn keine Signaturen gewünscht).
        /// </param>
        /// <param name="decrypter">Decrypter-Objekt, welches den Inhalt der Nachrichten
        /// entschlüsseln soll.
        /// </param>
        public Intermed(Signer signer, Decrypter decrypter)
        {
            Signer = signer;
            Decrypter = decrypter;
        }

        /// <summary> Konstruktor für ein Intermediärs-Objekt (clientseitig).
        /// Das Signaturzertifikat des Intermediärs muss nicht übergeben werden.
        /// Die verwendete URL muss durch die Transportimplementierung aufgelöst werden können.
        /// </summary>
        /// <param name="signatureCertificate">Zertifikat, mit dem die Signatur einer Antwort
        /// geprüft werden kann.
        /// </param>
        /// <param name="cipherCertificate">Zertifikat, mit dem die Nachricht verschlüsselt
        /// werden soll.
        /// </param>
        /// <param name="uri">URL des Intermediärs.
        /// </param>
        public Intermed(X509Certificate signatureCertificate, X509Certificate cipherCertificate, Uri uri)
        {
            SignatureCertificate = signatureCertificate;
            CipherCertificate = cipherCertificate;
            Uri = uri;
        }

    }
}