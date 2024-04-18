using Osci.MessageParts;

namespace Osci.Messagetypes
{
    /// <summary> Dieses Interface wird von allen Nachrichtentypen implementiert, die
    /// Inhaltsdaten enthalten.
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
    public interface IContentPackage
    {
        /// <summary>
        /// Liefert die Message-ID der Nachricht (Antwort). 
        /// </summary>
        /// <value>Message-ID 
        /// </value>
        string MessageId
        {
            get;
        }

        /// <summary>
        /// Liefert den im Antwortlaufzettel enthaltenen Betreff-Eintrag. 
        /// </summary>
        /// <value>Betreff der Antwortnachricht
        /// </value>
        string Subject
        {
            get;
        }

        /// <summary>
        /// Liefert die in die Nachricht eingestellten (unverschlüsselten) Inhaltsdaten als ContentContainer-Objekte.
        /// </summary>
        /// <value>enthaltene ContentContainer mit Inhaltsdaten
        /// </value>
        ContentContainer[] ContentContainer
        {
            get;
        }

        /// <summary>
        /// Liefert die in die Nachricht eingestellten verschlüsselten Inhaltsdaten 
        /// </summary>
        /// <value>enthaltene EncryptedData-Objekt mit verschlüsselten Inhaltsdaten
        /// </value>
        EncryptedDataOsci[] EncryptedData
        {
            get;
        }
    }
}