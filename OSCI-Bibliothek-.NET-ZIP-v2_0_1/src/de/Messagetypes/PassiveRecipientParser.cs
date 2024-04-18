using System.IO;
using Osci.Common;
using Osci.Helper;

namespace Osci.Messagetypes
{
    /// <summary> Die Klasse ist der Einrittspunkt für Nachrichten, die bei einem passiven Empfänger
    /// eingehen.
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
    public class PassiveRecipientParser
        : IncomingMessageParser
    {
        protected override OsciEnvelopeBuilder GetParser(XmlReader reader, DialogHandler dh)
        {
            return new OsciEnvelopeBuilder(reader, dh);
        }

        /// <summary> Diese Methode parst eine Nachricht, die über einen InputStream eingeht.
        /// Sie wird in der Regel von einem Servlet (o.ä.) aufgerufen.
        /// </summary>
        /// <param name="input">InputStream der eingehenden Daten.
        /// </param>
        /// <returns> die eingelesene OSCI-Auftragsnachricht (Annahme- oder Bearbeitungsauftrag).
        /// </returns>
        public virtual OsciRequest ParseStream(Stream input)
        {
            DefaultSupplier = DialogHandler.DefaultSuppliers;
            return (OsciRequest)ParseStream(input, null, true, null);
        }

        /// <summary> Diese Methode baut eine SOAP-Fehlermeldung auf Nachrichtenebene auf.
        /// </summary>
        /// <param name="code"> OSCI-Code des Fehlers
        /// </param>
        /// <returns> SOAP-Nachricht
        /// </returns>
        public static string CreateSoapError(string code)
        {
            return "SOAP-Error";
        }
    }
}