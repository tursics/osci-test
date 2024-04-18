using System.Collections.Generic;
using System.Text;
using Osci.Common;
using Osci.Exceptions;
using Osci.Helper;
using Osci.Interfaces;

namespace Osci.MessageParts
{
    /// <summary><H4>Inspection-Parser</H4>
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
    public class InspectionBuilder 
        : MessagePartParser
    {

        public Inspection InspectionObject
        {
            get
            {
                return Inspection;
            }

        }

        private static readonly Log _log = LogFactory.GetLog(typeof(InspectionBuilder));
        internal Inspection Inspection;
        private TimestampBuilder _timestampBuilder;
        private bool _isInsideOnlineResult;
        private readonly List<string> _onlineChecks = new List<string>();
        private readonly List<string> _onlineCheckNames = new List<string>();

        public InspectionBuilder(XmlReader xmLReader, DefaultHandler parenTHandler) : base(xmLReader, parenTHandler)
        {
            Inspection = new Inspection();
            ProcessCardBundle pcb = ((ProcessCardBundleBuilder)ParentHandler).ProcessCard;
            Inspection.SetNamespacePrefixes(pcb.SoapNsPrefix, pcb.OsciNsPrefix, pcb.DsNsPrefix, pcb.XencNsPrefix, pcb.XsiNsPrefix);
        }


        public override void StartElement(string uri, string localName, string qName, Attributes attributes)
        {
            _log.Trace("Start-Element: " + qName);
            if (localName.Equals("Timestamp") && uri.Equals(OsciXmlns))
            {
                _timestampBuilder = new TimestampBuilder(XmlReader, this);
                XmlReader.ContentHandler = _timestampBuilder;
            }
            else if (localName.Equals("X509IssuerName") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new StringBuilder();
            }
            else if (localName.Equals("X509SerialNumber") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new StringBuilder();
            }
            else if (localName.Equals("X509SubjectName") && uri.Equals(OsciXmlns))
            {
                CurrentElement = new StringBuilder();
            }
            else if (localName.Equals("CertType") && uri.Equals(OsciXmlns))
            {
                Inspection.CertType = attributes.GetValue("Type");
            }
            else if (localName.Equals("MathResult") && uri.Equals(OsciXmlns))
            {
                Inspection.MathResult = attributes.GetValue("Result").ToLower(); // TODO
                if (!Inspection.MathResult.Equals("ok") && !Inspection.MathResult.Equals("corrupted") && !Inspection.MathResult.Equals("indeterminate"))
                {
                    throw new IllegalArgumentException("Unexpected MathResult: " + Inspection.MathResult);
                }
            }
            else if (localName.Equals("OfflineResult") && uri.Equals(OsciXmlns))
            {
                Inspection.OfflineResult = attributes.GetValue("Result").ToLower();
                if (!Inspection.OfflineResult.Equals("valid") && 
                    !Inspection.OfflineResult.Equals("invalid") &&
                    !Inspection.OfflineResult.Equals("indeterminate"))
                {
                    throw new IllegalArgumentException("Unexpected OfflineResult: " + Inspection.OfflineResult);
                }
            }
            else if (localName.Equals("OnlineResult") && uri.Equals(OsciXmlns))
            {
                _isInsideOnlineResult = true;
                Inspection.IsOnlineChecked = true;
                Inspection.OnlineResult = attributes.GetValue("Result").Equals("ok");
            }
            else if (_isInsideOnlineResult)
            {
                if (localName.Equals("OCSP") && uri.Equals(OsciXmlns))
                {
                    CurrentElement = new StringBuilder();
                    _onlineCheckNames.Add("OCSP");
                }
                else if (localName.Equals("CRL") && uri.Equals(OsciXmlns))
                {
                    CurrentElement = new StringBuilder();
                    _onlineCheckNames.Add("CRL");
                }
                else if (localName.Equals("LDAP") && uri.Equals(OsciXmlns))
                {
                    CurrentElement = new StringBuilder();
                    _onlineCheckNames.Add("LDAP");
                }
                else
                {
                    throw new SaxException("Das Element " + localName + " ist im Content von Inspection nicht gültig");
                }
            }
        }


        public override void EndElement(string uri, string localName, string qName)
        {
            _log.Trace("End-Element: " + qName);
            if (localName.Equals("Inspection") && uri.Equals(OsciXmlns))
            {
                if ((Inspection.MathResult == null) || (Inspection.OfflineResult == null))
                {
                    throw new SaxException(DialogHandler.ResourceBundle.GetString("missing_entry") + ".");
                }
                Inspection.OnlineCheckNames = _onlineCheckNames.ToArray();
                Inspection.OnlineChecks = _onlineChecks.ToArray();
                ParentHandler.EndElement(uri, localName, qName);
                XmlReader.ContentHandler = ParentHandler;
            }
            else if (localName.Equals("Timestamp") && uri.Equals(OsciXmlns))
            {
                Inspection.TimeStamp = _timestampBuilder.TimestampObject;
                // hier sollte ein X509Certificate oder eine Ref auf ein bestehendes Zertifikat als Ersatz eingefügt werden
            }
            else if (localName.Equals("X509IssuerName") && uri.Equals(OsciXmlns))
            {
                Inspection.X509IssuerName = CurrentElement.ToString();
            }
            else if (localName.Equals("X509SerialNumber") && uri.Equals(OsciXmlns))
            {
                Inspection.X509SerialNumber = CurrentElement.ToString();
            }
            else if (localName.Equals("X509SubjectName") && uri.Equals(OsciXmlns))
            {
                Inspection.X509SubjectName = CurrentElement.ToString();
            }
            else if (localName.Equals("CertType") && uri.Equals(OsciXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("MathResult") && uri.Equals(OsciXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("OfflineResult") && uri.Equals(OsciXmlns))
            {
                // nothing to do
            }
            else if (localName.Equals("OnlineResult") && uri.Equals(OsciXmlns))
            {
                // dieser gilt für OCSP und CRL
                _isInsideOnlineResult = false;
            }
            else if (_isInsideOnlineResult && ((localName.Equals("OCSP") && uri.Equals(OsciXmlns))
                || (localName.Equals("CRL") && uri.Equals(OsciXmlns)) || (localName.Equals("LDAP") && uri.Equals(OsciXmlns))))
            {
                _onlineChecks.Add(CurrentElement.ToString());
            }
            else
            {
                throw new SaxException("Das Element " + localName + " ist im Content von Inspection nicht gültig");
            }
            CurrentElement = null;
        }
    }
}