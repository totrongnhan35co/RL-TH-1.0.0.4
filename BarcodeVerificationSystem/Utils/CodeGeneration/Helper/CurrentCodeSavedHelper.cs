using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCode.Utils
{
    public class CurrentCodeSavedHelper
    {
        public static int LoadCurrentCounter(string filePath)
        {
            if (!File.Exists(filePath)) return 100;
            return int.Parse(File.ReadAllText(filePath));
        }

        public static void SaveCurrentCounter(string filePath, int c)
        {
            File.WriteAllText(filePath, c.ToString());
        }

    }
}
