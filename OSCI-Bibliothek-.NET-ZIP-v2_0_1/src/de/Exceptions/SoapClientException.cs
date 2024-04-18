namespace Osci.Exceptions
{
    /// <summary> Diese Exception zeigt eine der in der OSCI-Spezifikation
    /// definierten Fehlermeldungen auf Nachrichtenebene (faultcode soap:Client).
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
    public class SoapClientException
        : OsciException
    {

        /// <summary> Erzeugt ein SoapClientException-Objekt mit einem Fehlercode und erklärendem
        /// String als Message. Als Message-String wird der dem Code entsprechende Eintrag
        /// in der zum aktuellen default-Locale gehörenden Sprachdatei 
        /// (s. Package de.osci.osci12.extinterfaces.language) gesetzt.
        /// </summary>
        /// <param name="errorCode">errorCode
        /// </param>
        public SoapClientException(string errorCode) 
            : base(errorCode)
        {
        }

        /// <summary>Erzeugt ein SoapClientException-Objekt mit einem erklärenden String als
        /// Message und einem Fehlercode.
        /// </summary>
        /// <param name="faultstring">faultstring</param>
        /// <param name="oscicode">oscicode</param>
        public SoapClientException(string oscicode, string faultstring) 
            : base(faultstring, oscicode)
        {
        }
    }
}