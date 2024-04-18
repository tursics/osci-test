using System;
using Osci.Common;

namespace Osci.Exceptions
{
    // Diese Exception wird f�r die in der OSCI-Spezifikation definierten Fehlermeldungen
    // sowie f�r allgemeine eigene Fehlermeldungen der Bibliothek, die keiner besonderen Kategorie
    // zugeordnet werden k�nnen. Au�erdem stellt sie die Superklasse aller �brigen bibliothekseigenen
    // Exceptions dar. 
    /// <summary> Diese Klasse stellt die Superklasse aller bibliothekseigenen
    /// Exceptions dar.
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
    public class OsciException
        : Exception
    {
        private readonly string _errorCode = "null";

        /// <summary> Liefert den OSCI-Fehlercode.
        /// </summary>
        /// <value> Code
        /// </value>
        public string ErrorCode
        {
            get
            {
                return _errorCode;
            }
        }

        /// <summary> Liefert die Exception-Nachricht in der jeweiligen Sprache (Default-Locale).
        /// </summary>
        /// <returns> lokale Nachricht
        /// </returns>
        public string GetLocalizedMessage()
        {
            try
            {
                return DialogHandler.ResourceBundle.GetString(_errorCode);
            }
            catch (NullReferenceException)
            {
                return _errorCode + " - " + Message;
            }
        }

        // <overloads>Diese Methode hat �berladungen</overloads>
        // <summary> Initialisiert eine neue Instanz der OSCIException Klasse.
        // </summary>
        public OsciException()
        {
        }

        /// <summary> Erzeugt ein OSCIException-Objekt mit einem Fehlercode. 
        /// Als Fehlercode kann jeder String verwendet werden, f�r den
        /// ein entsprechender Eintrag in der zum gesetzten default-Locale geh�renden
        /// Sprachdatei (s. Package de.osci.osci12.extinterfaces.language) vorhanden ist.
        /// </summary>
        /// <param name="errorCode">errorCode
        /// </param>
        public OsciException(string errorCode)
        {
            _errorCode = errorCode;
        }

        /// <summary> Erzeugt ein OSCIException-Objekt mit einem erkl�renden String und
        /// einem Fehlercode. Als Fehlercode kann jeder String verwendet werden, f�r den
        /// ein entsprechender Eintrag in der zum gesetzten default-Locale geh�renden
        /// Sprachdatei (s. Package de.osci.osci12.extinterfaces.language) vorhanden ist.
        /// </summary>
        /// <param name="message">message
        /// </param>
        /// <param name="errorCode">errorCode
        /// </param>
        public OsciException(string message, string errorCode) 
            : base(message)
        {
            _errorCode = errorCode;
        }
    }
}