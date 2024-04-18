using System.Collections;
using System.Collections.Generic;
using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Signature;
using System;

namespace Osci.Encryption
{
    // <p><b>de.osci.osci12.messageparts.EncryptedDataOSCI</b></p>
    // <p>Die EncryptedData-Klasse stellt einen Datencontainer für verschlüsselte
    // Daten in einer OSCI-Nachricht dar. Ein EncryptedData-Objekt wird in ein
    // ContentContainer-Objekt eingestellt und  kann selbst entweder ein Attachment-
    // Objekt enthalten (in Form einer Referenz auf ein verschlüsseltes Attachment,
    // welches in dem Parent-Nachrichtenobjekt liegt) oder aber wiederum ein
    // ContentContainer- oder EncryptedData-Objekt.</p>
    /// <exclude/>
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class EncryptedDataBuilder
        : DefaultHandler
    {
        public EncryptedData EncryptedData
        {
            get
            {
                return _encrytedDataObject;
            }
        }

        internal DefaultHandler ParentHandler
        {
            get;
        }

        internal XmlReader XmlReader
        {
            get;
        }

        internal Stack KeyInfoStack
        {
            get;
            private set;
        }

        internal Stack CipherDataStack
        {
            get;
            private set;
        }

        protected static string DsXmlns = Namespace.XmlDSig;
        protected static string XencXmlns = Namespace.XmlEnc;

        private static readonly Log _log = LogFactory.GetLog(typeof(EncryptedDataBuilder));
        private readonly List<string> _transformer;
        private EncryptedData _encrytedDataObject;
        private const string _edId = "";
        private string _cipherRefId = "";
        private string _currentElement;
        private const SymmetricCipherAlgorithm _defaultSymmetricCipherAlgorithm = Constants.DefaultSymmetricCipherAlgorithm;


        private EncryptedDataBuilder()
        {
            _transformer = new List<string>();
            KeyInfoStack = new Stack();
            CipherDataStack = new Stack();
        }

        public EncryptedDataBuilder(XmlReader xmlReader, DefaultHandler parentHandler, Attributes attributes)
            : this()
        {
            XmlReader = xmlReader;
            ParentHandler = parentHandler;
            _encrytedDataObject = new EncryptedData(_defaultSymmetricCipherAlgorithm, "");
            if ((attributes != null) && (attributes.GetValue("Id") != null))
            {
                _encrytedDataObject.Id = attributes.GetValue("Id");
            }
        }

        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start-Element: " + localName);
            if (localName.ToUpper().Equals("EncryptedData".ToUpper()) && uri.Equals(XencXmlns))
            {
                _log.Trace("encryptedData");
                if (_encrytedDataObject == null)
                {
                    _encrytedDataObject = new EncryptedData(_defaultSymmetricCipherAlgorithm, _edId);
                }
                _log.Trace("Konstruktor durchlaufen");
                if (attributes.GetValue("Id") != null)
                {
                    _encrytedDataObject.Id = attributes.GetValue("Id");
                }
            }
            else if (localName.Equals("EncryptionMethod") && uri.Equals(XencXmlns))
            {
                _encrytedDataObject.EncryptionMethodAlgorithm = attributes.GetValue("Algorithm");
            }
            else if (localName.Equals("IvLength") && uri.Equals(Common.Namespace.Osci128))
            {
                _encrytedDataObject.IVLength = Int32.Parse(attributes.GetValue("Value"));
				_encrytedDataObject.IVLengthParsed = true;
            }
            else if (localName.Equals("KeyInfo") && uri.Equals(DsXmlns))
            {
                DefaultHandler childBuilder = new KeyInfoBuilder(XmlReader, this, attributes);
                XmlReader.ContentHandler = childBuilder;
            }
            else if (localName.Equals("CipherData") && uri.Equals(XencXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("CipherReference") && uri.Equals(XencXmlns))
            {
                _cipherRefId = attributes.GetValue("URI");
            }
            else if (localName.Equals("Transforms") && uri.Equals(XencXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("Transform") && uri.Equals(DsXmlns))
            {
                _transformer.Add(attributes.GetValue("Algorithm"));
            }
            else if (localName.Equals("CipherValue") && uri.Equals(XencXmlns))
            {
                _currentElement = "";
            }
            else
            {
                throw new SaxException("Unerwartetes Element im EncryptedData-Builder: " + localName);
            }
        }

        public override void Characters(char[] ch, int start, int length)
        {
            if (_log.IsEnabled(LogLevel.Trace))
            {
                _log.Trace("Character: " + new string(ch, start, length));
            }
            if (_currentElement == null)
            {
                for (int i = 0; i < length; i++)
                {
                    if (ch[start + i] > ' ' && !Char.IsDigit(ch[start + i]))
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_char"));
                    }
                }
            }
            else
            {
                _currentElement += new string(ch, start, length);
            }
        }

        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("Ende-Element: " + localName);
            if (localName.ToUpper().Equals("EncryptedData".ToUpper()) && uri.Equals(XencXmlns))
            {
                XmlReader.ContentHandler = ParentHandler;
                ParentHandler.EndElement(uri, localName, qName);
            }
            else if (localName.Equals("EncryptionMethod") && uri.Equals(XencXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("CipherData") && uri.Equals(XencXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("EncryptedKey") && uri.Equals(XencXmlns))
            {
                // nothing to do
            }            
            else if (localName.Equals("CipherReference") && uri.Equals(XencXmlns))
            {
                CipherReference cr = new CipherReference(_cipherRefId);
                IEnumerator enumRenamed = _transformer.GetEnumerator();
                while (enumRenamed.MoveNext())
                {
                    cr.AddTransform((string)enumRenamed.Current);
                }
                _encrytedDataObject.CipherData = new CipherData(cr);
            }
            else if (localName.Equals("Transforms") && uri.Equals(XencXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("Transform") && uri.Equals(DsXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("CipherValue") && uri.Equals(XencXmlns))
            {
                CipherValue cv = new CipherValue(_currentElement);
                _encrytedDataObject.CipherData = new CipherData(cv);
                cv.StateOfObject = CipherValue.StateEncrypted;
            }
            else if (localName.Equals("IvLength") && uri.Equals(Common.Namespace.Osci128))
            {
                // nothing to do
            }
            else
            {
                throw new SaxException("Unerwartetes Element im EncryptedData: " + localName);
            }

			// Abwärtskompatibilität mit alten Nachrichten (OSCI-Bibliothek < 1.9.0)
			if (!_encrytedDataObject.IVLengthParsed)
			{
				// wenn IV-Length-Element nicht geparsed wurde, dann setze den alten Standard 128 Bit / 16 Byte
				_log.Warn(DialogHandler.ResourceBundle.GetString("warning_iv_length"));
				_encrytedDataObject.IVLength = 16;
			}


			_currentElement = null;
        }
    }
}