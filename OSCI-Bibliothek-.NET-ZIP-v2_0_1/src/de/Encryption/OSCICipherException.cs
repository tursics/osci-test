using Osci.Exceptions;

namespace Osci.Encryption
{
    /// <exclude/>
    /// <summary> Diese Exception zeigt ein Problem bei Ver- oder Entschlüsselungsvorgängen
    /// an. Die Bibliothek faßt auch einige JCE/JCA-Exceptions hiermit zusammen, um (aus
    /// Sicherheitsgründen) keine detaillierten Informationen über fehlgeschlagene
    /// Entschlüsselungsversuche zu liefern.
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
    public class OsciCipherException
        : OsciException
    {

        public OsciCipherException(string errorCode)
            : base(errorCode)
        {
        }
        public OsciCipherException(string message, string errorCode)
            : base(message, errorCode)
        {
        }
    }
}
