using System;
using System.Collections.Generic;
using System.Text;
using yotsuki.KIDSFansChannel.Common.BaseClass;
using System.IO;

namespace yotsuki.free.Script.AreYouAlice
{
    public class AreYouAliceWriter : ScriptWriter
    {
        protected override void WriteBinfile(Dictionary<char, byte[]> encodingtable, System.IO.FileInfo inf, string outfile, int startoffset, int endoffset, IList<yotsuki.KIDSFansChannel.Common.BaseClass.ScriptTxt> txtList)
        {
            #region 写入bin文件
            FileStream fs = null;
            BinaryWriter bw = null;
            int offset = startoffset;
            outfile = outfile.Replace(".bin", "");
            var infile =inf.FullName.Replace(".xml","");
            try {
                (new FileInfo(infile)).CopyTo(outfile, true);
                fs = new FileStream(outfile, FileMode.Open, FileAccess.ReadWrite);
                bw = new BinaryWriter(fs);
                var br = new BinaryReader(fs);
                foreach (ScriptTxt st in txtList) {
                    byte[] bytes = Encode(st.Text.Replace(" ",""), encodingtable);
                    fs.Seek(st.IndexOffset, SeekOrigin.Begin);
                    var txtoffset = br.ReadUInt32();
                    fs.Seek(txtoffset+0x10, SeekOrigin.Begin);
                    if (st.MaxLength < bytes.Length) {
                        bw.Write(bytes, 0, st.MaxLength);
                        //throw new Exception();
                    }
                    else {
                        bw.Write(bytes, 0, bytes.Length);
                        bw.Write(new byte[st.MaxLength - bytes.Length], 0, st.MaxLength - bytes.Length);
                    }
                    //bw.Write((short)0);
                }
            }
            catch (Exception e) {

                //this.WriteLog(e.Message, null);
            }
            finally {
                if (bw != null)
                    bw.Close();
                if (fs != null)
                    fs.Close();
            }
            #endregion
        }
    }
}
