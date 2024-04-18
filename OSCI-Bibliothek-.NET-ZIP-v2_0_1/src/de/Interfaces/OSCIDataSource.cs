using System.IO;
using Osci.Common;

namespace Osci.Interfaces
{
    /// <summary> <p>Implementierungen dieser Klasse können mit Hilfe der Methode
    /// setDataBuffer(OSCIDataSource buffer) des DialogHandlers installiert
    /// werden, falls Inhaltsdaten nicht durch die default-Implementierung
    /// SwapBuffer im Arbeitsspeicher bzw. in temporären Dateien gepuffert werden
    /// sollen, sondern beispielsweise in einer Datenbank.</p>
    /// Dieser Puffer-Mechanismus wird von den Klassen EncryptedData, Content und Attachment
    /// genutzt.
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
    /// <seealso cref="SwapBuffer">
    /// </seealso>
    public abstract class OsciDataSource
    {

        /// <summary> Die Implementierung dieser Methode muss einen OutputStream liefern, in den
        /// die zu puffernden Daten geschrieben werden können.
        /// </summary>
        /// <value> OutputStream
        /// </value>
        public abstract Stream OutputStream
        {
            get;
        }

        /// <summary> Die Implemetierung dieser Methode muß einen InputStream liefern, aus dem
        /// die gepufferten Daten gelesen werden können. Der erste Aufruf dieser
        /// Methode beendet den Schreibvorgang in diesen Puffer.
        /// <b>Achtung:</b> Der zurückgegebene InputStream muß die reset()-Methode
        /// in der Weise implementieren, daß nach deren Aufruf wieder von vorn ab dem
        /// ersten Byte gelesen wird. Die markSupported()-Methode muß
        /// <b>false</b> zurückliefern.
        /// </summary>
        /// <value> InputStream
        /// </value>
        public abstract Stream InputStream
        {
            get;
        }

        /// <summary> Diese Methode muß die Anzahl der in den Puffer geschriebenen Bytes
        /// zurückgeben.
        /// </summary>
        /// <value> Anzahl der Bytes
        /// </value>
        public abstract long Length
        {
            get;
        }

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

        /// <summary> Die Implementierung dieser statischen Methode muss eine neue Instanz der Klasse
        /// zurückgeben.
        /// </summary>
        /// <returns> neue Instanz der implementierenden Klasse
        /// </returns>
        public abstract OsciDataSource NewInstance();
    }
}