using System;
using System.IO;
using Osci.Common;
using Osci.Extensions;
using Osci.Helper;

namespace Osci.Signature
{
    /// <exclude/>
    /// <summary>
    /// <p>Copyright © 2021 Governikus GmbH &amp; Co. KG, Germany</p>
    /// <p>Erstellt von Governikus GmbH &amp; Co. KG</p>
    /// <p>Diese Bibliothek kann von jedermann nach Maßgabe der European Union
    /// Public Licence genutzt werden.</p><p>Die Lizenzbestimmungen können
    /// unter der URL <a href="https://eupl.eu/">https://eupl.eu/</a> abgerufen werden.</p>
    /// 
    /// <p>Author: P. Ricklefs, N. Büngener</p> 
    /// <p>Version: 2.0.1</p>
    /// </summary>
    public class X509Data
    {
        private X509Certificate _x509Cert;

        public X509Certificate X509Certificate
        {
            get
            {
                return _x509Cert;
            }
            set
            {
                if (_dataState > -1)
                {
                    throw new Exception("X509Data wurde bereits anders Instanziert");
                }
                _dataState = _stateCertificate;
                _x509Cert = value;
            }
        }

        public void SetX509Certificate(byte[] x509Certificate)
        {
            if (_dataState > -1)
            {
                throw new Exception("X509Data wurde bereits anders Instanziert");
            }
            else
            {
                _dataState = _stateCertificate;
            }
            _x509Cert = new X509Certificate(x509Certificate);
        }

        public void SetX509Certificate(X509Certificate x509Certificate)
        {
            if (_dataState > -1)
            {
                throw new Exception("X509Data wurde bereits anders Instanziert");
            }
            _dataState = _stateCertificate;
            _x509Cert = x509Certificate;
        }

        public string X509Crl
        {
            get;
            set;
        }

        public string X509Ski
        {
            get
            {
                return _x509Ski;
            }
            set
            {
                if (_dataState > -1)
                {
                    throw new Exception("X509Data wurde bereits anders Instanziert");
                }
                else
                {
                    _dataState = _stateSki;
                }
                _x509Ski = value;
            }
        }

        public string X509SubjectName
        {
            get
            {
                return _x509SubjectName;
            }
            set
            {
                if (_dataState > -1)
                {
                    throw new Exception("X509Data wurde bereits anders Instanziert");
                }
                else
                {
                    _dataState = _stateSubjectName;
                }
                _x509SubjectName = value;
            }
        }

        public string IssuerName
        {
            set
            {
                if (_dataState > -1 && _dataState != _stateIssuerSerial)
                {
                    throw new Exception("X509Data wurde bereits anders Instanziert");
                }
                else
                {
                    _dataState = _stateIssuerSerial;
                }
                if (_issuerSerial == null)
                {
                    _issuerSerial = new IssuerSerial(this);
                }
                _issuerSerial.X509IssuerName = value;
            }
        }

        public string SerialNumber
        {
            set
            {
                if (_dataState > -1 && _dataState != _stateIssuerSerial)
                {
                    throw new Exception("X509Data wurde bereits anders Instanziert");
                }
                else
                {
                    _dataState = _stateIssuerSerial;
                }
                if (_issuerSerial == null)
                {
                    _issuerSerial = new IssuerSerial(this);
                }
                _issuerSerial.X509SerialNumber = value;
            }
        }

        private string _x509Ski;
        private string _x509SubjectName;
        private IssuerSerial _issuerSerial;
        private int _dataState = -1;
        private const int _stateIssuerSerial = 0;
        private const int _stateSki = 1;
        private const int _stateSubjectName = 2;
        private const int _stateCertificate = 3;
        private const int _stateCrl = 4;

        public class IssuerSerial
        {
            private readonly X509Data _enclosingInstance;

            public string X509IssuerName
            {
                get; internal set;
            }

            public string X509SerialNumber
            {
                get; internal set;

            }

            public X509Data EnclosingInstance
            {
                get
                {
                    return _enclosingInstance;
                }
            }

            internal IssuerSerial(X509Data enclosingInstance)
            {
                _enclosingInstance = enclosingInstance;
            }
        }

        public virtual void WriteXml(Stream stream, string ds)
        {
            stream.Write("<" + ds + ":X509Data>");
            if (_dataState == _stateCertificate)
            {
                stream.Write("<" + ds + ":X509Certificate>");
                Base64OutputStream myOut = new Base64OutputStream(stream, true);
                myOut.Write(_x509Cert.GetEncoded(), 0, _x509Cert.GetEncoded().Length);
                myOut.Flush();
                stream.Write("</" + ds + ":X509Certificate>");
            }
            else if (_dataState == _stateCrl)
            {
                stream.Write("<" + ds + ":X509CRL>" + X509Crl + "</" + ds + ":X509CRL>");
            }
            else if (_dataState == _stateIssuerSerial)
            {
                stream.Write("<" + ds + ":X509IssuerSerial><" + ds + ":X509IssuerName>");
                stream.Write(_issuerSerial.X509IssuerName + "</" + ds + ":X509IssuerName><" + ds + ":X509SerisalNumber>" + _issuerSerial.X509SerialNumber);
                stream.Write("</" + ds + ":X509SerisalNumber></" + ds + ":X509IssuerSerial>");
            }
            else if (_dataState == _stateSki)
            {
                stream.Write("<" + ds + ":X509SKI>" + _x509Ski + "</" + ds + ":X509SKI>");
            }
            else if (_dataState == _stateSubjectName)
            {
                stream.Write("<" + ds + ":X509SubjectName>" + _x509SubjectName + "</" + ds + ":X509SubjectName>");
            }
            stream.Write("</" + ds + ":X509Data>");
        }
    }
}