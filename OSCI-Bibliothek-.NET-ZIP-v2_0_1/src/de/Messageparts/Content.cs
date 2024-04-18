using System;
using System.IO;
using System.Security.Cryptography;
using Osci.Common;
using Osci.Encryption;
using Osci.Exceptions;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;

namespace Osci.MessageParts
{
    /// <summary> <p><H4>Content</H4></p>
    /// <p>Die Content-Klasse repräsentiert einen Content-Eintrag in einer OSCI-
    /// Nachricht. Die Content-Einträge befinden sich in ContentContainer-Einträgen
    /// und enthalten die eigentlichen Nutzdaten, die in beliebigen Daten,
    /// Refenrenzen auf Attachments oder wiederum in Inhaltsdatencontainern bestehen
    /// können.</p>
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
    public class Content
        : MessagePart
    {
        internal string coNS;

        protected OsciDataSource SwapBuffer;

        private readonly Attachment _attachment;
        private readonly ContentContainer _coco;
        private Stream _transformedDataStream;

        private static int _zaehl;
        private static readonly Log _log = LogFactory.GetLog(typeof(Content));

        /// <summary> Liefert den InputStream der Daten zurück, wenn der Content einer
        /// empfangenen Nachricht entnommen wurde.
        /// </summary>
        /// <value> InputStream der Inhaltsdaten, oder null, wenn die Nachricht
        /// nicht empfangen (d.h. mit Hilfe eine Konstruktors erzeugt wurde).
        /// </value>
        public Stream ContentStream
        {
            get
            {
                if (SwapBuffer != null)
                {
                    SwapBuffer.InputStream.Seek(0, SeekOrigin.Begin);
                    return new Base64InputStream(SwapBuffer.InputStream);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary> Liefert die eingestellten Daten des Content zurück.
        /// </summary>
        /// <value> String der Inhaltsdaten, oder null, wenn die Nachricht
        /// nicht empfangen (d.h. mit Hilfe eine Konstruktors erzeugt wurde).
        /// </value>
        public string ContentData
        {
            get
            {
                if (SwapBuffer != null)
                {
                    SwapBuffer.InputStream.Seek(0, SeekOrigin.Begin);
                    Stream inputStream = new Base64InputStream(SwapBuffer.InputStream);
                    return inputStream.AsString();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary> Liefert das referenzierte Attachment zurück.
        /// </summary>
        /// <value> Attachment-Objekt oder null, wenn das Content-Objekt Nutzdaten
        /// oder einen Inhaltsdatencontainern enthält.
        /// </value>
        public Attachment Attachment
        {
            get
            {
                return _attachment;
            }
        }

        /// <summary> Liefert den Inhaltsdatencontainer zurück.
        /// </summary>
        /// <value> ContentContainer-Objekt oder null, wenn das Content-Objekt
        /// Nutzdaten oder eine Referenz auf ein Attachment enthält.
        /// </value>
        public ContentContainer ContentContainer
        {
            get
            {
                return _coco;
            }
        }

        /// <summary> Gibt die Art des Inhalts des Content-Objektes in Form eines Identifiers
        /// zurück. Mögliche Werte sind ATTACHMENT_REFERENCE, CONTENT_CONTAINER und DATA
        /// </summary>
        /// <value> Inhaltstyp
        /// </value>
        public ContentType ContentType
        {
            get;
        }

        #region c'tor

        /// <summary> Legt ein Content-Objekt an. Die Daten werden aus dem übergebenen
        /// InputStream gelesen.
        /// </summary>
        /// <param name="inputStream">InputStream
        /// </param>
        public Content(Stream inputStream)
            : this(CreateRefId(), inputStream)
        {
        }
        public Content(string refId, Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }

            RefId = refId;
            ContentType = ContentType.Data;
            Load(inputStream);
            Transformers.Add(Base64);
        }

        /// <summary> Legt ein Content-Objekt an. Die Daten werden aus dem übergebenen
        /// InputStream gelesen.
        /// </summary>
        /// <param name="dataSource">InputStream
        /// </param>
        /// <exception cref="System.ArgumentException">Im Fehlerfall
        /// </exception>
        public Content(OsciDataSource dataSource)
            : this(CreateRefId(), dataSource)
        {
        }

        public Content(string refId, OsciDataSource dataSource)
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }

            _log.Trace("Konstruktor");

            RefId = refId;
            SwapBuffer = dataSource;
            ContentType = ContentType.Data;
        }

        /// <summary> Legt ein Content-Objekt mit dem Inhalt des übergebenen Strings an.
        /// <b>Hinweis:</b> Die Übergabe von Inhaltsdaten als String ist zur 
        /// Bequemlichkeit für Daten vorgesehen, die empfängerseitig ebenfalls
        /// als String der OSCI-Nachricht entnommen und weiterverarbeitet werden.
        /// Dies ist zu beachten, wenn z.B. XML-Dokumente übergeben werden, die
        /// eine Zeichensatzdeklaration beinhalten und möglicherweise empfängerseitig
        /// als Binärdaten einem Parser übergeben werden. Die OSCI-Bibliothek
        /// überträgt die Daten UTF-8-codiert.   
        /// </summary>
        /// <param name="data">Inhalt
        /// </param>
        /// <exception cref="System.ArgumentException">Im Fehlerfall
        /// </exception>
        public Content(string data)
            : this(CreateRefId(), data)
        {
        }

        public Content(string refdId, string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            RefId = refdId;
            Load(new MemoryStream(data.ToByteArray()));
            ContentType = ContentType.Data;
            Transformers.Add(Base64);
        }

        /// <summary> Legt ein Content-Objekt an, welches eine Referenz auf ein Attachment
        /// enthält.
        /// </summary>
        /// <param name="attachment">Attachment
        /// </param>
        /// <exception cref="System.ArgumentException">Im Fehlerfall
        /// </exception>
        public Content(Attachment attachment)
            : this(CreateRefId(), attachment)
        {
        }

        public Content(string refdId, Attachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }
            RefId = refdId;
            _attachment = attachment;
            ContentType = ContentType.AttachmentReference;
            Transformers.Add(Canonizer);
        }

        /// <summary>Legt ein Content-Objekt mit dem Inhalt des übergebenen Strings an.
        /// Übergebene Transformer-Strings werden in die XML-Signatur eingetragen,
        /// die Strings müssen die gesamten Transformer-Einträge gemäß der
        /// XML-Signature-Spezifikation in kanonischer Form enthalten.
        /// Die zu signierenden transformierten Daten müssen in dem als dritten
        /// Parameter übergebenen String enthalten sein.
        /// <b>Man beachte den Hinweis zur Übergabe von Inhaltsdaten als String.</b>
        /// </summary>
        /// <param name="data">Inhaltsdaten.
        /// </param>
        /// <param name="transformer">Array der Transformereinträge.
        /// </param>
        /// <param name="transformedData">String mit den transformierten Daten.
        /// </param>
        ///@throws IOException bei Lesefehlern
        ///@throws OSCIException bei Problemn beim Aufbau des OSCI-Signatureintrags
        ///@throws NoSuchAlgorithmException wenn der verwendete Security-Provider
        ///den erforderlichen Hash-Algorithmus nicht unterstützt.
        public Content(string data, string[] transformer, string transformedData)
            : this(data)
        {
            if (transformer != null && transformer.Length > 0)
            {
                _transformedDataStream = new MemoryStream(transformedData.ToByteArray());
                for (int i = 0; i < transformer.Length; i++)
                {
                    Transformers.Add(transformer[i]);
                }
            }
        }
        /// <summary>
        /// Legt ein Content-Objekt an. Die Daten werden aus dem übergebenen
        /// InputStream gelesen. Übergebene Transformer-Strings werden in die
        /// XML-Signatur eingetragen, die Strings müssen die gesamten Transformer-Einträge
        /// gemäß der XML-Signature-Spezifikation in kanonischer Form enthalten.
        /// Die zu signierenden transformierten Daten werden aus dem als dritten
        /// Parameter übergebenen Stream gelesen.
        /// </summary>
        /// <param name="ins">InputStream der Inhaltsdaten
        /// </param>
        /// <param name="transformer">Array der Transformereinträge
        /// </param>
        /// <param name="transformedData">InputStream der transformierten Daten
        /// </param>
        /// <exception cref="OsciException">bei Problemen beim Aufbau des OSCI-Signatureintrags
        /// </exception>
        public Content(InputStream ins, string[] transformer, InputStream transformedData)
            : this(ins)
        {
            if (transformer != null && transformer.Length > 0)
            {
                _transformedDataStream = transformedData;
                for (int i = 0; i < transformer.Length; i++)
                {
                    Transformers.Add(transformer[i]);
                }
            }
        }
        /// <summary> Legt ein Content-Objekt an, welches ein ContentContainer-Objekt enthält.
        /// </summary>
        /// <param name="contentContainer">Inhaltsdatencontainer
        /// </param>
        public Content(ContentContainer contentContainer)
            : this(CreateRefId(), contentContainer)
        {
        }

        public Content(string refId, ContentContainer contentContainer)
        {
            if (contentContainer == null)
            {
                throw new ArgumentNullException("contentContainer");
            }
            RefId = refId;
            Transformers.Add(Canonizer);
            _coco = contentContainer;
            ContentType = ContentType.ContentContainer;
        }

        private static string CreateRefId()
        {
            return "content_" + Guid.NewGuid().ToString("N");
        }

        #endregion


        private void Load(Stream input)
        {
            if (SwapBuffer == null)
            {
                SwapBuffer = DialogHandler.NewDataBuffer;

                Base64OutputStream b64Out = new Base64OutputStream(SwapBuffer.OutputStream, false);
                byte[] bytes = new byte[1024];
                int anz;

                while ((anz = input.CopyTo(bytes, 0, bytes.Length)) > 0)
                {
                    b64Out.Write(bytes, 0, anz);
                }
                b64Out.Close();
            }
        }

        public override byte[] GetDigestValue(string digestAlgorithm)
        {
            if (DigestValues.ContainsKey(digestAlgorithm))
            {
                return (byte[])DigestValues[digestAlgorithm];
            }

            if (ContentType == ContentType.Data)
            {
                HashAlgorithm msgDigest = Crypto.CreateMessageDigest(digestAlgorithm);
                if (_transformedDataStream == null)
                {
                    // Bibliothek akzeptiert nur Base64-codierte Daten. Workaround für den Fall,
                    // dass dieser Transformer beim Laden zwischengespeicherter Daten verloren gegangen ist (StoredMessage).
                    // Signaturen mit Transformern können in diesem Fall nur mit Hilfe neuer Content-Instanzen
                    // erzeugt werden.
                    if (Transformers.Count == 0)
                    {
                        Transformers.Add(Base64);
                    }
                    if (Transformers.Count > 1)
                    {
                        throw new IllegalStateException(DialogHandler.ResourceBundle.GetString("no_transformed_data"));
                    }
                    SwapBuffer.InputStream.Seek(0, SeekOrigin.Begin);
                    _transformedDataStream = new Base64InputStream(SwapBuffer.InputStream);
                }
                CryptoStream outStream = new CryptoStream(new MemoryStream(), msgDigest, CryptoStreamMode.Write);
                byte[] tmp = new byte[1024];
                int s;
                while ((s = _transformedDataStream.Read(tmp, 0, tmp.Length)) > 0)
                {
                    outStream.Write(tmp, 0, s);
                }
                outStream.Close();
                DigestValues.Add(digestAlgorithm, msgDigest.Hash);
            }
            else
            {
                return base.GetDigestValue(digestAlgorithm);
            }

            return (byte[])DigestValues[digestAlgorithm];
        }

        public override void WriteXml(Stream outRenamed)
        {
            WriteXml(outRenamed, false);
        }

        internal void WriteXml(Stream stream, bool inner)
        {
            stream.Write("<" + OsciNsPrefix + ":Content");
            if (_attachment != null)
            {
                _log.Trace("SCHREIBE Attachment." + RefId);
                if (!inner)
                {
                    if (coNS == null)
                    {
                        stream.Write(Ns, 0, Ns.Length);
                    }
                    else
                    {
                        stream.Write(coNS);
                    }
                }
                stream.Write(" Id=\"" + RefId + "\" href=\"cid:" + _attachment.RefId + "\"></" + OsciNsPrefix + ":Content>");
            }
            else if (_coco != null)
            {
                _log.Trace("Schreibe ContentContainer." + RefId);
                if (!inner)
                {
                    if (coNS == null)
                    {
                        stream.Write(Ns, 0, Ns.Length);
                    }
                    else
                    {
                        stream.Write(coNS);
                    }
                }
                stream.Write(" Id=\"" + RefId + "\">");
                _coco.WriteXml(stream);
                stream.Write("</" + OsciNsPrefix + ":Content>");
            }
            else
            {
                _log.Trace("Schreibe Data.");
                stream.Write("><" + OsciNsPrefix + ":Base64Content");
                if (!inner)
                {
                    if (coNS == null)
                    {
                        stream.Write(Ns, 0, Ns.Length);
                    }
                }
                stream.Write(" Id=\"" + RefId + "\">");
                int count;
                byte[] inBytes = new byte[1024];
                SwapBuffer.InputStream.Seek(0, SeekOrigin.Begin);

                while ((count = SwapBuffer.InputStream.CopyTo(inBytes, 0, inBytes.Length)) > 0)
                {
                    stream.Write(inBytes, 0, count);
                }
                stream.Flush();
                stream.Write("</" + OsciNsPrefix + ":Base64Content></" + OsciNsPrefix + ":Content>");
            }
        }

        /// <summary>Liefert die Transformereinträge in der Signatur.
        /// </summary>
        /// <returns>String-Array mit den Transformereinträgen
        /// </returns>
        public string[] GetTransformerForSignature()
        {
            if (Transformers == null || Transformers.Count < 2)
            {
                return null;
            }
            string[] ret = new string[Transformers.Count - 1];
            for (int i = 1; i < Transformers.Count; i++)
            {
                ret[i - 1] = Transformers[i];
            }
            return ret;
        }

        /// <summary>Bevor eine Signaturprüfung an dem ContentContainer-Objekt durchgeführt
        /// werden kann, welches dieses Content-Objekt enthält, müssen mit dieser
        /// Methode die transformierten Daten übergeben werden. Dies betrifft nur
        /// Content-Objekte, die unter Anwendung von Transformationen signiert wurden.
        /// </summary>
        /// <param name="transformedData">transformierte Daten
        /// </param>
        /// <seealso cref="SetTransformedData(System.IO.Stream)">
        /// </seealso>
        /// <seealso cref="MessageParts.ContentContainer.CheckSignature">
        /// </seealso>
        public void SetTransformedData(string transformedData)
        {
            try
            {
                SetTransformedData(new MemoryStream(transformedData.ToByteArray()));
            }
            catch (Exception)
            {
                // Kann bei UTF-8 nicht auftreten
            }
        }
        /// <summary>Bevor eine Signaturprüfung an dem ContentContainer-Objekt durchgeführt
        /// werden kann, welches dieses Content-Objekt enthält, müssen mit dieser
        /// Methode die transformierten Daten übergeben werden. Dies betrifft nur
        /// Content-Objekte, die unter Anwendung von Transformationen signiert wurden.
        /// </summary>
        /// <param name="transformedData">transformierte Daten
        /// </param>
        /// <seealso cref="MessageParts.ContentContainer.CheckSignature">
        /// </seealso>
        public void SetTransformedData(Stream transformedData)
        {
            if (GetTransformerForSignature() == null)
            {
                throw new Exception(DialogHandler.ResourceBundle.GetString("no_transformer_state"));
            }
            if (_transformedDataStream != null)
            {
                throw new Exception(DialogHandler.ResourceBundle.GetString("illegal_change_of_transformed_data"));
            }
            _transformedDataStream = transformedData;
        }
    }
}