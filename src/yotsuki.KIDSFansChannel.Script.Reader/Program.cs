using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using yotsuki.KIDSFansChannel.Common.BaseClass;
using yotsuki.free.Script.H2OPS2;


namespace yotsuki.KIDSFansChannel.Script.Reader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("命令格式:Reader <bin文件或目录> <读取器名称> [t|f]");
                return;
            }
            string binfile=args[0].ToLower();
            string game=args[1];
            string xml = args.Length == 3 ? args[2] : "f";
            bool isxml=false;
            if (xml.ToLower() == "t")
            {
                isxml = true;
            }
            ScriptReader sr =null;
            sr = ScriptFactory.GetReader(game);
            if (sr != null)
            {
                if (Directory.Exists(binfile))
                {
                    string[] files = sr.GetFiles(binfile);
                    foreach (string file in files)
                    {
                        string f = file.ToLower();
                        sr.start = 0;
                        if (f.IndexOf(@".\") == 0)
                            f = f.Substring(2, f.Length - 2);
                        sr.Reader(f, sr.GetTxtFileName(f), isxml ? sr.GetXmlFileName(f) : string.Empty);
                        Console.WriteLine(f + "处理完成");
                    }
                }
                else
                {
                    if (File.Exists(binfile))
                    {
                        sr.Reader(binfile, sr.GetTxtFileName(binfile), isxml ? sr.GetXmlFileName(binfile) : string.Empty);
                        Console.WriteLine(binfile + "处理完成");
                    }
                }
            }
            else
            {
                Console.WriteLine(string.Format("没有找到{0}的读取器", game));
            }

            Console.WriteLine("按任意键退出");
            Console.ReadKey();
        }
    }
}
