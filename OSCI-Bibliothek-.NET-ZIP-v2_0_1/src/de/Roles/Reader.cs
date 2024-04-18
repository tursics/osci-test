using Osci.Cryptographic;
using Osci.Exceptions;
using Osci.Helper;

namespace Osci.Roles
{
    /// <summary> Diese Klasse stellt einen OSCI-Leser dar.
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
    public class Reader
        : Role
    {
        public X509Certificate SignatureCertificate
        {
            set
            {
                throw new UnsupportedOperationException("Readerobjekte haben kein Signaturzertifikat.");
            }
        }

        private static int _idNr = -1;

        /// <summary>  Konstruktor für den Empfang einer Nachricht. Wird als Parameter
        /// null übergeben, weil keine Verschlüsselung der Rückantwort gewünscht wird,
        /// muß trotzdem mit CipherCertificate(X509Certificate) ein
        /// Verschlüsselungszertifikat gesetzt werden, weil dies Voraussetzung für
        /// die Teilnahme an OSCI ist.
        /// </summary>
        /// <param name="decrypter">Decrypter-Objekt, welches den Inhalt der Nachricht entschlüsseln
        /// soll.
        /// </param>
        public Reader(Decrypter decrypter)
        {
            Decrypter = decrypter;
            _idNr++;
            Id += _idNr;
        }

        /// <summary> Konstruktor für das Versenden einer Nachricht.
        /// </summary>
        /// <param name="cipherCertificate">Zertifikat, mit dem die Nachricht verschlüsselt werden soll.
        /// </param>
        public Reader(X509Certificate cipherCertificate)
        {
            CipherCertificate = cipherCertificate;
            _idNr++;
            Id += _idNr;
        }
    }
}