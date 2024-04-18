using Osci.Cryptographic;
using Osci.Helper;

namespace Osci.Roles
{
    /// <summary> Diese Klasse stellt einen OSCI-Absender dar.
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
    public class Originator
        : Role
    {
        /// <summary> Konstruktor für den Versand einer Nachricht.
        /// </summary>
        /// <param name="signer">Signer-Objekt, welches die Signatur der Nachricht erstellen
        /// soll (null, wenn keine Signatur gewünscht).
        /// </param>
        /// <param name="decrypter">Decrypter-Objekt, welches den Inhalt der Rückantwort entschlüsseln
        /// soll.
        /// </param>
        public Originator(Signer signer, Decrypter decrypter)
        {
            Signer = signer;
            Decrypter = decrypter;
        }

        /// <summary> Konstruktor für den Empfang einer Nachricht.
        /// </summary>
        /// <param name="signatureCertificate">Zertifikat, mit dem die Signatur der Nachricht geprüft werden kann.
        /// </param>
        /// <param name="cipherCertificate">Zertifikat, mit dem die Rückantwort verschlüsselt werden soll.
        /// </param>
        public Originator(X509Certificate signatureCertificate, X509Certificate cipherCertificate)
        {
            SignatureCertificate = signatureCertificate;
            CipherCertificate = cipherCertificate;
        }

    }
}