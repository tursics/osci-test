using Osci.Helper;
using Osci.Roles;

namespace Osci.Cryptographic
{
    /// <summary> Diese abstrakte Klasse stellt die Schnittstelle der Bibliothek für
    /// die Anbindung von Signier-Modulen (Crypto-Token) dar. Anwendungen, die
    /// OSCI-Nachrichten signieren wollen, müssen an das signierende Rollenobjekt
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
    /// <seealso cref="Role.SignatureAlgorithm()">
    /// </seealso>
    public abstract class Signer
    {

        /// <summary> Liefert die Versionsnummer
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

        /// <summary> Die Implementierung dieser Methode muss das Signaturzertifikat zurückgeben.
        /// </summary>
        /// <value> Signaturzertifikat
        /// </value>
        public abstract X509Certificate Certificate
        {
            get;
        }

        /// <summary> Die Implementierung dieser Methode muss das übergebene Byte-Array signieren.
        /// Der algorithm-Parameter hat keine weitere Bedeutung mehr, da ab Version 1.3 die
        /// Signer-Implemetierung über die Methode getAlgorithm() den Algorithmus festlegt.
        /// Die OSCI-Bibliothek fragt vor dem Aufruf von sign(byte[], String) den Signaturalgorithmus
        /// ab und übergibt den String wieder an die sign-Methode.
        /// </summary>
        /// <param name="hash">Zu signierendes Byte-Array.
        /// </param>
        /// <param name="algorithm">Algorithmus
        /// </param>
        /// <returns> Signatur-Wert
        /// </returns>
        public abstract byte[] Sign(byte[] hash, string algorithm);

        /// <summary> Diese Methode sollte den Signaturalgorithmus als XML-Identifier zurückgeben,
        /// den die Implementierung bei der Erzeugung eine Signatur verwendet.
        /// 
        /// </summary>
        /// <returns> Signaturalgorithmus
        /// </returns>
        public abstract string GetAlgorithm();
    }
}