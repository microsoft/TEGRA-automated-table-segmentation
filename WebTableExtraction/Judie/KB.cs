using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WebTableExtraction.Utils;

namespace WebTableExtraction.Judie
{
    class KB
    {

        private static Dictionary<string, HashSet<string>> attri2Values = new Dictionary<string, HashSet<string>>();
        private static Dictionary<string, HashSet<string>> value2Attris = new Dictionary<string, HashSet<string>>();

       static KB()
        {

            string[] allLines = File.ReadAllLines(Parameter.kb_value2attrisFilePath);
            foreach(string line in allLines)
            {
                string[] temp = line.Split('\t');
                string value = null;
                HashSet<string> attris = new HashSet<string>();
                for(int i = 0; i < temp.Length; i++)
                {
                    if(i == 0)
                    {
                        value = temp[i];
                    }
                    else
                    {
                        if(temp[i] != null && temp[i] != "")
                        {
                            attris.Add(temp[i]);
                         
                        }
                    }
                }
                value2Attris[value] = attris;
               
            }

            Console.WriteLine("finished loading value 2 attris");
            string[] allLines2 = File.ReadAllLines(Parameter.kb_attri2valuesFilePath);
            foreach(string line in allLines2)
            {
                string[] temp = line.Split('\t');
                string attri = null;
                HashSet<string> values = new HashSet<string>();
                for (int i = 0; i < temp.Length; i++)
                {
                    if (i == 0)
                    {
                        attri = temp[i];
                    }
                    else
                    {
                        if (temp[i] != null && temp[i] != "")
                        {
                            values.Add(temp[i]);
                            
                        }
                    }
                }
                attri2Values[attri] = values;
            }

            Console.WriteLine("finished loading attri 2 values");



            Console.WriteLine("Done loading the kb to memory");
        }

        public static void load()
        {

        }
        public static HashSet<string> getValues(string attri)
        {
            if(attri == null)
                return new HashSet<string>();
            if (attri2Values.ContainsKey(attri))
                return attri2Values[attri];
            else
                return new HashSet<string>();
        }

        public static HashSet<string> getAttris(string value)
        {
            if(value == null)
                return new HashSet<string>();
            if(value2Attris.ContainsKey(value))
            {
                return value2Attris[value];
            }
            else
            {
                return new HashSet<string>();
            }
        }
    }
}
