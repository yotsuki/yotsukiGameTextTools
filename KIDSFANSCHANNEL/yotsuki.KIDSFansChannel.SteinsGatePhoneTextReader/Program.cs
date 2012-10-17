using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace yotsuki.KIDSFansChannel.SteinsGatePhoneTextReader
{
    class Program
    {
        /// <summary>
        /// Write [输入目录] [输出目录]
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length != 2 )
            {
                Console.WriteLine("参数错误");
                return;
            }

            string indir = args[0];
            string outdir = args[1];

            if (!Directory.Exists(indir))
            {
                Console.WriteLine(indir+"目录不存在");
                return;
            }

            if (!Directory.Exists(outdir))
            {
                Console.WriteLine(outdir + "目录不存在");
                return;
            }

            string[] files = Directory.GetFiles(indir, "*.xml");
            //encodingtable.Clear();
            foreach (string file in files)
            {
                ReadPhoneText(file,outdir);
            }

            Console.ReadKey();

        }

        private static void ReadPhoneText(string infile,string outdir)
        {
            FileInfo inf = new FileInfo(infile);
            string outfile = ((outdir + "\\").Replace(@"\\", "\\") + inf.Name).Replace(inf.Extension, ".txt");
            //this._writeLog("开始导入"+outfile, null);
            int startoffset = 0;
            int endoffset = 0;
            IList<ScriptTxt> txtList = null;

            FileStream outfs = null;
            StreamWriter sw = null;

            #region 读取XML
            XmlDocument doc = new XmlDocument();
            doc.Load(infile);
            XmlNodeList nodelist = doc.GetElementsByTagName("Script");
            foreach (XmlNode node in nodelist)
            {
                //this._writeLog("游戏名称" + node.Attributes["Game"].Value,null);
                XmlNode firstChild = node.FirstChild;
                string txtfile = "";
                if (firstChild.Name == "TxtFilePath")
                {
                    txtfile = firstChild.InnerText;
                }
                else
                {

                    sw_WriteLog("ERROR:未找到节点TxtFilePath", null);
                    return;
                }
                txtList = ReadTxt(inf.DirectoryName + "\\" + txtfile);
                if (txtList == null)
                {
                    sw_WriteLog("ERROR:文本文件读取错误" + txtfile, null);
                    return;
                }
                XmlNode child = firstChild.NextSibling;

                try
                {
                    if (txtList.Count != int.Parse(child.Attributes["Count"].Value))
                    {
                        sw_WriteLog("ERROR:文本文件行数错误" + txtfile, null);
                        return;
                    }
                    startoffset = int.Parse(child.Attributes["Start"].Value);
                    endoffset = int.Parse(child.Attributes["End"].Value);
                }
                catch (Exception)
                {
                    sw_WriteLog("ERROR:TextFileIndex节点参数错误" + txtfile, null);
                    return;
                }
                int i = 0;
                foreach (XmlNode txtNode in child.ChildNodes)
                {
                    if (txtNode.Name == "Text")
                    {
                        XmlAttribute xa = txtNode.Attributes["Phone"];
                        if (xa != null && xa.Value == "true")
                        {
                            if (outfs == null)
                            {
                                outfs = new FileStream(outfile, FileMode.Create, FileAccess.Write);
                                sw = new StreamWriter(outfs, Encoding.Unicode);
                            }

                            i = int.Parse(txtNode.Attributes["Index"].Value);
                            //txtList[i - 1].IndexOffset = int.Parse(txtNode.Attributes["IndexOffset"].Value);
                            sw.WriteLine(txtList[i - 1].Text);
                        }
                    }
                }
            }
            #endregion
            if (sw != null)
            {
                sw.Flush();
                sw.Close();
            }
        }

        static void sw_WriteLog(object sender, EventArgs e)
        {
            string message=sender as string;
            Console.WriteLine(message);
        }
        private static IList<ScriptTxt> ReadTxt(string filename)
        {
            FileStream fs = null;
            StreamReader sr = null;
            IList<ScriptTxt> list = new List<ScriptTxt>();
            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(fs, Encoding.UTF8);
                while (!sr.EndOfStream)
                {
                    ScriptTxt st = new ScriptTxt();
                    st.Text = sr.ReadLine();
                    if (string.IsNullOrEmpty(st.Text))
                    {
                        st.Text = "\0";
                    }
                    list.Add(st);
                }
                return list;
            }
            catch (Exception ex)
            {
                sw_WriteLog(ex.Message, null);
                return null;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
                if (fs != null)
                    fs.Close();
            }
        }

        class ScriptTxt
        {
            public string Text;
            public int IndexOffset;
        }
    }
}
