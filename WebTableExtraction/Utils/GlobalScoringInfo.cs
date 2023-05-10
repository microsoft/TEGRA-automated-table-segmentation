using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using System.Globalization;
namespace WebTableExtraction.Utils
{
    class GlobalScoringInfo
    {
        private static Dictionary<string, double> singleStringOccurrence;
        private static Dictionary<string, double> doubleStringOccurrence;
        public  static double totalStringOccurrence = 1082164240;

        private static Dictionary<string, double> singleStringLanguageModel;


        private static Regex[] regexes = new Regex[] {
            //email
            new Regex( @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"),    
            
            //url
            //new Regex(@"^(?:(?:https?|ftp)://)(?:\S+(?::\S*)?@)?(?:(?!10(?:\.\d{1,3}){3})(?!127(?:\.\d{1,3}){3})(?!169\.254(?:\.\d{1,3}){2})(?!192\.168(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:/[^\s]*)?$", RegexOptions.IgnoreCase | RegexOptions.Multiline),

            new Regex(@"/^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/"),

            //currency

            //new Regex(@"^\$\-?([1-9]{1}[0-9]{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$|^\-?\$?([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$|^\(\$?([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))\)$"),

            // phone number
            //new Regex(@"/^(?:(?:\(?(?:00|\+)([1-4]\d\d|[1-9]\d?)\)?)?[\-\.\ \\\/]?)?((?:\(?\d{1,}\)?[\-\.\ \\\/]?){0,})(?:[\-\.\ \\\/]?(?:#|ext\.?|extension|x)[\-\.\ \\\/]?(\d+))?$/i"),

        };
        public static void load()
        {

        }

        static GlobalScoringInfo()
        {

            if(Parameter.global_scoring_options == 0)
            {
                totalStringOccurrence = 1082164240;
            }
            else if(Parameter.global_scoring_options !=0)
            {
                totalStringOccurrence = 222404360;

                if(Parameter.whichStats == 1)
                    totalStringOccurrence = 222404360;
                else if(Parameter.whichStats == 2)
                    totalStringOccurrence = 1176193;
                else if(Parameter.whichStats == 3)
                    totalStringOccurrence = 222404360 + 1176193;
                else if (Parameter.whichStats == 4)
                    totalStringOccurrence = 222404360 + 222404360;
            }

            //Load 

            Console.WriteLine("Start to load string occurence statistics");
            string[] temps1 = System.IO.File.ReadAllLines(Parameter.singleCellOccurrenceFilePath);
            singleStringOccurrence = new Dictionary<string, double>(temps1.Count());

            foreach (string temp in temps1)
            {

                string[] split = temp.Split('\t');
                if (split.Count() != 2)
                {
                    Console.WriteLine("we have a problem in single");
                }
                singleStringOccurrence[split[0]] = Convert.ToDouble(split[split.Count() - 1]);
                if (split[0]  == @"China")
                {

                }
            }
            Console.WriteLine("Done loading single string occurence statistics");


            foreach(string doubleCellOccurrence_one in Parameter.doubleCellOccurrenceFilepath.Split('\t'))
            {
                string[] temps2 = System.IO.File.ReadAllLines(doubleCellOccurrence_one);
                Console.WriteLine("Done reading the file: Total: " + temps2.Count());
                doubleStringOccurrence = new Dictionary<string, double>(temps2.Count());

                int i = 0;
                foreach (string temp in temps2)
                {
                    if (i++ % 100000 == 0)
                        Console.WriteLine(@"finished loading: " + i);
                    string[] split = temp.Split('\t');
                    if (split.Count() != 3)
                    {
                        Console.WriteLine("we have a problem in cooccur");
                    }

                    doubleStringOccurrence[split[0] + "\t" + split[1]] = Convert.ToDouble(split[split.Count() - 1]);


                }
            }

            Console.WriteLine("Done loading double string occurence statistics");



            Console.WriteLine("Start to load string language model");
            string[] temps3 = System.IO.File.ReadAllLines(Parameter.singleCellLanguageModelFilePath);
            singleStringLanguageModel = new Dictionary<string, double>(temps3.Count());

            foreach (string temp in temps3)
            {

                string[] split = temp.Split('\t');
                if (split.Count() != 2)
                {
                    Console.WriteLine("we have a problem in language model");
                }
                singleStringLanguageModel[split[0]] = Convert.ToDouble(split[split.Count() - 1]);
            }
            Console.WriteLine("Done loading single string language model");

        }
     
        
        
        public static void subtractTableStats(Table table)
        {
           if (Parameter.exp_num != 2 && Parameter.exp_num != 3 && Parameter.exp_num != 10)
                return;
           
           
           totalStringOccurrence = totalStringOccurrence - table.getNumCols();
           for (int k = 0; k < table.getNumCols(); k++)
           {
               HashSet<string> done = new HashSet<string>();
               for (int i = 0; i < table.getNumRecords(); i++)
               {
                   string s1 = table.getCell(i, k).getCellValue();
                   if (done.Contains(s1))
                       continue;
                   if (singleStringOccurrence.ContainsKey(s1))
                   {
                       singleStringOccurrence[s1]--;
                       if (singleStringOccurrence[s1] < 0)
                           singleStringOccurrence[s1] = 0;
                   }
                       

                   done.Add(s1);
               }
           }
           for (int k = 0; k < table.getNumCols(); k++)
           {
               HashSet<string> done = new HashSet<string>();
               for (int i = 0; i < table.getNumRecords(); i++)
               {
                   for (int j = i; j < table.getNumRecords(); j++)
                   {
                           string s1 = table.getCell(i, k).getCellValue();
                           string s2 = table.getCell(j, k).getCellValue();
                           

                           if (done.Contains(s1 + "\t" + s2) || done.Contains(s2 + "\t" + s1))
                               continue;

                           if (doubleStringOccurrence.ContainsKey(s1 + "\t" + s2))
                           {
                               doubleStringOccurrence[s1 + "\t" + s2]--;
                               if (doubleStringOccurrence[s1 + "\t" + s2] < 0)
                               {
                                   doubleStringOccurrence[s1 + "\t" + s2] = 0;
                               }
                           }
                               
                           else if (doubleStringOccurrence.ContainsKey(s2 + "\t" + s1))
                           {
                               doubleStringOccurrence[s2 + "\t" + s1]--;
                               if (doubleStringOccurrence[s2 + "\t" + s1] < 0)
                               {
                                   doubleStringOccurrence[s2 + "\t" + s1] = 0;
                               }
                           }
                              

                           done.Add(s1 + "\t" + s2);
                           done.Add(s2 + "\t" + s1);
                   }
               }
           }
           
         
        }
        public static void addBackTableStats(Table table)
        {
            if (Parameter.exp_num != 2 && Parameter.exp_num != 3 && Parameter.exp_num != 10)
                return;
            for (int k = 0; k < table.getNumCols(); k++)
            {
                HashSet<string> done = new HashSet<string>();
                for (int i = 0; i < table.getNumRecords(); i++)
                {
                    string s1 = table.getCell(i, k).getCellValue();
                    if (done.Contains(s1))
                        continue;
                    if (singleStringOccurrence.ContainsKey(s1))
                        singleStringOccurrence[s1]++;

                    done.Add(s1);
                }
            }
            for (int k = 0; k < table.getNumCols(); k++)
            {
                HashSet<string> done = new HashSet<string>();
                for (int i = 0; i < table.getNumRecords(); i++)
                {
                    for (int j = i; j < table.getNumRecords(); j++)
                    {
                        string s1 = table.getCell(i, k).getCellValue();
                        string s2 = table.getCell(j, k).getCellValue();

                        if (done.Contains(s1 + "\t" + s2) || done.Contains(s2 + "\t" + s1))
                            continue;

                        if (doubleStringOccurrence.ContainsKey(s1 + "\t" + s2))
                            doubleStringOccurrence[s1 + "\t" + s2]++;
                        else if (doubleStringOccurrence.ContainsKey(s2 + "\t" + s1))
                            doubleStringOccurrence[s2 + "\t" + s1]++;

                        done.Add(s1 + "\t" + s2);
                        done.Add(s2 + "\t" + s1);
                    }
                }
            }
        }
        
        
        
        
        public static double getOccurCount(string s1)
        {
            System.Diagnostics.Debug.Assert(s1 != null);
            double occurCount = 0;
            if (singleStringOccurrence.ContainsKey(s1))
                occurCount = singleStringOccurrence[s1];

            return occurCount;
        }

        public static double getCooccurCount(string s1, string s2)
        {
            System.Diagnostics.Debug.Assert(s1 != null && s2 != null);

            
            if (Parameter.global_scoring_options == 0 && s1 == s2 )
                return getOccurCount(s1);

            if (doubleStringOccurrence.ContainsKey(s1 + "\t" + s2))
                return doubleStringOccurrence[s1 + "\t" + s2];
            else if (doubleStringOccurrence.ContainsKey(s2 + "\t" + s1))
                return doubleStringOccurrence[s2 + "\t" + s1];
            else
                return 0;
        }


       
        public static double getStringLangugeModelProb(string s)
        {
            if(singleStringLanguageModel.ContainsKey(s))
            {
                return Math.Pow(10, singleStringLanguageModel[s]);
            }
            else
            {
                return 0;
            }
        }
       
        
        /*
         * returns a probability in the range of [0,1]
         */ 
        public static double getStringTableProOverDocuProb(string s)
        {
           


            if (singleStringOccurrence.ContainsKey(s))
            {
                double document_prob = Math.Pow(10, singleStringLanguageModel[s]);
                double table_prob = singleStringOccurrence[s] / totalStringOccurrence;

                double unnormalized_table_over_doc = table_prob / document_prob;

                return unnormalized_table_over_doc;
            }
            else
            {
                return 0;
            }
          
        }

        public static int getType(string s1)
        {
            System.Diagnostics.Debug.Assert(regexes.Length <= 10);
            for (int i = 1; i <= regexes.Length; i++)
            {
                Regex regex = regexes[i - 1];
                if (regex.IsMatch(s1))
                    return i;
            }

            //date
            //DateTime dt1;
            //if ( DateTime.TryParse(s1, out dt1))
               // return 11;

            //numerical values
            double d1;
            if (Double.TryParse(s1, out d1))
            {
                return 13;
            }

            if (s1.StartsWith(@"$") && Double.TryParse(s1.Substring(1), out d1))
            {
                return 14;
            }


            return 0;
        }




       

    }
}
