using System;
using System.Text;
using Osci.Common;

namespace Osci.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string AsString(this byte[] bytes, string encoding = Constants.CharEncoding)
        {
            Encoding enc = Encoding.GetEncoding(encoding);
            return enc.GetString(bytes);
        }

        public static string AsHexString(this byte[] bytes, bool removeDashes = true)
        {
            string hexString = BitConverter.ToString(bytes);
            return removeDashes ? hexString.Replace("-", "") : hexString;
        }
    }
}
