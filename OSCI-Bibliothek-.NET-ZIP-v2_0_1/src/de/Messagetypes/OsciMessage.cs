using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Osci.Common;
using Osci.Encryption;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary> Diese Klasse ist die Superklasse aller OSCI-Nachrichten-Objekte.
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
    public abstract class OsciMessage
    {
        #region Public properties

        /// <summary>Liefert Status der Nachricht (signiert/unsigniert)
        /// </summary>
        /// <returns>signierte Nachricht -> true</returns>
        /// <seealso cref="Common.DialogHandler.CheckSignatures()">
        /// </seealso>
        public bool IsSigned
        {
            get
            {
                return SignatureHeader != null;
            }
        }

        /// <summary> Liefert die in die Nachricht eingestellten (unverschlüsselten) Inhaltsdaten als ContentContainer-Objekte.
        /// </summary>
        /// <value> enthaltene ContentContainer mit Inhaltsdaten.
        /// </value>
        /// <seealso cref="MessageParts.ContentContainer">
        /// </seealso>		
        public ContentContainer[] ContentContainer
        {
            get
            {
                ContentContainer[] cc = new ContentContainer[_contentContainer.Count];
                _contentContainer.Values.CopyTo(cc, 0);
                return cc;
            }
        }

        public bool Base64Encoding
        {
            get; internal set;
        }

        /// <summary> Verschlüsselt die Nachricht auf Nachrichtenebene
        /// </summary>
        /// An dieser Stelle wird für die Generierung des XML-Files ein "value" ausgelesen, ein "return" nicht!
        /// <value> Verschlüsselte Transportdaten
        /// </value>
        public EncryptedDataOsci[] EncryptedData
        {
            get
            {
                EncryptedDataOsci[] ed = new EncryptedDataOsci[EncryptedDataList.Count];
                EncryptedDataList.Values.CopyTo(ed, 0);
                return ed;
            }
        }

        /// <summary> Liefert die in die Nachricht eingestellten Attachment als Attachment-Objekte.
        /// </summary>
        /// <value> enthaltene Attachment mit Inhaltsdaten.
        /// </value>
        /// <seealso cref="Attachment">
        /// </seealso>
        public Attachment[] Attachments
        {
            get
            {
                Attachment[] attachments = new Attachment[AttachmentList.Count];
                AttachmentList.Values.CopyTo(attachments, 0);
                return attachments;
            }
        }

        /// <summary> Liefert den DialogHandler des Nachrichtenobjektes.
        /// </summary>
        /// <value> Der DialogHandler
        /// </value>
        public DialogHandler DialogHandler
        {
            get; internal set;
        }

        /// <summary>  Liefert den Identifier für den Nachrichtentyp (ACCEPT_DELIVERY,
        /// EXIT_DIALOG...). Für Instanzen von OSCIMessage ist der Typ
        /// TYPE_UNDEFINED.
        /// </summary>
        /// <value> Der Messagetyp-Identifier
        /// </value>
        public int MessageType
        {
            get; protected set;
        }

        /// <summary> Liefert das Addressee-Rollenobjekt des Nachrichtenobjektes.
        /// </summary>
        /// <value> Der Addressee
        /// </value>
        public Addressee Addressee
        {
            get; internal set;
        }

        /// <summary> Liefert das Originator-Rollenobjekt des Nachrichtenobjektes.
        /// </summary>
        /// <value> Der Originator
        /// </value>
        public Originator Originator
        {
            get; internal set;
        }

        /// <summary> Liefert alle Author-Rollenobjekte, die für die Signatur von Inhaltsdaten
        /// in der Nachricht verwendet wurden oder die mit der Methode addRole(Role)
        /// der Nachricht hinzugefügt wurden.
        /// </summary>
        /// <value> Array von Author-Objekten
        /// </value>
        /// <seealso cref="AddRole">
        /// </seealso>
        /// <seealso cref="MessageParts.ContentContainer.Sign">
        /// </seealso>
        public Author[] OtherAuthors
        {
            get
            {
                Author[] au = new Author[OtherAutorsList.Count];
                OtherAutorsList.Values.CopyTo(au, 0);
                return au;
            }
        }

        /// <summary> Liefert alle Reader-Rollenobjekte, für die Inhaltsdaten
        /// in der Nachricht verschlüsselt wurden oder die mit der Methode addRole(Role)
        /// der Nachricht hinzugefügt wurden.
        /// </summary>
        /// <value> Array von Reader-Objekten
        /// </value>
        /// <seealso cref="AddRole">
        /// </seealso>
        /// <seealso cref="EncryptedDataOsci.Encrypt(Osci.Roles.Role)">
        /// </seealso>
        public Reader[] OtherReaders
        {
            get
            {
                Reader[] re = new Reader[OtherReadersList.Count];
                OtherReadersList.Values.CopyTo(re, 0);
                return re;
            }
        }

        /// <summary>Signatureintrag im Header (Client- oder Suppliersignatur). 
        /// </summary>
        public OsciSignature SignatureHeader
        {
            get; set;
        }

        #endregion

        #region Header

        /// <summary>DesiredLanguages-Header 
        /// </summary>
        public DesiredLanguagesH DesiredLanguagesH
        {
            get; internal set;
        }

        /// <summary>QualityOfTimestampCreation-Headereintrag 
        /// </summary>
        public QualityOfTimestampH QualityOfTimestampTypeCreation
        {
            get; internal set;
        }

        /// <summary>QualityOfTimestampReception-Headereintrag
        /// </summary>
        public QualityOfTimestampH QualityOfTimestampTypeReception
        {
            get; internal set;
        }

        public OsciH OsciH
        {
            get; internal set;
        }

        /// <summary>NonIntermediaryCertificates-Headereintrag
        /// </summary>
        public NonIntermediaryCertificatesH NonIntermediaryCertificatesH
        {
            get; internal set;
        }

        /// <summary>IntermediaryCertificates-Headereintrag
        /// </summary>
        public IntermediaryCertificatesH IntermediaryCertificatesH
        {
            get; set;
        }

        public ControlBlockH ControlBlock
        {
            get; internal set;
        }

        internal Body Body
        {
            get; set;
        }
        internal X509Certificate SignerCertificate
        {
            get; set;
        }

        public FeatureDescriptionH FeatureDescription { get; set; }

        #endregion


        #region Namespaces

        public string SoapNsPrefix
        {
            get; internal set;
        }

        public string OsciNsPrefix
        {
            get; internal set;
        }

        public string Osci2017NsPrefix
        {
            get; internal set;
        }

        public string Osci128NsPrefix
        {
            get; internal set;
        }

        public string DsNsPrefix
        {
            get; internal set;
        }

        public string XencNsPrefix
        {
            get; internal set;
        }

        public string XsiNsPrefix
        {
            get; private set;
        }

        public string Ns
        {
            get; internal set;
        }

        #endregion

        public string BoundaryString
        {
            get; internal set;
        }


        #region Constants

        /// <summary>Konstante, die einen undefinierten Nachrichtentyp anzeigt. 
        /// </summary>
        public const int TypeUndefined = 0;

        /// <summary>Konstante, die einen Annahmeauftrag anzeigt. 
        /// </summary>
        public const int AcceptDelivery = 0x01;

        /// <summary>Konstante, die einen Dialogendeauftrag anzeigt. 
        /// </summary>
        public const int ExitDialog = 0x02;

        /// <summary>Konstante, die einen Abbholauftrag anzeigt. 
        /// </summary>
        public const int FetchDelivery = 0x03;

        /// <summary>Konstante, die einen Abbholauftrag anzeigt. 
        /// </summary>
        public const int PartialFetchDelivery = 0x32;

        /// <summary>Konstante, die einen Laufzettelabholauftrag anzeigt. 
        /// </summary>
        public const int FetchProcessCard = 0x04;

        /// <summary>Konstante, die einen Weiterleitungsauftrag anzeigt. 
        /// </summary>
        public const int ForwardDelivery = 0x05;

        /// <summary>Konstante, die einen MessageId-Anforderungsauftrag anzeigt. 
        /// </summary>
        public const int GetMessageId = 0x06;

        /// <summary>Konstante, die einen Dialoginitialisierungsauftrag anzeigt. 
        /// </summary>
        public const int InitDialog = 0x07;

        /// <summary>Konstante, die einen Abwicklungsauftrag anzeigt. 
        /// </summary>
        public const int MediateDelivery = 0x08;

        /// <summary>Konstante, die einen Bearbeitungsauftrag anzeigt. 
        /// </summary>
        public const int ProcessDelivery = 0x09;

        /// <summary>Konstante, die einen Zustellungsauftrag anzeigt. 
        /// </summary>
        public const int StoreDelivery = 0x0A;

        /// <summary>Konstante, die einen paketierten Zustellungsauftrag anzeigt. 
        /// </summary>
        public const int PartialStoreDelivery = 0x0B;

        /// <summary>Konstante, die die Antwort zu einem paketierten Zustellungsauftrag anzeigt. 
        /// </summary>
        public const int ResponseToPartialStoreDelivery = 0xA1;

        /// <summary>Konstante, die eine Annahmeantwort anzeigt. 
        /// </summary>
        public const int ResponseToAcceptDelivery = 0x10;

        /// <summary>Konstante, die eine Dialogendeantwort anzeigt. 
        /// </summary>
        public const int ResponseToExitDialog = 0x20;

        /// <summary>Konstante, die eine Abbholantwort anzeigt. 
        /// </summary>
        public const int ResponseToFetchDelivery = 0x30;

        /// <summary>Konstante, die eine paketierte Abbholantwort anzeigt. 
        /// </summary>
        public const int ResponseToPartialFetchDelivery = 0x31;

        /// <summary>Konstante, die eine Laufzettelabholantwort anzeigt. 
        /// </summary>
        public const int ResponseToFetchProcessCard = 0x40;

        /// <summary>Konstante, die eine Weiterleitungsantwort anzeigt. 
        /// </summary>
        public const int ResponseToForwardDelivery = 0x50;

        /// <summary>Konstante, die ein MessageId-Anforderungsantwort anzeigt. 
        /// </summary>
        public const int ResponseToGetMessageId = 0x60;

        /// <summary>Konstante, die eine Dialoginitialisierungsantwort anzeigt. 
        /// </summary>
        public const int ResponseToInitDialog = 0x70;

        /// <summary>Konstante, die eine Abwicklungsantwort anzeigt. 
        /// </summary>
        public const int ResponseToMediateDelivery = 0x80;

        /// <summary>Konstante, die eine Bearbeitungsantwort anzeigt. 
        /// </summary>
        public const int ResponseToProcessDelivery = 0x90;

        /// <summary>Konstante, die eine Zustellungsantwort anzeigt. 
        /// </summary>
        public const int ResponseToStoreDelivery = 0xA0;
        /// <summary>Konstante, die eine Rückmeldung auf Nachrichtenebene (SOAP-Fault) anzeigt.
        /// </summary>
        public const int SoapFaultMessage = 0xB0;

        /// <summary>Konstante, die eine verschlüsselte SOAP-Nachricht anzeigt. 
        /// </summary>
        public const int SoapMessageEncrypted = 0x100;

        /// <summary>Kein Auswahlmodus gesetzt.
        /// </summary>
        public const int NoSelectionRule = -1;

        /// <summary>Auswahlmodus für Nachrichten/Laufzettel nach Message-Id. 
        /// </summary>
        public const int SelectByMessageId = 0;

        /// <summary>Auswahlmodus für Nachrichten/Laufzettel nach Empfangsdatum. 
        /// </summary>
        public const int SelectByDateOfReception = 1;

        /// <summary>Auswahlmodus für Nachrichten/Laufzettel nach Datum der letzen Modifikation. 
        /// </summary>
        public const int SelectByRecentModification = 2;

        /// <summary>Auswahlmodus für alle Laufzettel (default). </summary>
        public const int SelectAll = -1;
        /// <summary> Auswahlmodus für Laufzettel von Nachrichten an den Absender eines Laufzettelabholauftrags.</summary>
        public const int SelectAddressee = 0;
        /// <summary>Auswahlmodus für Laufzettel von Nachrichten vom Absender eines Laufzettelabholauftrags.</summary>
        public const int SelectOriginator = 1;

        protected internal string MessageId;

        /// <summary>Content-Id der Nachricht 
        /// </summary>
        public static string ContentId = "osci@message";

        #endregion


        /// <summary>Enthält die EncryptedData-Objekte der Nachricht 
        /// </summary>
        internal Hashtable EncryptedDataList
        {
            get; set;
        }

        /// <summary>Enthält die Attachment-Objekte der Nachricht 
        /// </summary>
        internal Hashtable AttachmentList
        {
            get; set;
        }

        internal Hashtable HashableMsgPart
        {
            get; set;
        }

        internal Hashtable OtherAutorsList
        {
            get; set;
        }

        internal Hashtable OtherReadersList
        {
            get; set;
        }

        internal string BodyId
        {
            get; set;
        }

        internal UniqueElementTracker SignatureRelevantElements
        {
            get;
            set;
        }

        // hier sollten schon einmal die States definiert werden
        internal int StateOfMessage = 0;

        protected ArrayList CustomHeaders = new ArrayList();

        protected internal const int StateComposed = 0x01;
        protected internal const int StateSigned = 0x02;
        protected internal const int StateParsed = 0x04;

        protected XmlReader XmlReader = null;
        protected string CurrentElement;
        protected Hashtable Xmlns;

        private string _digestAlgorithm;
        private OsciDataSource _encryptedMessage = null;
        private EncryptedDataOsci _transportEncryptedData = null;
        private DefaultHandler _parentHandler;

        private readonly Dictionary<string, ContentContainer> _contentContainer;
        private static readonly Log _log = LogFactory.GetLog(typeof(OsciMessage));

        #region c'tor

        /// <summary> Konstruktor for the OSCIMessage object
        /// </summary>
        protected OsciMessage()
        {
            MessageType = 0;
            Base64Encoding = true;
            BodyId = "body";
            SoapNsPrefix = "soap";
            OsciNsPrefix = "osci";
            Osci2017NsPrefix = "osci2017";
            DsNsPrefix = "ds";
            XencNsPrefix = "xenc";
            XsiNsPrefix = "xsi";
            Ns = " " + Constants.DefaultNamespaces;

            _contentContainer = new Dictionary<string, ContentContainer>();

            OtherAutorsList = new Hashtable();
            OtherReadersList = new Hashtable();
            EncryptedDataList = new Hashtable();
            AttachmentList = new Hashtable();
            HashableMsgPart = new Hashtable();

            Xmlns = new Hashtable();
            _digestAlgorithm = DialogHandler.DigestAlgorithm;
        }

        /// <summary> Konstruktor für ein OSCIMessage-Objekt
        /// </summary>
        /// <param name="dialogHandler">
        /// </param>
        protected OsciMessage(DialogHandler dialogHandler)
            : this()
        {
            DialogHandler = dialogHandler;
        }

        #endregion

        internal void AddContentContainer(string id, ContentContainer contentContainer)
        {
            _contentContainer[id] = contentContainer;
        }

        /// <summary>Durchsucht die Nachrichtensignatur nach den verwendeten
        /// Algorithmen. Es wird true zurückgegeben, wenn Referenzen der 
        /// XML-Signatur oder die Signatur selbst mit Algorithmen erzeugt
        /// wurden, die zu dem übergebenen Prüfzeitpunkt als unsicher 
        /// eingestuft wurden.
        /// </summary>
        /// <param name="date"> Prüfzeitpunkt </param>
        /// <returns>true, wenn unsichere Algorithmen zur Signatur verwendet wurden,
        /// andernfalls false</returns>
        public bool HasWeakSignature(DateTimeOffset date)
        {
            if (SignatureHeader == null)
            {
                throw new Exception("Message is not signed.");
            }

            if (SignerCertificate == null)
            {
                throw new Exception("Role object referenced in signature not found.");
            }

            return Crypto.IsWeak(SignerCertificate, date) || Crypto.IsWeak(SignatureHeader, date);
        }

        /// <todo>Checken, ob dies noch nötig ist, wenn die Rollen erst beim Aufbau der Nachricht zusammengesucht werden.</todo>
        /// <summary> <p>Diese Methode ermöglicht es Anwendungen, zusätzliche Zertifikate in den
        /// NonIntermediaryCertificates-Header einzustellen, die dann vom Intermediär mit
        /// geprüft werden. Die Zertifikate werden in Form von Reader- oder Author-Objekten
        /// übergeben, die die entsprechenden Zertifikate enthalten müssen.</p>
        /// Die Methoden ContentContainer.sign(Role) und EncryptedData.encrypt(Role)
        /// fügen die übergebenen Rollenobjekte der Nachricht automatisch hinzu, so daß
        /// diese Methode in der Regel nicht benötigt wird.
        /// </summary>
        /// <param name="role">Hinzuzfügendes Reader- oder Author-Objekt
        /// <exception cref="System.Exception">Throws IllegalArgumentException, when ...</exception> 
        /// </param>
        /// <seealso cref="MessageParts.ContentContainer.Sign">
        /// </seealso>
        /// <seealso cref="EncryptedDataOsci.Encrypt(Osci.Roles.Role)">
        /// </seealso>
        public void AddRole(Role role)
        {
            if (role is Originator)
            {
                Originator = (Originator)role;
            }
            else if (role is Addressee)
            {
                Addressee = (Addressee)role;
            }
            else
            {
                if (role is Author && !OtherAutorsList.ContainsValue(role))
                {
                    {
                        if (OtherAutorsList.ContainsKey(role.Id))
                        {
                            // Role.id wird nur noch intern verwendet 
                            role.CipherRefId = role.CipherCertificateId;
                            role.SignatureRefId = role.SignatureCertificateId;
                            role.Id += '0';
                        }
                        OtherAutorsList.Put(role.Id, role);
                    }

                }
                else if (role is Reader && !OtherReadersList.ContainsValue(role))
                {
                    if (OtherReadersList.ContainsKey(role.Id))
                    {
                        // Role.id wird nur noch intern verwendet
                        role.CipherRefId = role.CipherCertificateId;
                        role.Id += '0';
                    }
                    OtherReadersList.Put(role.Id, role);
                }
            }
        }

        /// <summary> Fügt der Nachricht einen Inhaltsdatencontainer hinzu.
        /// </summary>
        /// <param name="container">Inhaltsdatencontainer
        /// </param>
        /// <seealso cref="MessageParts.ContentContainer">
        /// </seealso>
        public void AddContentContainer(ContentContainer container)
        {
            _log.Debug("ContentContainer wird hinzugefügt.");
            // übernehmen der Role Objekte
            _contentContainer[container.RefId] = container;

            Role[] roles = container.Roles;

            for (int i = 0; i < roles.Length; i++)
            {
                _log.Trace("Ein weiteres Role-Objekt wird der Nachricht hinzugefügt.");

                if (roles[i] is Originator)
                {
                    if ((Originator != null) && (Originator != roles[i]))
                    {
                        throw new OsciRoleException("Es wurde versucht einen weiteren anderen Originator der Nachricht zuzufügen.");
                    }

                    _log.Trace("Eine Originator wird hinzugefügt.");
                    Originator = (Originator)roles[i];
                }
                else if (roles[i] is Addressee)
                {
                    if (Addressee != null && Addressee != roles[i])
                    {
                        throw new OsciRoleException("Es wurde versucht einen weiteren anderen Addressee der Nachricht zuzufügen.");
                    }
                    Addressee = (Addressee)roles[i];
                    _log.Trace("Eine Addressee wird hinzugefügt.");
                    break;
                }

                if (roles[i] is Author)
                {
                    OtherAutorsList[roles[i].Id] = roles[i];
                }

                if (roles[i] is Reader)
                {
                    OtherReadersList[roles[i].Id] = roles[i];
                }
            }

            // Übernehmen der Attachments
            Attachment[] atts = container.Attachments;
            if (atts != null && atts.Length > 0)
            {
                for (int i = 0; i < atts.Length; i++)
                {
                    AddAttachment(atts[i]);
                }
            }
        }


        /// <summary> Entfernt einen Inhaltsdatencontainer aus der Nachricht.
        /// </summary>
        /// <param name="container">Inhaltsdatencontainer
        /// </param>
        /// <seealso cref="AddContentContainer">
        /// </seealso>
        public void RemoveContentContainer(ContentContainer container)
        {
            _contentContainer.Remove(container.RefId);
        }

        /**
          * Durchsucht <b>die unverschlüsselten</b> Inhaltsdaten nach dem ContentContainer
          * mit der übergebenen RefID.     
          * @param refID zu suchende RefID
          * @return den zugehörigen ContentContainer oder null, wenn die Referenz
          * nicht gefunden wurde.
          */
        public ContentContainer GetContentContainerByRefId(string refId)
        {
            ContentContainer[] containers = ContentContainer;
            for (int i = 0; i < containers.Length; i++)
            {
                MessagePart ret;
                if ((ret = SearchMessagePart(containers[i], refId)) != null)
                {
                    if (ret is ContentContainer)
                    {
                        return (ContentContainer)ret;
                    }
                    else
                    {
                        _log.Info("RefID " + refId + " does not refer to a ContentContainer.");
                        return null;
                    }
                }
            }
            _log.Info("RefID " + refId + " not found.");
            return null;
        }

        /**
         * Durchsucht <b>die unverschlüsselten</b> ContentContainer nach dem Content
         * mit der übergebenen RefID.     
         * @param refID zu suchende RefID
         * @return den zugehörigen Content oder null, wenn die Referenz
         * nicht gefunden wurde.
         */
        public Content GetContentByRefId(string refId)
        {
            ContentContainer[] containers = ContentContainer;
            MessagePart ret;
            for (int i = 0; i < containers.Length; i++)
            {
                if ((ret = SearchMessagePart(containers[i], refId)) != null)
                {
                    if (ret is Content)
                    {
                        return (Content)ret;
                    }
                    else
                    {
                        _log.Info("RefID " + refId + " does not refer to a Content.");
                        return null;
                    }
                }
            }
            _log.Info("RefID " + refId + " not found.");
            return null;
        }


        private MessagePart SearchMessagePart(ContentContainer cnt, string refId)
        {
            if (cnt.RefId.Equals(refId))
            {
                return cnt;
            }

            Content[] contents = cnt.Contents;
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i].RefId.Equals(refId))
                {
                    return contents[i];
                }
                if (contents[i].ContentType == ContentType.ContentContainer)
                {
                    MessagePart mp = SearchMessagePart(contents[i].ContentContainer, refId);
                    if (mp != null)
                    {
                        return mp;
                    }
                }
            }
            return null;
        }

        /// <summary> Fügt der Nachricht einen EncryptedData-Eintrag mit verschlüsselten
        /// Inhaltsdaten hinzu.
        /// </summary>
        /// <param name="encData">verschlüsselte Daten
        /// </param>
        /// <seealso cref="EncryptedDataOsci">
        /// </seealso>
        public virtual void AddEncryptedData(EncryptedDataOsci encData)
        {
            if ((encData.RefId == null) || encData.RefId.Equals(""))
            {
                throw new NullReferenceException("EncryptedDataOSCI-Objekt besitzt keine oder leere RefID.");
            }

            EncryptedDataList.Put(encData.RefId, encData);

            // übernehmen der Role Objekte
            Role[] roles = encData.Roles;

            for (int i = 0; i < roles.Length; i++)
            {
                if (roles[i] is Originator)
                {
                    if (Originator != null && Originator != roles[i])
                    {
                        throw new OsciRoleException("Es wurde versucht einen weiteren anderen Originator der Nachricht zuzufügen.");
                    }

                    Originator = (Originator)roles[i];
                }
                else if (roles[i] is Addressee)
                {
                    if (Addressee != null && Addressee != roles[i])
                    {
                        throw new OsciRoleException("Es wurde versucht einen weiteren anderen Addressee der Nachricht zuzufügen.");
                    }

                    Addressee = (Addressee)roles[i];
                }

                if (roles[i] is Author)
                {
                    OtherAutorsList[roles[i].Id] = roles[i];
                }

                if (roles[i] is Reader)
                {
                    OtherReadersList[roles[i].Id] = roles[i];
                }
            }

            // Übernehmen der Attachments
            Attachment[] atts = encData.Attachments;

            for (int i = 0; i < atts.Length; i++)
            {
                AddAttachment(atts[i]);
            }
        }


        /// <summary> Entfernt einen EncryptedData-Eintrag mit verschlüsselten Daten
        /// aus der Nachricht.
        /// </summary>
        /// <param name="encData">verschlüsselte Daten
        /// </param>
        /// <seealso cref="AddEncryptedData">
        /// </seealso>
        /// <seealso cref="EncryptedDataOsci">
        /// </seealso>
        public void RemoveEncryptedData(EncryptedDataOsci encData)
        {
            SupportClass.HashtableRemove(EncryptedDataList, encData.RefId);
        }

        /// <summary> Interne Methode. Attachments werden automatisch einer Nachricht hinzugefügt,
        /// wenn der referenzierende Content eingestellt wird. Diese Methode sollte
        /// von Anwendungen daher normalerweise nicht aufgerufen werden.
        /// </summary>
        /// <param name="attachment">Attachment
        /// </param>
        /// <seealso cref="MessageParts.ContentContainer.AddContent">
        /// </seealso>
        public void AddAttachment(Attachment attachment)
        {
            AttachmentList.Put(attachment.RefId, attachment);
            if (BoundaryString != null)
            {
                attachment.BoundaryString = BoundaryString;
            }
        }

        /// <summary> Interne Methode. Attachments werden automatisch aus einer Nachricht entfernt,
        /// wenn der referenzierende Content entfernt wird. Diese Methode sollte
        /// von Anwendungen daher normalerweise nicht aufgerufen werden.
        /// </summary>
        /// <param name="attachment">Attachment
        /// </param>
        /// <seealso cref="MessageParts.ContentContainer.RemoveContent">
        /// </seealso>
        public void RemoveAttachment(Attachment attachment)
        {
            SupportClass.HashtableRemove(AttachmentList, attachment.RefId);
        }

        private bool CompareRoles(Role org, Role next)
        {
            X509Certificate orgCert = null;
            X509Certificate nextCert = null;

            try
            {
                if (org.HasCipherCertificate())
                {
                    if (next.HasCipherCertificate() && !org.CipherCertificate.Equals(next.CipherCertificate))
                    {
                        return false;
                    }

                }
                else if (next.HasCipherCertificate())
                {
                    org.CipherCertificate = next.CipherCertificate;
                }

                if (org.HasSignatureCertificate())
                {
                    if (next.HasSignatureCertificate() && !org.SignatureCertificate.Equals(next.SignatureCertificate))
                    {
                        return false;
                    }
                }
                else if (next.HasSignatureCertificate())
                {
                    org.SignatureCertificate = next.SignatureCertificate;
                }

                return true;
            }
            catch (OsciRoleException)
            {
                return false;
            }
        }

        private void ImportCertificates(Role[] roles)
        {
            for (int i = 0; i < roles.Length; i++)
            {
                if ((roles[i] is Originator) && (Originator != null) && (!CompareRoles(Originator, roles[i])))
                {
                    throw new OsciRoleException(DialogHandler.ResourceBundle.GetString("incompatible_role_error") + " Originator");
                }
                else if ((roles[i] is Addressee) && (Addressee != null) && !CompareRoles(Addressee, roles[i]))
                {
                    throw new OsciRoleException(DialogHandler.ResourceBundle.GetString("incompatible_role_error") + " Addressee");
                }
                else if (roles[i] is Reader)
                {
                    OtherReadersList[roles[i].Id] = roles[i];
                }
                else if (roles[i] is Author)
                {
                    OtherAutorsList[roles[i].Id] = roles[i];
                }
            }
        }

        protected void CollectCertificatesFromContentContainer(ContentContainer coco)
        {
            ImportCertificates(coco.Roles);

            Content[] cnt = coco.Contents;

            for (int i = 0; i < cnt.Length; i++)
            {
                if (cnt[i].ContentType == ContentType.ContentContainer)
                {
                    CollectCertificatesFromContentContainer(cnt[i].ContentContainer);
                }
            }

            EncryptedDataOsci[] enc = coco.EncryptedData;

            for (int i = 0; i < enc.Length; i++)
            {
                ImportCertificates(enc[i].Roles);
            }
        }

        protected void ImportAllCertificates()
        {
            IEnumerator it = _contentContainer.Values.GetEnumerator();

            while (it.MoveNext())
            {
                CollectCertificatesFromContentContainer((ContentContainer)it.Current);
            }

            it = EncryptedDataList.Values.GetEnumerator();

            while (it.MoveNext())
            {
                ImportCertificates(((EncryptedDataOsci)it.Current).Roles);
            }
        }

        public virtual void Compose()
        {
            if (BoundaryString == null)
            {
                BoundaryString = DialogHandler.Boundary + "_" + Tools.CreateRandom(24);
            }

            IEnumerator e = AttachmentList.Values.GetEnumerator();
            while (e.MoveNext())
            {
                ((Attachment)e.Current).BoundaryString = BoundaryString;
            }
        }

        internal void Sign(Role role)
        {
            if (!(role is Intermed) && !(role is Originator) && !(role is Addressee))
            {
                throw new OsciRoleException("Falsches Rollenobjekt (" + role.GetType() + ").\nÄußere Signaturen können nur von Originator-, Intermediaer- oder Addressee-Objekte angebracht werden.", null);
            }

            if ((StateOfMessage & StateComposed) == 0)
            {
                Compose();
            }

            string name;

            if (this is OsciRequest)
            {
                name = "<" + OsciNsPrefix + ":ClientSignature Id=\"clientsignature\" " + SoapNsPrefix + ":actor=\"http://schemas.xmlsoap.org/soap/actor/next\" " + SoapNsPrefix + ":mustUnderstand=\"1\">";
            }
            else
            {
                name = "<" + OsciNsPrefix + ":SupplierSignature Id=\"suppliersignature\" " + SoapNsPrefix + ":actor=\"http://schemas.xmlsoap.org/soap/actor/next\" " + SoapNsPrefix + ":mustUnderstand=\"1\">";
            }
            SignatureHeader = MessagePartsEntry.OsciSignature(name);

            SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(DialogHandler.Controlblock, _digestAlgorithm));

            if (DesiredLanguagesH != null)
            {
                SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(DesiredLanguagesH, _digestAlgorithm));
            }

            // FeatureDescription nur signieren, wenn mitgesendet (es gibt immer eine Default-FeatureDescription)
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(FeatureDescription, _digestAlgorithm));
            }
            if (QualityOfTimestampTypeCreation != null)
            {
                SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(QualityOfTimestampTypeCreation, _digestAlgorithm));
            }
            if (QualityOfTimestampTypeReception != null)
            {
                SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(QualityOfTimestampTypeReception, _digestAlgorithm));
            }
            if (OsciH != null)
            {
                SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(OsciH, _digestAlgorithm));
            }

            for (int i = 0; i < CustomHeaders.Count; i++)
                SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(((CustomHeader)CustomHeaders[i]), _digestAlgorithm));

            if (AttachmentList.Count > 0)
            {
                IEnumerator atts = AttachmentList.GetEnumerator();
                Attachment att;
                byte[] tmp;

                while (atts.MoveNext())
                {
                    // Workaround für das Problem, dass Attchments im Header verschlüsselt signiert werden.
                    att = (Attachment)((DictionaryEntry)(((object)((atts.Current))))).Value;
                    tmp = null;
                    if (att.HasDigestValue(_digestAlgorithm))
                    {
                        tmp = att.GetDigestValue(_digestAlgorithm);
                        att.DigestValues.Remove(_digestAlgorithm);
                    }
                    att.DigestValues.Add(_digestAlgorithm, att.GetEncryptedDigestValue(_digestAlgorithm));
                    SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(att, _digestAlgorithm));
                    att.DigestValues.Remove(_digestAlgorithm);
                    if (tmp != null)
                    {
                        att.DigestValues.Add(_digestAlgorithm, tmp);
                    }
                }
            }

            if (role is Intermed)
            {
                if (IntermediaryCertificatesH == null)
                {
                    IntermediaryCertificatesH = new IntermediaryCertificatesH();
                }
                IntermediaryCertificatesH.SignatureCertificateIntermediary = (Intermed)role;
            }
            else
            {
                if (NonIntermediaryCertificatesH == null)
                {
                    NonIntermediaryCertificatesH = new NonIntermediaryCertificatesH();
                }
                if (role is Originator)
                {
                    NonIntermediaryCertificatesH.SignatureCertificateOriginator = (Originator)role;
                }
                else
                {
                    NonIntermediaryCertificatesH.SignatureCertificateAddressee = (Addressee)role;
                }
            }

            if (IntermediaryCertificatesH != null)
            {
                SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(IntermediaryCertificatesH, _digestAlgorithm));
            }
            if (NonIntermediaryCertificatesH != null)
            {
                SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(NonIntermediaryCertificatesH, _digestAlgorithm));
            }
            SignatureHeader.AddSignatureReference(MessagePartsEntry.OsciSignatureReference(Body, _digestAlgorithm));
            SignatureHeader.Sign(role);
        }

        // Diese Methode fügt alle in den Role-Objekte auffindbaren Zertifikate zum non-Intermed Header hinzu.
        public void CreateNonIntermediaryCertificatesH()
        {
            if ((StateOfMessage & StateParsed) != 0)
            {
                return;
            }

            bool empty = true;

            if (NonIntermediaryCertificatesH == null)
            {
                NonIntermediaryCertificatesH = new NonIntermediaryCertificatesH();
            }
            else
            {
                empty = false;
            }
            if (Originator != null)
            {
                if (Originator.HasSignatureCertificate())
                {
                    NonIntermediaryCertificatesH.SignatureCertificateOriginator = Originator;
                    empty = false;
                }

                if (Originator.HasCipherCertificate())
                {
                    NonIntermediaryCertificatesH.CipherCertificateOriginator = Originator;
                    empty = false;
                }
            }

            ArrayList v;

            if ((v = SearchCertificates(false, OtherAuthors)).Count > 0)
            {
                empty = false;
            }
            Author[] au = new Author[v.Count];
            for (int i = 0; i < au.Length; i++)
            {
                au[i] = (Author)v[i];
            }
            NonIntermediaryCertificatesH.CipherCertificatesOtherAuthors = au;

            if (Addressee != null)
            {
                if (Addressee.HasCipherCertificate())
                {
                    NonIntermediaryCertificatesH.CipherCertificateAddressee = Addressee;
                    empty = false;
                }
            }

            if ((v = SearchCertificates(false, OtherReaders)).Count > 0)
            {
                empty = false;
            }
            Reader[] rd = new Reader[v.Count];
            for (int i = 0; i < rd.Length; i++)
            {
                rd[i] = (Reader)v[i];
            }
            NonIntermediaryCertificatesH.CipherCertificatesOtherReaders = rd;

            if ((v = SearchCertificates(true, OtherAuthors)).Count > 0)
            {
                empty = false;
            }
            au = new Author[v.Count];
            for (int i = 0; i < au.Length; i++)
            {
                au[i] = (Author)v[i];
            }
            NonIntermediaryCertificatesH.SignatureCertificatesOtherAuthors = au;

            if (empty)
            {
                NonIntermediaryCertificatesH = null;
            }
        }

        private ArrayList SearchCertificates(bool signature, Role[] roles)
        {
            ArrayList v = new ArrayList();
            X509Certificate r;
            for (int i = 0; i < roles.Length; i++)
            {
                if ((signature && roles[i].HasSignatureCertificate()) || (!signature && roles[i].HasCipherCertificate()))
                {
                    v.Add(roles[i]);
                }
            }
            return v;
        }

        /// <summary> Diese Methode liefert ein Role Objekt passend zu der übergebenen RefID
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns> Role Objekt oder Null
        /// </returns>
        public Role GetRoleForRefId(string uri)
        {
            Role role;
            IDictionaryEnumerator enu = OtherAutorsList.GetEnumerator();
            foreach (object en in OtherAutorsList.Keys)
            {
                role = (Role)OtherAutorsList[en];
                if (CheckRole(uri, role))
                {
                    return role;
                }

            }
            foreach (object en in OtherReadersList.Keys)
            {
                role = (Role)OtherReadersList[en];
                if (CheckRole(uri, role))
                {
                    return role;
                }
            }

            if (CheckRole(uri, Originator))
            {
                return Originator;
            }

            if (CheckRole(uri, Addressee))
            {
                return Addressee;
            }
            if (CheckRole(uri, DialogHandler.Supplier))
            {
                return DialogHandler.Supplier;
            }
            if (CheckRole(uri, DialogHandler.Client))
            {
                return DialogHandler.Client;
            }
            return null;
        }

        private bool CheckRole(string uri, Role role)
        {
            try
            {
                if (uri.Equals(role.CipherCertificateId))
                    return true;
                else if (uri.Equals(role.SignatureCertificateId))
                    return true;
            }
            catch (NullReferenceException)
            {
            }

            return false;
        }

        /// <summary> Setzt den Hash-Algorithmus für die Signatur der Nachrichten
        /// (Voreinstellung aus DialogHandler).</summary>
        /// <param name="_digestAlgorithm">Hashalgorithmus-XML-Attribut</param>
        public void SetDigestAlgorithm(string digestAlgorithm)
        {
            if (IsSigned)
            {
                throw new Exception("Message is already signed.");
            }
            _digestAlgorithm = digestAlgorithm;
        }

        protected void CompleteMessage(Stream stream)
        {
            WriteCustomSoapHeaders(stream);
            stream.Write("</" + SoapNsPrefix + ":Header>");
            Body.WriteXml(stream);
            stream.Write("</" + SoapNsPrefix + ":Envelope>");

            for (IEnumerator e = AttachmentList.GetEnumerator(); e.MoveNext();)
            {
                ((Attachment)((DictionaryEntry)e.Current).Value).WriteXml(stream);
            }

            stream.Write("\r\n--" + BoundaryString + "--\r\n");
            stream.Flush();
        }

        protected void WriteCustomSoapHeaders(Stream stream)
        {
            if (CustomHeaders.Count > 0)
            {
                for (int i = 0; i < CustomHeaders.Count; i++)
                {
                    stream.Write(((CustomHeader)CustomHeaders[i]).Data);
                }
            }
        }

        /// <summary> Mit dieser Methode können beliebige Einträge dem SOAP-Header der
        /// Nachricht hinzugefügt werden. Die übergebenen Strings müssen vollständige
        /// XML-Tags sein. Das unterste Element muss ein id-Attribut besitzen, welches
        /// innerhalb der Nachricht eindeutig sein muss. Die Bibliothek verwendet
        /// für die id-Attribute die Namen der Einträge (ohne Namespace) in
        /// Kleinschreibung, es sollten daher beispielsweise nicht "desiredlanguages",
        /// "clientsignature" oder "body" verwendet werden.
        /// Bei zu signierenden Nachrichten muss der Tag außerdem in kanonischer Form übergeben
        /// werden. Der Ãußerste Tag muss alle Namespace-Deklarationen der OSCI-Nachricht enthalten.
        /// </summary>
        /// <param name="xml">XML-Tag</param>
        /// <seealso cref="GetCustomHeaders">
        /// </seealso>
        public void AddCustomHeader(string xml)
        {
            CustomHeaders.Add(new CustomHeader(xml));
        }

        /// <summary>Liefert vorhandene SOAP-Header-Einträge.
        /// </summary>
        /// <returns>Array der SOAP-Header-Einträge
        /// </returns>
        /// <seealso cref="AddCustomHeader">
        /// </seealso>
        public string[] GetCustomHeaders()
        {
            string[] headers = new string[CustomHeaders.Count];

            for (int i = 0; i < headers.Length; i++)
            {
                headers[i] = ((CustomHeader)CustomHeaders[i]).Data;
            }

            return headers;
        }


        public virtual void WriteXml(Stream stream)
        {
            if ((StateOfMessage & StateComposed) == 0)
            {
                Compose();
            }
            stream.Write("MIME-Version: 1.0\r\nContent-Type: Multipart/Related; boundary=" + BoundaryString + "; type=text/xml\r\n");
            stream.Write("\r\n--" + BoundaryString + "\r\nContent-Type: text/xml; charset=UTF-8\r\n");
            stream.Write("Content-Transfer-Encoding: 8bit\r\nContent-ID: <" + ContentId + ">\r\n\r\n");
            stream.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n\r\n");
            stream.Write("<" + SoapNsPrefix + ":Envelope " + Constants.DefaultNamespaces + " xsi:schemaLocation=\"http://schemas.xmlsoap.org/soap/envelope/ soap");
            stream.Write(GetType().Name + ".xsd http://www.w3.org/2000/09/xmldsig# oscisig.xsd http://www.w3.org/2001/04/xmlenc# oscienc.xsd\"><" + SoapNsPrefix + ":Header>");
            DialogHandler.Controlblock.WriteXml(stream);
        }

        public override string ToString()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    WriteXml(stream);
                    return stream.ToArray().AsString();
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.StackTrace);
                return "";
            }
        }
    }
}