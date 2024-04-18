using System;
using System.IO;

namespace Osci.Interfaces
{
    /// <summary> Interface-Klasse für das Übermitteln von OSCI-Nachrichten
    /// Die OSCI-Bibliothek sieht nicht vor den Nutzer auf ein Transportprotokoll
    /// festzulegen. Aus diesem Grund wird ein Transport-Interface zur Verfügung
    /// gestellt welches es der Anwendung ermöglicht die erstellten OSCI-Nachrichten
    /// mit dem gewünschtem Protokoll oder auf die gewünschte Art zu übermitteln oder
    /// zu speichern. Vorstellbare Implementierungen sind z.B. http, https, ftp,
    /// smpt/pop, Filesystem, oder jms.
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
    public interface ITransport
    {

        /// <summary> Liefert die Versionsnummer.
        /// </summary>
        /// <value> Versionsnummer
        /// </value>
        string Version
        {
            get;
        }

        /// <summary> Liefert den Namen des Herstellers.
        /// </summary>
        /// <value> Herstellername
        /// </value>
        string Vendor
        {
            get;
        }

        /// <summary> Liefert den Response Stream.
        /// </summary>
        /// <value> InputStream der eingehenden Antwortdaten.
        /// </value>
        Stream ResponseStream
        {
            get;
        }

        /// <summary> Sollte die Länge des Response Streams liefern. Diese Methode wird
        /// von der Bibliothek z.Zt. nicht benötigt.
        /// </summary>
        /// <value> Anzahl der empfangenen Bytes.
        /// </value>
        long ContentLength
        {
            get;
        }

        /// <summary> Methode kann zur Überprüfung der Erreichbarkeit einer URL implementiert
        /// und verwendet werden. Die Bibliothek selbst ruft diese Methode nicht auf.
        /// </summary>
        /// <param name="uri">URI des Kommunikationspartners.
        /// </param>
        /// <returns>true, wenn der Kommunikationspartner erreichbar ist.
        /// </returns>
        bool IsOnline(Uri uri);

        /// <summary>  Liefert eine konkrete Verbindung zum Versenden eines Streams.
        /// Die Methode konnektet zu der übergebenen URI und liefert als Ergebnis
        /// einen Outputstream, in den die Bibliothek dann die serialisierte OSCI-Nachricht
        /// schreibt.
        /// </summary>
        /// <param name="uri">URI des Kommunikationspartners.
        /// </param>
        /// <param name="length">Länge der Übertragungsdaten (Anzahl der Bytes).
        /// </param>
        /// <returns>Output-Stream, in den die Daten geschrieben werden können.
        /// </returns>
        ITransport NewInstance();
        /** 
        * Die Implementierung dieser statischen Methode muss eine neue Instanz der Klasse 
        * zurückgeben. 
        * @return neue Instanz der implementierenden Klasse 
        * @throws IOException wenn ein Fehler auftritt 
        */
        Stream GetConnection(Uri uri, long length);
    }
}