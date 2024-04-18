using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Osci.Common;
using Osci.Encryption;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Interfaces;
using Osci.Messagetypes;

namespace Osci.Helper
{
    /// <summary> <p>Handler für die Kanonisierung.</p>
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

    // Dieser Parser wird auch zum Parsen von zusätzlichen SOAP-Headern verwendet.
    public class CanParser
        : DefaultHandler
    {
        public Hashtable DigestValues
        {
            get; private set;
        }

        internal Hashtable MessageDigests
        {
            get; private set;
        }

        private readonly HashSet<string> _foundIdSet = new HashSet<string>();

        internal List<byte[]> SignedInfos
        {
            get; private set;
        }

        internal List<string> SignedProperties
        {
            get; private set;
        }

        internal List<string> CocoNs
        {
            get; private set;
        }

        protected bool UseComment;
        private int _elementDepth;

        private static readonly Log _log = LogFactory.GetLog(typeof(CanParser));
        private static readonly string _defaultEncoding = Constants.CharEncoding;
        private readonly DigestStream _outs;
        private readonly XmlReader _parser;
        private readonly Stack _sampleNsStack;
        private bool _insideHeader;
        private bool _headerAlreadySet;
        private bool _insideContainerSignatureProps;
        private bool _readNs;
        private bool _signedElement;
        private HashAlgorithm _md;
        private bool _insideTransportSignedInfo;
        private string _transportSignatureMethod;
        private string _currentTransportSignatureRef;
        private Hashtable _transportDigestMethods;
        private byte[] _controlBlock;
        private string _controlBlockId;
        private string _id;
        private readonly Stream _stream;
        private OsciMessage _msg;
        private readonly IContentHandler _parent;
        private readonly string _name;
        private readonly List<string> _transformerAlgorithms;

        /**
         * SOAP-Namespace Identifier
         */
        private string _soapId;
        private readonly StoreInputStream _sis;
        private TextWriter _outWriter;
        private TextWriter _tmpWriter;

        private CanParser()
        {
            _sampleNsStack = new Stack();
        }

        public CanParser(List<string> transformerAlgorithms, XmlReader xmlReader, IContentHandler parent, string name)
            : this(xmlReader, new MemoryStream())
        {
            _transformerAlgorithms = transformerAlgorithms;
            _parent = parent;
            _name = name;
        }

        public CanParser(OsciMessage msg, XmlReader xmlReader, IContentHandler parent, string name)
            : this(xmlReader, new MemoryStream())
        {
            _msg = msg;
            _parent = parent;
            _name = name;
        }

        internal CanParser(Stream stream, StoreInputStream sis)
            : this(new XmlReader(), stream)
        {
            _sis = sis;
        }

        public CanParser(XmlReader xmlReader, Stream stream)
            : this()
        {
            _stream = stream;
            _parser = xmlReader;
            _parser.ContentHandler = this;
            _outs = new DigestStream(stream);
            _outWriter = new StreamWriter(_outs, Encoding.UTF8);
            _outWriter.NewLine = "";
        }

        public override void StartDocument()
        {
            Hashtable sampleNsMap = new Hashtable();
            _elementDepth = 0;
            sampleNsMap.Add("xmlns", "");
            _sampleNsStack.Push(sampleNsMap);
            DigestValues = new Hashtable();
            _transportDigestMethods = new Hashtable();
            MessageDigests = new Hashtable();
            _currentTransportSignatureRef = "";

            if (SignedInfos == null)
            {
                SignedInfos = new List<byte[]>();
            }
            if (SignedProperties == null)
            {
                SignedProperties = new List<string>();
            }
            if (CocoNs == null)
            {
                CocoNs = new List<string>();
            }
            _soapId = null;
        }

        /// <summary>Start element. 
        /// </summary>
        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace(string.Format("StartElement: {0}, {1}", qName, uri));

            if (attributes.HasValues)
            {
                _log.Trace("Attributes: " + attributes);
            }

            CheckForDuplicateIds(qName, attributes);

            _signedElement = false;
            _readNs = false;
            Hashtable ht;

            if (_sampleNsStack.Count > 0)
            {
                ht = (Hashtable)_sampleNsStack.Peek();
            }
            else
            {
                ht = new Hashtable();
            }
            Hashtable sampleNsMap = new Hashtable(ht);
            _elementDepth++;

            // SOAP-Namespacedefinition suchen
            if (_soapId == null)
            {
                _soapId = "";
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes.GetValue(i).Equals(Namespace.SoapEnvelope))
                    {
                        if (_soapId.Length != 0)
                        {
                            throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + _soapId);
                        }
                        _soapId = attributes.GetQualifiedName(i).Substring(attributes.GetQualifiedName(i).IndexOf(':') + 1);
                    }
                }
            }

            // Entscheiden, ob der StoreStream die jetzt gepufferten Bytes wegwerfen oder
            // wegschreiben soll.
            if ((_sis != null) && qName.Equals(_soapId + ":Envelope"))
            {
                string schemaValue = attributes.GetValue("xsi:schemaLocation");
                _sis.Save = schemaValue == null || schemaValue.IndexOf("soapMessageEncrypted.xsd") < 0;
            }

            if (qName.Equals(_soapId + ":Header"))
            {
                if (!_headerAlreadySet)
                {
                    _headerAlreadySet = true;
                    _insideHeader = true;
                }
                else
                {
                    throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + _soapId + ":Header");
                }
            }
            else if (_insideHeader && localName.Equals("ControlBlock"))
            {
                _signedElement = true;
                _controlBlockId = "#" + attributes.GetValue("Id");
                _outWriter = new SplitWriter((StreamWriter)_outWriter);
            }
            else if ((_insideHeader && _elementDepth == 3 && !qName.EndsWith("Signature")) || qName.Equals(_soapId + ":Body"))
            {
                _id = attributes.GetValue("Id");
                string mdm = (string)_transportDigestMethods["#" + _id];
                if (mdm == null)
                {
                    mdm = Constants.DigestAlgorithmSha256;
                }
                _signedElement = true;
                try
                {
                    _outWriter.Flush();
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
                _md = (HashAlgorithm)MessageDigests[mdm];
                if (_md == null)
                {
                    _md = Crypto.CreateMessageDigest(mdm);
                    MessageDigests.Add(mdm, _md);
                }

                _outs.SetMessageDigest(_md);
                _outs.On(true);

            }
            else if (localName.Equals("SignedInfo") || ((!_insideHeader) && localName.Equals("SignedProperties")))
            {
                _signedElement = true;
                _readNs = true;
                _outWriter = new SplitWriter((StreamWriter)_outWriter);
                if (_insideHeader && localName.Equals("SignedInfo"))
                {
                    _insideTransportSignedInfo = true;
                }
                else if (localName.Equals("SignedProperties"))
                {
                    _insideContainerSignatureProps = true;
                }
            }
            else if (_insideTransportSignedInfo)
            {
                if (localName.Equals("Reference"))
                {
                    _currentTransportSignatureRef = attributes.GetValue("URI");
                }
                else if (localName.Equals("DigestMethod"))
                {
                    _transportDigestMethods.Add(_currentTransportSignatureRef, attributes.GetValue("Algorithm"));
                }
            }
            else if (localName.Equals("ContentContainer"))
            {
                _signedElement = true;
                _readNs = true;
            }

            //Sorting Attributes
            SortedList outMap = new SortedList(StringComparer.Ordinal);

            for (int i = 0; i < attributes.Length; i++)
            {
                string qualifiedName = attributes.GetQualifiedName(i);
                string val = attributes.GetValue(i);
                if (qualifiedName.StartsWith("xmlns:") || qualifiedName.Equals("xmlns"))
                {
                    if (!sampleNsMap.ContainsKey(qualifiedName))
                    {
                        outMap.Add(" ," + qualifiedName, val);
                        sampleNsMap.Add(qualifiedName, val);
                    }
                    else if (!val.Equals((string)sampleNsMap[qualifiedName]))
                    {
                        outMap.Add(" ," + qualifiedName, val);
                        sampleNsMap[qualifiedName] = val;
                    }
                }
                else
                {
                    // looking for Attributes with Namespace Prefix
                    if (!attributes.GetUri(i).Equals(""))
                    {
                        outMap.Add(attributes.GetUri(i) + "," + qualifiedName, val);
                    }
                    else
                    {
                        outMap.Add("," + qualifiedName, val);
                    }

                }
            }

            _sampleNsStack.Push(sampleNsMap);

            SortedList tmpMap = null;
            if (_readNs)
            {
                tmpMap = outMap;
                outMap = new SortedList(StringComparer.Ordinal);
            }

            _outWriter.Write("<" + qName);

            if (_signedElement)
            {
                foreach (object o in sampleNsMap.Keys)
                {
                    string nskey = " ," + (string)o;
                    string nsval = (string)sampleNsMap[o];
                    if (nsval.Length > 0)
                    {
                        if (!outMap.ContainsKey(nskey))
                        {
                            outMap.Add(nskey, nsval);
                        }
                    }
                }
            }

            if (_readNs)
            {
                _tmpWriter = _outWriter;
                _outWriter = new StringWriter { NewLine = "" };

                foreach (DictionaryEntry entry in outMap)
                {
                    string sKey = (string)entry.Key;
                    _outWriter.Write(" " + sKey.Substring(sKey.IndexOf(",") + 1) + "=\"");
                    NormalizeAttribute((string)entry.Value);
                    _outWriter.Write("\"");
                }

                outMap = tmpMap;

                if (localName.Equals("SignedInfo") || _insideContainerSignatureProps)
                {
                    ((SplitWriter)_tmpWriter).StringBuilder.Append(((StringWriter)_outWriter).GetStringBuilder());
                }
                else
                {
                    CocoNs.Add(_outWriter.ToString());
                }
                _outWriter = _tmpWriter;
                _tmpWriter = null;
            }

            // Normalize Attributes			
            foreach (DictionaryEntry it in outMap)
            {
                string sKey = (string)it.Key;
                string s = " " + sKey.Substring(sKey.IndexOf(",") + 1) + "=\"";
                _outWriter.Write(s);
                NormalizeAttribute((string)it.Value);
                _outWriter.Write("\"");
            }

            _outWriter.Write(">");
        }

        private void CheckForDuplicateIds(string qName, Attributes attributes)
        {
            string idAll = attributes.GetValue("Id");

            if (_log.IsEnabled(LogLevel.Debug))
            {
                _log.Debug("Element: " + qName + " Id: " + idAll);
            }

            if (idAll != null)
            {
                if (!_foundIdSet.Add("#" + idAll))
                {
                    _log.Error("Element with Id: " + idAll + " already exist");

                    if (IsDuplicateIdCheckEnabled)
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry"));
                    }
                }
            }
        }

        /// <summary>Characters. 
        /// </summary>
        public override void Characters(char[] ch, int start, int length)
        {
            NormalizeText(ch, start, length);
        }

        /// <summary>Ignorable whitespace. 
        /// </summary>
        public void IgnorableWhiteSpace(char[] ch, int start, int length)
        {
            Characters(ch, start, length);
        }

        /// <summary>End element. 
        /// </summary>
        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace(string.Format("EndElement: {0}, {1}", qName, uri));

            _elementDepth--;
            _outWriter.Write("</" + qName + ">");
            if (localName.Equals("SignedInfo"))
            {
                // Keine Nachrichtenverschlüsselung 
                if (!_insideHeader && SignedInfos.Count == 0)
                {
                    SignedInfos.Add(null);
                }
                byte[] ba0 = ((SplitWriter)_outWriter).StringBuilder.ToString().ToByteArray();
                SignedInfos.Add(ba0);
                _outWriter = ((SplitWriter)_outWriter).StreamWriter;
                if (_insideTransportSignedInfo)
                {
                    _insideTransportSignedInfo = false;
                    string digestMethod = (string)_transportDigestMethods[_controlBlockId];
                    if (digestMethod != null)
                    {
                        _md = Crypto.CreateMessageDigest(digestMethod);
                        MessageDigests.Add(digestMethod, _md);
                        if (DigestValues.ContainsKey(_controlBlockId))
                        {
                            throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry"));
                        }
                        DigestValues.Add(_controlBlockId, _md.ComputeHash(_controlBlock));
                    }
                }
            }
            else if (_insideContainerSignatureProps && localName.Equals("SignedProperties"))
            {
                SignedProperties.Add(((SplitWriter)_outWriter).StringBuilder.ToString());
                _outWriter = ((SplitWriter)_outWriter).StreamWriter;
                _insideContainerSignatureProps = false;
            }
            else if (_insideHeader && localName.Equals("ControlBlock"))
            {
                _outWriter.Flush();
                _controlBlock = ((SplitWriter)_outWriter).StringBuilder.ToString().ToByteArray();
                _outWriter = ((SplitWriter)_outWriter).StreamWriter;
                _id = null;
            }
            else if ((_insideHeader && (_elementDepth == 2) && !qName.EndsWith("Signature"))
                || (_elementDepth == 1 && qName.Equals(_soapId + ":Body")))
            {
                _outWriter.Flush();
                if (_id != null)
                {
                    if (DigestValues.ContainsKey("#" + _id))
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry"));
                    }
                    else
                    {
                        DigestValues.Put("#" + _id, _outs.GetDigest());
                    }
                }
                _outs.On(false);
                _id = null;
            }

            if (_sampleNsStack.Count != 0)
            {
                _sampleNsStack.Pop();
            }

            if (qName.Equals(_soapId + ":Header"))
            {
                _insideHeader = false;
            }

            if ((_name != null) && qName.Equals(_name))
            {
                EndDocument();
            }
        }

        /// <summary>End document. 
        /// </summary>
        public override void EndDocument()
        {
            _outs.Flush();
            _outWriter.Close();
            if (_transformerAlgorithms != null)
            {
                _stream.Position = 0;
                byte[] tmp = ((MemoryStream)_stream).ToArray();

                // trim byte order mark (shouldn't be there but it was actually found in some StoredMessages)
                string uf = Encoding.UTF8.GetString(tmp).Trim(new char[] { '\uFEFF' });

                _transformerAlgorithms.Add(uf);
                _parser.ContentHandler = (DefaultHandler)_parent;
            }
        }

        public override void Warning(SaxParseException ex)
        {
            _log.Debug("Warning: " + ex);
            throw ex;
        }

        /// <summary>Error. 
        /// </summary>
        public override void Error(SaxParseException ex)
        {
            _log.Debug("Error: " + ex);
            throw ex;
        }

        /// <summary>Fatal error. 
        /// </summary>
        public override void FatalError(SaxParseException ex)
        {
            _log.Debug("Fatal Error: " + ex);
            throw ex;
        }

        public void StartDtd(string name, string publicId, string systemId)
        {
        }

        /// <summary>End DTD. 
        /// </summary>
        public void EndDtd()
        {
        }

        /// <summary>Start entity. 
        /// </summary>
        public void StartEntity(string name)
        {
        }

        /// <summary>End entity. 
        /// </summary>
        public void EndEntity(string name)
        {
        }

        /// <summary>Start CDATA section. 
        /// </summary>
        public void StartCdata()
        {
            _log.Debug("CDATA");
        }

        /// <summary>End CDATA section. 
        /// </summary>
        public void EndCdata()
        {
            _log.Debug("CDATA");
        }

        /// <summary>Comment. 
        /// </summary>
        public void Comment(char[] ch, int start, int length)
        {
            if (UseComment)
            {
                byte[] ba;
                if (_elementDepth == 0)
                {
                    _outWriter.Write("\n");
                }

                _outWriter.Write("<!--");
                NormalizeText(ch, start, length);
                _outWriter.Write("-->");
            }
        }

        /// <summary>Normalize Attribute 
        /// </summary>
        protected string NormalizeAttribute(string value)
        {
            StringBuilder stringbuffer = new StringBuilder();

            if (value != null)
            {
                foreach (char c in value)
                {
                    switch (c)
                    {
                        case '&':
                            stringbuffer.Append("&amp;");
                            break;

                        case '<':
                            stringbuffer.Append("&lt;");
                            break;

                        case '>':
                            stringbuffer.Append("&gt;");
                            break;
                        case '"':
                            stringbuffer.Append("&quot;");
                            break;

                        case (char)0x09:
                            stringbuffer.Append("&#x9;");
                            break;

                        case (char)0x0A:
                            _outWriter.NewLine = "";
                            _outWriter.Write(c);
                            break;

                        case (char)0x0D:
                            stringbuffer.Append("&#xD;");
                            break;

                        default:
                            stringbuffer.Append(c);
                            break;
                    }
                }
            }

            _outWriter.Write(stringbuffer.ToString());
            return stringbuffer.ToString();
        }

        /// <summary>Normalize Processing Instruction
        /// </summary>
        protected string NormalizeProcessingInstruction(string text)
        {
            StringBuilder stringbuffer = new StringBuilder();
            if (text != null)
            {
                foreach (char c in text)
                {
                    switch (c)
                    {
                        case (char)0x0D:
                            stringbuffer.Append("&#xD;");
                            break;

                        default:
                            stringbuffer.Append(c);
                            break;
                    }
                }
            }
            _outWriter.Write(stringbuffer.ToString());
            return stringbuffer.ToString();
        }

        /// <summary>Normalize Text
        /// </summary>
        protected void NormalizeText(char[] text, int start, int length)
        {
            if (text != null)
            {
                for (int i = 0; i < length; i++)
                {
                    char c = text[start + i];
                    switch (c)
                    {
                        case '&':
                            _outWriter.Write("&amp;");
                            break;
                        case '<':
                            _outWriter.Write("&lt;");
                            break;
                        case '>':
                            _outWriter.Write("&gt;");
                            break;
                        case (char)0xD:
                            break;
                        case (char)0xA:
                            _outWriter.NewLine = "";
                            _outWriter.Write(c);
                            break;
                        default:
                            _outWriter.Write(c);
                            break;
                    }
                }
            }
        }

        /// <summary>Start Canonicalization
        /// </summary>
        public void StartCanonicalization(Stream isRenamed, bool withComment)
        {
            BufferedStream input = new BufferedStream(isRenamed);
            UseComment = withComment;
            _parser.Parse(input);
        }
    }
}