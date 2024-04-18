using System.IO;
using Osci.Common;
using Osci.Extensions;

namespace Osci.Helper
{
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class Tools
    {
		/// <summary>
		/// Vergleiche zwei Byte-Arrays (time-attack-sicher)
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
        public static bool CompareByteArrays(byte[] a, byte[] b)
        {			
			uint diff = (uint)a.Length ^ (uint)b.Length;

			for (int i = 0; i < a.Length && i < b.Length; i++)
			{
				diff |= (uint)(a[i] ^ b[i]);
			}
			return diff == 0;
		}

		public static X509Certificate CreateCertificateFromBase64String(string base64String)
        {
            using (MemoryStream memoryStream = new MemoryStream(base64String.ToByteArray()))
            {
                using (Base64InputStream base64InputStream = new Base64InputStream(memoryStream))
                {
                    return new X509Certificate(base64InputStream);
                }
            }
        }

        /// <summary> Erzeugen eines Zertifikates.
        /// </summary>
        /// <param name="path">Path des einzulesenden Zertifikates
        /// </param>
        /// <returns> X509Certificate
        /// @throws CryptographicException
        /// </returns>
        public static X509Certificate CreateCertificate(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return new X509Certificate(data);
        }


        public static string CreateRandom(int length)
        {
            SupportClass.SecureRandomSupport random = new SupportClass.SecureRandomSupport();
            byte[] bytes = new byte[length];
            bytes = random.NextBytes(bytes);
            return Base64.Encode(bytes);
        }
    }
}