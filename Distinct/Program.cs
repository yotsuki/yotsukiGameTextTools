using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Distinct
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args[0];
            var list = new List<string>();
            using (var fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite)) {
                var sr = new StreamReader(fs);
                while(!sr.EndOfStream)
                {
                    var line =sr.ReadLine();
                    if(!list.Contains(line))
                        list.Add(line);
                }
            }
            using (var fs = File.CreateText(file))
            {
                foreach(var line in list)
                {
                    fs.WriteLine(line);
                }
            }
        }
    }
}
