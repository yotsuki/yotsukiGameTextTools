using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace yotsuki.KIDSFansChannel.Common.BaseClass
{
    public abstract class ScriptReader
    {
        public FileStream bin_fs = null;
        public BinaryReader bin_br = null;

        public List<string> txtList = new List<string>();

        public Encoding enc = Encoding.GetEncoding("shift-jis");

        public XmlDocument xmldoc=null;
        XmlElement root = null;
        XmlElement TextNode = null;

        public int start = 0;
        public int end = 0;

        /// <summary>
        /// 打开Bin文件
        /// </summary>
        /// <param name="binfile"></param>
        public void Open(string binfile)
        {
            txtList.Clear();
            xmldoc = null;
            try
            {
                bin_fs = new FileStream(binfile, FileMode.Open, FileAccess.Read);
                bin_br = new BinaryReader(bin_fs);
            }
            catch (IOException)
            {
                throw new Exception("Bin文件" + binfile + "打开错误");
            }
        }

        /// <summary>
        /// 从指定位置读取字符串,遇到0x00结束
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public string readString(int offset)
        {
            if (start == 0)
                start = offset;
            Stream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);
            int origin =(int) bin_fs.Position;
            bin_fs.Seek(offset, SeekOrigin.Begin);
            byte b=0;
            while (bin_fs.Position<bin_fs.Length && (b=bin_br.ReadByte()) != 0)
            {
                bw.Write(b);
            }
            end =(int) bin_fs.Position;
            if (end == 3)
            {
                end += 0;
            }
            bin_fs.Seek(origin, SeekOrigin.Begin);

            BinaryReader br = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] bs = br.ReadBytes((int)stream.Length);
            return enc.GetString(bs);
        }


        /// <summary>
        /// 向txt文件写入一行文本
        /// </summary>
        /// <param name="text"></param>
        /// <returns>行号</returns>
        public int txtAdd(string text)
        {
            txtList.Add(text);
            return txtList.Count;
        }


        #region XML处理相关
        public void XmlCreate(string game,string txtfile)
        {
            xmldoc = new XmlDocument();
            root = xmldoc.CreateElement("Script");
            FileInfo fi = new FileInfo(txtfile);

            XmlAttribute att = xmldoc.CreateAttribute("Game");
            att.Value = game;
            root.Attributes.Append(att);
            xmldoc.AppendChild(root);
            XmlElement info = xmldoc.CreateElement("TxtFilePath");
            XmlText text = xmldoc.CreateTextNode(fi.Name);
            info.AppendChild(text);
            root.AppendChild(info);

            TextNode = xmldoc.CreateElement("TextFileIndex");
            root.AppendChild(TextNode);
        }

        public void XmlAddComment(string str)
        {
            XmlComment comment = xmldoc.CreateComment(str);
            root.AppendChild(comment);
        }
        public void Save(string txtfile,string xmlfile)
        {
            if (txtList.Count != 0)
            {
                FileStream txt_fs=null;
                StreamWriter txt_sw = null;
                try
                {
                    txt_fs = new FileStream(txtfile, FileMode.Create, FileAccess.Write);
                    txt_sw = new StreamWriter(txt_fs, Encoding.UTF8);
                }
                catch (IOException)
                {
                    throw new Exception("txt文件" + txtfile + "创建错误");
                }

                foreach (string str in txtList)
                {
                    txt_sw.WriteLine(str);
                }

                txt_sw.Close();

                XmlAttribute txtCount = xmldoc.CreateAttribute("Count");
                txtCount.Value = txtList.Count.ToString();
                TextNode.Attributes.Append(txtCount);

                XmlAttribute AttStart = xmldoc.CreateAttribute("Start");
                AttStart.Value = start.ToString();
                TextNode.Attributes.Append(AttStart);

                XmlAttribute AttEnd = xmldoc.CreateAttribute("End");
                AttEnd.Value = end.ToString();
                TextNode.Attributes.Append(AttEnd);
                if(!string.IsNullOrEmpty(xmlfile))
                    xmldoc.Save(xmlfile);
            }

        }

        public XmlElement XmlTextNodeAdd(int index, int offset, string type,int maxLength = -1)
        {
            XmlElement txt = xmldoc.CreateElement("Text");
            XmlAttribute textindex = xmldoc.CreateAttribute("Index");
            textindex.Value = index.ToString();

            XmlAttribute textoffset = xmldoc.CreateAttribute("IndexOffset");
            textoffset.Value = offset.ToString();

            XmlAttribute texttype = xmldoc.CreateAttribute("Type");
            texttype.Value = type;

            txt.Attributes.Append(textindex);
            txt.Attributes.Append(texttype);
            txt.Attributes.Append(textoffset);
            if (maxLength >0) {
                XmlAttribute textMaxLength = xmldoc.CreateAttribute("MaxLength");
                textMaxLength.Value = maxLength.ToString();
                txt.Attributes.Append(textMaxLength);
            }

            TextNode.AppendChild(txt);

            return txt;
        }


        #endregion

        //public abstract void Reader(string binfile,string txtfile,string xmlfile,Encoding binEnc);
        /// <summary>
        /// 读取器抽象方法
        /// </summary>
        /// <param name="binfile">bin文件名</param>
        /// <param name="txtfile">txt文件名</param>
        /// <param name="xmlfile">xml文件名</param>
        public abstract void Reader(string binfile, string txtfile, string xmlfile);

        public virtual string[] GetFiles(string path)
        {
            return Directory.GetFiles(path, "*.bin");
        }

        public virtual string GetTxtFileName(string binfile)
        {
            return binfile.Replace(".bin", ".txt");
        }

        public virtual string GetXmlFileName(string binfile)
        {
            return binfile.Replace(".bin", ".xml");
        }

    }
}
