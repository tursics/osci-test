using System.IO;
using System.Text;

namespace Osci.Common
{
    internal class SplitStream
        : OutputStream
    {
        public static string DefaultLineEnding
        {
            get { return Encoding.UTF8.GetString(_lineEnding); }
        }

        private static readonly byte[] _lineEnding = {0x0A};
        private int _index;
        private readonly Stream _stream;

        public SplitStream(Stream s)
        {
            _stream = s;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (_index + count > 76)
            {
                _stream.Write(buffer, offset, 76 - _index);
                _stream.Write(_lineEnding, 0, _lineEnding.Length);
                offset += 76 - _index;
                count -= 76 - _index;
                _index = 0;
            }

            _stream.Write(buffer, offset, count);
            _index += count;
        }

        public override void Close()
        {
            _stream.Close();
            base.Close();
        }
    }
}