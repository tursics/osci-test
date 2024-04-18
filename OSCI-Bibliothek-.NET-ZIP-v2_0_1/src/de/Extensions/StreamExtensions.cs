using System;
using System.IO;
using System.Text;
using Osci.Common;

namespace Osci.Extensions
{
    public static class StreamExtensions
    {
        public static void Write(this Stream stream, string value, string encoding = Constants.CharEncoding)
        {
            byte[] bytes = Encoding.GetEncoding(encoding).GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void CopyToStream(this Stream stream, Stream output)
        {
            byte[] buffer = new byte[16 * 1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        public static int CopyTo(this Stream stream, byte[] target, int start, int count)
        {
            byte[] receiver = new byte[count];
            int bytesRead = stream.Read(receiver, 0, count);

            if (bytesRead <= 0)
            {
                return -1;
            }

            Buffer.BlockCopy(receiver, 0, target, start, bytesRead);

            return bytesRead;
        }

        public static string AsString(this Stream stream, string encoding = Constants.CharEncoding)
        {
            MemoryStream memoryStream = stream as MemoryStream;
            if (memoryStream == null)
            {
                using (memoryStream = new MemoryStream())
                {
                    stream.CopyToStream(memoryStream);
                    return memoryStream.ToArray().AsString(encoding);
                }
            }
            else
            {
                return memoryStream.ToArray().AsString(encoding);
            }
        }

        public static byte[] ToArray(this Stream stream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyToStream(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
