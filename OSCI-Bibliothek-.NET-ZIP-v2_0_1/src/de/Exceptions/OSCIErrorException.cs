using Osci.Messagetypes;

namespace Osci.Exceptions
{
    /// <summary> Diese Exception wird für die in der OSCI 1.2 Transport-Spezifikation
    /// definierten Fehlermeldungen verwendet.
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
    public class OsciErrorException
        : OsciException
    {
        public OsciMessage OsciMessage
        {
            get; private set;
        }

        public OsciErrorException(string errorCode)
            : base(errorCode)
        {
        }

        public OsciErrorException(string errorCode, OsciMessage osciMessage)
            : base(errorCode)
        {
            OsciMessage = osciMessage;
        }
    }
}