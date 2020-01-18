using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOPS_2
{
    public static class Vars
    {
        /// <summary>
        /// Список полных имен файлов
        /// </summary>
        public static List<string> files = new List<string>();

        public static string GetFileName(string file)
        {
            string[] tmp = file.Split('\\');
            return tmp[tmp.Length - 1];
        }
    }
}
