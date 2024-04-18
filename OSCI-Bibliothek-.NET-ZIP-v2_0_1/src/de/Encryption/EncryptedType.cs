using Osci.Common;
using Osci.Helper;
using Osci.Signature;

namespace Osci.Encryption
{
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
    public class EncryptedType
    {
        public string Id
        {
            get; set;
        }

        public string Type
        {
            get; set;
        }

        public string Encoding
        {
            get; set;
        }

        public string MimeType
        {
            get; set;
        }

        public string EncryptionMethodAlgorithm // Can be either a symmetric or asymmetric algorithm
        {
            get; set;
        }

		/// <summary>
		/// Länge des Initialisierungsvektors (in Byte) fuer die Verschluesselung mit AES-GCM
		/// </summary>
		public int IVLength
        {
            get
            {
                return _ivLength;
            }

            set
            {
                _ivLength = value;
            }
        }

		/// <summary>
		/// Notwendiger Status, solange noch Nachrichten ohne IvLength-Element versendet werden dürfen
		/// </summary>
		public bool IVLengthParsed
		{
			get
			{
				return _ivLengthParsed;
			}

			set
			{
				_ivLengthParsed = value;
			}
		}

		public CipherData CipherData
        {
            get; set;
        }

        public KeyInfo KeyInfo
        {
            get
            {
                return _keyInfo;
            }

            set
            {
                if (value == null)
                {
                    _log.Warn("KeyInfo darf nicht null sein.");
                }
                _keyInfo = value;
            }
        }

        private static readonly Log _log = LogFactory.GetLog(typeof(EncryptedType));

        /// <summary>Das KeyInfo Element. 
        /// </summary>
        private KeyInfo _keyInfo;

        private int _ivLength = Constants.DefaultGcmIVLength;

		private bool _ivLengthParsed = false;
    }
}