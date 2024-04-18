using System.Text;
using Osci.Common;

namespace Osci.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToByteArray(this string aString, string encoding = Constants.CharEncoding)
        {
            Encoding encoder = Encoding.GetEncoding(encoding);
            return encoder.GetBytes(aString);
        }
    }
}
