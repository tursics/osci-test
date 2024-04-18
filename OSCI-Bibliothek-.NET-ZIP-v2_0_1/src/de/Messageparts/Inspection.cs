using System.IO;
using Osci.Extensions;
using Osci.Helper;

namespace Osci.MessageParts
{
    /// <summary> <p>Diese Klasse repräsentiert das OSCI-Inspektion-Element. Hier werden
    /// Informationen für die ausgewerteten Zertifikate der OSCI-Nachricht gehalten. </p>
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
    public class Inspection
        : MessagePart
    {
        /// <summary>
        /// Liefert die Seriennummer des Zertifikats.
        /// </summary>
        /// <value>Zertifikatsnummer
        /// </value>
        public string X509SubjectName
        {
            get; internal set;
        }

        public bool IsOnlineChecked
        {
            get; internal set;
        }

        /// <summary>
        /// Liefert die Seriennummer des Zertifikats.
        /// </summary>
        /// <value>Zertifikatsnummer
        /// </value>
        public string X509SerialNumber
        {
            get; internal set;
        }

        /// <summary>
        /// Liefert den Ausstellernamen den Zertifikats.
        /// </summary>
        /// <value>Ausstellername
        /// </value>
        public string X509IssuerName
        {
            get; internal set;
        }

        /// <summary>
        /// Liefert den Zeitstempel der Prüfung.
        /// </summary>
        /// <value>Zeitstempel
        /// </value>
        public Timestamp TimeStamp
        {
            get; internal set;
        }

        /// <summary>
        /// Liefert das Ergebnis der online-Prüfung.
        /// </summary>
        /// <value>true -> ok, false -> revoked. Bei nicht durchgeführter
        /// online-Prüfung wird revoked zurückgegeben.
        /// </value>
        public bool OnlineResult
        {
            get; internal set;
        }

        /// <summary>
        /// Liefert den Namen des online-Prüfverfahrens.
        /// </summary>
        /// <value>online-Prüfverfahren (OCSP/CRL/LDAP)
        /// </value>
        public string[] OnlineCheckNames
        {
            get; internal set;
        }

        /// <summary>
        /// Liefert den Inhalt des Prüfverfahrens-Eintrags. Dies ist bei OCSP-Prüfung
        /// der Base-64-codierte OCSP-Request und bei CRL-Prüfung das Datum der CRL-Liste.
        /// </summary>
        /// <value>Inhalt des Eintrags
        /// </value>
        public string[] OnlineChecks
        {
            get; internal set;
        }

        /// <summary>
        /// Liefert das Ergebnis der mathematischen Zertifikatsprüfung.
        /// </summary>
        /// <value>true -> ok, false -> corrupted
        /// </value>
        public string MathResult
        {
            get; internal set;
        }

        /// <summary>
        /// Liefert das Ergebnis der offline-Prüfung.
        /// </summary>
        /// <value>Ergebnis: true -> valid, false -> invalid
        /// </value>
        public string OfflineResult
        {
            get; internal set;
        }

        /// <summary>
        /// Liefert den Typ des Zertifikats.
        /// </summary>
        /// <value>Typ (advanced/qualified/unknown)
        /// </value>
        public string CertType
        {
            get; internal set;
        }

        /// <summary>
        /// Liefert die ID-Nummer.
        /// </summary>
        /// <value>ID-Nummer</value>
        public int IdNr
        {
            get
            {
                return _idNr;
            }
        }

        public static string CertTypeAdvanced = "advanced";
        public static string CertTypeQualified = "qualified";
        public static string CertTypeUnknown = "unknown";
        public static string CertTypeAccredited = "accredited";
        private static int _idNr = -1;

        internal Inspection()
        {
        }

        /// <summary> Dieser Konstruktur wird nur bei Offlineprüfung benutzt.
        /// </summary>
        /// <param name="cert">Zertifikat für das diese Prüfergebnisse gelten.
        /// </param>
        /// <param name="certType">Art des Zertifikates (advanced, qualified oder unknown).
        /// </param>
        /// <param name="timeStamp">Timestamp Element zu diesem Eintrag.
        /// </param>
        /// <param name="mathResult">Ergbnis der Prüfung der Zertifikatssignatur OK oder corrupted (true or false).
        /// </param>
        /// <param name="offlineResult">bei Offlineprüfung Ergebnis der Offline-Gültigkeitsprüfung des Zertifikates valid oder invalid (true, false)).
        /// </param>
        public Inspection(X509Certificate cert, string certType, Timestamp timeStamp, string mathResult, string offlineResult)
        {
            X509IssuerName = cert.GetIssuerName();
            X509SerialNumber = cert.GetSerialNumber();
            X509SubjectName = cert.GetName();
            this.TimeStamp = timeStamp;
            this.MathResult = mathResult;
            this.OfflineResult = offlineResult;
            this.CertType = certType;
            IsOnlineChecked = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="cert">Zertifikat für das diese Prüfergebnisse gelten
        /// </param>
        /// <param name="certType">Art des Zertifikates (advanced, qualified oder unknown)
        /// </param>
        /// <param name="timeStamp">Timestamp Element zu diesem Eintrag
        /// </param>
        /// <param name="mathResult">Ergbnis der Prüfung der Zertifikatssignatur OK oder corrupted (true or false)
        /// </param>
        /// <param name="offlineResult">bei Offlineprüfung Ergebnis der Offline-Gültigkeitsprüfung des Zertifikates valid oder invalid (tur, false))
        /// </param>
        /// <param name="onlineResult">Ergebnis der Online Prüfung OK oder revoked
        /// </param>
        /// <param name="onlineCheckName">Art der Onlineprüfung OCSP, CRL oder LDAP
        /// </param>
        /// <param name="onlineCheck">Ergebnis der CRL oder OCSP. Entweder base64 oder dateTime Objekt
        /// </param>
        public Inspection(X509Certificate cert, string certType, Timestamp timeStamp, string mathResult, string offlineResult, bool onlineResult, string[] onlineCheckNames, string[] onlineChecks)
        {
            X509IssuerName = cert.GetIssuerName();
            X509SerialNumber = cert.GetSerialNumber();
            X509SubjectName = cert.GetName();
            this.TimeStamp = timeStamp;
            this.MathResult = mathResult;
            this.OfflineResult = offlineResult;
            OnlineResult = onlineResult;
            this.OnlineCheckNames = onlineCheckNames;
            this.OnlineChecks = onlineChecks;
            IsOnlineChecked = true;
            this.CertType = certType;
        }

        public override void WriteXml(Stream stream)
        {
            stream.Write("<" + OsciNsPrefix + ":Inspection>");
            TimeStamp.WriteXml(stream);
            stream.Write("<" + OsciNsPrefix + ":X509SubjectName>" + ProcessCardBundle.Encode(X509SubjectName) + "</" + OsciNsPrefix + ":X509SubjectName>");
            stream.Write("<" + OsciNsPrefix + ":X509IssuerName>" + ProcessCardBundle.Encode(X509IssuerName) + "</" + OsciNsPrefix + ":X509IssuerName>");
            stream.Write("<" + OsciNsPrefix + ":X509SerialNumber>" + X509SerialNumber + "</" + OsciNsPrefix + ":X509SerialNumber>");
            stream.Write("<" + OsciNsPrefix + ":CertType Type=\"" + CertType + "\"></" + OsciNsPrefix + ":CertType><" + OsciNsPrefix + ":MathResult Result=\"");
            stream.Write(MathResult.ToLower());
            stream.Write("\"></" + OsciNsPrefix + ":MathResult><" + OsciNsPrefix + ":OfflineResult Result=\"" + OfflineResult.ToLower() + "\"></" + OsciNsPrefix + ":OfflineResult>");
            if (IsOnlineChecked)
            {
                string res = "revoked";
                if (OnlineResult)
                {
                    res = "ok";
                }
                stream.Write("<" + OsciNsPrefix + ":OnlineResult Result=\"" + res + "\">");
                for (int i = 0; i < OnlineCheckNames.Length; i++)
                {
                    stream.Write("<" + OsciNsPrefix + ":" + OnlineCheckNames[i] + ">"
                        + OnlineChecks[i] + "</" + OsciNsPrefix + ":" + OnlineCheckNames[i] + ">");
                }
                stream.Write("</" + OsciNsPrefix + ":OnlineResult>");
            }
            stream.Write("</" + OsciNsPrefix + ":Inspection>");
        }
    }
}