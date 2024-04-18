using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Osci.Common
{
    public class OsciFeatures
    {
        private OsciFeatures(string version)
        {
            Version = version;
        }

        public static OsciFeatures DisableBase64 { get; } = new OsciFeatures("1.2.0");
        public static OsciFeatures OAEPEncryption { get; } = new OsciFeatures("1.6.0");
        public static OsciFeatures SHA3HashAlgo { get; } = new OsciFeatures("1.6.3");
        public static OsciFeatures GCMPaddingModus { get; } = new OsciFeatures("1.7.0");
        public static OsciFeatures PartialMessageTransmission { get; } = new OsciFeatures("1.8.0");
		public static OsciFeatures Support96Bit12ByteIV { get; } = new OsciFeatures("1.9.0");

		public string Version { get; }

        public static string GetNameFromInstance(OsciFeatures feature)
        {
            if (feature != null)
            {
                if (feature.Equals(DisableBase64))
                {
                    return nameof(DisableBase64);
                }
                else if (feature.Equals(OAEPEncryption))
                {
                    return nameof(OAEPEncryption);
                }
                else if (feature.Equals(SHA3HashAlgo))
                {
                    return nameof(SHA3HashAlgo);
                }
                else if (feature.Equals(GCMPaddingModus))
                {
                    return nameof(GCMPaddingModus);
                }
                else if (feature.Equals(PartialMessageTransmission))
                {
                    return nameof(PartialMessageTransmission);
                }
				else if (feature.Equals(Support96Bit12ByteIV))
				{
					return nameof(Support96Bit12ByteIV);
				}
			}
            return null;
        }

        public static OsciFeatures GetInstanceFromString(string name)
        {
            if (name != null)
            {
                if (name.Equals(nameof(DisableBase64)))
                {
                    return DisableBase64;
                }
                else if (name.Equals(nameof(OAEPEncryption)))
                {
                    return OAEPEncryption;
                }
                else if (name.Equals(nameof(SHA3HashAlgo)))
                {
                    return SHA3HashAlgo;
                }
                else if (name.Equals(nameof(GCMPaddingModus)))
                {
                    return GCMPaddingModus;
                }
                else if (name.Equals(nameof(PartialMessageTransmission)))
                {
                    return PartialMessageTransmission;
                }
				else if (name.Equals(nameof(Support96Bit12ByteIV)))
				{
					return Support96Bit12ByteIV;
				}
			}
            return null;
        }
    }

}
