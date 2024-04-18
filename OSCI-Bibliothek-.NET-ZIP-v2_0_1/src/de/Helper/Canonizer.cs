using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Osci.Common;
using Osci.Interfaces;

namespace Osci.Helper
{
    /// <summary> Diese Klasse führt die Kanonisierung gemäß der Spezifikation
    /// http://www.w3.org/TR/xml-c14n durch. Die Funktion beschränkt
    /// sich auf die Anforderungen der OSCI 1.2 Transportbibliothek.
    /// 
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: H. Tabrizi / N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    /// <seealso cref="DialogFinder">
    /// </seealso>
    public sealed class Canonizer 
        : MemoryStream
    {
        /// <summary>Default parser name. 
        /// </summary>
        //  protected static final String DEFAULT_PARSER_NAME = "org.apache.xerces.parsers.SAXParser";
        private static readonly string _defaultEncoding = Constants.CharEncoding;

        private readonly CanParser _canParser;

        public Hashtable DigestValues
        {
            get
            {
                return _canParser.DigestValues;
            }
        }

        public List<byte[]> SignedInfos
        {
            get
            {
                return _canParser.SignedInfos;
            }
        }

        public List<string> SignedProperties
        {
            get
            {
                return _canParser.SignedProperties;
            }
        }

        public List<string> ContainerNs
        {
            get
            {
                return _canParser.CocoNs;
            }
        }

        public Canonizer(Stream stream, StoreInputStream storeInputStream)
            : this(stream, storeInputStream, true)
        {
        }

        public Canonizer(Stream stream, StoreInputStream storeInputStream, bool isDuplicateIdCheckEnabled)
        {
            _canParser = new CanParser(this, storeInputStream)
            {
                IsDuplicateIdCheckEnabled =  isDuplicateIdCheckEnabled
            };
            _canParser.StartCanonicalization(stream, false);

            Position = 0;
        }
    }
}