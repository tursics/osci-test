using Osci.Cryptographic;
using Osci.Helper;

namespace Osci.Roles
{
    /// <summary> Diese Klasse stellt einen OSCI-Autor dar.
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
    public class Author
        : Role
    {
        private static int _idNr = -1;

        /// <summary> Konstruktor für den Versand einer Nachricht. Wird als zweiter Parameter
        /// null übergeben, weil keine Verschlüsselung der Rückantwort gewünscht wird,
        /// muß trotzdem mit CipherCertificate(X509Certificate) ein
        /// Verschlüsselungszertifikat gesetzt werden, weil dies Voraussetzung für
        /// die Teilnahme an OSCI ist.
        /// </summary>
        /// <param name="signer">Signer-Objekt, welches die Signatur der Nachricht erstellen
        /// soll (null, wenn keine Signatur gewünscht).
        /// </param>
        /// <param name="decrypter">Decrypter-Objekt, welches den Inhalt der Rückantwort entschlüsseln
        /// soll (null, wenn die Nachricht nicht verschlüsselt wird).
        /// </param>
        public Author(Signer signer, Decrypter decrypter)
        {
            Signer = signer;
            Decrypter = decrypter;
            _idNr++;
            Id += _idNr;
        }

        /// <summary> Konstruktor für den Empfang einer Nachricht.
        /// </summary>
        /// <param name="signatureCertificate">Zertifikat, mit dem die Signatur der Nachricht geprüft wird.
        /// </param>
        /// <param name="cipherCertificate">Zertifikat, mit dem die Rückantwort verschlüsselt werden soll.
        /// </param>
        public Author(X509Certificate signatureCertificate, X509Certificate cipherCertificate)
        {
            SignatureCertificate = signatureCertificate;
            CipherCertificate = cipherCertificate;
            _idNr++;
            Id += _idNr;
        }
    }
}