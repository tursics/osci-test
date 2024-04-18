using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Messagetypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Osci.SoapHeader
{
    public class FeatureDescriptionHBuilder : MessagePartParser
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(FeatureDescriptionHBuilder));

        private FeatureDescriptionH featureDesc = null;

        private bool insideSupportedFeatures = false;

        /// <summary> Constructor for the FeatureDescriptionHBuilder object
        /// </summary>
        /// <param name="parentHandler">
        /// </param>
        /// <param name="attributes">
        /// </param>
        public FeatureDescriptionHBuilder(OsciMessageBuilder parentHandler, Attributes attributes) : base(parentHandler)
        {
            parentHandler.SignatureRelevantElements.AddElement("FeatureDescription", Osci2017Xmlns, attributes);

            string id = attributes.GetValue("Id");
            if (id != null)
            {
                featureDesc = new FeatureDescriptionH(id);
            }
            else
            {
                featureDesc = new FeatureDescriptionH();
            }

            featureDesc.SetNamespacePrefixes(Msg);
        }

        /// <summary> Liefert die konstruierte FeatureDescription </summary>
        public FeatureDescriptionH getFeatureDescriptionH()
        {
            return featureDesc;
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
        public override void StartElement(String uri, String localName, String qName, Attributes attributes)
        {
            _log.Trace("FeatureDescription Element: " + localName);

            if (localName.Equals("FeatureDescription") && uri.Equals(Osci2017Xmlns))
            {
                if (attributes.GetValue("MaxChunkSize") != null)
                {
                    featureDesc.MaxChunkSize = Convert.ToInt64(attributes.GetValue("MaxChunkSize"));
                }
                if (attributes.GetValue("MaxMessageSize") != null)
                {
                    featureDesc.MaxMessageSize = Convert.ToInt64(attributes.GetValue("MaxMessageSize"));
                }
                if (attributes.GetValue("MinChunkSize") != null)
                {
                    featureDesc.MinChunkSize = Convert.ToInt64(attributes.GetValue("MinChunkSize"));
                }
                if (attributes.GetValue("ChunkMessageTimeout") != null)
                {
                    featureDesc.ChunkMessageTimeout = Convert.ToInt64(attributes.GetValue("ChunkMessageTimeout"));
                }
            }
            else if ("SupportedFeatures".Equals(localName) && uri.Equals(Osci2017Xmlns))
            {
                insideSupportedFeatures = true;
            }
            else if (insideSupportedFeatures && "Feature".Equals(localName)
                     && uri.Equals(Osci2017Xmlns))
            {
                if (attributes.GetValue("Key") != null)
                {
                    OsciFeatures feature = OsciFeatures.GetInstanceFromString(attributes.GetValue("Key"));

                    if (feature != null)
                    {
                        featureDesc.SupportedFeatures.Add(feature);
                    }
					else
					{
						_log.Info("Could not parse key from feature description! Unknown feature (maybe new?): " 
							+ attributes.GetValue("Key"));
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
        public override void EndElement(String uri, String localName, String qName)
        {
            _log.Trace("End-Element Von FeatureDescription: " + localName);

            if ("SupportedFeatures".Equals(localName) && uri.Equals(Osci2017Xmlns))
            {
                insideSupportedFeatures = false;
            }
            else if ("Feature".Equals(localName) && uri.Equals(Osci2017Xmlns))
            {
                // nothing to do
            }
            else if ("FeatureDescription".Equals(localName) && uri.Equals(Osci2017Xmlns))
            {
                XmlReader.ContentHandler = ParentHandler;
                Msg.FeatureDescription = featureDesc;
            }
            else
            {
                throw new SaxException(DialogHandler.ResourceBundle.GetString("unexpected_entry") + ": " + localName);
            }
        }
    }
}