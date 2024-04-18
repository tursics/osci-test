using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Osci.Common;
using Osci.Interfaces;
using Attribute = Osci.Common.Attribute;

namespace Osci.Helper
{
    /// <exclude/>
    /// <summary>
    /// Zusammenfassung für XMLReader.
    /// </summary>
    public class XmlReader
    {
        private DefaultHandler _contentHandler;
        private static readonly Log _log = LogFactory.GetLog(typeof(XmlReader));

        private readonly XmlStructureValidator _structureValidator;

        public XmlReader()
        {
            _structureValidator = new XmlStructureValidator(XmlStructureValidator.ValidationRule.CreateOsciMessageValidationRules);
        }

        public XmlReader(XmlStructureValidator structureValidator)
        {
            _structureValidator = structureValidator;
        }

        public DefaultHandler ContentHandler
        {
            get { return _contentHandler;  }
            set { _contentHandler = value; }
        }

        public void Parse(Stream input)
        {
            Parse(input, true);
        }

        public void Parse(Stream input, bool supportNamespaces)
        {
            XmlTextReader reader = new XmlTextReader(input)
            {
                Namespaces = supportNamespaces,
                XmlResolver = null, // must be null since XXE is not supported because of potential attacks!
            };

            string startElement = null;
            string startNs = null;
            int nestingCounter = 0;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Whitespace:
                        if (startElement == null)
                        {
                            break;
                        }
                        char[] ws = reader.Value.ToCharArray();
                        _contentHandler.Characters(ws, 0, ws.Length);
                        break;

                    case XmlNodeType.Attribute:
                    // XXE is not supported because of potential attacks!
                    case XmlNodeType.EntityReference:
                    case XmlNodeType.Entity:
                    case XmlNodeType.EndEntity:
                        break;

                    case XmlNodeType.Element:
                        {
                            string localName = reader.LocalName;
                            string namespaceUri = reader.NamespaceURI;
                            string name = reader.Name;
                            bool isEmptyElement = reader.IsEmptyElement;

                            _log.Trace("START ELEM " + name + ":" + _contentHandler);

                            if (startElement == null)
                            {
                                startElement = name;
                                startNs = namespaceUri;
                                _contentHandler.StartDocument();
                            }
                            else if (startElement.Equals(name) && startNs.Equals(namespaceUri))
                            {
                                nestingCounter++;
                            }

                            Attributes atts = CreateAttributeList(reader);

                            _structureValidator.StartElement(localName, namespaceUri);
                            _contentHandler.StartElement(namespaceUri, localName, name, atts);

                            if (isEmptyElement)
                            {
                                _structureValidator.EndElement(localName, namespaceUri);
                                _contentHandler.EndElement(namespaceUri, localName, name);
                            }
                        }
                        break;

                    case XmlNodeType.EndElement:
                        {
                            string localName = reader.LocalName;
                            string namespaceUri = reader.NamespaceURI;

                            if (_log.IsEnabled(LogLevel.Trace))
                            {
                                _log.Trace("END ELEM " + reader.Name + ":" + _contentHandler);
                            }

                            _structureValidator.EndElement(localName, namespaceUri);

                            _contentHandler.EndElement(namespaceUri, localName, reader.Name);

                            if (reader.Name.Equals(startElement) && startNs.Equals(namespaceUri))
                            {
                                if (nestingCounter > 0)
                                {
                                    nestingCounter--;
                                }
                                else
                                {
                                    _contentHandler.EndDocument();
                                    input.Close();
                                    return;
                                }
                            }
                        }
                        break;

                    case XmlNodeType.Text:
                        _contentHandler.Characters(reader.Value.ToCharArray(), 0, reader.Value.Length);
                        break;
                        // There are many other types of nodes, but
                        // we are not interested in them
                }
            }
        }


        private Attributes CreateAttributeList(XmlTextReader reader)
        {
            if (reader.HasAttributes)
            {
                List<Attribute> attributes = new List<Attribute>();
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    Attribute attribute = new Attribute(reader.Prefix,
                        reader.LocalName,
                        reader.NamespaceURI,
                        reader.Value);
                    if (reader.Prefix.ToLower().Equals("xmlns"))
                    {
                        _contentHandler.StartPrefixMapping(reader.LocalName, reader.Value);
                    }

                    attributes.Add(attribute);
                }
                return new Attributes(attributes);
            }
            else
            {
                return Attributes.Empty;
            }
        }

    }
}
