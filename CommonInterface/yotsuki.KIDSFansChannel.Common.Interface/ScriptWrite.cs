using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Xml;

namespace yotsuki.KIDSFansChannel.Common.BaseClass
{
    public class ScriptWriter
    {
        protected event EventHandler _writeLog;
        public event EventHandler WriteLog
        {
            add
            {
                this._writeLog = value;
            }
            remove
            {
                this._writeLog = null;
            }
        }

        public virtual void Write(string infile, string outdir, Dictionary<char,byte[]> encodingtable)
        {
            FileInfo inf = new FileInfo(infile);
            string outfile = ((outdir + "\\").Replace(@"\\", "\\") + inf.Name).Replace(inf.Extension, ".bin");
            //this._writeLog("开始导入"+outfile, null);
            int startoffset=0;
            int endoffset=0;
            IList<ScriptTxt> txtList=null;
            string errMsg = string.Empty;
            ReadXml(inf, ref startoffset, ref endoffset, ref txtList, ref errMsg);
            if (!string.IsNullOrEmpty(errMsg)) {
                return;
            }

            WriteBinfile(encodingtable, inf, outfile, startoffset, endoffset, txtList);
        }

        protected virtual void WriteBinfile(Dictionary<char, byte[]> encodingtable, FileInfo inf, string outfile, int startoffset, int endoffset, IList<ScriptTxt> txtList)
        {
            #region 写入bin文件
            FileStream fs = null;
            BinaryWriter bw = null;
            int offset = startoffset;

            try {
                fs = new FileStream(inf.FullName.Replace(inf.Extension, ".bin"), FileMode.OpenOrCreate, FileAccess.Read);
                byte[] startbyte = new byte[startoffset];
                fs.Read(startbyte, 0, startbyte.Length);

                byte[] endbyte = new byte[fs.Length - endoffset];
                fs.Seek(endoffset, SeekOrigin.Begin);
                fs.Read(endbyte, 0, endbyte.Length);



                if (File.Exists(outfile))
                    File.Delete(outfile);
                fs = new FileStream(outfile, FileMode.CreateNew, FileAccess.Write);
                bw = new BinaryWriter(fs);
                bw.Write(startbyte);
                foreach (ScriptTxt st in txtList) {
                    byte[] bytes = Encode(st.Text, encodingtable);
                    fs.Seek(st.IndexOffset, SeekOrigin.Begin);
                    if (offset <= 0xfff) {
                        bw.Write((UInt16)offset);
                    }
                    else {
                        bw.Write((UInt32)offset);
                    }
                    fs.Seek(offset, SeekOrigin.Begin);
                    bw.Write(bytes);
                    bw.Write((byte)0);
                    //if (st.IndexOffset == txtList[txtList.Count - 1].IndexOffset)
                    //{
                    //}
                    offset += bytes.Length + 1;
                }
                bw.Write(endbyte);
            }
            catch (Exception e) {
                this._writeLog(e.Message, null);
            }
            finally {
                if (bw != null)
                    bw.Close();
                if (fs != null)
                    fs.Close();
            }
            #endregion
        }

        protected virtual void ReadXml(FileInfo inf, ref int startoffset, ref int endoffset, ref IList<ScriptTxt> txtList, ref string errMsg)
        {
            #region 读取XML
            XmlDocument doc = new XmlDocument();
            doc.Load(inf.FullName);
            XmlNodeList nodelist = doc.GetElementsByTagName("Script");
            foreach (XmlNode node in nodelist) {
                //this._writeLog("游戏名称" + node.Attributes["Game"].Value,null);
                XmlNode firstChild = node.FirstChild;
                string txtfile = "";
                if (firstChild.Name == "TxtFilePath") {
                    txtfile = firstChild.InnerText;
                }
                else {
                    errMsg = "ERROR:未找到节点TxtFilePath";
                    this._writeLog(errMsg, null);
                    break;
                }
                txtList = ReadTxt(inf.DirectoryName + "\\" + txtfile);
                if (txtList == null) {
                    errMsg = "ERROR:文本文件读取错误" + txtfile;
                    this._writeLog(errMsg, null);
                    break;
                }
                XmlNode child = firstChild.NextSibling;

                try {
                    if (txtList.Count != int.Parse(child.Attributes["Count"].Value)) {
                        errMsg = "ERROR:文本文件行数错误" + txtfile;
                        this._writeLog(errMsg, null);
                        break;
                    }
                    startoffset = int.Parse(child.Attributes["Start"].Value);
                    endoffset = int.Parse(child.Attributes["End"].Value);
                }
                catch (Exception) {
                    errMsg = "ERROR:TextFileIndex节点参数错误" + txtfile;

                    this._writeLog(errMsg, null);
                    break;
                }
                int i = 0;
                foreach (XmlNode txtNode in child.ChildNodes) {
                    if (txtNode.Name == "Text") {
                        i = int.Parse(txtNode.Attributes["Index"].Value);
                        txtList[i - 1].IndexOffset = int.Parse(txtNode.Attributes["IndexOffset"].Value);
                        if (txtNode.Attributes["MaxLength"] != null)
                            txtList[i - 1].MaxLength = int.Parse(txtNode.Attributes["MaxLength"].Value);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// 将字符串转为换为字节数组
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="encodingtable">码表</param>
        /// <returns></returns>
        protected byte[] Encode(string str, Dictionary<char,byte[]> encodingtable)
        {
            byte[] bytes = new byte[GetStringByteLength(str)];
                byte[] tmp = new byte[] { 0x81, 0x40 };//如果查不到码表的默认值
                int index = 0;
                char[] chars = str.ToCharArray();
                foreach (char c in chars)
                {
                    if (c > 255)
                    {
                        if (encodingtable.ContainsKey(c))
                        {
                            encodingtable[c].CopyTo(bytes, index);
                            index += encodingtable[c].Length;
                        }
                        else
                        {
                            try
                            {
                            //tmp = Encoding.GetEncoding("shift-jis").GetBytes(c.ToString());
                            tmp.CopyTo(bytes, index);
                            index += tmp.Length;
                            }catch(Exception ex)
                            {
                            }
                        }
                    }
                    else
                    {
                        byte[] b = Encoding.GetEncoding("GB2312").GetBytes(c.ToString());
                        try
                        {
                            b.CopyTo(bytes, index);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        index += b.Length;
                    }
                }
                return bytes;
        }


        private IList<ScriptTxt> ReadTxt(string filename)
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
                this._writeLog(ex.Message,null);
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

        private int GetStringByteLength(string str)
        {
            char[] chars = str.ToCharArray();
            int count = 0;
            foreach (char c in chars)
            {
                if (c > 0x7f)
                    count += 2;
                else
                    count += 1;
            }
            return count;
        }

    }

    public class ScriptTxt
    {
        public string Text;
        public int IndexOffset;
        public int MaxLength;
    }

}
