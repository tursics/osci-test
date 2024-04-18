using Osci.Exceptions;
using Osci.Helper;

namespace Osci.Cryptographic
{
    /// <summary>Diese abstrakte Klasse stellt die Schnittstelle der Bibliothek für
    /// die Anbindung von Entschlüsselungs-Modulen (Crypto-Token) dar. Anwendungen, die
    /// OSCI-Nachrichten entschlüsseln wollen, müssen an das entschlüsselnde Rollenobjekt
    /// eine Implementation dieser Klasse übergeben.
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
    public abstract class Decrypter
    {

        /// <summary> Liefert die Versionsnummer.
        /// </summary>
        /// <value> Versionsnummer
        /// </value>
        public abstract string Version
        {
            get;
        }

        /// <summary> Liefert den Namen des Herstellers.
        /// </summary>
        /// <value> Herstellername
        /// </value>
        public abstract string Vendor
        {
            get;
        }

        /// <summary> Die Implementierung dieser Methode muss das Verschlüsselungszertifikat zurückgeben.
        /// </summary>
        /// <value> Verschlüsselungszertifikat
        /// </value>
        public abstract X509Certificate Certificate
        {
            get;
        }

        /// <summary> Die Implementierung dieser Methode muss das übergebene Byte-Array gemäß RSAES-PKCS1-v1_5 entschlüsseln.
        /// Diese Methode muss überschrieben werden.
        /// </summary>
        /// <param name="data">Zu entschlüsselndes Byte-Array.
        /// </param>
        /// <returns> Entschlüsseltes Byte-Array.
        /// </returns>
        public abstract byte[] Decrypt(byte[] data);

        /// <summary> Die Implementierung dieser Methode muss das übergebene Byte-Array gemäß RSAES-OAEP-ENCRYPT entschlüsseln.
        /// Diese Methode muss überschrieben werden.
        /// </summary>
        /// <param name="data">Zu entschlüsselndes Byte-Array
        /// </param>
        /// <param name="mfgAlgorithm">Identifier der Mask-Generation-Function (OAEP), z.B. http://www.w3.org/2009/xmlenc11#mgf1sha256
        /// </param>
        /// <param name="digestAlgorithm">Identifier des verwendeten Hashalgorithmus (OAEP), z.B. http://www.w3.org/2001/04/xmlenc#sha256
        /// </param>
        /// <returns> Entschlüsseltes Byte-Array.
        /// </returns>
        public abstract byte[] Decrypt(byte[] data, string mfgAlgorithm, string digestAlgorithm);

    }
}