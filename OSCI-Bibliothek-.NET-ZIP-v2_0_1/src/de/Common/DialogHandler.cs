using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Resources;
using Osci.Roles;
using Osci.SoapHeader;
using System;
using System.Globalization;

namespace Osci.Common
{
    /// <summary> Der DialogHandler steuert die Kommunikation mit dem Intermediär.
    /// Für die Kommunikation mit dem Intermediär müssen eine Reihe von Rahmenparametern
    /// gesetzt werden. Daher ist diese Klasse <b>zentral</b> für jede Kommunikation.
    /// Ein DialogHandler-Objekt ist für jede Nachricht erforderlich, unabhängig davon,
    /// ob diese innerhalb eines impliziten oder expliziten Dialogs verarbeitet wird.
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
    public class DialogHandler
    {
        #region Static

        /// <summary> Liefert den Hash-Algorithmus für die Signatur der Nachrichten
        /// oder setzt ihn fest (Voreinstellung SHA256).
        /// Der hier gesetzte Algorithmus wird bei RSA-OAEP-Verschlüsselung
        /// ebenfalls für die Hashwert- und Maskenerzeugung verwendet.
        /// </summary>
        /// <value>Identifier des Hash-Algorithmus
        /// </value>
        public static string DigestAlgorithm
        {
            get; set;
        }

        /// <summary> Diese Eigenschaft ist noch für die Abwärtskompatibilität vorhanden,
        /// wurde ersetzt durch DefaultSuppliers
        /// </summary>
        /// <value>Rollenobjekt
        /// </value>
        public static Addressee DefaultSupplier
        {
            set
            {
                DefaultSuppliers = new Addressee[1];
                DefaultSuppliers[0] = value;
            }
            get
            {
                if (DefaultSuppliers == null)
                {
                    throw new Exception("No default supplier configured.");
                }
                return DefaultSuppliers[0];
            }
        }

        /// <summary> Passive Empfänger müssen Addressee-Objekte als Default-Supplier setzen,
        /// die für den Empfang von Nachrichten die richtigen Decrypter- und gegebenenfalls
        /// Signer-Objekte mit den Privatschlüsseln zur Verfügung stellen.
        /// </summary>
        /// <value>Rollenobjekte
        /// </value>
        public static Addressee[] DefaultSuppliers
        {
            set; get;
        }

        /// <summary> Registriert eine Instanz (einer Implementierung) der abstrakten Klasse
        /// DialogFinder zur Verwendung in diesem Dialog. Voreingestellt ist eine Instanz
        /// der Default-Implementierung de.osci.osci12.extinterfaces.DialogFinder.
        /// </summary>
        /// <value>dialogFinder
        /// </value>
        /// <seealso cref="Interfaces.DialogFinder">
        /// </seealso>
        public static DialogFinder DialogFinder
        {
            private get; set;
        }

        internal static OsciDataSource NewDataBuffer
        {
            get
            {
                return _dataBuffer.NewInstance();
            }
        }

        /// <summary>ResourceBundle-Objekt, welches bibliotheksweit für die Textausgaben verwendet wird.</summary>
        public static ResourceBundle ResourceBundle
        {
            get; private set;
        }

        /// <summary>Der Trenner für die einzelnen MIME boundaries.</summary>
        public static string Boundary
        {
            get; internal set;
        }

        private static OsciDataSource _dataBuffer;
        private static readonly Log _log;

        #endregion Static

        #region Public Properties

        /// <summary> Liefert eine Wert, der festlegt, ob Nachrichtensignaturen (Client-/Suppliersignaturen)
        /// beim Empfang geprüft werden sollen oder legt ihn fest. Voreinstellung ist <b>true</b>.
        /// <p><b>Achtung: </b>Diese Eigenschaft legt lediglich fest, ob vorhandene
        /// Signaturen eingehender Nachrichten mathematisch geprüft werden. Fehlt eine
        /// Signatur ganz, wird keine Exception o.ä. ausgelöst. Es sollte daher
        /// zusätzlich OSCIMessage.isSigned() geprüft werden.</p>
        /// </summary>
        /// <value><b>true</b> - Signaturprüfung wird durchgeführt.
        /// </value>
        public bool CheckSignatures
        {
            get; set;
        }

        /// <summary> Liefert eine Wert, der festlegt, ob Nachrichtensignaturen (Client-/Suppliersignaturen)
        /// beim Versand angebracht werden sollen oder legt ihn fest. Die Voreinstellung ist <b>true</b>.
        /// </summary>
        /// <value><b>true</b> -> ausgehende Nachrichten werden signiert.
        /// </value>
        /// #isCreateSignatures()
        // <seealso cref="CreateSignatures()">
        // </seealso>
        public bool CreateSignatures
        {
            get; set;
        }

        /// <summary> Liefert die installierte Implementierung des Transportinterfaces.
        /// </summary>
        /// <value> Transportinterface-Implementierung.
        /// </value>
        /// <seealso cref="ITransport">
        /// </seealso>
        public ITransport TransportModule
        {
            get;
        }

        /// <summary>Liefert eine Wert, der festlegt, ob die Nachrichten als verschlüsselte Auftragsdaten versendet
        /// werden oder legt ihn fest. Voreinstellung ist <b>true</b>.
        /// </summary>
        /// <value><b>true</b> -> ausgehende Nachrichten werden verschlüsselt.
        /// </value>
        public bool Encryption
        {
            get; set;
        }

        /// <summary> Liefert den aktuellen Controlblock. Ein Controlblock beinhaltet
        /// Challenge, Response, ConversationID und SequenzeNumber.
        /// </summary>
        /// <value> ControlBlock dieses Dialogs.
        /// </value>
        public ControlBlockH Controlblock
        {
            get; private set;
        }

        /// <summary> Liefert den mit diesem DialogHandler verbundenen Client.
        /// </summary>
        /// <value> das Rollenobjekt, welches als Client fungiert.
        /// </value>
        public Role Client
        {
            get; internal set;
        }

        /// <summary> Liefert den mit diesem DialogHandler verbundenen Supplier.
        /// </summary>
        /// <value> das Rollenobjekt, welches als Supplier fungiert
        /// </value>
        public Role Supplier
        {
            get; internal set;
        }

        /// <summary> Liefert die Liste der Sprachkürzel, die in den DesiredLanguages-Elementen
        /// eingetragen wird oder setzt sie. Voreingestellt ist das Kürzel der im default-CurrentCulture
        /// eingetragenen Sprache.
        /// </summary>
        /// <value>Liste der Sprachkürzel, getrennt durch Leerzeichen, z.B. "de en-US fr".
        /// </value>
        public string LanguageList
        {
            get; set;
        }

        /// <summary> Liefert den eingestellten Algorithmus für den verwendeten
        /// Zufallszahlengenerator oder setzt ihn. Dieser String wird von der Bibliothek bei der
        /// Initialisierung an die Methode java.security.SecureRandom#getInstance(String)
        /// übergeben. Voreingestellt ist "SHA1PRNG".
        /// </summary>
        /// <value>String-Identifier für den Algorithmus, der von dem installierten
        /// Provider unterstützt werden muß.
        /// </value>
        // <seealso cref="">java.security.SecureRandom
        // </seealso>
        public string SecureRandomAlgorithm
        {
            get; set;
        }

        /// <summary> Liefert/Setzt den Identifier des Signaturalgorithmus, der für die
        /// Signatur der Nachrichten (verschlüsselte Auftragsdaten) verwendet wird, sofern
        /// die Signer-Implementierung keinen anderen Algorithmus verlangt.
        /// </summary>
        /// <value> Identifier
        /// </value>
        /// #setSignatureAlgorithm(String)
        /// <seealso cref="SignatureAlgorithm()">
        /// </seealso>
        public static string SignatureAlgorithm
        {
            get; set;
        }

        /// <summary><p>Hiermit kann eine Implementierung der abstrakten Klasse OSCIDataSource
        /// installiert werden, falls Inhaltsdaten nicht durch die default-Implementierung
        /// SwapBuffer im Arbeitsspeicher bzw. in temporären Dateien gepuffert werden
        /// sollen, sondern beispielsweise in einer Datenbank.</p>
        /// Dieser Puffer-Mechanismus wird von den Klassen EncryptedData, Content und Attachment
        /// genutzt. Zur Implementierung eigener Klassen sind die Hinweise in der
        /// Dokumentation von OSCIDataSource zu beachten.
        /// </summary>
        /// <seealso cref="OsciDataSource">
        /// </seealso>
        /// <seealso cref="SwapBuffer">
        /// </seealso>
        /// <value>die OSCIDataSource-Implementierung
        /// </value>
        public static OsciDataSource DataBuffer
        {
            set
            {
                _dataBuffer = value;
            }
        }

        /// <summary> Liefert den symmetrischen Verschlüsselungs-Algorithmus.
        /// </summary>
        /// <value>Identifier des Algorithmus
        /// </value>
        public SymmetricCipherAlgorithm SymmetricCipherAlgorithm
        {
            get; set;
        }

        /// <summary> Liefert die Länge des Initialisierungsvektors (in Byte) für die Transportverschlüsselung bei Nutzung von AES-GCM.
        /// </summary>
        /// <value>Länge in Byte
        /// </value>
        public int IvLength
        {
            get; set;
        }

        /// <summary> Liefert den asymmetrischen Verschlüsselungs-Algorithmus.
        /// </summary>
        /// <value>Identifier des Algorithmus
        /// </value>
        public AsymmetricCipherAlgorithm AsymmetricCipherAlgorithm
        {
            get; set;
        }

        /// <summary>Speichert den Challenge-Wert einer vorangegangenen Response-Nachricht.
        /// Wird von Anwendungen normalerweise nicht benötigt und sollte auch nicht gesetzt werden.
        /// </summary>
        public string PreviousChallenge
        {
            get; private set;
        }

        private bool sendFeatureDescription = true;

        public bool SendFeatureDescription
        {
            get
            {
                if ("false".Equals(System.Environment.GetEnvironmentVariable("de.osci.SendFeatureDescription")))
                {
                    return false;
                }
                else if ("true".Equals(System.Environment.GetEnvironmentVariable("de.osci.SendFeatureDescription")))
                {
                    return true;
                }
                return sendFeatureDescription;
            }
            set
            {
                this.sendFeatureDescription = value;
            }
        }

        #endregion Public Properties

        // CipherCert immer mitschicken. Auch wenn nicht verschlüsselt wird.
        // Diese Var wird nur beim Intermediär ausgewertet
        /// <summary> Status impliziter/expliziter Dialog. Sollte von Anwendungen nicht gesetzt werden.
        /// </summary>
        internal bool ExplicitDialog
        {
            get; set;
        }

        #region c'tor

        static DialogHandler()
        {
            Boundary = "MIME_boundary";
            ResourceBundle = ResourceBundle.GetBundle("Text", CultureInfo.CurrentCulture);
            _log = LogFactory.GetLog(typeof(DialogHandler));
            _dataBuffer = new SwapBuffer();
            DigestAlgorithm = Constants.DigestAlgorithmSha256;
            SignatureAlgorithm = Constants.SignatureAlgorithmRsaSha256;
        }

        private DialogHandler()
        {
            CreateSignatures = true;
            Encryption = true;
            ExplicitDialog = false;

            CheckSignatures = true;
            SecureRandomAlgorithm = Constants.SecureRandomAlgorithmSha1;
            Controlblock = new ControlBlockH();
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            LanguageList = cultureInfo.TwoLetterISOLanguageName;
            SymmetricCipherAlgorithm = Constants.DefaultSymmetricCipherAlgorithm;
            AsymmetricCipherAlgorithm = Constants.DefaultAsymmetricCipherAlgorithm;
            IvLength = Constants.DefaultGcmIVLength;
        }

        /// <summary> DialogHandler für alle Aufträge/Auftragsantworten,
        /// ausgenommen Annahmeauftrag/-antwort und Bearbeitungsauftrag/-antwort.
        /// </summary>
        /// <param name="client">Sollen Aufträge signiert versendet werden,
        /// oder werden verschlüsselte Rückantworten erwartet, so muss für dieses
        /// Objekt ein Decrypter- bzw. Signer-Objekt gesetzt sein. Dies gilt für die
        /// Verschlüsselung bzw. Signatur der Nachricht wie für die der Inhaltsdaten.
        /// </param>
        /// <param name="supplier">Intermediaer als Supplier.
        /// </param>
        /// <param name="transportModule">zu verwendende Implementierung des TransportInterfaces.
        /// </param>
        /// <seealso cref="Originator">
        /// </seealso>
        public DialogHandler(Originator client, Intermed supplier, ITransport transportModule)
            : this()
        {
            Supplier = supplier;
            if (supplier == null && DefaultSuppliers != null)
            {
                Supplier = DefaultSuppliers[0];
            }
            TransportModule = transportModule;
            Client = client;
        }

        /// <summary>  DialogHandler für Annahmeauftrag/-antwort und Bearbeitungsauftrag/-antwort.
        /// Dieser Konstruktor wird vom Anwender nicht benötigt.
        /// </summary>
        /// <param name="client">Intermediär als Client.
        /// </param>
        /// <param name="supplier">Addressee als Supplier.
        /// </param>
        /// <param name="transportModule">Implementierung des TransportInterfaces
        /// </param>
        /// <seealso cref="Addressee">
        /// </seealso>
        public DialogHandler(Intermed client, Addressee supplier, ITransport transportModule)
            : this()
        {
            Supplier = supplier;
            if ((supplier == null) && (DefaultSuppliers != null))
            {
                Supplier = DefaultSuppliers[0];
            }
            TransportModule = transportModule;
            Client = client;
        }

        #endregion c'tor

        internal void CheckControlBlock(ControlBlockH cb)
        {
            _log.Debug("!!!!!!!RSP!!!!!!!!! " + Controlblock.Response + " : " + cb.Response);
            _log.Debug("!!!!!!!CHALL!!!!!!!!! " + Controlblock.Challenge + " : " + cb.Challenge);
            _log.Debug("!!!!!!!ConvID!!!!!!!!! " + Controlblock.ConversationId + " : " + cb.ConversationId);
            _log.Debug("!!!!!!!SQU!!!!!!!!! " + Controlblock.SequenceNumber + " : " + cb.SequenceNumber);

            if (Controlblock.Challenge != null)
            {
                if (!cb.Response.Equals(Controlblock.Challenge))
                {
                    throw new OsciErrorException("9400");
                }
            }

            PreviousChallenge = cb.Challenge;

            if (Controlblock.ConversationId != null)
            {
                // der folgende Fall kann eigentlich nicht eintreten, weil der ControlBlock anhand der ConvId gesucht wurde
                if (!Controlblock.ConversationId.Equals(cb.ConversationId))
                {
                    throw new OsciErrorException("9400");
                }
            }
            else
            {
                Controlblock.ConversationId = cb.ConversationId;
            }
            if ((cb.SequenceNumber > -1) && (cb.SequenceNumber != Controlblock.SequenceNumber))
            {
                throw new OsciErrorException("9400");
            }

            Controlblock.SequenceNumber = cb.SequenceNumber;
            Controlblock.Challenge = cb.Challenge;
            Controlblock.Response = cb.Response;
        }

        /// <summary> Setzt den ControlBlock zurück. Erlaubt die Wiederverwendung dieses
        /// Objekts in einem neuen Dialog.
        /// </summary>
        public void ResetControlBlock()
        {
            Controlblock = new ControlBlockH();
        }

        /// <summary>
        /// Diese Methode liefert den in einem Dialog schon benutzen DialogHandler.
        /// </summary>
        /// <param name="controlBlock">
        /// Control-Block nach dem der evt.schon bestehende Dialog gefunden werden soll.
        /// Hierbei wird als erstes ein bestehender expliziter Dialog d.h.ConversationId ist vorhanden
        /// kontrolliert ansonsten eine Bestehender Response Wert untersucht.
        /// </param>
        /// <returns>
        /// Sollte es sich um einen expliziten/ oder impliziten  Dialog handeln wird der alte DialogHandler zurückgegeben
        /// ansonsten this
        /// </returns>
        internal static DialogHandler FindDialog(ControlBlockH controlBlock)
        {
            DialogHandler dh = null;
            if (DialogFinder != null)
            {
                dh = DialogFinder.FindDialog(controlBlock);
            }
            if (dh == null)
            {
                throw new OsciErrorException("9400");
            }
            else
            {
                return dh;
            }
        }
    }
}