using System;
using System.IO;
using System.Text;
using Osci.Extensions;

namespace Osci.Helper
{
    /// <summary> Dieser Klasse dient der Erzeugung von Log-Ausgaben.
    /// Der Loglevel, der global für alle Log-Objekte voreingestellt wird,
    /// kann über eine Umgebungsvariable "OSCI_LIB_DEBUG_LEVEL" gesetzt werden.
    /// Außerdem kann durch Setzen einer weiteren Umgebungsvariable
    /// "OSCI_LIB_DEBUG_FILE" bewirkt werden, dass alle Log-Ausgaben in eine Datei geschrieben
    /// werden. Dadurch wird allerdings der Programmablauf stark verlangsamt.
    /// 
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
    public class Log
    {
        public static LogLevel Level
        {
            get;
        }

        public static LogLevel DefaultLevel
        {
            get
            {
                return LogLevel.Error;
            }
        }

        public static FileInfo LogFile
        {
            get;
        }

        private static bool IsLoggingToFile
        {
            get
            {
                return LogFile != null && LogFile.Exists;
            }
        }


        public bool IsEnabled(LogLevel level)
        {
            return Level <= level;
        }

        private static readonly object _lock = new object();
        private readonly string _source;



        static Log()
        {
            LogFile = GetLogFilePath();
            Level = GetLogLevel();
            Write(string.Format("Logging initialized on level {0}", Level.FriendlyName()));
        }


        /// <summary>
        /// Enthält den aktuellen Log-Level. Default ist LEVEL_ERROR. Durch Zuweisung im Code kann der
        /// Loglevel für einzelne Klassen geändert werden.
        /// </summary>
        public Log(Type type)
            : this(type.Name)
        {
        }

        public Log(string name)
        {
            _source = name;
        }

        private static FileInfo GetLogFilePath()
        {
            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return new FileInfo(Environment.GetEnvironmentVariable("OSCI_LIB_DEBUG_FILE", EnvironmentVariableTarget.Machine));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static LogLevel GetLogLevel()
        {
            try
            {
                string logLevel = Environment.GetEnvironmentVariable("OSCI_LIB_DEBUG_LEVEL", EnvironmentVariableTarget.Machine) ?? "";
                foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
                {
                    if (string.Equals(logLevel, level.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return level;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return DefaultLevel;
        }

        private void Write(LogLevel level, object data, Exception exception = null)
        {
            if (!IsEnabled(level))
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("{0} [{1}]: {2}", level.FriendlyName(), _source, data));
            if (exception != null)
            {
                sb.AppendLine(exception.Message);
                sb.AppendLine(exception.StackTrace);
            }
            Write(sb.ToString());
        }

        private static void Write(string message)
        {
            if (IsLoggingToFile)
            {
                WriteToFile(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        private static void WriteToFile(string text)
        {
            lock (_lock)
            {
                File.AppendAllText(LogFile.FullName, text);
            }
        }

        public void Trace(object objectRenamed, Exception exception = null)
        {
            Write(LogLevel.Trace, objectRenamed, exception);
        }

        public void Debug(object objectRenamed, Exception exception = null)
        {
            Write(LogLevel.Debug, objectRenamed, exception);
        }

        public void Info(object objectRenamed, Exception exception = null)
        {
            Write(LogLevel.Info, objectRenamed, exception);
        }

        public void Warn(object objectRenamed, Exception exception = null)
        {
            Write(LogLevel.Warn, objectRenamed, exception);
        }

        public void Error(object objectRenamed, Exception exception = null)
        {
            Write(LogLevel.Error, objectRenamed, exception);
        }

        public void Fatal(object objectRenamed, Exception exception = null)
        {
            Write(LogLevel.Fatal, objectRenamed, exception);
        }
    }
}