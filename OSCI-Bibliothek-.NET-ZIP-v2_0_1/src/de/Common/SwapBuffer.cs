using System.IO;
using Osci.Interfaces;
using Osci.Cryptographic;
using System.Diagnostics;
using Osci.Helper;
using System;
using Osci.Extensions;
using Osci.Encryption;

namespace Osci.Common
{
    /// <summary> Diese Klasse stellt die Standard-Implementierung der abstrakten
    /// OSCIDataSource-Klasse dar. Diese Implementierung puffert Inhaltsdaten
    /// bis zu einer konfigurierbaren Anzahl von Bytes im Arbeitsspeicher.
    /// Wird diese Anzahl überschritten, werden die gepufferten Bytes wie alle
    /// folgenden Bytes in eine temporäre Datei geschrieben.<p></p><p></p>
    /// Zur Dokumentation der Methoden s. OSCIDataSource.
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
    /// <seealso cref="OsciDataSource">
    /// </seealso>
    public class SwapBuffer
        : OsciDataSource
    {
        private static readonly Log log = LogFactory.GetLog(typeof(SwapBuffer));

        private Stream ins;
        private SwapBufferInputStream dbis;
        private SwapBufferOutputStream dbos;
        private byte[] buffer;
        private string swapFilePath;
        private static SecretKey tempKey;
        /**
         * Limit für die Anzahl von Bytes, die im Arbeitsspeicher gepuffert werden,
         * bevor in eine temporäre Datei geswapt wird. Als Voreinstellung wird dieser Wert
         * auf 1 % des (beim ersten Laden dieser Klasse) verfügbaren freien Arbeitsspeichers
         * gesetzt.
         */
        public static long maxBufferSize = Process.GetCurrentProcess().VirtualMemorySize64 / 100;

        private static FileInfo tmpDir = new FileInfo(Path.GetTempPath());
        private long byteCount;

        /**
         * Creates a new SwapBuffer object.
         */
        public SwapBuffer()
        {
            _os = new MStream();
            dbos = new SwapBufferOutputStream(this);
            byteCount = 0;
            maxBufferSize = Process.GetCurrentProcess().VirtualMemorySize64 / 100;
        }


        /**
         * Destructor to imitate File.DeleteOnExit in Java
         */
        ~SwapBuffer()
        {
            if (swapFilePath != null)
            {
                try
                {
                    new FileInfo(swapFilePath).Delete();
                }
                catch (Exception ex)
                {
                    log.Debug("Could not delete encrypted temp file '" + swapFilePath + "', but go on; exception was: " + ex.Message);
                }
            }
        }

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
        public class MStream
            : MemoryStream
        {
            public override void Close()
            {
                Seek(0, SeekOrigin.Begin);
            }
        }

        private Stream _os;
        private bool _isFirstCall = true;

        public override Stream OutputStream
        {
            get
            {
                if(_os == null)
                {
                    log.Error("DataBuffer ist bereits im Lesemodus, kann nicht beschrieben werden.");
                    throw new InvalidOperationException();
                }
                return dbos;
            }
        }

        public override Stream InputStream
        {
            get
            {
                if (ins == null)
                    resetInputStream();

                return dbis;
            }
        }

        /// <summary> Liefert die Anzahl der (momentan) gespeicherten Bytes.
        /// </summary>
        /// <value> Anzahl der Bytes
        /// </value>
        public override long Length
        {
            get
            {
                return byteCount;
            }
        }

        /// <summary> Liefert den Namen des Herstellers.
        /// </summary>
        /// <value> Herstellername
        /// </value>
        public override string Vendor
        {
            get
            {
                return "Governikus";
            }
        }

        /// <summary> Liefert die Versionsnummer
        /// </summary>
        /// <value> Versionsnummer
        /// </value>
        public override string Version
        {
            get
            {
                return "0.1";
            }
        }

        public string SwapFilePath
        {
            get
            {
                return swapFilePath;                    
            }
        }
        

        public void Close()
        {
            _os.Close();
        }


        /**
         * Setzt den Pfad zum Verzeichnis für temporäre Dateien.
         * Bei erhöhten Sicherheitsanforderungen kann hier z.B. ein
         * Verzeichnis mit eingeschränkten Zugriffsrechten oder
         * in einem verschlüsslten Dateisystem angegeben werden.
         * @param dir undocumented
         */
        public static void setTmpDir(string tmpDirPath)
        {
            tmpDir = new FileInfo(tmpDirPath);
        }

        /**
         * undocumented
         *
         * @return undocumented
         *
         * @throws IOException undocumented
         */
        public override OsciDataSource NewInstance()
        {
            return new SwapBuffer();
        }

        private void resetInputStream()
        {
            if (dbos != null)
            {
                dbos.Close();
                dbos = null;
            }

            if (ins != null)
            {
                ins.Close();
            }

            ins = null;
            byteCount = 0;

            if (buffer != null)
            {
                ins = new MemoryStream(buffer);
            }
            else
            {
                ins = new SymCipherInputStream(new FileStream(swapFilePath, FileMode.Open, FileAccess.Read, FileShare.Write | FileShare.Read | FileShare.Delete,
                    4096), getTempSymKey(), false);
            }

            dbis = new SwapBufferInputStream(this);
        }
                      

        class SwapBufferOutputStream : OutputStream
        {
            private FileStream fos;

            private SwapBuffer swapBuffer;


            public SwapBufferOutputStream(SwapBuffer swapBuffer)
            {
                this.swapBuffer = swapBuffer;
            }

            public override void Write(byte[] b, int off, int len)
            {
                if (swapBuffer.ins != null)
                {
                    log.Error("DataBuffer ist bereits im Lesemodus, kann nicht beschrieben werden.");
                    throw new InvalidOperationException();
                }

                if ((swapBuffer._os is MemoryStream) && ((swapBuffer.byteCount + len) > maxBufferSize))
                {
                    if (log.IsEnabled(LogLevel.Debug))
                    {
                        log.Debug("SWAPPE AUF PLATTE");
                    }

                    swapBuffer.swapFilePath = Path.Combine(tmpDir.FullName, Guid.NewGuid().ToString("N").ToUpper());
                    Flush();
                    swapBuffer.buffer = ((MemoryStream)swapBuffer._os).ToArray();

                    fos = new FileStream(swapBuffer.swapFilePath, FileMode.Create, FileAccess.Write, FileShare.Write | FileShare.Read | FileShare.Delete, 
                        4096, FileOptions.RandomAccess);

                    swapBuffer._os = new SymCipherOutputStream(fos, getTempSymKey(), true);

                    swapBuffer._os.Write(swapBuffer.buffer, 0, swapBuffer.buffer.Length);
                    swapBuffer.buffer = null;
                }

                swapBuffer._os.Write(b, off, len);
                swapBuffer.byteCount += len;
            }

            public override void Write(int b)
            {
                this.Write(new byte[] { (byte)b });
            }

            public override void Flush()
            {
                swapBuffer._os.Flush();
            }

            public override void Close()
            {
                if (swapBuffer._os == null)
                {
                    return;
                }

                swapBuffer._os.Flush();
                swapBuffer._os.Close();

                if (fos != null)
                {
                    fos.Close();
                    fos = null;
                }

                if (swapBuffer._os is MemoryStream)
                {
                    swapBuffer.buffer = ((MemoryStream)swapBuffer._os).ToArray();
                }

                swapBuffer._os = null;
            }
        }

        class SwapBufferInputStream : InputStream
        {
            private SwapBuffer swapBuffer;

            public SwapBufferInputStream(SwapBuffer swapBuffer)
            {
                this.swapBuffer = swapBuffer;
            }

            public override int Read(byte[] b, int off, int len)
            {
                if (swapBuffer._os != null)
                {
                    throw new InvalidOperationException();
                }

                return swapBuffer.ins.Read(b, off, len);
            }
            
            public override long Seek(long start, SeekOrigin origin)
            {
                if(start == 0 && SeekOrigin.Begin.Equals(origin))
                {
                    swapBuffer.resetInputStream();
                }
                return 0;
            }

            public override void Close()
            {
                swapBuffer.ins.Close();
                //swapBuffer.DeleteTmpFileIfStillExists();
            }
        }

        private static SecretKey getTempSymKey()
        {
            if (tempKey == null)
            {
                tempKey = new SecretKey();
            }

            return tempKey;
        }

        private void DeleteTmpFileIfStillExists()
        {
            if (swapFilePath != null)
            {
                try
                {
                    FileInfo swapFileInfo = new FileInfo(swapFilePath);

                    // check if accessible
                    swapFileInfo.Delete();
                }
                catch (Exception ex)
                {
                    log.Debug("Could not delete encrypted temp file '" + swapFilePath + "', but go on; exception was: " + ex.Message);
                }
            }
        }
    }
}