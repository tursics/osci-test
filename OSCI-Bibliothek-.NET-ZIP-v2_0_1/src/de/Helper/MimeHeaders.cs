using System;
using System.Collections;
using Osci.Extensions;

namespace Osci.Helper
{
    public class MimeHeaders
    {
        public string Version
        {
            get; private set;
        }

        public string ContentId
        {
            get; private set;
        }

        public string ContentType
        {
            get; private set;
        }

        public long ContentLength
        {
            get; private set;
        }

        public string Encoding
        {
            get; private set;
        }

        public string ContentTransferEncoding
        {
            get; private set;
        }

        public string Boundary
        {
            get; private set;
        }


        private readonly Hashtable _rawHeaders;

        internal MimeHeaders(Hashtable headers)
        {
            _rawHeaders = headers;

            Version = GetHeader("mime-Version");
            ContentId = GetContentId();
            ContentTransferEncoding = GetHeader("content-transfer-encoding");

            ContentLength = GetContentLength();
            ParseContentType();
        }

        public string GetHeader(string key)
        {
            return _rawHeaders[key] as string;
        }

        public void SetHeader(string key, string value)
        {
            _rawHeaders[key] = value;
        }

        public Hashtable GetHashtable()
        {
            return new Hashtable(_rawHeaders);
        }


        private void ParseContentType()
        {
            string contentType = GetHeader("content-type");
            Hashtable contentTypeHashtable = ParseHeader("content-type", contentType);
            Boundary = contentTypeHashtable["boundary"] as string;
            ContentType = contentTypeHashtable["content-type"] as string;
            Encoding = contentTypeHashtable["charset"] as string;
        }

        private string GetContentId()
        {
            string id = GetHeader("content-id");
            return string.IsNullOrEmpty(id) ? null : id.TrimStart('<').TrimEnd('>');
        }


        private long GetContentLength()
        {
            string length = GetHeader("content-length");
            return string.IsNullOrEmpty(length) ? -1 : Convert.ToInt64(length);
        }


        private static Hashtable ParseHeader(string fieldName, string header)
        {
            Hashtable lparams = new Hashtable();
            string[] ct = header.Split(';');
            lparams.Put(fieldName, ct[0].Trim());

            char[] sep = { '=' };
            for (int i = 1; i < ct.Length; i++)
            {
                string[] tmp = ct[i].Split(sep, 2);
                if (tmp[1].StartsWith("\"") && tmp[1].EndsWith("\""))
                {
                    tmp[1] = tmp[1].Substring(1, tmp[1].Length - 2);
                }
                lparams.Put(tmp[0].Trim(), tmp[1].Trim());
            }

            return lparams;
        }
    }
}