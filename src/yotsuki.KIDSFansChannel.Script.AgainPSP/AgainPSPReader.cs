﻿using System;
using System.Collections.Generic;
using System.Text;
using yotsuki.KIDSFansChannel.Common.BaseClass;
using System.IO;

namespace yotsuki.KIDSFansChannel.Script.AgainPSP
{
    public class AgainPSPReader : ScriptReader
    {

        public override void Reader(string binfile, string txtfile, string xmlfile)
        {
            Open(binfile);
            enc = Encoding.GetEncoding("shift-jis");//设置bin文件编码
            byte dia = 0x18;
            byte select = 0x1b;
            byte movie = 0xd8;
            byte top = 0x24;

            XmlCreate("AgainPSP", txtfile);

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
                    if (bytes[0] != 0x10)
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
                    tmp = bin_br.ReadByte();
                    long tmp_offset = bin_fs.Position;
                    if (tmp !=0x1c)
                    {
                        isTrue = true;
                    }

                    int selectCount = bin_br.ReadUInt16();

                    if (selectCount < 2 | selectCount > 6)
                    {
                        isTrue = true;
                    }
                    if (isTrue)
                    {
                        bin_fs.Seek(tmp_offset, SeekOrigin.Begin);
                        continue;
                    }
                    for (i = 0; i < selectCount; i++)
                    {
                        bin_fs.Seek(4, SeekOrigin.Current);
                        int indexOffset = (int)bin_fs.Position;
                        offset = (int)bin_br.ReadInt32();
                        if (offset < bin_fs.Length)
                        {
                            string text = readString(offset);
                            XmlTextNodeAdd(txtAdd(text), indexOffset, "Select");
                        }
                        if(i<selectCount-1)
                            bin_fs.Seek(2, SeekOrigin.Current);
                    }
                    #endregion
                }
                //else if (b == movie)
                //{
                //    #region 视频代码提取
                //    tmp = bin_br.ReadByte();
                //    if (tmp != 0x18)
                //    {
                //        bin_fs.Seek(-1, SeekOrigin.Current);
                //        continue;
                //    }
                //    int indexOffset = (int)bin_fs.Position;
                //    offset = (int)bin_br.ReadUInt16();
                //    if (offset < bin_fs.Length)
                //    {
                //        string text = readString(offset);
                //        XmlTextNodeAdd(txtAdd(text), indexOffset, "Movie");
                //    }
                //    #endregion
                //}
                else if (b == top)
                {
                    #region 开头代码提取
                    tmp = bin_br.ReadByte();
                    if (tmp != 0x06)
                    {
                        bin_fs.Seek(-1, SeekOrigin.Current);
                        continue;
                    }
                    int indexOffset = (int)bin_fs.Position;
                    offset = (int)bin_br.ReadUInt16();
                    if (offset < bin_fs.Length)
                    {
                        string text = readString(offset);
                        //if (!string.IsNullOrEmpty(text))
                        //{
                            XmlTextNodeAdd(txtAdd(text), indexOffset, "Code");
                        //}
                    }
                    #endregion
                }

            }
            Save(txtfile, xmlfile);
        }
    }
}
