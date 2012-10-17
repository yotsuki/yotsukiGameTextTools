using System;
using System.Collections.Generic;
using System.Text;
using yotsuki.KIDSFansChannel.Common.BaseClass;
using System.IO;
using System.Xml;

namespace yotsuki.KIDSFansChannel.Script.ChaosHeadLoveChuChu
{
    public class ChaosHeadLoveChuChuReader:ScriptReader
    {
        private FileStream fsPhone = null;
        private StreamWriter swPhone = null;

        public ChaosHeadLoveChuChuReader():base()
        {
            fsPhone = new FileStream("phone.txt",FileMode.Create,FileAccess.Write);
            swPhone = new StreamWriter(fsPhone, Encoding.Unicode);
        }

        ~ChaosHeadLoveChuChuReader()
        {
            //if (swPhone != null)
            //{
            //    swPhone.Close();
            //    swPhone = null;
            //}

            if (fsPhone != null)
            {
                fsPhone.Close();
                fsPhone = null;
            }
        }

        public override void Reader(string binfile, string txtfile, string xmlfile)
        {
            Open(binfile);
            
            enc = Encoding.GetEncoding("shift-jis");//设置bin文件编码
            byte dia = 0x18;
            byte select = 0x1b;
            byte movie = 0xd9;
            byte top = 0x24;
            byte dict = 0x01;
            byte phone = 0x1a;
            //byte msgbox = 

            XmlCreate("ChaosHeadLoveChuChu", txtfile);

            byte b = 0;
            byte tmp = 0;
            byte[] bytes = null;
            int offset = 0;
            int i = 0;
            int count = 0;
            bool isTrue = false;
            bool isPhone = false;
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
                    //uint u32tmp = bin_br.ReadUInt32();
                    //if (u32tmp >= bin_fs.Length)
                    //{
                    //    offset = (int)(u32tmp & 0xffff);
                    //}
                    offset = (int)bin_br.ReadUInt32();
                    if (offset < bin_fs.Length)
                    {
                        string text = readString(offset);
                        XmlElement txt = XmlTextNodeAdd(txtAdd(text), indexOffset, "Text");
                        if (isPhone)
                        {
                            XmlAttribute textphone = xmldoc.CreateAttribute("Phone");
                            textphone.Value = "true";
                            txt.Attributes.Append(textphone);
                            swPhone.WriteLine(text);
                            swPhone.Flush();
                        }
                        //count++;
                    }
                    #endregion
                }
                else if (b == select)
                {
                    #region 选项处理
                    tmp = bin_br.ReadByte();
                    long tmp_offset = bin_fs.Position;
                    if (tmp != 0x1c)
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
                        if (i < selectCount - 1)
                            bin_fs.Seek(2, SeekOrigin.Current);
                    }
                    #endregion
                }
                else if (b == movie)
                {
                    #region 视频代码提取
                    tmp = bin_br.ReadByte();
                    if (tmp != 0x18)
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
                else if (b == dict)
                {
                    #region DICT
                    bytes = bin_br.ReadBytes(8);
                    if (bytes[0] != 0x01 || bytes[1] != 0xff || bytes[2] != 0xff || bytes[3] != 0x00 || bytes[4] != 0x00 || bytes[5] != 0x00 || bytes[6] != 0x00 || bytes[7] != 0x01)
                    {
                        isTrue = true;
                    }
                    if (isTrue)
                    {
                        bin_fs.Seek(-8, SeekOrigin.Current);
                        continue;
                    }
                    bin_fs.Seek(7, SeekOrigin.Current);
                    for (int j = 0; j < 5; j++)
                    {
                        int indexOffset = (int)bin_fs.Position;
                        offset = (int)bin_br.ReadUInt16();
                        string text = readString(offset);
                        XmlTextNodeAdd(txtAdd(text), indexOffset, "Dict"+(j+1).ToString());
                    }
                    #endregion
                }
                else if (b == phone)
                {
                    #region Phone
                    bytes = bin_br.ReadBytes(5);
                    if (bytes[0] != 0x04 | bytes[3] != 0x20 | bytes[4] != 0x08 )
                    {
                        isTrue = true;
                    }

                    if (isTrue)
                    {
                        if (bytes[0] == 0x04 && bytes[3] == 0x1e && bytes[4] == 0x12)
                        {
                            #region 提示信息
                            int indexOffset = (int)bin_fs.Position;
                            offset = (int)bin_br.ReadUInt32();
                            string text = readString(offset);
                            XmlElement txt = XmlTextNodeAdd(txtAdd(text), indexOffset, "MessageBox");
                            #endregion
                        }
                        else
                        {
                            bin_fs.Seek(-5, SeekOrigin.Current);
                        }
                    }
                    else
                    {
                        byte bPhone = bin_br.ReadByte();
                        if (bPhone == 1)
                        {
                            isPhone = true;
                        }
                        else
                        {
                            isPhone = false;
                        }
                    }

                    #endregion
                }
            }
            Save(txtfile, xmlfile);
        }
    }
}
