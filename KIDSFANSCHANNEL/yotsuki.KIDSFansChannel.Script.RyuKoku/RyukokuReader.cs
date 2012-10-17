using System;
using System.Collections.Generic;
using System.Text;
using yotsuki.KIDSFansChannel.Common.BaseClass;
using System.IO;

namespace yotsuki.KIDSFansChannel.Script.Reader
{
    public class RyukokuReader : ScriptReader
    {
        public override void Reader(string binfile, string txtfile, string xmlfile)
        {
            Open(binfile);
            enc = Encoding.GetEncoding("shift-jis");//设置bin文件编码
            byte dia = 0x18;
            byte select = 0x23;
            byte movie = 0xd8;
            byte top = 0x24;

            XmlCreate("RyuKoku", txtfile);

            byte b = 0;
            byte tmp = 0;
            byte[] bytes = null;
            int offset = 0;
            int i = 0;
            int count = 0;
            bool isTrue = false;
            while (bin_fs.Position < bin_fs.Length)
            {
                isTrue = false;
                b = bin_br.ReadByte();
                if (b == dia)
                {
                    #region 对话处理
                    bytes = bin_br.ReadBytes(5);
                    if (bytes[0] != 0x0e | bytes[1] != 0x00 | bytes[2] != 0x00 | bytes[3] != 0xff | bytes[4] != 0xff)
                    {
                        isTrue = true;
                    }
                    if (isTrue)
                    {
                        bin_fs.Seek(-5, SeekOrigin.Current);
                        continue;
                    }
                    int indexOffset = (int)bin_fs.Position;
                    offset = (int)bin_br.ReadUInt16();
                    if (offset < bin_fs.Length)
                    {
                        string text = readString(offset);
                        XmlTextNodeAdd(txtAdd(text), indexOffset, "Text");
                        //count++;
                    }
                    #endregion
                }
                else if (b == select)
                {
                    #region 选项处理
                    
                    bytes = bin_br.ReadBytes(2);
                    if (bytes[0] != 0x02 | bytes[1] != 0x1b)
                    {
                        isTrue = true;
                    }
                    if (isTrue)
                    {
                        bin_fs.Seek(-2, SeekOrigin.Current);
                        continue;
                    }
                    bin_fs.Seek(3, SeekOrigin.Current);
                    int selectCount = bin_br.ReadByte();
                    for (i = 0; i < selectCount; i++)
                    {
                        bin_fs.Seek(5, SeekOrigin.Current);
                        int indexOffset = (int)bin_fs.Position;
                        offset = (int)bin_br.ReadUInt16();
                        if (offset < bin_fs.Length)
                        {
                            string text = readString(offset);
                            XmlTextNodeAdd(txtAdd(text), indexOffset, "Select");
                        }
                        bin_fs.Seek(2, SeekOrigin.Current);
                    }
                    #endregion

                }
                else if (b == movie)
                {
                    #region 视频代码提取
                    tmp = bin_br.ReadByte();
                    if (tmp != 0x0e)
                    {
                        bin_fs.Seek(-1, SeekOrigin.Current);
                        continue;
                    }
                    int indexOffset = (int)bin_fs.Position;
                    offset = (int)bin_br.ReadUInt16();
                    if (offset < bin_fs.Length)
                    {
                        string text = readString(offset);
                        XmlTextNodeAdd(txtAdd(text), indexOffset, "Movie");
                    }
                    #endregion
                }
                else if (b == top)
                {
                    #region 开头代码提取
                    tmp = bin_br.ReadByte();
                    if (tmp != 0x04)
                    {
                        bin_fs.Seek(-1, SeekOrigin.Current);
                        continue;
                    }
                    int indexOffset = (int)bin_fs.Position;
                    offset = (int)bin_br.ReadUInt16();
                    if (offset < bin_fs.Length)
                    {
                        string text = readString(offset);
                        XmlTextNodeAdd(txtAdd(text), indexOffset, "Code");
                    }
                    #endregion
                }

            }
            Save(txtfile, xmlfile);

        }
    }
}
