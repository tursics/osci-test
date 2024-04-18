using Osci.Common;
using Osci.Extensions;
using System.Collections.Generic;
using System.IO;
using static Osci.Common.Constants;

namespace Osci.SoapHeader
{
    public class FeatureDescriptionH : HeaderEntry
    {
        public List<OsciFeatures> SupportedFeatures { get; set; } = new List<OsciFeatures>();

        /*
         * Initial mit dem neutralem Wert -1 instanziert
         */
        public long MaxMessageSize { get; set; } = -1;

        public long MaxChunkSize { get; set; } = -1;

        public long MinChunkSize { get; set; } = -1;

        public long ChunkMessageTimeout { get; set; } = -1;

        // Dies sind die default Features der aktuellen Version
        public static readonly FeatureDescriptionH DefaultFeatureDescription = new FeatureDescriptionH();

        public FeatureDescriptionH()
        {
        }

        internal FeatureDescriptionH(string refId)
        {
            RefId = refId;
        }

        static FeatureDescriptionH()
        {
            DefaultFeatureDescription.SupportedFeatures.Add(OsciFeatures.DisableBase64);
			DefaultFeatureDescription.SupportedFeatures.Add(OsciFeatures.GCMPaddingModus);
			DefaultFeatureDescription.SupportedFeatures.Add(OsciFeatures.OAEPEncryption);
			DefaultFeatureDescription.SupportedFeatures.Add(OsciFeatures.PartialMessageTransmission);
			DefaultFeatureDescription.SupportedFeatures.Add(OsciFeatures.SHA3HashAlgo);
			DefaultFeatureDescription.SupportedFeatures.Add(OsciFeatures.Support96Bit12ByteIV);
		}

        public override void WriteXml(Stream stream)
        {
            stream.Write("<" + Osci2017NsPrefix + ":FeatureDescription");
            stream.Write(Ns2017, 0, Ns2017.Length);

            // lexikografische Reihenfolge der Attribute ist entscheidend fuer die Signaturpruefung!

            if (ChunkMessageTimeout != -1)
            {
                stream.Write(" ChunkMessageTimeout=\"" + ChunkMessageTimeout + "\"");
            }

            stream.Write(" Id=\"" + RefId + "\"");

            if (MaxChunkSize != -1)
            {
                stream.Write(" MaxChunkSize=\"" + MaxChunkSize + "\"");
            }

            if (MaxMessageSize != -1)
            {
                stream.Write(" MaxMessageSize=\"" + MaxMessageSize + "\"");
            }

            if (MinChunkSize != -1)
            {
                stream.Write(" MinChunkSize=\"" + MinChunkSize + "\"");
            }

            stream.Write(">");

            if (SupportedFeatures != null && SupportedFeatures.Count > 0)
            {
                stream.Write("<" + Osci2017NsPrefix + ":SupportedFeatures>");

                foreach (OsciFeatures feature in SupportedFeatures)
                {
                    stream.Write("<");
                    stream.Write(Osci2017NsPrefix);
                    string featureName = OsciFeatures.GetNameFromInstance(feature);
                    stream.Write(":Feature Key=\"" + featureName + "\"");
                    stream.Write(" Version=\"" + feature.Version + "\">");
                    stream.Write("</");
                    stream.Write(Osci2017NsPrefix);
                    stream.Write(":Feature>");
                }
                stream.Write("</" + Osci2017NsPrefix + ":SupportedFeatures>");
            }
            stream.Write("</" + Osci2017NsPrefix + ":FeatureDescription>");
        }
    }
}