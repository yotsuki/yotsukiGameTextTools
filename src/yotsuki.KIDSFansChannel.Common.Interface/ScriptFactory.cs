using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;

namespace yotsuki.KIDSFansChannel.Common.BaseClass
{
    public class ScriptFactory
    {
        public static ScriptReader GetReader(string game)
        {
            ScriptReader sr = null;
            try
            {
                string assemblyFilePath = Assembly.GetExecutingAssembly().Location;
                string assemblyDirPath = Path.GetDirectoryName(assemblyFilePath);
                var file = assemblyDirPath + "\\" + game + ".dll";
                if (File.Exists(file)) {
                    Assembly assembly = Assembly.LoadFile(file);
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types) {
                        if (type.BaseType == typeof(ScriptReader) && type.Name == game + "Reader") {
                            sr = assembly.CreateInstance(type.FullName) as ScriptReader;
                            break;
                        }
                    }
                }
            }
            catch{}
            return sr;
        }

        public static ScriptWriter GetWriter(string game = null)
        {
            ScriptWriter sw = null;
            if (!string.IsNullOrEmpty(game)) {
                try {
                    string assemblyFilePath = Assembly.GetExecutingAssembly().Location;
                    string assemblyDirPath = Path.GetDirectoryName(assemblyFilePath);
                    Assembly assembly = Assembly.LoadFile(assemblyDirPath + "\\" + game + ".dll");
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types) {
                        if (type.BaseType == typeof(ScriptWriter) && type.Name == game + "Writer") {
                            sw = assembly.CreateInstance(type.FullName) as ScriptWriter;
                            break;
                        }
                    }
                }
                catch { }
            }
            if (sw == null) {
                sw = new ScriptWriter();
            }
            return sw;
        }

        public static ScriptWriter GetWriterForFile(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            return GetWriter(doc.DocumentElement.Attributes["Game"] == null ? string.Empty : doc.DocumentElement.Attributes["Game"].Value);
        }

    }
}
