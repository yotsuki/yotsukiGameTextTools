using System;
using System.Collections.Generic;
using System.Text;
using yotsuki.KIDSFansChannel.Common.BaseClass;
using System.IO;

namespace yotsuki.KIDSFansChannel.Script.Again
{
    public class AgainReader : ScriptReader
    {
        public override void Reader(string binfile, string txtfile, string xmlfile)
        {
            Open(binfile);
            enc = Encoding.GetEncoding("shift-jis");//设置bin文件编码
            byte dia = 0x18;
            byte select = 0x1B;
            byte sys = 0x24;

            XmlCreate("Again", txtfile);
            byte b=0;
            int offset = 0;
            int count = 0;
            bool isTrue=false;
            while (bin_fs.Position < bin_fs.Length)
            {
                isTrue = false;
                b = bin_br.ReadByte();
                #region 文本
                if (b == dia)
                {
                    byte tmp = bin_br.ReadByte();
                    if (tmp == 0x0e)
                    {
                        bin_br.ReadBytes(4);
                    }
                    else
                    {
                        isTrue = true;
                        bin_fs.Seek(-1, SeekOrigin.Current);
                        continue;
                    }

                    int indexOffset = (int)bin_fs.Position;
                    offset = (int)bin_br.ReadUInt16();
                    if (offset < bin_fs.Length & offset > indexOffset)
                    {
                        string text = readString(offset);
                        XmlTextNodeAdd(txtAdd(text), indexOffset, "Text");
                        count++;
                    }
                }
                #endregion

                #region 选项
                if (b == select)
                {
                    byte tmp = bin_br.ReadByte();
                    if (tmp % 0x08 > 0)
                    {
                        isTrue = true;
                        bin_fs.Seek(-1, SeekOrigin.Current);
                        continue;
                    }

                    int selectcount = bin_br.ReadUInt16();
                    if (select < 2 || select > 8)
                    {
                        isTrue = true;
                        bin_fs.Seek(-2, SeekOrigin.Current);
                        continue;
                    }
                    for (int i = 0; i < selectcount; i++)
                    {
                        bin_br.ReadBytes(4);
                        int indexOffset = (int)bin_fs.Position;
                        offset = (int)bin_br.ReadUInt16();
                        if (offset < bin_fs.Length & offset > indexOffset)
                        {
                            string text = readString(offset);
                            XmlTextNodeAdd(txtAdd(text), indexOffset, "Select");
                            count++;
                        }
                        bin_br.ReadBytes(2);
                    }
                }
                #endregion

                #region 系统字符
                if(b==sys)
                {
                    byte tmp=bin_br.ReadByte();
                    if(tmp !=0x04)
                    {
                        isTrue = true;
                        bin_fs.Seek(-1, SeekOrigin.Current);
                        continue;

                    }
                    int indexOffset = (int)bin_fs.Position;
                    offset = (int)bin_br.ReadUInt16();
                    if (offset < bin_fs.Length & offset > indexOffset)
                    {
                        string text = readString(offset);
                        XmlTextNodeAdd(txtAdd(text), indexOffset, "System");
                        count++;
                    }

                }
                #endregion

            }
            Save(txtfile, xmlfile);
        }
    }
}
