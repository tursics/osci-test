using System.IO;
using System.Text;

namespace Osci.Helper
{
    internal class SplitWriter
        : StreamWriter
    {
        internal StreamWriter StreamWriter
        {
            get;
        }

        internal StringBuilder StringBuilder
        {
            get;
        }

        public SplitWriter(StreamWriter streamWriter)
            : base(new MemoryStream())
        {
            StreamWriter = streamWriter;
            StringBuilder = new StringBuilder();
        }



        #region Override

        public override void Write(char value)
        {
            StringBuilder.Append(value);
            StreamWriter.Write(value);
        }

        public override void Write(string value)
        {
            StringBuilder.Append(value);
            StreamWriter.Write(value);
        }

        public override void Write(int value)
        {
            Write((char)value);
        }

        public override void Write(char[] cbuf, int off, int len)
        {
            Write(new string(cbuf, off, len));
        }

        public override void Write(char[] buffer)
        {
            Write(new string(buffer));
        }

        public override void Close()
        {
            StreamWriter.Close();
        }

        public override void Flush()
        {
            StreamWriter.Flush();
        }

        #endregion
    }
}