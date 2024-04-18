using System;
using System.IO;
using Osci.Common;
using Osci.Encryption;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Messagetypes;
using System.Collections.Generic;

namespace Osci.MessageParts
{
    /// <summary> <H4>ContentContainer-Parser</H4> 
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class ContentContainerBuilder
        : MessagePartParser
    {
        /// <summary> Gets the contentContainer attribute of the ContentContainerBuilder object
        /// </summary>
        /// <value> The contentContainer value
        /// </value>
        public ContentContainer ContentContainer
        {
            get
            {
                return _coco;
            }
        }

        private static readonly Log _log = LogFactory.GetLog(typeof(ContentContainerBuilder));
        private bool _readContent;
        private bool _underlayingCoco;
        private ContentContainerBuilder _cocoBuilder;
        private OsciDataSource _swapBuffer;
        private Attachment _attachment;
        private string _contentId;
        private string _cocoId = null;
        private readonly ContentContainer _coco;
        private EncryptedDataBuilder _encDataBuilder;
        private OsciSignatureBuilder _sigBuilder;
        private StreamWriter _osw;
        private Stream _b64;

        // Canonizer, aus dem der Parser liest 
        internal Canonizer Can;

        private readonly string _cocoNs;
        private readonly string _soapNsPrefix;
        private readonly string _osciNsPrefix;
        private readonly string _dsNsPrefix;
        private readonly string _xencNsPrefix;
        private readonly string _xsiNsPrefix;

        /// <summary>  Constructor for the ContentContainerBuilder object
        /// </summary>
        /// <param name="xmlReader">
        /// </param>
        /// <param name="parentHandler">
        /// </param>
        /// <param name="parentMessage">
        /// </param>
        /// <param name="atts">
        /// </param>
        public ContentContainerBuilder(XmlReader xmlReader, DefaultHandler parentHandler, OsciMessage parentMessage, Attributes atts, Canonizer can)
            : base(xmlReader, parentHandler)
        {
            Msg = parentMessage;

            _coco = atts.GetValue("Id") != null
                ? new ContentContainer(atts.GetValue("Id"))
                : new ContentContainer();
            _log.Debug("coco ID: " + atts.GetValue("Id"));

            _cocoNs = can.ContainerNs[0];
            _coco.Ns = _cocoNs.ToByteArray();
            string[] ns = _cocoNs.Substring(1).Split(" =".ToCharArray());
            string prefix;
            string uri;
            for (int i = 0; i < ns.Length; i += 2)
            {
                prefix = ns[i];
                prefix = prefix.Substring(prefix.IndexOf(":") + 1);
                uri = ns[i + 1];
                uri = uri.Substring(1, uri.Length - 2);
                if (uri.Equals(SoapXmlns))
                {
                    _soapNsPrefix = prefix;
                }
                else if (uri.Equals(OsciXmlns))
                {
                    _osciNsPrefix = prefix;
                }
                else if (uri.Equals(DsXmlns))
                {
                    _dsNsPrefix = prefix;
                }
                else if (uri.Equals(XencXmlns))
                {
                    _xencNsPrefix = prefix;
                }
                else if (uri.Equals(XsiXmlns))
                {
                    _xsiNsPrefix = prefix;
                }
            }
            _coco.SetNamespacePrefixes(_soapNsPrefix, _osciNsPrefix, _dsNsPrefix, _xencNsPrefix, _xsiNsPrefix);
            can.ContainerNs.RemoveAt(0);
            _coco.StateOfObject = ContentContainer.StateOfObjectParsing;

            Can = can;
        }

        /// <summary>
        /// </summary>
        /// <param name="uri">
        /// </param>
        /// <param name="localName">
        /// </param>
        /// <param name="qName">
        /// </param>
        /// <param name="attributes">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("start-Element: " + qName);
            if (localName.Equals("Signature") && uri.Equals(DsXmlns))
            {
                _sigBuilder = new OsciSignatureBuilder(XmlReader, this, attributes, true);
                _sigBuilder.OsciSignature.SetNamespacePrefixes(_soapNsPrefix, _osciNsPrefix, _dsNsPrefix, _xencNsPrefix, _xsiNsPrefix);
                XmlReader.ContentHandler = _sigBuilder;
            }
            else if (localName.Equals("Content") && uri.Equals(OsciXmlns))
            {
                _contentId = attributes.GetValue("Id");
                if (attributes.GetValue("href") != null)
                {
                    string href = attributes.GetValue("href").Substring(4);
                    if (Msg.AttachmentList.ContainsKey(href))
                    {
                        _attachment = (Attachment)Msg.AttachmentList[href];
                        _log.Trace("########## Attachment gefunden. ");
                    }
                    else
                    {
                        _log.Debug("Attachment wird der Nachricht hinzugefügt.");

                        try
                        {
                            if (Msg.IsSigned)
                            {
                                _attachment = new Attachment(null, href, 0, (string)Msg.SignatureHeader.GetDigestMethods()["cid:" + href]);
                            }
                            else
                            {
                                _attachment = new Attachment(null, href, 0, null);
                            }
                        }
                        catch (Exception ex1)
                        {
                            throw new SaxException(ex1);
                        }
                        Msg.AttachmentList.Put(_attachment.RefId, _attachment);
                        _attachment.StateOfAttachment = Attachment.StateOfAttachmentParsing;
                    }
                }
            }
            else if (localName.Equals("Base64Content") && uri.Equals(OsciXmlns))
            {
                _contentId = attributes.GetValue("Id");
                _readContent = true;
                try
                {
                    _swapBuffer = DialogHandler.NewDataBuffer;
                    _osw = new StreamWriter(_swapBuffer.OutputStream);
                }
                catch (Exception ex)
                {
                    throw new SaxException(ex);
                }
            }
            else if (localName.ToUpper().Equals("EncryptedData".ToUpper()) && uri.Equals(XencXmlns))
            {
                if (_encDataBuilder != null)
                {
                    _log.Debug("Encrypted-Data wird hinzugefügt.");
                    EncryptedDataOsci enc = new EncryptedDataOsci(_encDataBuilder.EncryptedData, Msg);
                    enc.Namespace = _cocoNs;
                    enc.SetNamespacePrefixes(_soapNsPrefix, _osciNsPrefix, _dsNsPrefix, _xencNsPrefix, _xsiNsPrefix);
                    _coco.AddEncryptedData(enc);
                }
                _encDataBuilder = new EncryptedDataBuilder(XmlReader, this, attributes);
                XmlReader.ContentHandler = _encDataBuilder;
            }
            else if (localName.Equals("ContentContainer") && uri.Equals(OsciXmlns))
            {
                _readContent = false;
                _underlayingCoco = true;
                _cocoBuilder = new ContentContainerBuilder(XmlReader, this, Msg, attributes, Can);
                XmlReader.ContentHandler = _cocoBuilder;
            }
            else
            {
                throw new SaxException("Unerwartetes Element im ContentContainer: " + localName);
            }
        }

        /// <summary> 
        /// </summary>
        /// <param name="parm1">
        /// </param>
        /// <param name="parm2">
        /// </param>
        /// <param name="parm3">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void Characters(char[] ch, int start, int length)
        {
            _log.Trace("readContent: " + _readContent + " " + new string(ch, start, length));
            if (_readContent)
            {
                _osw.Write(new string(ch, start, length));
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    if (ch[start + i] > ' ')
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_char"));
                    }
                }
            }
        }

        /// <summary>  
        /// </summary>
        /// <param name="uri">
        /// </param>
        /// <param name="localName">
        /// </param>
        /// <param name="qName">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End-Element: " + qName);
            // Ein Content Element wird beendet und dem coco zugefügt
            if (localName.Equals("Content") && uri.Equals(OsciXmlns))
            {
                if (_attachment != null)
                {
                    Content co = new Content(_contentId, _attachment);
                    co.Transformers[0] = "<" + _dsNsPrefix + ":Transform Algorithm=\"" + Constants.TransformCanonicalization + "\"></" + _dsNsPrefix + ":Transform>";
                    co.coNS = _cocoNs;
                    co.SetNamespacePrefixes(_soapNsPrefix, _osciNsPrefix, _dsNsPrefix, _xencNsPrefix, _xsiNsPrefix);
                    _log.Trace("RefID: " + _contentId);
                    _coco.AddContent(co, true);
                    _attachment = null;
                }
            }
            else if (localName.Equals("Base64Content") && uri.Equals(OsciXmlns))
            {
                _osw.Close();
                Content co = new Content(_contentId, _swapBuffer);
                co.coNS = _cocoNs;
                co.SetNamespacePrefixes(_soapNsPrefix, _osciNsPrefix, _dsNsPrefix, _xencNsPrefix, _xsiNsPrefix);
                _coco.AddContent(co, true);
                _readContent = false;
                _swapBuffer = null;
            }
            else if (localName.Equals("ContentContainer") && uri.Equals(OsciXmlns))
            {
                if (_underlayingCoco)
                {
                    ContentContainer newCoco = _cocoBuilder.ContentContainer;
                    Content tmpCnt = new Content(_contentId, newCoco);
                    tmpCnt.Transformers[0] = "<" + _cocoBuilder._dsNsPrefix + ":Transform Algorithm=\"" + Constants.TransformCanonicalization + "\"></" + _cocoBuilder._dsNsPrefix + ":Transform>";
                    Content cocoTmp = new Content(newCoco);
                    cocoTmp.coNS = _cocoBuilder._cocoNs;
                    tmpCnt.SetNamespacePrefixes(_cocoBuilder._soapNsPrefix, _cocoBuilder._osciNsPrefix, _cocoBuilder._dsNsPrefix, _cocoBuilder._xencNsPrefix, _cocoBuilder._xsiNsPrefix);
                    _coco.AddContent(tmpCnt, true);
                    _underlayingCoco = false;
                }
                else
                {
                    // gibt es noch weitere encryptedData Elemente die dem coco noch nicht hinzugefügt wurden
                    if (_encDataBuilder != null)
                    {
                        _log.Debug("Encrypted-Data wird hinzugefügt.Parent Message: ");
                        EncryptedDataOsci enc = new EncryptedDataOsci(_encDataBuilder.EncryptedData, Msg);
                        enc.SetNamespacePrefixes(_soapNsPrefix, _osciNsPrefix, _dsNsPrefix, _xencNsPrefix, _xsiNsPrefix);
                        enc.Namespace = _cocoNs;
                        _coco.AddEncryptedData(enc);
                        _encDataBuilder = null;
                    }
                    if (_coco.SignerList.Count > 0)
                    {
                        Dictionary<string, OsciSignatureReference> refs = ((OsciSignature)_coco.SignerList[0]).GetReferences();
                        Content[] cnt = _coco.Contents;
                        string refKey;
                        for (int i = 0; i < cnt.Length; i++)
                        {
                            refKey = "#" + cnt[i].RefId;
                            cnt[i].Transformers = refs[refKey].TransformerAlgorithms;
                        }
                    }
                    // zurück zum Parent
                    XmlReader.ContentHandler = ParentHandler;
                    _log.Trace("parentHandler: " + ParentHandler);
                    ParentHandler.EndElement(uri, localName, qName);
                }
            }
            else if (localName.Equals("EncryptedData") && uri.Equals(XencXmlns))
            {
                if (_encDataBuilder != null)
                {
                    EncryptedDataOsci enc = new EncryptedDataOsci(_encDataBuilder.EncryptedData, Msg);
                    enc.SetNamespacePrefixes(_soapNsPrefix, _osciNsPrefix, _dsNsPrefix, _xencNsPrefix, _xsiNsPrefix);
                    enc.Namespace = _cocoNs;
                    _coco.AddEncryptedData(enc);
                }
                _encDataBuilder = null;
            }
            else
            {
                _log.Trace("Name des Elemtes: " + localName);
                throw new SaxException("Nicht erwartetes Element im ContentContainer");
            }
        }
    }
}