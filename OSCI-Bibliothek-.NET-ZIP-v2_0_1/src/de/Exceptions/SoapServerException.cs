namespace Osci.Exceptions
{
    /// <summary> Diese Exception zeigt eine der in der OSCI-Spezifikation
    /// definierten Fehlermeldungen auf Nachrichtenebene (faultcode soap:Server).
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
    public class SoapServerException
        : OsciException
    {

        /// <summary> Erzeugt ein SoapServerException-Objekt mit einem erklärenden String und
        /// einem Fehlercode. Als Fehlercode kann jeder String verwendet werden, für den
        /// ein entsprechender Eintrag in der zum gesetzten default-Locale gehörenden
        /// Sprachdatei (s. Package de.osci.osci12.extinterfaces.language) vorhanden ist.
        /// </summary>
        /// <param name="message">message
        /// </param>
        /// <param name="errorCode">errorCode
        /// </param>
        public SoapServerException(string code)
            : base(code)
        {
        }

        /// <summary>Erzeugt ein SoapServerException-Objekt mit einem erklärenden String als
        /// Message und einem Fehlercode.
        /// </summary>
        /// <param name="faultstring">Fehlertext</param>
        /// <param name="oscicode">oscicode</param>
        public SoapServerException(string oscicode, string faultstring)
            : base(faultstring, oscicode)
        {
        }
    }
}