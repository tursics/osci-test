using System;
using Osci.Extensions;

namespace Osci.Helper
{
    /// <summary><p> Wrapper zum Base64-Ver-/Entschlüsseln kleinerer Datenmengen.</p>
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
    public class Base64
    {
        private const int _bufferSize = 4096;
        public Base64()
        {
        }

        public static string Encode(string s)
        {
            byte[] binaryData = s.ToByteArray();
            return Encode(binaryData);
        }

        public static string Encode(string s, string encoding)
        {
            byte[] binaryData = s.ToByteArray(encoding);
            return Encode(binaryData);
        }

        public static string Encode(byte[] binaryData)
        {
            // TODO
            string b64 = Convert.ToBase64String(binaryData, 0, binaryData.Length);
            string ret = "";
            int i = 0;
            while (i + 76 < b64.Length)
            {
                ret += b64.Substring(i, 76) + "\n";
                i += 76;
            }
            ret += b64.Substring(i, b64.Length - i);
            return ret;
        }

        public static byte[] Decode(string data)
        {
            return Convert.FromBase64String(data);
        }

        public static long CalculateBase64Length(long len)
        {
            if (len == 0)
            {
                return 0;
            }
            long l = ((len - 1) / 3 * 4) + 4;
            return l + (l / 76);
        }
    }
}
