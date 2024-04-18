using System;
using System.IO;
using Osci.Common;
using Osci.Cryptographic;
using Osci.Extensions;
using Osci.Helper;
using Osci.Interfaces;
using Osci.MessageParts;

namespace Osci.Encryption
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
    public class CipherValue
    {
        private static readonly Log _log = LogFactory.GetLog(typeof(CipherValue));
        private string _valueRenamed;
        private OsciDataSource _swapBuffer;
        private readonly ContentContainer _coco;
        private bool _base64EncodingRenamedField = true;

        private bool _usageSecretKeyRenamedField = true;
        private SecretKey _key;
        public const int StateDecrypted = 0;
        public const int StateEncrypted = 2;
        internal int StateOfObject = StateDecrypted;
        private int ivLength;

        internal SecretKey SecretKey
        {
            set
            {
                if (StateOfObject == StateEncrypted)
                {
                    throw new SystemException("Das Object befindet sich schon in verschlüsseltem Zustand");
                }
                _key = value;
            }
        }

        public OsciDataSource CipherValueBuffer
        {
            get
            {
                _log.Trace("This Object:" + this);

                if (_coco != null)
                {
                    _log.Trace("########Es handlet sich um ein COCO");

                    OsciDataSource tempSwap = DialogHandler.NewDataBuffer;
                    WriteCipherValue(tempSwap.OutputStream, ivLength);

                    return tempSwap;
                }
                else
                {
                    _log.Trace("########Es handlet sich um ein byte data. Länge des SwapBuffers: " + _swapBuffer.Length + " State ist:" + StateOfObject);

                    if (StateOfObject == StateDecrypted)
                    {
                        WriteCipherValue(null, ivLength);
                    }
                    _swapBuffer.InputStream.Seek(0, SeekOrigin.Begin);
                    return _swapBuffer;
                }
            }
        }

        public byte[] CipherValueBytes
        {
            get
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    _swapBuffer.InputStream.Seek(0, SeekOrigin.Begin);
                    _swapBuffer.InputStream.CopyToStream(memoryStream);
                    _swapBuffer.InputStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary> Erstellt ein neues CipherValue-Objekt.
        /// </summary>
        /// <param name="data"> data
        /// </param>
        public CipherValue(string data)
        {
            _swapBuffer = DialogHandler.NewDataBuffer;
            _log.Trace("Konstruktor String Data");

            _swapBuffer.OutputStream.Write(data);
            _swapBuffer.OutputStream.Flush();
        }

        /// <summary> Erstellt ein neues CipherValue-Objekt.
        /// </summary>
        /// <param name="dataBytes">dataBytes
        /// </param>
        public CipherValue(byte[] dataBytes)
        {
            _swapBuffer = DialogHandler.NewDataBuffer;
            _log.Trace("Konstruktor bytes. Länge: " + dataBytes.Length + " swap: " + _swapBuffer);

            Stream outRenamed = _swapBuffer.OutputStream;
            byte[] tempByteArray;
            tempByteArray = dataBytes;
            outRenamed.Write(tempByteArray, 0, tempByteArray.Length);
            outRenamed.Flush();
            outRenamed.Close();
        }

        internal void SetSecretKey(SecretKey key, int ivLengthByte)
        {
            if (StateOfObject == StateEncrypted)
                throw new SystemException("Das Object befindet sich schon in verschlüsseltem Zustand");
            _key = key;
            ivLength = ivLengthByte;
        }

        /// <summary> Erstellt ein neues CipherValue-Objekt.
        /// </summary>
        /// <param name="contentContainer">contentContainer
        /// </param>
        /// <param name="ivLengthByte">IV Länge in Byte
        /// </param>
        public CipherValue(ContentContainer contentContainer, int ivLengthByte)
        {
            _coco = contentContainer;
            ivLength = ivLengthByte;
        }


        /// <summary> Erstellt ein neues CipherValue-Objekt.
        /// </summary>
        /// <param name="contentContainer">contentContainer
        /// </param>
        public CipherValue(ContentContainer contentContainer) 
            : this(contentContainer, Constants.DefaultGcmIVLength)
        {
        }

        /// <summary> Erstellt ein neues CipherValue-Objekt.
        /// </summary>
        /// <param name="dataSource">dataSource
        /// </param>
        public CipherValue(OsciDataSource dataSource)
        {
            _swapBuffer = dataSource;
        }

        /// <summary> Erstellt ein neues CipherValue-Objekt.
        /// </summary>
        /// <param name="dataStream">dataStream
        /// </param>
        public CipherValue(Stream dataStream)
        {
            _swapBuffer = DialogHandler.NewDataBuffer;
            _log.Trace("Konstruktor InputStream");

            byte[] bytes = new byte[1024];
            int anz;
            while ((anz = dataStream.CopyTo(bytes, 0, bytes.Length)) > 0)
            {
                _swapBuffer.OutputStream.Write(bytes, 0, anz);
            }
            _swapBuffer.OutputStream.Flush();
        }

        public void UseSecretKey(bool usage)
        {
            _usageSecretKeyRenamedField = usage;
        }

        public bool UsageSecretKey()
        {
            return _usageSecretKeyRenamedField;
        }

        public bool Base64Encoding()
        {
            return _base64EncodingRenamedField;
        }

        public void DoBase64Encoding(bool encodingOn)
        {
            _base64EncodingRenamedField = encodingOn;
        }

        private void WriteCipherValue(Stream stream, int ivLength)
        {
            // TODO: aufräumen!

            _log.Trace("(start) writeCipherValue state:" + StateOfObject + "base64: " + _base64EncodingRenamedField + " ist contentData: " + _coco);

            Stream returnOut;
            OsciDataSource tempSwap = null;

            if (_coco != null)
            {
                returnOut = stream;
            }
            else
            {
                _log.Trace("SwapBuffer wird gesetzt!");
                tempSwap = DialogHandler.NewDataBuffer;
                returnOut = tempSwap.OutputStream;
            }

            if ((_coco != null) || ((StateOfObject == StateDecrypted) && (_key != null) && _usageSecretKeyRenamedField))
            {
                _log.Trace("base64 und verschlüsselung");
                returnOut = new Base64OutputStream(returnOut);
                SymCipherOutputStream tdesOut = new SymCipherOutputStream(returnOut, _key, ivLength, null);
                returnOut = tdesOut;
            }
            else if (_base64EncodingRenamedField && (StateOfObject == StateDecrypted))
            {
                returnOut = new Base64OutputStream(returnOut);
            }

            if (_coco != null)
            {
                _coco.WriteXml(returnOut);
                stream.Flush();
                if (returnOut is SymCipherOutputStream)
                {
                    _log.Trace("der Outputstream wird geclosed" + this);
                    returnOut.Close();
                }
            }
            else if (StateOfObject == StateDecrypted)
            {
                // es handles sich um StreamData oder um Byte data (kein COCO) verschlüsselung und / oder base64 wurde noch nicht durchgeführt
                Stream inRenamed = _swapBuffer.InputStream;
                inRenamed.Seek(0, SeekOrigin.Begin);

                byte[] bytes = new byte[1024];
                int anz;
                MemoryStream my = new MemoryStream();

                while ((anz = inRenamed.CopyTo(bytes, 0, bytes.Length)) > 0)
                {
                    returnOut.Write(bytes, 0, anz);
                }
                StateOfObject = StateEncrypted;
                _swapBuffer = tempSwap;
                if (returnOut is SymCipherOutputStream)
                {
                    _log.Trace("der Outputstream wird geclosed ohne coco");
                    returnOut.Close();
                }

                returnOut.Flush();
                // und nun noch einmal die Bytes herausschreiben
                if (stream != null)
                {
                    inRenamed = _swapBuffer.InputStream;
                    inRenamed.Seek(0, SeekOrigin.Begin);
                    while ((anz = inRenamed.CopyTo(bytes, 0, bytes.Length)) > 0)
                    {
                        stream.Write(bytes, 0, anz);
                        my.Write(bytes, 0, anz);
                    }
                }
                // interner Buffer ist auf dem neustem Stand herausschreiben des Buffers
            }
            else if (stream != null)
            {
                Stream inRenamed = _swapBuffer.InputStream;
                inRenamed.Seek(0, SeekOrigin.Begin);
                int anz;
                byte[] bytes = new byte[1024];

                while ((anz = inRenamed.CopyTo(bytes, 0, bytes.Length)) > 0)
                {
                    stream.Write(bytes, 0, anz);
                }
                inRenamed.Seek(0, SeekOrigin.Begin);
            }
        }

        public void WriteXml(Stream stream, string xenc)
        {
            _log.Trace("Wert base64 " + _base64EncodingRenamedField + " do encryption: " + _usageSecretKeyRenamedField);
            stream.Write("<" + xenc + ":CipherValue>");
            WriteCipherValue(stream, ivLength);
            stream.Write("</" + xenc + ":CipherValue>");
        }
    }
}