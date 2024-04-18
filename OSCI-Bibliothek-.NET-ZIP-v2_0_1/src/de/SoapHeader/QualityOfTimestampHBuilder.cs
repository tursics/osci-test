using Osci.Common;
using Osci.Exceptions;
using Osci.MessageParts;
using Osci.Messagetypes;

namespace Osci.SoapHeader
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
    public class QualityOfTimestampHBuilder
        : MessagePartParser
    {

        /// <summary> Gets the qualityOfTimestampH attribute of the QualityOfTimestampHBuilder
        /// object
        /// </summary>
        /// <value>The qualityOfTimestampH value
        /// </value>
        public QualityOfTimestampH QualityOfTimestampH
        {
            get;
        }

        /// <summary> Constructor for the QualityOfTimestampHBuilder object
        /// </summary>
        /// <param name="parentHandler">
        /// </param>
        /// <param name="attributes">
        /// </param>
        /// <exception cref="SaxException">
        /// </exception>
        public QualityOfTimestampHBuilder(OsciMessageBuilder parentHandler, Attributes attributes)
            : base(parentHandler)
        {
            parentHandler.SignatureRelevantElements.AddElement("QualityOfTimestamp", OsciXmlns, attributes, attributes.GetValue("Service"));

            bool serviceRecption = attributes.GetValue("Service").Equals("reception");
            bool qualityCryptographic = attributes.GetValue("Quality").Equals("cryptographic");
            string id = attributes.GetValue("Id");

            QualityOfTimestampH = id == null
                ? new QualityOfTimestampH(serviceRecption, qualityCryptographic)
                : new QualityOfTimestampH(id, serviceRecption, qualityCryptographic);

            if (QualityOfTimestampH.ServiceReception)
            {
                parentHandler.OsciMessage.QualityOfTimestampTypeReception = QualityOfTimestampH;
            }
            else
            {
                parentHandler.OsciMessage.QualityOfTimestampTypeCreation = QualityOfTimestampH;
            }

            OsciMessage msg = parentHandler.OsciMessage;
            QualityOfTimestampH.SetNamespacePrefixes(msg);
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
            if (!(localName.Equals("QualityOfTimestamp") && uri.Equals(OsciXmlns)))
            {
                throw new SaxException("Unbekanntes Element gefunden: " + localName);
            }
            XmlReader.ContentHandler = ParentHandler;
        }
    }
}