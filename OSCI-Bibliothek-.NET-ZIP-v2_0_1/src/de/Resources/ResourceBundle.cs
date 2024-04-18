using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using Osci.Helper;

namespace Osci.Resources
{
    /// <summary>
    /// Die Unterstützung der mehrsprachigen Ausgaben wurde vereinfacht.
    /// Sprachdateien können nun als einfache Java-properties-Dateien in
    /// das Verzeichnis der DLL osci-bib.dll kopiert werden. Die Dateinamen
    /// müssen aus dem String "Text_" bestehen, dem der CultureInfo.TwoLetterISOLanguageName
    /// sowie die Erweiterung ".properties" angehängt ist, also z.B. "Text_de.properties".
    /// Als default-Datei für unbekannte Sprachen wird die Datei "Text.properties"
    /// geladen.
    /// 
    /// </summary>
    public class ResourceBundle
    {
        private ResourceManager _rm;
        private readonly Hashtable _props;
        private static readonly Log _log = LogFactory.GetLog(typeof(ResourceBundle));

        internal string Path { get; private set; }


        private ResourceBundle(string bundleName, CultureInfo cultureInfo)
        {
            _props = new Hashtable();

            try
            {
                string[] lines = GetResourceBundleText(bundleName, cultureInfo).Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines.Select(_ => _.Trim()).Where(_ => !string.IsNullOrEmpty(_) && !_.StartsWith("#")))
                {
                    string[] keyValue = line.Split(new[] { '=' }, 2);
                    _props.Add(keyValue[0].Trim(), keyValue[1].Trim());
                }
            }
            catch (Exception e)
            {
                _log.Warn("ResourceBundle couldn't be initialized properly:", e);
            }
        }

        private string GetResourceBundleText(string bundleName, CultureInfo cultureInfo)
        {
            string rawText;
            if (!TryGetResourceFile(bundleName, cultureInfo, out rawText))
            {
                // the fallback
                rawText = Encoding.UTF8.GetString(Properties.Resources.DefaultText);
            }
            return rawText;
        }

        private bool TryGetResourceFile(string bundleName, CultureInfo cultureInfo, out string rawText)
        {
            try
            {
                string language = "_" + cultureInfo.TwoLetterISOLanguageName;
                FileInfo assemblyFile = new FileInfo(GetType().Assembly.Location);

                FileInfo resourceFile = new FileInfo(System.IO.Path.Combine(assemblyFile.DirectoryName, string.Format("{0}{1}.properties", bundleName, language)));
                if (resourceFile.Exists)
                {
                    Path = resourceFile.FullName;
                    rawText = File.ReadAllText(resourceFile.FullName);
                    return true;
                }
            }
            catch (Exception e)
            {
                _log.Warn("ResourceBundle could not be loaded properly:", e);
            }

            rawText = null;
            return false;
        }




        public string GetString(string key)
        {
            return _props.ContainsKey(key) ? (string)_props[key] : "";
        }

        public static ResourceBundle GetBundle(string bundleName, CultureInfo cultureInfo)
        {
            return new ResourceBundle(bundleName, cultureInfo);
        }
    }
}
