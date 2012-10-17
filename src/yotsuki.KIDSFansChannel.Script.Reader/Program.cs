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
                Console.WriteLine("�����ʽ:Reader <bin�ļ���Ŀ¼> <��ȡ������> [t|f]");
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
                        Console.WriteLine(f + "�������");
                    }
                }
                else
                {
                    if (File.Exists(binfile))
                    {
                        sr.Reader(binfile, sr.GetTxtFileName(binfile), isxml ? sr.GetXmlFileName(binfile) : string.Empty);
                        Console.WriteLine(binfile + "�������");
                    }
                }
            }
            else
            {
                Console.WriteLine(string.Format("û���ҵ�{0}�Ķ�ȡ��", game));
            }

            Console.WriteLine("��������˳�");
            Console.ReadKey();
        }
    }
}
