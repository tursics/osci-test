using System;
using System.IO;
using Osci.Common;
using Osci.Extensions;
using Osci.Helper;
using Osci.MessageParts;
using Osci.Roles;
using Osci.SoapHeader;

namespace Osci.Messagetypes
{
    // Storestreams

    /// <summary><p><H4>Zustellungsabholauftrag</H4></p>
    /// Mit dieser Klasse werden Nachrichtenobjekte für Zustellungsabholaufträge
    /// angelegt. Clients können hiermit maximal eine Nachricht vom Intermediär
    /// abrufen.  Als Antwort auf diese Nachricht erhalten sie vom Intermediär
    /// ein ResponseToFetchDelivery-Nachrichtenobjekt, welches eine Rückmeldung über den
    /// Erfolg der Operation (getFeedback()) und ggf. die gewünschte Nachricht enthält.
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
    /// <seealso cref="ResponseToFetchDelivery">
    /// </seealso>
    public class FetchDelivery
        : FetchRequestAbstract
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(FetchDelivery));

        internal FetchDelivery()
        {
            MessageType = FetchDelivery;
        }

        /// <summary> Legt ein Nachrichtenobjekt für einen Zustellungsabholauftrag an.
        /// </summary>
        /// <param name="dh">DialogHandler-Objekt des Dialogs, innerhalb dessen die Nachricht
        /// versendet werden soll.
        /// </param>
        /// <seealso cref="DialogHandler">
        /// </seealso>
        public FetchDelivery(DialogHandler dh)
            : base(dh)
        {
            MessageType = FetchDelivery;
            Originator = ((Originator)dh.Client);

            DialogHandler.Controlblock.Response = DialogHandler.PreviousChallenge;
            DialogHandler.Controlblock.Challenge = Tools.CreateRandom(10);
            DialogHandler.Controlblock.SequenceNumber = DialogHandler.Controlblock.SequenceNumber + 1;
        }

        /// <summary> Versendet die Nachricht und liefert die Antwortnachricht zurück.
        /// Diese Methode wirft eine Exception, wenn beim Aufbau oder Versand der
        /// Nachricht ein Fehler auftritt. Fehlermeldungen vom Intermediär müssen
        /// dem Feedback der Antwortnachricht entnommen werden.
        /// </summary>
        /// <returns> das Antwortnachricht-Objekt
        /// </returns>
        public virtual ResponseToFetchDelivery Send()
        {
            return (ResponseToFetchDelivery)Transmit(null, null);
        }

        public virtual ResponseToFetchDelivery Send(Stream storeOutput, Stream storeInput)
        {
            return (ResponseToFetchDelivery)Transmit(storeOutput, storeInput);
        }

        public override void Compose()
        {
            base.Compose();
            if (DialogHandler.Controlblock.Challenge == null)
            {
                throw new SystemException("Kein Challenge-Wert in DialogHandler eingestellt.");
            }

            if (DialogHandler.Controlblock.Response == null)
            {
                throw new SystemException("Kein Response-Wert in DialogHandler eingestellt.");
            }

            if (DialogHandler.Controlblock.ConversationId == null)
            {
                throw new SystemException("Keine Conversation-ID in DialogHandler eingestellt.");
            }

            if (DialogHandler.Controlblock.SequenceNumber == -1)
            {
                throw new SystemException("Kein Squenznummer in DialogHandler eingestellt.");
            }
            
            OsciH = new OsciH("fetchDelivery", GetSelectionRuleString());
            Body = new Body("");

            StateOfMessage |= StateComposed;
        }

        public override void WriteXml(Stream outRenamed)
        {
            _log.Debug("CreateMsg child");
            base.WriteXml(outRenamed);

            // ClientSignatur
            if (SignatureHeader != null)
            {
                SignatureHeader.WriteXml(outRenamed);
            }
            // DesiredLanguage
            DesiredLanguagesH.WriteXml(outRenamed);
            OsciH.WriteXml(outRenamed);

            if (NonIntermediaryCertificatesH != null)
            {
                NonIntermediaryCertificatesH.WriteXml(outRenamed);
            }
            if (FeatureDescription != null && DialogHandler.SendFeatureDescription)
            {
                FeatureDescription.WriteXml(outRenamed);
            }
            CompleteMessage(outRenamed);
        }
    }
}