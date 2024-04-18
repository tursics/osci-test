using Osci.Common;
using Osci.SoapHeader;

namespace Osci.Interfaces
{
    /// <summary> <p>Clients, die mehrere Dialoge gleichzeitig verwalten müssen,
    /// können zu diesem Zweck mit der statischen Methode
    /// DialogHandler.setDialogFinder(de.osci.osci12.common.DialogFinder)
    /// eine Instanz dieser Klasse installieren.</p>
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
    /// <seealso cref="DialogHandler">
    /// </seealso>
    public abstract class DialogFinder
    {

        /// <summary> Liefert eine Versionsnummer.
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

        /// <summary> Diese Methode muß anhand des übergebenen ControlBlockH-Objektes das
        /// zugehörige DialogHandler-Objekt ermitteln und zurückgeben.
        /// </summary>
        /// <param name="controlBlock"> das ControlBlock-Objekt als Identifier des 
        /// gesuchten DialogHandlers
        /// </param>
        /// <returns> gefundener DialogHandler oder null, wenn er nicht gefunden wurde.
        /// </returns>
        public abstract DialogHandler FindDialog(ControlBlockH controlBlock);

        /// <summary> Entfernt ein DialogHandler-Objekt aus der Liste der verwalteten
        /// DialogHandler-Objekte. Wird nach Beendigung eines Dialogs aufgerufen.
        /// </summary>
        /// <param name="controlBlock"> das ControlBlock-Objekt als Identifier des 
        /// gesuchten DialogHandlers
        /// </param>
        /// <returns> true, wenn der DialogHandler erfolgreich entfernt wurde.
        /// </returns>
        public abstract bool RemoveDialog(ControlBlockH controlBlock);

        /// <summary> Fügt den verwalteten DialogHandler-Objekten ein weiteres hinzu.
        /// </summary>
        /// <param name="dialog">dialog
        /// </param>
        public abstract void AddDialog(DialogHandler dialog);
    }
}