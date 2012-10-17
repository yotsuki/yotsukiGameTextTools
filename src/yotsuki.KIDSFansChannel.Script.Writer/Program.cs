using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using yotsuki.KIDSFansChannel.Common.BaseClass;

namespace yotsuki.KIDSFansChannel.Script.Writer
{
    class Program
    {
        /// <summary>
        /// Write [����Ŀ¼] [���Ŀ¼] [����ļ�]
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length != 3 )
            {
                Console.WriteLine("��������");
                return;
            }

            string indir = args[0];
            string outdir = args[1];
            string encodingFile = args[2];

            if (!Directory.Exists(indir))
            {
                Console.WriteLine(indir+"Ŀ¼������");
                return;
            }

            if (!Directory.Exists(outdir))
            {
                Console.WriteLine(outdir + "Ŀ¼������");
                return;
            }

            if (!File.Exists(encodingFile))
            {
                Console.WriteLine(encodingFile + "�ļ�������");
                return;
            }

            Dictionary<char,byte[]> encodingtable = EncodingTable.ReadEncodingTable(encodingFile);

            if (encodingtable == null)
            {
                throw new Exception();
            }

            string[] files = Directory.GetFiles(indir, "*.xml");

            //encodingtable.Clear();
            foreach (string file in files)
            {
                ScriptWriter sw = ScriptFactory.GetWriterForFile(file);
                sw.WriteLog += new EventHandler(sw_WriteLog);
                sw.Write(file, outdir, encodingtable);
            }

            Console.ReadKey();

        }

        static void sw_WriteLog(object sender, EventArgs e)
        {
            string message=sender as string;
            Console.WriteLine(message);
        }

        


    }
}
