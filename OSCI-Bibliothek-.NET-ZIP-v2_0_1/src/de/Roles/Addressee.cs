using Osci.Cryptographic;
using Osci.Helper;

namespace Osci.Roles
{
    /// <summary> Diese Klasse stellt einen OSCI-Empfänger dar.
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
    public class Addressee
        : Role
    {
        /// <summary> Konstruktor für den Empfang einer Nachricht.
        /// </summary>
        /// <param name="signer">Signer-Objekt, welches die Signatur der Rückantwort erstellen
        /// soll (null, wenn keine Signatur gewünscht).
        /// </param>
        /// <param name="decrypter">Decrypter-Objekt, welches den Inhalt der Nachricht entschlüsseln
        /// soll.
        /// </param>
        public Addressee(Signer signer, Decrypter decrypter)
        {
            Signer = signer;
            Decrypter = decrypter;
        }

        /// <summary> Konstruktor für das Versenden einer Nachricht.
        /// </summary>
        /// <param name="signatureCertificate">Zertifikat, mit dem die Signatur der Rückantwort geprüft wird.
        /// </param>
        /// <param name="cipherCertificate">Zertifikat, mit dem die Nachricht verschlüsselt werden soll.
        /// </param>
        public Addressee(X509Certificate signatureCertificate, X509Certificate cipherCertificate)
        {
            SignatureCertificate = signatureCertificate;
            CipherCertificate = cipherCertificate;
        }
    }
}