using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ScopeRuntime;

using System.Text.RegularExpressions;
using System.IO;

public class HighParallelProcessor : Processor
{
    public override void GetProperties(RowSetProperties.RSPCategory category, List<RowSetProperties> inputRowsetProps, RowSetProperties outputRowsetProp)
    {
        // sample user code for overriding the function
        switch (category)
        {
            case RowSetProperties.RSPCategory.Partition:
                {
                    // Parallelism related properties
                    outputRowsetProp.PartitionCount = 250;
                }
                break;

            default:
                break;
        }

    }



    public override Schema Produces(string[] requested_columns, string[] args, Schema input_schema)
    {

        var output_schema = new Schema();


        output_schema.Add(input_schema[0]);


        var newcol = new ColumnInfo("list", typeof(string));
        output_schema.Add(newcol);

        return output_schema;

    }


    public override IEnumerable<Row> Process(RowSet input_rowset, Row output_row, string[] args)
    {

        foreach (Row input_row in input_rowset.Rows)
        {
            string url = input_row[0].String;
            string content = input_row[1].String;

            string list = MyListHandler.extractList(content);

            if (list == null)
                continue;

            //input_row.CopyTo(output_row);
            output_row[0].Set(url);
            output_row[1].Set(list);

            yield return output_row;

        }

    }


    public static class MyListHandler
    {
        private static string myLineSep = @"#Xu#Xu#Xu#";
        public static string extractList(string content)
        {
            
            List<string> lis = getLIs(content);

            if(!good_list(lis))
                return null;


            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < lis.Count; i++)
            {
                if(i != lis.Count - 1)
                {
                    sb.Append(lis[i]);
                    sb.Append(myLineSep);
                }
                else
                {
                    sb.Append(lis[i]);
                }
            }
            return sb.ToString();

        }
         private static bool good_list(List<string> lis)
        {

            foreach (string item in lis)
            {
                if (item.Contains("About Wikipedia")
                    || item.Contains("Donate to Wikipedia")
                    || item.Contains("What links here")
                    )
                    return false;
            }


            if (lis.Count < 3)
                return false;


            //(2) Non English chars
            foreach (string item in lis)
            {
                foreach (char c in item)
                {
                    if (c >= 256)
                    {

                        return false;
                    }

                }
            }
          



            int numShorts = 0;
            foreach (string item in lis)
            {
                string[] words = item.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length < 2)
                    numShorts++;
            }
            if (numShorts > lis.Count * 0.9)
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