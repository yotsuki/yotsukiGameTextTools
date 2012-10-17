using System;
using System.Collections.Generic;
using System.Text;
using yotsuki.KIDSFansChannel.Common.BaseClass;
using System.IO;

namespace yotsuki.free.Script.H2OPS2
{
    public class H2OReader : ScriptReader
    {
        public override void Reader(string binfile, string txtfile, string xmlfile)
        {
            Open(binfile);
            enc = Encoding.GetEncoding("shift-jis");//设置bin文件编码
            byte dia = 0x60;
            byte select = 0x72;
            //byte movie = 0xd8;
            //byte top = 0x24;

            XmlCreate("H2O", txtfile);

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
                    bytes = bin_br.ReadBytes(3);
                    if (bytes[0] != 0x00  |bytes[1] != 0x00 | bytes[2] != 0x00)
                    {
                        isTrue = true;
                    }
                    if (isTrue)
                    {
                        bin_fs.Seek(-3, SeekOrigin.Current);
                        continue;
                    }
                    int indexOffset = (int)bin_fs.Position;
                    offset = (int)bin_br.ReadUInt32();
                    if (offset < bin_fs.Length & offset > start)
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
                    long tmp_offset = bin_fs.Position;

                    int selectCount = bin_br.ReadByte();

                    if (selectCount < 2 | selectCount > 6)
                    {
                        isTrue = true;
                    }
                    byte[] tmps = bin_br.ReadBytes(2);
                    if (tmps[0] != 0x07 | tmps[1] != 0x60)
                    {
                        isTrue = true;
                    }
                    if (isTrue)
                    {
                        bin_fs.Seek(tmp_offset, SeekOrigin.Begin);
                        continue;
                    }
                    long select_offset = bin_br.ReadUInt32();
                    bin_br.BaseStream.Seek(select_offset, SeekOrigin.Begin);
                    for (i = 0; i < selectCount; i++)
                    {
                        int indexOffset = (int)bin_fs.Position;
                        offset = (int)bin_br.ReadUInt32();
                        if (offset < bin_fs.Length)
                        {
                            string text = readString(offset);
                            XmlTextNodeAdd(txtAdd(text), indexOffset, "Select");
                        }
                        bin_fs.Seek(8, SeekOrigin.Current);
                    }
                    bin_fs.Seek(tmp_offset+3, SeekOrigin.Begin);
                    #endregion

                }

            }
            Save(txtfile, xmlfile);
        }
    }
}
