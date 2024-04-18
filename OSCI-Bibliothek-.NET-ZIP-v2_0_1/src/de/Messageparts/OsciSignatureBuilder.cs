using System;
using System.Linq;
using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Messagetypes;
using Osci.Roles;

namespace Osci.MessageParts
{
    /// <summary><H4>Signature-Parser</H4>
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
    // Transformationen werden hier nicht unterstützt
    public class OsciSignatureBuilder
        : MessagePartParser
    {
        internal OsciSignature OsciSignature;

        private static readonly Log _log = LogFactory.GetLog(typeof(OsciSignatureBuilder));
        private OsciSignatureReference _signatureReference;
        private string _signingTime;
        private string _signingPropsId;

        Boolean insideSignature = false;

        Boolean? insideSignedInfo = null;

        Boolean insideReference = false;

        Boolean insideObject = false;

        Boolean insideXades = false;

        Boolean insideKeyInfo = false;

        /// <summary> Constructor for the OSCISignatureBuilder object
        /// </summary>
        /// <param name="xmlReader">
        /// </param>
        /// <param name="parentHandler">
        /// </param>
        /// <param name="atts">
        /// </param>
        public OsciSignatureBuilder(XmlReader xmlReader, DefaultHandler parentHandler, Attributes atts, Boolean signatureElementAvaliable)

            : base(xmlReader, parentHandler)
        {
            insideSignature = signatureElementAvaliable;

            if (parentHandler is OsciMessageBuilder)
            {
                OsciMessage osciMessage = ((OsciMessageBuilder)parentHandler).OsciMessage;
                string name = "<" + osciMessage.OsciNsPrefix + ":";
                string soap = osciMessage.SoapNsPrefix;
                name += ((OsciMessageBuilder)parentHandler).OsciMessage is OsciRequest ? "ClientSignature Id=\"" : "SupplierSignature Id=\"";
                name += atts.GetValue("Id") + "\" ";
                name += soap + ":actor=\"http://schemas.xmlsoap.org/soap/actor/next\" " + soap + ":mustUnderstand=\"1\">";
                OsciSignature = new OsciSignature(name);
                OsciSignature.SetNamespacePrefixes(soap, osciMessage.OsciNsPrefix, osciMessage.DsNsPrefix, osciMessage.XencNsPrefix, osciMessage.XsiNsPrefix);
            }
            else
            {
                OsciSignature = new OsciSignature();
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
        /// <param name="attributes">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start Element" + localName);

            if (uri.Equals(DsXmlns) && "Signature".Equals(localName) && !(insideSignature || insideKeyInfo || insideObject || insideReference || (insideSignedInfo != null && insideSignedInfo.Value) || insideXades))
            {
                insideSignature = true;
            }
            else if (insideSignature)
            {
                if (insideSignedInfo != null && insideSignedInfo.Value && uri.Equals(DsXmlns))
                {
                    if (localName.Equals("Reference"))
                    {
                        insideReference = true;
                        string rf = attributes.GetValue("URI");
                        _signatureReference = new OsciSignatureReference(rf);

                        _log.Trace("############ Anzahl refs Start:" + OsciSignature.Refs.Count);

                        try
                        {
                            OsciSignature.AddSignatureReference(_signatureReference);
                        }
                        catch (OsciErrorException ex)
                        {
                            _log.Error("Error add the reference!", ex);
                            throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName, ex);
                        }
                    }
                    else if (localName.Equals("CanonicalizationMethod"))
                    {
                        // do nothing
                    }
                    else if (insideReference)
                    {
                        if (localName.Equals("Transforms"))
                        {
                            // do nothing
                        }
                        else if (localName.Equals("Transform"))
                        {
                            try
                            {
                                CanParser cp = new CanParser(_signatureReference.TransformerAlgorithms, XmlReader, this, qName);
                                XmlReader.ContentHandler = cp;
                                cp.StartDocument();
                                cp.StartElement(uri, localName, qName, attributes);
                            }
                            catch (Exception ex)
                            {
                                throw new SaxException(DialogHandler.ResourceBundle.GetString("sax_exception_transfomation"), ex);
                            }
                        }
                        else if (localName.Equals("DigestMethod"))
                        {
                            _signatureReference.DigestMethodAlgorithm = attributes.GetValue("Algorithm");
                        }
                        else if (localName.Equals("DigestValue"))
                        {
                            CurrentElement = new System.Text.StringBuilder();
                        }
                        else
                        {
                            throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                        }
                    }
                    else if (localName.Equals("SignatureMethod"))
                    {
                        OsciSignature.SignatureAlgorithm = attributes.GetValue("Algorithm");

                        if (!Constants.GetAllSignatureAlgorithms().Any(_ => _.Equals(OsciSignature.SignatureAlgorithm)))
                        {
                            throw new SaxException(DialogHandler.ResourceBundle.GetString("invalid_signature_algorithm") + " " + attributes.GetValue("Algorithm"));
                        }
                    }
                    else
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                    }
                }
                else
                {
                    if (!insideKeyInfo && !insideObject && uri.Equals(DsXmlns))
                    {
                        if (localName.Equals("SignedInfo") && insideSignedInfo == null)
                        {
                            insideSignedInfo = true;
                        }
                        else if (localName.Equals("KeyInfo"))
                        {
                            insideKeyInfo = true;
                        }
                        else if (localName.Equals("Object"))
                        {
                            insideObject = true;
                        }
                        else if (localName.Equals("SignatureValue") && OsciSignature.SignatureValue == null)
                        {
                            CurrentElement = new System.Text.StringBuilder();
                        }
                        else
                        {
                            throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                        }
                    }
                    else if (insideKeyInfo && uri.Equals(DsXmlns) && localName.Equals("RetrievalMethod"))
                    {
                        string rf = attributes.GetValue("URI");
                        OsciSignature.SignerId = rf;
                    }
                    else if (insideObject && uri.Equals(XadesXmlns))
                    {
                        if (localName.Equals("QualifyingProperties"))
                        {
                            insideXades = true;
                        }
                        else if (insideXades)
                        {
                            if (localName.Equals("QualifyingProperties") || localName.Equals("SignedSignatureProperties"))
                            {

                            }
                            else if (localName.Equals("SigningTime"))
                            {
                                CurrentElement = new System.Text.StringBuilder();
                            }
                            else if (localName.Equals("SignedProperties"))
                            {
                                _signingPropsId = attributes.GetValue("Id");
                            }
                        }
                        else
                        {
                            throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                        }
                    }
                    else
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                    }
                }
            }
            else
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
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
            _log.Trace("End-Element: " + localName);
            try
            {
                if (insideSignature)
                {
                    if (uri.Equals(DsXmlns))
                    {
                        if (localName.Equals("Signature"))
                        {
                            insideSignature = false;
                            if (ParentHandler is ContentContainerBuilder)
                            {
                                _log.Trace("##########" + OsciSignature.SignerId);
                                string sigId = OsciSignature.SignerId;
                                if (sigId[0] == '#')
                                    sigId = sigId.Substring(1);
                                Role signer = ((ContentContainerBuilder)ParentHandler).Msg.GetRoleForRefId(sigId);

                                if (signer == null)
                                {
                                    _log.Error("Das Referenziert Signer Object konnte nicht gefunden werden.");
                                    throw new SaxException("Das Referenziert Signer Object konnte nicht gefunden werden.");
                                }
                                OsciSignature.Signer = signer;
                                ((ContentContainerBuilder)ParentHandler).ContentContainer.AddSignature(OsciSignature);
                                OsciSignature.SignedInfo = ((ContentContainerBuilder)ParentHandler).Can.SignedInfos[1];
                                ((ContentContainerBuilder)ParentHandler).Can.SignedInfos.RemoveAt(1);
                                if (_signingPropsId != null)
                                {
                                    OsciSignature.SigningProperties = ((ContentContainerBuilder)ParentHandler).Can.SignedProperties[0];
                                    ((ContentContainerBuilder)ParentHandler).Can.SignedProperties.RemoveAt(0);
                                    OsciSignature.SigningPropsId = _signingPropsId;
                                    OsciSignature.SigningTime = _signingTime;
                                }
                            }
                            else if (ParentHandler is OsciMessageBuilder)
                            {
                                if (((OsciMessageBuilder)ParentHandler).OsciMessage.SignatureHeader != null)
                                {
                                    throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                                }
                                ((OsciMessageBuilder)ParentHandler).OsciMessage.SignatureHeader = OsciSignature;
                            }

                            XmlReader.ContentHandler = ParentHandler;
                        }
                        else if (localName.Equals("Object"))
                        {
                            insideObject = false;
                        }
                        else if (localName.Equals("Reference"))
                        {
                            insideReference = false;
                            _log.Trace("############ Anzahl refs End :" + OsciSignature.Refs.Count);
                            _signatureReference = null;
                        }
                        else if (localName.Equals("SignedInfo"))
                        {
                            insideSignedInfo = false;
                        }
                        else if (localName.Equals("SignatureValue"))
                        {
                            OsciSignature.SignatureValue = Base64.Decode(CurrentElement.ToString());
                        }
                        else if (localName.Equals("KeyInfo"))
                        {
                            insideKeyInfo = false;
                        }
                        else if (insideReference)
                        {
                            if (localName.Equals("DigestValue"))
                            {
                                _log.Trace("STARTE B64DEC " + CurrentElement);
                                _log.Trace("ID " + _signatureReference.RefId);

                                byte[] dv = Base64.Decode(CurrentElement.ToString());
                                _signatureReference.digestValue = dv;
                            }
                        }
                    }
                    else if (insideXades && uri.Equals(XadesXmlns))
                    {
                        if (localName.Equals("QualifyingProperties"))
                        {
                            insideXades = false;
                        }
                        else if (localName.Equals("SigningTime") && ParentHandler is ContentContainerBuilder)
                        {
                            _signingTime = CurrentElement.ToString();
                        }
                        else if ((localName.Equals("SignedProperties") || localName.Equals("SignedSignatureProperties")))
                        {
                            // nothing to do
                        }
                        else
                        {
                            throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                        }
                    }
                    else
                    {
                        throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                    }
                }
                else
                {
                    throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Fehler im End-Element!", ex);
                throw new SaxException(ex);
            }
            CurrentElement = null;
        }
    }
}