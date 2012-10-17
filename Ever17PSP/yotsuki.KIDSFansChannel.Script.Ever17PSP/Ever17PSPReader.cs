using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using yotsuki.KIDSFansChannel.Common.BaseClass;

namespace yotsuki.KIDSFansChannel.Script.Reader
{
    public class Ever17PSPReader:ScriptReader
    {
        public override void Reader(string binfile, string txtfile, string xmlfile)
        {
            Open(binfile);
            enc = Encoding.GetEncoding("shift-jis");//设置bin文件编码
            byte dia = 0x61;
            byte select = 0x73;
            
            XmlCreate("Ever17PSP", txtfile);

            byte b=0;
            byte tmp=0;
            int offset = 0;
            int i=0;
            int count = 0;
            bool isTrue=false;
            while (bin_fs.Position < bin_fs.Length)
            {
                isTrue = false;
                b = bin_br.ReadByte();

                #region 处理对话
                if (b == dia)
                {
                    
                    byte[] bs = bin_br.ReadBytes(3);
                    if (bs[0] != 0x00 || bs[1] > 0xf || bs[2] != 0x00)
                        isTrue = true;
                    
                    if (isTrue)
                    {
                        bin_fs.Seek(-3, SeekOrigin.Current);
                        continue;
                    }
                    int indexOffset = (int)bin_fs.Position;
                    offset = (int)bin_br.ReadUInt32();
                    if (offset < bin_fs.Length & offset > indexOffset)
                    {
                        string text = readString(offset);
                        XmlTextNodeAdd(txtAdd(text), indexOffset, "Text");
                        count++;
                    }
                }
                #endregion

                #region 处理选项
                if (b == select)
                {
                    int selectCount = bin_br.ReadByte();
                    byte[] selectTmp = bin_br.ReadBytes(2);
                    if (selectTmp[0] != 0x07 && selectTmp[1] != 0x60)
                    {
                        bin_fs.Seek(-3, SeekOrigin.Current);
                        continue;
                    }
                    int tmpOffset =(int)bin_fs.Position;
                    int selectOffset = bin_br.ReadUInt16();
                    bin_fs.Seek(selectOffset, SeekOrigin.Begin);
                    for (i = 0; i < selectCount; i++)
                    {
                        int indexOffset = (int)bin_fs.Position;
                        offset =(int)bin_br.ReadUInt32();
                        if (offset < bin_fs.Length)
                        {
                            string selectStr = readString(offset);
                            XmlTextNodeAdd(txtAdd(selectStr), indexOffset, "Select");
                            bin_fs.Seek(8, SeekOrigin.Current);
                        }
                    }
                    bin_fs.Seek(tmpOffset, SeekOrigin.Begin);
                }
                #endregion
            }
            Save(txtfile,xmlfile);
        }
    }
}
