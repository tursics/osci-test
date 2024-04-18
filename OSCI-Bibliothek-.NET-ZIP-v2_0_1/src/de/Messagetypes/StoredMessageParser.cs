using System.IO;
using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Signature;

namespace Osci.Messagetypes
{
    internal class StoredMessageParser 
        : IncomingMessageParser
    {
        protected override OsciEnvelopeBuilder GetParser(XmlReader reader, DialogHandler dh)
        {
            return new StoredEnvelopeBuilder(reader);
        }

        public StoredMessage ParseStream(Stream input)
        {
            try
            {
                return (StoredMessage)ParseStream(input, null, true, null);
            }
            catch (OsciSignatureException)
            {
                throw new OsciErrorException("9601");
            }
        }
    }
}