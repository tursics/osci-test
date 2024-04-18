using Osci.Exceptions;

namespace Osci.Signature
{
    /// <summary>Diese Exception zeigt ein Problem bei Signaturvorgängen an.
    /// Die Bibliothek faßt auch einige JCE/JCA-Exceptions hiermit zusammen, um 
    /// (aus Sicherheitsgründen) keine detaillierten Informationen über fehlgeschlagene
    /// Signierversuche zu liefern.
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

    public class OsciSignatureException 
        : OsciException
    {
        public OsciSignatureException(string message)
            : base(message)
        {
        }

        public OsciSignatureException(string message, string errorCode) 
            : base(message, errorCode)
        {
        }
    }
}