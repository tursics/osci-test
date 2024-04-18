using System;
using System.IO;
using Osci.Common;
using Osci.Helper;
using Osci.Interfaces;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    /// <summary> Diese	Klasse ist die	Superklasse	aller	OSCI-Auftragsnachrichtenobjekte.
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
    public abstract class OsciRequest
        : OsciMessage
    {
        /// <todo>falls ein statisches Parserobjekt unperformant	ist, 
        /// evtl. eine getInstance()-Methode in den	Parser einbauen
        /// </todo>
        internal static IncomingMessageParser Parser = new PassiveRecipientParser();

        protected internal string UriReceiver;

        /// <summary> Diese Methode liefert	die im DialogHandler	gesetzte	Liste	der gewünschten Sprachen.
        /// </summary>
        /// <value> Liste der Sprachkürzel, getrennt durch Leerzeichen,z.B. "de	en-US	fr"
        /// </value>
        /// <seealso href="de.osci.osci12.common.DialogHandler.LanguageList()">
        /// </seealso>
        public string DesiredLanguages
        {
            get
            {
                return DialogHandler.LanguageList;
            }
        }

        internal OsciRequest()
        {
        }

        internal OsciRequest(DialogHandler dh)
            : base(dh)
        {
            FeatureDescription = FeatureDescriptionH.DefaultFeatureDescription;
            DesiredLanguagesH = new DesiredLanguagesH(dh.LanguageList);
        }

        protected virtual OsciMessage Transmit(Stream outp, Stream inp)
        {
            ITransport transport = DialogHandler.TransportModule.NewInstance();
            bool isIntermed = this is AcceptDelivery || this is ProcessDelivery;

            Uri uri = isIntermed ? new Uri(UriReceiver) : ((Intermed)DialogHandler.Supplier).Uri;

            MemoryStream outRenamed = new MemoryStream();
            StoreOutputStream sos = null;

            if (DialogHandler.CreateSignatures)
            {
                Sign();
            }
            if (DialogHandler.Encryption)
            {
                new SoapMessageEncrypted(this, outp).WriteXml(outRenamed);
            }
            else
            {
                if (outp != null)
                {
                    sos = new StoreOutputStream(outRenamed, outp);
                    WriteXml(sos);
                }
                else
                {
                    WriteXml(outRenamed);
                }
            }

            Stream outRenamed2 = transport.GetConnection(uri, outRenamed.Length);
            outRenamed.WriteTo(outRenamed2);

            if (sos != null)
            {
                sos.Close();
            }
            outRenamed.Close();
            outRenamed2.Close();

            Stream rspStream = transport.ResponseStream;
            OsciMessage ret;
            try
            {
                ret = Parser.ParseStream(rspStream, DialogHandler, false, inp);
            }
            finally
            {
                rspStream.Close();
            }
            return ret;
        }

        /// <summary> Bringt	eine Client-Signatur	an.
        /// </summary>
        internal void Sign()
        {
            base.Sign(DialogHandler.Client);
        }
    }
}