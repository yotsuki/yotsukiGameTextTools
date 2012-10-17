using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace yotsuki.KIDSFansChannel.Common.BaseClass
{
    public class EncodingTable
    {
        public static Dictionary<char,byte[]> ReadEncodingTable(string file)
        {
            FileStream fs = null;
            StreamReader sr = null;
            Dictionary<char,byte[]> dic = new Dictionary<char,byte[]>();
            string line = "";
            string[] str;
            try
            {
                fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(fs, Encoding.Unicode);
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    str = line.Split(new char[]{'='},StringSplitOptions.RemoveEmptyEntries);
                    if (line.IndexOf("==") != -1 && str.Length == 1)
                    {
                        str = new string[] { str[0], "=" };
                    }

                    if(!dic.ContainsKey(str[1][0]))
                        dic.Add(str[1][0], Hex2Dec(str[0]));
                }

                return dic;
            }
            catch (Exception)
            {
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

        static byte[] Hex2Dec(string str)
        {
            byte[] bytes = new byte[2];
            char[] chars = str.ToUpper().ToCharArray();
            int dec = 0;
            for (int i = chars.Length - 1; i >= 0; i--)
            {
                if (chars[i] >= '0' & chars[i] <= '9')
                {
                    dec += (chars[i] - '0') << (chars.Length - i - 1) * 4;
                }
                else if(chars[i] >= 'A' & chars[i] <= 'F')
                {
                    dec += (chars[i] - 'A' + 10) << (chars.Length - i - 1) * 4;
                }
            }
            bytes[0] = (byte)((dec & 0xff00) >> 8);
            bytes[1] = (byte)(dec & 0xff);
            return bytes;
        }
    }
}
