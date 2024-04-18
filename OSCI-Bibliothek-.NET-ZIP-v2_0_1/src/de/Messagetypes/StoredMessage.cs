using System;
using System.IO;
using Osci.Exceptions;
using Osci.MessageParts;
using Osci.Roles;

namespace Osci.Messagetypes
{
    /// <summary> <p>Eine Instanz dieser Klasse wird beim Einlesen einer serialisierten
    /// OSCI-Nachricht beliebigen Typs (Nachrichten mit Inhaltsdaten) angelegt.</p>
    /// Die Klasse dient folgenden Zwecken:<ul><li>Abspeichern und Wiedereinlesen von
    /// Nachrichten mit Inhaltsdaten</li><li>Austausch von Inhaltsdaten zwischen
    /// Autoren und Sendern bzw. Empfänger und Lesern.</li></ul>
    /// <p>
    /// Leser können ihre Inhaltsdaten in Nachrichten beliebigen Typs ablegen, diese
    /// speichern und z.B. als Datei weiterreichen. Absender können eine solche Datei
    /// mit der Methode loadMessage(InputStream) laden, die Inhaltdatencontainer
    /// entnehmen und anderen Nachrichten hinzufügen.</p>
    /// <p>
    /// Ein generelles Problem ist, dass in verschlüsselten Inhaltsdaten einer
    /// OSCI-Nachricht die Informationen über die enthaltenen Referenzen auf
    /// Zertifikate und Attachments ohne Entschlüsselung nicht verfügbar sind.
    /// Im Zweifel müssen daher alle Zertifikate (z.B. mit den Methoden
    /// OSCIMessage.getOtherAuthors(), OSCIMessage.getOtherReaders() und
    /// OSCIMessage.addRole(Role)) und Attachments (Methoden
    /// exportAttachment(OSCIMessage, Attachment) und exportAttachments(OSCIMessage))
    /// entnommen und der neuen Nachricht hinzugefügt werden.</p>
    /// <p>
    /// Anwendungen sollten dies berücksichtigen und für den Inhaltsdatenaustausch
    /// möglichst mehrere einzelne Nachrichten statt einer komplexen verwenden.
    /// Besonders problematisch ist in diesem Zusammenhang die Signatur durch
    /// Originator- bzw. Verschlüsselung für Addressee-Rollenobjekte, weil diese
    /// i.d.R. nicht in eine neue Nachricht übernommen werden können. Hier sollten
    /// grundsätzlich Author- und Reader-Objekte verwendet werden.</p>
    /// <p>
    /// Weiter ist zu beachten, dass es beim Zusammensetzen einer neuen Nachricht aus
    /// Inhaltdatencontainern, die anderen Nachrichten entnommen wurden, zu Konflikten
    /// mit den Ref-Ids der Content-Einträge kommen kann. Da die Bibliothek wegen der
    /// ggf. vorhandenen Signatur diese Ids nicht selbst anpassen kann, sollten
    /// Anwendungen eindeutige Ref-Ids setzen. Diese können z.B. aus Message-Ids
    /// und laufenden Nummern oder Zertfikats-Ids (z.B. IssuerDN und SerialNumber)
    /// und Datum/Uhrzeit generiert werden.</p>
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
    public class StoredMessage
        : OsciResponseTo
    {
        internal ProcessCardBundle[] processCardBundles;
        internal ProcessCardBundle processCardBundleReply;
        internal string subject;
        internal Uri uriReceiver;
        internal string selectionRule;
        internal int selectionMode = -1;
        internal long quantityLimit = -1;

        /// <summary> Diese Methode liefert den Laufzettel der Nachricht zurück.
        /// Im Falle einer Abwicklungsantwort wird der Laufzettel des Requests
        /// zurückgegeben.
        /// </summary>
        /// <value> Laufzettel des Auftrags als ProcessCardBundle-Objekt oder null,
        /// wenn der Nachrichtentyp keinen Laufzettel enthält.
        /// </value>
        /// <seealso cref="ProcessCardBundleReply">
        /// </seealso>
        /// <seealso cref="ResponseToMediateDelivery.ProcessCardBundleRequest()">
        /// </seealso>
        public ProcessCardBundle ProcessCardBundle
        {
            get
            {
                return processCardBundles == null ? null : processCardBundles[0];
            }
        }

        /// <summary> Diese Methode liefert die Laufzettel einer Laufzettelabholantwort zurück.
        /// </summary>
        /// <value> Laufzettel des Auftrags als ProcessCardBundle-Objekt.
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        /// <seealso cref="ResponseToFetchProcessCard.ProcessCardBundles()">
        /// </seealso>
        /// <exception cref="System.Exception">Im Fehlerfall
        /// </exception>
        public ProcessCardBundle[] ProcessCardBundles
        {
            get
            {
                if (MessageType != ResponseToFetchProcessCard)
                {
                    throw new Exception("Funktion wird nicht unterstützt. (ProcessCardBundles)");
                }

                return processCardBundles;
            }
        }

        /// <summary> Diese Methode liefert den Antwort-Laufzettel einer Abwicklungsantwort zurück.
        /// </summary>
        /// <value> Laufzettel der Nachricht als ProcessCardBundle-Objekt.
        /// </value>
        /// <seealso cref="ProcessCardBundle">
        /// </seealso>
        /// <exception cref="System.Exception">Im Fehlerfall
        /// </exception>
        public ProcessCardBundle ProcessCardBundleReply
        {
            get
            {
                if (MessageType != ResponseToMediateDelivery)
                {
                    throw new Exception("Funktion wird nicht unterstützt. (ProcessCardBundleReply)");
                }

                return processCardBundleReply;
            }
        }

        /// <summary> Liefert die Message-Id der Nachricht.
        /// Im Falle einer Abwicklungsantwort wird die Message-Id des Requests
        /// zurückgegeben.
        /// </summary>
        /// <value> Message-ID
        /// </value>
        public string MessageId
        {
            get
            {
                return ((OsciMessage) this).MessageId;
            }
        }

        /// <summary> Liefert das Intermediäresobjekt oder null, wenn keine Zertifikate in der
        /// Nachricht enthalten sind.
        /// </summary>
        /// <value> Intermediär
        /// </value>
        public Intermed Intermediary
        {
            get
            {
                if (DialogHandler.Supplier is Intermed)
                {
                    return (Intermed)DialogHandler.Supplier;
                }
                else
                {
                    return (Intermed)DialogHandler.Client;
                }
            }
        }

        /// <summary> Liefert den Betreff der Nachricht oder null, wenn kein Betreff in der
        /// Nachricht enthalten ist.
        /// </summary>
        /// <value> Betreff
        /// </value>
        public string Subject
        {
            get
            {
                return subject;
            }
        }

        /// <summary> Liefert die URI des Nachrichtenempfängers oder null, wenn keine Empfänger-URI
        /// in der Nachricht enthalten ist.
        /// </summary>
        /// <value> URI
        /// </value>
        public Uri UriReceiver
        {
            get
            {
                return uriReceiver;
            }
        }

        /// <summary> Liest eine (unverschlüsselte) Nachricht aus dem übergebenen Stream. Die in
        /// der Nachricht enthaltenen Inhalte können dem zurückgegebenen
        /// Nachrichtenobjekt entnommen werden.
        /// </summary>
        /// <param name="input">zu lesender Stream.
        /// </param>
        /// <returns> StoredMessage-Instanz
        /// </returns>
        /// <seealso cref="StoreMessage">
        /// </seealso>
        public static StoredMessage LoadMessage(Stream input)
        {
            return new StoredMessageParser().ParseStream(input);
        }

        /// <summary> Serialisiert die übergebene Nachricht und schreibt die Daten in den übergebenen Stream.
        /// </summary>
        /// <param name="msg">zu speichernde Nachricht; z.Zt. können StoredMessage-Objekte selbst nicht
        /// gespeichert werden.
        /// </param>
        /// <param name="output">Ausgabestream
        /// </param>
        /// <seealso cref="LoadMessage">
        /// </seealso>
        /// <exception cref="System.Exception">Im Fehlerfall
        /// </exception>
        public static void StoreMessage(OsciMessage msg, Stream output)
        {
            if (msg is StoredMessage)
            {
                throw new Exception("Funktion wird nicht unterstützt. (storeMessage)");
            }
            msg.WriteXml(output);
        }

        /// <summary>Liest eine (unverschlüsselte) Nachricht aus dem übergebenen Stream. Die in
        /// der Nachricht enthaltenen Inhalte können dem zurückgegebenen
        /// Nachrichtenobjekt entnommen werden. Diese Methode prüft außerdem 
        /// eine ggf. vorhandene Nachrichtensignatur. Liefert die Prüfung ein 
        /// negatives Ergebnis, wird eine OSCIException (Code 9601) geworfen.
        /// Die Methode prüft keine Inhaltsdatensignaturen. Ebenso wird
        /// keine Exception ausgelöst, wenn die Nachricht unsigniert ist.
        /// </summary>
        /// <param name="input">zu lesender Stream
        /// </param>
        /// <param name="output">Ausgabestream
        /// </param>
        /// <returns> StoredMessage-Instanz
        /// </returns>
        /// <seealso cref="LoadMessage">
        /// </seealso>
        /// <exception cref="System.IO.IOException"> bei Schreibfehlern
        /// </exception>
        /// <exception cref="OsciException"> bei Problemen beim Aufbau der Nachricht oder ungültiger Nachrichtensignatur
        /// </exception>
        /// <exception cref="OsciException"> bei Problemen beim Aufbau der Nachricht oder ungültiger Nachrichtensignatur
        /// </exception>

        public static StoredMessage LoadMessageCheckingSignature(Stream input)
        {
            StoredMessageParser parser = new StoredMessageParser();
            StoredMessage storedMessage = parser.ParseStream(input);
            if (storedMessage.IsSigned)
            {
                if (storedMessage.MessageType <= 0x10)
                {
                    storedMessage.DialogHandler.Supplier.SignatureCertificate = storedMessage.DialogHandler.Client.SignatureCertificate;
                }

                if (!parser.CheckMessageHashes(storedMessage))
                {
                    throw new OsciException("9601");
                }
            }
            return storedMessage;
        }

        /// <summary>Diese Methode exportiert alle Attachments der Nachricht in eine andere
        /// OSCI-Nachricht beliebigen Typs.
        /// </summary>
        /// <param name="destinationMessage">Zielnachricht
        /// </param>
        public void ExportAttachments(OsciMessage destinationMessage)
        {
            Attachment[] atts = Attachments;

            for (int i = 0; i < atts.Length; i++)
            {
                destinationMessage.AddAttachment(atts[i]);
            }
        }

        /// <summary>Diese Methode exportiert ein Attachment der Nachricht in eine andere
        /// OSCI-Nachricht beliebigen Typs.
        /// </summary>
        /// <param name="destinationMessage">Zielnachricht
        /// </param>
        /// <param name="att">zu exportierendes Attachment
        /// </param>
        public void ExportAttachment(OsciMessage destinationMessage, Attachment att)
        {
            Attachment[] atts = Attachments;

            for (int i = 0; i < atts.Length; i++)
            {
                if (att.Equals(atts[i]))
                {
                    destinationMessage.AddAttachment(atts[i]);
                    return;
                }
            }
        }

        /// <summary> Liefert die Qualität des Zeitstempels, mit dem der Intermediär den
        /// Eingang des Auftrags im Laufzettel protokolliert.
        /// </summary>
        /// <value> Qualität des Zeitstempels:
        /// <p> <b>true</b> - kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst.</p>
        /// <p> <b>false</b> - Einfacher Zeitstempel
        /// (lokale Rechnerzeit des Intermedärs).</p>
        /// </value>
        /// <seealso cref="QualityOfTimeStampReception()">
        /// </seealso>
        /// <exception cref="System.Exception">Im Fehlerfall
        /// </exception>
        public bool QualityOfTimeStampCreation
        {
            get
            {
                if ((MessageType != StoreDelivery) && (MessageType != ForwardDelivery) && (MessageType != MediateDelivery) && (MessageType != ResponseToProcessDelivery))
                {
                    throw new Exception("Funktion wird nicht unterstützt. (QualityOfTimeStampCreation)");
                }
                return QualityOfTimestampTypeCreation.QualityCryptographic;
            }
        }

        /// <summary> Liefert die geforderte Qualität des Zeitstempels, mit dem der Intermediär den
        /// Empfang der Annahmeantwort im Laufzettel protokolliert.
        /// </summary>
        /// <value> Qualität des Zeitstempels: 
        /// <p> <b>true</b> - kryptographischer Zeitstempel von einem
        /// akkreditierten Zeitstempeldienst.</p>
        /// <p> <b>false</b> - Einfacher Zeitstempel
        /// (lokale Rechnerzeit des Intermedärs).</p>
        /// </value>
        /// <seealso cref="QualityOfTimeStampCreation()">
        /// </seealso>
        /// <exception cref="System.Exception">Im Fehlerfall
        /// </exception>
        public bool QualityOfTimeStampReception
        {
            get
            {
                if ((MessageType != StoreDelivery) && (MessageType != ForwardDelivery) && (MessageType != MediateDelivery) && (MessageType != ResponseToProcessDelivery))
                {
                    throw new Exception("Funktion wird nicht unterstützt. (QualityOfTimeStampReception)");
                }
                return QualityOfTimestampTypeReception.QualityCryptographic;
            }
        }

        /// <summary> Liefert den gesetzten Auswahlmodus für Nachrichten oder Laufzettel.
        /// </summary>
        /// <value> Auswahlmodus SELECT_BY_MESSAGE_ID, SELECT_BY_DATE_OF_RECEPTION
        /// oder NO_SELECTION_RULE
        /// </value>
        /// <seealso cref="SelectionRule()">
        /// </seealso>
        /// <exception cref="System.Exception">Im Fehlerfall
        /// </exception>
        public int SelectionMode
        {
            get
            {
                if ((MessageType != FetchDelivery) && (MessageType != FetchProcessCard) && (MessageType != ResponseToFetchDelivery) && (MessageType != ResponseToFetchProcessCard))
                {
                    throw new Exception("Funktion wird nicht unterstützt. (SelectionMode)");
                }
                return selectionMode;
            }
        }

        /// <summary> Liefert die gesetzte Auswahlregel für Nachrichten oder Laufzettel.
        /// Wurde keine Regel gesetzt, wird als default null zurückgegeben.
        /// </summary>
        /// <value> Auswahlregel (Message-Id oder Datum)
        /// </value>
        /// <seealso cref="SelectionMode()">
        /// </seealso>
        /// <exception cref="System.Exception">Im Fehlerfall
        /// </exception>
        public string SelectionRule
        {
            get
            {
                if ((MessageType != FetchDelivery) && (MessageType != FetchProcessCard) && (MessageType != ResponseToFetchDelivery) && (MessageType != ResponseToFetchProcessCard))
                {
                    throw new Exception("Funktion wird nicht unterstützt. (SelectionRule)");
                }
                return selectionRule;
            }
        }

        /// <summary> Liefert die maximale Anzahl zurückzugebender Laufzettel.
        /// </summary>
        /// <value> gesetzte maximale Anzahl.
        /// </value>
        /// <exception cref="System.Exception">Im Fehlerfall
        /// </exception>
        public long QuantityLimit
        {
            get
            {
                if ((MessageType != FetchProcessCard) && (MessageType != ResponseToFetchProcessCard))
                {
                    throw new Exception("Funktion wird nicht unterstützt. (QuantityLimit)");
                }
                return quantityLimit;
            }
        }

        internal StoredMessage(int messageType)
        {
            MessageType = messageType;
        }
    }
}