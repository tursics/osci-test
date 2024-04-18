using System;
using System.IO;
using Osci.Common;
using Osci.Extensions;
using Osci.Helper;

namespace Osci.SoapHeader
{
    /// <exclude/>
    public class CustomHeader
        : HeaderEntry
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(CustomHeader));

        public string Data
        {
            get; private set;
        }

        public CustomHeader(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            int i = data.IndexOf(" Id=");
            if (i == -1)
            {
                i = data.IndexOf(" Id =");
            }
            if ((i == -1) || (i > data.IndexOf('>')))
            {
                throw new Exception(DialogHandler.ResourceBundle.GetString("missing_entry") + ": Id");
            }
            else
            {
                i = data.IndexOf("\"", i) + 1;
            }

            RefId = data.Substring(i, data.IndexOf('\"', i) - i);
            Data = data;
        }

        public override void WriteXml(Stream stream)
        {
            _log.Debug("Custom RefID: " + RefId);
            stream.Write(Data);
        }
    }
}
