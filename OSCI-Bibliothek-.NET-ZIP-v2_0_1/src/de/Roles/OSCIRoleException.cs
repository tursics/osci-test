using Osci.Exceptions;

namespace Osci.Roles
{
    /// <summary> Diese Exception zeigt an, dass einem Rollenobjekt für eine unzulässige Operation
    /// verwendet wurde. <p>Beispiel: An die Methode ContentContainer.sign(Role) wird ein
    /// Role-Objekt übergeben, welches kein Signer-Objekt enthält.</p>
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
    public class OsciRoleException 
        : OsciException
    {

        public OsciRoleException(string message) 
            : base(message)
        {
        }

        public OsciRoleException(string message, string errorCode) 
            : base(message, errorCode)
        {
        }
    }
}