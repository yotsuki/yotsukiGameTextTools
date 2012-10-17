using System;
using System.Collections.Generic;
using System.Text;
using yotsuki.KIDSFansChannel.Common.BaseClass;
using System.IO;

namespace yotsuki.free.Script.AreYouAlice
{
    public class AYAScriptHead
    {
        public uint Offset { get; set; }
        public uint Zero { get; set; }
        public uint Type { get; set; }
        public uint Unknow { get; set; }
        public uint Length { get; set; }
    }

    public class AreYouAliceReader : ScriptReader
    {
        public override string[] GetFiles(string path)
        {
            var files = Directory.GetFiles(path);
            var list = new List<string>();
            foreach (var file in files) {
                var info = new FileInfo(file);
                
                if (string.IsNullOrEmpty(info.Extension))
                    list.Add(file);
            }
            return list.ToArray();
        }

        public override string GetTxtFileName(string binfile)
        {
            return binfile + ".txt";
        }

        public override string GetXmlFileName(string binfile)
        {
            return binfile + ".xml";
        }

        public override void Reader(string binfile, string txtfile, string xmlfile)
        {
            Open(binfile);
            enc = Encoding.GetEncoding("shift-jis");//设置bin文件编码

            XmlCreate("AreYouAlice", txtfile);
            bin_br.BaseStream.Seek(0x20, SeekOrigin.Begin);
            int exprot_data_offset = (int)bin_br.ReadUInt32() - 0xc;
            bin_br.BaseStream.Seek(0x50, SeekOrigin.Begin);
            var str = readString(0x50);
            uint tmp = 0;
            if (str != "GLOBAL_DATA") {
                throw new Exception("文件" + binfile + "没有找到GLOBAL_DATA");
            }
            var pos = 0x60;
            while (true) {
                str = readString((int)pos);
                if (str == "CODE_START_") {
                    break;
                }
                pos += 0x10;
                if (pos >= exprot_data_offset) {
                    throw new Exception("文件" + binfile + "没有找到CODE_START_");
                }
                bin_br.BaseStream.Seek(pos, SeekOrigin.Begin);

            }

            bin_br.BaseStream.Seek(pos + 0xc, SeekOrigin.Begin);

            uint length = 0;
            var text = "";
            
            while(bin_br.BaseStream.Position < exprot_data_offset)
            {
                var model = ReadAYAScriptHead();

                if (model == null || model.Length < 0x20)
                    continue;
                switch (model.Type) {
                    case 0xd2:
                        bin_br.BaseStream.Seek(0x4 * 6, SeekOrigin.Current);
                        length = bin_br.ReadUInt32();
                        text = readString((int)bin_br.BaseStream.Position);
                        XmlTextNodeAdd(txtAdd(text), (int)model.Offset + 0x10, model.Type.ToString("x"), (int)length);
                        bin_br.BaseStream.Seek(model.Offset + model.Length, SeekOrigin.Begin);
                        break;
                    case 0xd8:
                        bin_br.BaseStream.Seek(0x4 * 9, SeekOrigin.Current);
                        
                        length = bin_br.ReadUInt32();
                        text = readString((int)bin_br.BaseStream.Position);
                        XmlTextNodeAdd(txtAdd(text), (int)model.Offset + 0x10, model.Type.ToString("x"), (int)length);
                        bin_br.BaseStream.Seek(model.Offset + model.Length, SeekOrigin.Begin);
                        break;
                }
            }

            //byte b = 0;
            //byte[] bytes = null;
            //int offset = 0;
            //bool isTrue = false;
            //while (bin_fs.Position < bin_fs.Length) {
            //    isTrue = false;
            //    b = bin_br.ReadByte();
            //    if (b == dia) {
            //        #region 对话处理
            //        bytes = bin_br.ReadBytes(3);
            //        if (bytes[0] != 0x00 | bytes[1] != 0x00 | bytes[2] != 0x00) {
            //            isTrue = true;
            //        }
            //        if (isTrue) {
            //            bin_fs.Seek(-3, SeekOrigin.Current);
            //            continue;
            //        }
            //        int indexOffset = (int)bin_fs.Position;
            //        offset = (int)bin_br.ReadUInt32();
            //        if (offset < bin_fs.Length & offset > start) {
            //            string text = readString(offset);
            //            //count++;
            //        }
            //        #endregion
            //    }
            //}
            Save(txtfile, xmlfile);
        }

        public AYAScriptHead ReadAYAScriptHead()
        {
            var pos = bin_br.BaseStream.Position;
            var model = new AYAScriptHead() {
                Offset = (uint)pos,
                Zero = bin_br.ReadUInt32(),
                Type = bin_br.ReadUInt32(),
                Unknow = bin_br.ReadUInt32(),
                Length = bin_br.ReadUInt32()
            };

            if (model.Zero != 0) {
                bin_br.BaseStream.Seek(pos + 4, SeekOrigin.Begin);
                return null;
            }
            return model;
        }
        
    }
}
