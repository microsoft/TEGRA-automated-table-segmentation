using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;


namespace WebTableExtraction.Experiments
{
    class FilterLists
    {

        public static void filter(string listDumpFile, string outputDir)
        {
            

            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(outputDir);
            if (!directory.Exists)
                Directory.CreateDirectory(outputDir);

            Empty(directory);

            int i = 0;
            foreach (string line in System.IO.File.ReadLines(listDumpFile))
            {
                System.Console.WriteLine(line);


                string url = line.Split('\t')[0];
                string content = line.Split('\t')[1];


                List<string> lis = getLIs(content);
                if (lis.Count == 0)
                    continue;

                if (!good_list(lis))
                    continue;



                i++;

                string file = outputDir + @"list" + i + ".txt";


                StreamWriter sw = new StreamWriter(file);
                //sw.WriteLine(url);
                foreach (string item in lis)
                {
                    sw.WriteLine(item);
                }
                sw.Close();


            }

        }


        public static void Empty(System.IO.DirectoryInfo directory)
        {
            if (!directory.Exists)
                return;
            foreach (System.IO.FileInfo file in directory.GetFiles())
            {
                if (file.FullName.Contains("list"))
                    file.Delete();

            }
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        private static bool good_list(List<string> lis)
        {

            foreach (string item in lis)
            {
                if (item.Contains("About Wikipedia")
                    || item.Contains("Donate to Wikipedia")
                    || item.Contains("What links here")
                    || item.Contains("Site Map")
                    || item.Contains("Terms")
                    )
                    return false;
            }


            if (lis.Count < 5 || lis.Count > 50)
                return false;


            //(2) Non English chars
            foreach (string item in lis)
            {
                int numLetters = 0;
                foreach (char c in item)
                {
                    numLetters++;
                    if (c >= 128)
                    {

                        return false;
                    }
                    if (c == '#')
                        return false;

                }
                if (numLetters > 100)
                    return false;
            }




            int numShorts = 0;
            foreach (string item in lis)
            {
                string[] words = item.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length < 3)
                    numShorts++;
            }
            if (numShorts > lis.Count * 0.5)
                return false;




            return true;
        }
        private static string[] seperators = new string[]{
            @" ",
            @",",
            @"-",
            @"(",
            @")",
            @"/",
            @"\",
            @";",
            @":",
            @"&",
        };

        private static List<string> getLIs(string content)
        {
            List<string> result = new List<string>();

            string tempContent = content;


            int open_index = content.IndexOf(@"<li", 0);
            int close_index = content.IndexOf(@"/li>", 0);
            if (close_index < open_index)
                return result;

            while (open_index > 0 && close_index > 0)
            {
                string substring = tempContent.Substring(open_index, close_index - open_index + 4);
                string pat = @">([^<>]+)<";

                string tempDup = substring;
                Match match = Regex.Match(tempDup, pat,
                RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string key = match.Groups[1].Value;
                    Console.WriteLine(key);




                    result.Add(key);
                }



                tempContent = tempContent.Substring(close_index + 4, tempContent.Length - close_index - 4);
                open_index = tempContent.IndexOf(@"<li", 0);
                close_index = tempContent.IndexOf(@"/li>", 0);

                if (close_index < open_index)
                {
                    return result;
                }
            }



            return result;

        }
    }
}
