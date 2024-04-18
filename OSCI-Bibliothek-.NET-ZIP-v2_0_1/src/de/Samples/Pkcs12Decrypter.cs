using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Osci.Cryptographic;
using Osci.Encryption;
using Osci.Helper;

namespace Osci.Samples
{
    /// <summary><p>Diese Klasse ist eine Beispiel-Implementierung der abstrakten Decrypter-Klasse.
    /// Für die Verwendung wird ein PKCS#12-Keystore in Form einer *.p12-Datei benötigt.
    /// Die Implementierung ist für Testzwecke bestimmt, sie greift auf den ersten
    /// verfügbaren Alias zu. Die PIN für dessen Privatschlüssel muss die gleiche sein
    /// wie die des Keystores.</p>
    /// <p>Diese einfache Implementierung hält die PIN des Keystores als Character-Array
    /// im Arbeitsspeicher, sie wird als String übergeben. Es wird Anwendern empfohlen,
    /// eigene Implementierungen zu schreiben, die die PIN in der Methode decrypt(...)
    /// abfragen und nach Gebrauch wieder löschen oder anderweitig für ein sicheres
    /// Pin-Cashing zu sorgen.</p>
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
    /// <seealso cref="Decrypter">
    /// </seealso>
    public class Pkcs12Decrypter
        : Decrypter
    {
        private static Log _log = LogFactory.GetLog(typeof(Pkcs12Decrypter));
        private readonly AsymmetricKeyParameter _key;


        /// <summary> Liefert den Namen des Herstellers.
        /// </summary>
        /// <value> Herstellername
        /// </value>
        public override string Vendor
        {
            get
            {
                return "Governikus";
            }
        }

        /// <summary> Liefert die Versionsnummer.
        /// </summary>
        /// <value> Versionsnummer
        /// </value>
        public override string Version
        {
            get
            {
                return "2.0";
            }
        }

        /// <summary> Liefert das Verschlüsselungszertifikat.
        /// </summary>
        /// <value> Verschlüsselungszertifikat
        /// </value>
        public override X509Certificate Certificate
        {
            get;
        }

        public Pkcs12Decrypter(string p12FileName, string pin)
            : this(new FileStream(p12FileName, FileMode.Open, FileAccess.Read), pin)
        {
        }

        public Pkcs12Decrypter(byte[] rawData, string pin)
            : this(new MemoryStream(rawData), pin)
        {
        }


        private Pkcs12Decrypter(Stream stream, string pin)
        {
            Pkcs12Store p12 = new Pkcs12Store(stream, pin.ToCharArray());
            stream.Close();

            foreach (string alias in p12.Aliases)
            {
                if (p12.IsKeyEntry(alias))
                {
                    _key = p12.GetKey(alias).Key;
                    Certificate = new X509Certificate(p12.GetCertificate(alias).Certificate.GetEncoded());
                    break;
                }
            }
        }


        /// <summary> Entschlüsselt das übergebene Byte-Array gemäß RSAES-PKCS1-v1_5.
        /// </summary>
        /// <param name="data"> die zu entschlüsselnden Daten
        /// </param>
        /// <returns> die verschlüsselten Daten
        /// </returns>
        public override byte[] Decrypt(byte[] data)
        {
            return Crypto.Decrypt(_key, data, null, null);
        }

        /// <summary> Entschlüsselt das übergebene Byte-Array gemäß RSAES-OAEP oder RSAES-PKCS1-v1_5.
        /// </summary>
        /// <param name="data"> die zu entschlüsselnden Daten
        /// </param>
        /// <param name="mfgAlgorithm"> der vwendete Mask-Generation-Function (null für RSAES-PKCS1-v1_5)
        /// </param>
        /// <param name="digestAlgorithm">der verwendete Hashalgorithmus (wird ignoriert für RSAES-PKCS1-v1_5)
        /// </param>
        /// <returns> die zu entschlüsselnden Daten
        /// </returns>
        public override byte[] Decrypt(byte[] data, string mfgAlgorithm, string digestAlgorithm)
        {
            return Crypto.Decrypt(_key, data, mfgAlgorithm, digestAlgorithm);
        }
    }
}