using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using WebTableExtraction.Utils;
using System.Configuration;
namespace WebTableExtraction.Experiments
{
    class DataPrep
    {

        public static void prepre_kb()
        {

          


            List<string> allSequenceFiles = FileUtil.getAllFiles(Parameter.inputSequence_directory);
            Console.WriteLine("The number of files:" + allSequenceFiles.Count());
            
            //collect all required values
            HashSet<string> singleCellsSet = new HashSet<string>();
            foreach (string sequenceFile in allSequenceFiles)
            {
                string[] temps = System.IO.File.ReadAllLines(sequenceFile);
                foreach (string temp in temps)
                {
                    Line line = new Line(temp);
                    for (int i = 0; i < line.getNumWords(); i++)
                        for (int j = i; j < line.getNumWords(); j++)
                        {
                            if (j - i + 1 > Parameter.maxTokensPerColumn)
                                continue;
                            string ha = line.getCellValue(i, j);
                            singleCellsSet.Add(ha);
                        }
                }
                
            }

            
            Dictionary<string, HashSet<string>> value2Attris = new Dictionary<string, HashSet<string>>();
            List<string> kb_files = FileUtil.getAllFiles(Parameter.kb_directory);
            foreach (string kb_file in kb_files)
            {
                string[] alllines = System.IO.File.ReadAllLines(kb_file);
                bool firstLine = true;
                foreach (string line in alllines)
                {
                    if (firstLine)
                    {
                        firstLine = false;
                        continue;
                    }
                    string[] terms = line.Split('\t');
                    if (terms.Length == 0 || terms[0] == null || terms[0] == "")
                    {
                        continue;
                    }
                    else
                    {
                        string value = terms[0];
                        string attri = kb_file; // Path.GetFileName(kb_file);


                        if(!singleCellsSet.Contains(value))
                        {
                            continue;
                        }
                        if (value2Attris.ContainsKey(value))
                        {
                            value2Attris[value].Add(attri);
                        }
                        else
                        {
                            HashSet<string> attris = new HashSet<string>();
                            attris.Add(attri);
                            value2Attris[value] = attris;

                        }
                    }
                }
            }

            //materiaze these two to fiels
            StreamWriter sw_value_2_attris = new StreamWriter(Parameter.kb_value2attrisFilePath);
           
            HashSet<string> allAttris = new HashSet<string>();
            foreach(string value in value2Attris.Keys)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(value);
                sb.Append('\t');

                foreach(string attri in value2Attris[value])
                {
                    sb.Append(attri);
                    allAttris.Add(attri);
                    sb.Append('\t');
                }

                sw_value_2_attris.WriteLine(sb.ToString());
            }
            sw_value_2_attris.Close();

            Console.WriteLine("done outputting value 2 attri for this dataset" + Parameter.inputSequence_directory);

            Dictionary<string, HashSet<string>> attri2Values = new Dictionary<string, HashSet<string>>();
            foreach(string attri in allAttris)
            {
                HashSet<string> representative_values = new HashSet<string>();

                string[] alllines = System.IO.File.ReadAllLines(attri);
                bool firstLine = true;
                foreach (string line in alllines)
                {
                    if (firstLine)
                    {
                        firstLine = false;
                        continue;
                    }
                    string[] terms = line.Split('\t');
                    if (terms.Length == 0 || terms[0] == null || terms[0] == "")
                    {
                        continue;
                    }
                    else
                    {
                        string value = terms[0];

                        if (value != null && representative_values.Count() < Parameter.max_n_reps)
                        {
                            representative_values.Add(value);
                        }

                    }
                    if (representative_values.Count() == Parameter.max_n_reps)
                        break;
                }

                attri2Values[attri] = representative_values;
            }
            StreamWriter sw_attri_2_values = new StreamWriter(Parameter.kb_attri2valuesFilePath);
            foreach (string attri in attri2Values.Keys)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(attri);
                sb.Append('\t');

                foreach (string value in attri2Values[attri])
                {
                    sb.Append(value);
                    sb.Append('\t');
                }

                sw_attri_2_values.WriteLine(sb.ToString());
            }
            sw_attri_2_values.Close();
            Console.WriteLine("done outputting attri 2 values for this dataset" + Parameter.inputSequence_directory);
        }
        /*
        public static void generateDocumentLanguage()
        {

            StreamWriter sw = new StreamWriter(Parameter.singleCellLanguageModelFilePath);

            HashSet<string> allSingleCells = new HashSet<string>();
            foreach (string s in File.ReadAllLines(Parameter.singleCellOccurrenceFilePath))
            {
                allSingleCells.Add(s.Split('\t')[0]);
            }
            string userToken = ConfigurationManager.AppSettings.Get("userToken");
            string ngramModel = ConfigurationManager.AppSettings.Get("ngramModel");

            string[] allSingleCellsArray = allSingleCells.ToArray();
            NGramService.LookupServiceClient client = new NGramService.LookupServiceClient();

            try
            {

                for (int i = 0; i < allSingleCellsArray.Length; i++)
                {
                    string s = allSingleCellsArray[i] + "\t" + client.GetProbability(userToken, ngramModel, allSingleCellsArray[i]);
                    Console.WriteLine(s);
                    sw.WriteLine(s);
                }

            }
            finally
            {
                sw.Close();
                client.Close();
            }

        }
         * */
        public static void requiredStats()
        {
            string singleCell_occur_file = @"F:\WebLists\Stats2\weblists_single.txt";
            string doubleCell_occur_file = @"F:\WebLists\Stats2\weblists_double.txt";
            if (!Directory.Exists(Path.GetDirectoryName(singleCell_occur_file)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(singleCell_occur_file));
            }
            if (!Directory.Exists(Path.GetDirectoryName(doubleCell_occur_file)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(doubleCell_occur_file));
            }
            StreamWriter swSingleCell = new StreamWriter(singleCell_occur_file);
            StreamWriter swDoubleCell = new StreamWriter(doubleCell_occur_file);

            List<string> allSequenceFiles = FileUtil.getAllFiles(Parameter.inputSequence_directory);
            Console.WriteLine("The number of files:" + allSequenceFiles.Count());
            foreach (string sequenceFile in allSequenceFiles)
            {
                Console.WriteLine("we start to process: " + sequenceFile);

                if(sequenceFile.Contains("9999"))
                {

                }
                HashSet<string> singleCellsSet = new HashSet<string>();

                string[] temps = System.IO.File.ReadAllLines(sequenceFile);
                foreach (string temp in temps)
                {
                    Line line = new Line(temp);
                    for (int i = 0; i < line.getNumWords(); i++)
                        for (int j = i; j < line.getNumWords(); j++)
                        {
                            if (j - i + 1 > Parameter.maxTokensPerColumn)
                                continue;
                            string ha = line.getCellValue(i, j);
                            singleCellsSet.Add(ha);
                        }
                }
                List<string> singleCells = new List<string>(singleCellsSet);
                Console.WriteLine("before sorting");
                singleCells.Sort();
                Console.WriteLine("finished sorting: number of single values: " + singleCells.Count());
                for (int i = 0; i < singleCells.Count(); i++)
                {
                    string cell1 = singleCells.ElementAt(i);
                    swSingleCell.WriteLine(cell1);
                    for (int j = i; j < singleCells.Count(); j++)
                    {
                        string cell2 = singleCells.ElementAt(j);

                        swDoubleCell.WriteLine(cell1 + "\t" + cell2);
                    }
                }
                Console.WriteLine("we have finished file: " + sequenceFile);

            }
            swSingleCell.Close();
            swDoubleCell.Close();
            Console.WriteLine("we have finished all files");

        }




        public static void requiredStats_add_empty_strings()
        {
            string[] singleCells = File.ReadAllLines(@"D:\xuchu\TableStats\combined for xu\singleCell_occur_combined.txt");
            //string[] doubleCells = File.ReadAllLines(@"D:\xuchu\TableStats\combined for xu\doubleCell_occur_combined.txt");

            StreamWriter sw_1 = new StreamWriter(@"D:\xuchu\TableStats\combined for xu\singleCell_occur_combined_add_nulls.txt");
            StreamWriter sw_2 = new StreamWriter(@"D:\xuchu\TableStats\combined for xu\doubleCell_occur_combined_add_nulls.txt");


            foreach (string temp in singleCells)
            {
                sw_1.WriteLine(temp);
            }
            sw_1.WriteLine("");
            sw_1.Close();


            foreach (string temp in singleCells)
            {
                sw_2.WriteLine("" + "\t" + temp);
            }
            
            sw_2.Close();

        }


        /*
         *The following methods are for preparing data files 
         */


        public static void read_WebTables( int numTables)
        {
            string path = @"D:\xuchu\TableStats\WebTableFileParsed_10perc.txt";
            string tableOutputDir = Parameter.inputTabular_directory;
            string sequenceOutputDir = Parameter.inputSequence_directory;
            
            if (Directory.Exists(tableOutputDir))
            {
                Directory.Delete(tableOutputDir,true);
            }
            if (Directory.Exists(sequenceOutputDir))
            {
                Directory.Delete(sequenceOutputDir,true);
            }
            Directory.CreateDirectory(tableOutputDir);
            Directory.CreateDirectory(sequenceOutputDir);
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine(path + " does not exist!!");
                }
                int j = 0;
                int i = 0;
                using (StreamReader sr = new StreamReader(path))
                {
                    while (sr.Peek() >= 0)
                    {
                        string tableLine = sr.ReadLine();
                        if(path.Contains(@"10"))
                            tableLine = tableLine.Split('\t')[1];
                        
                        
                        string[] records = tableLine.Split(new string[] { "___" }, StringSplitOptions.None);
                        List<Record> records1 = new List<Record>();
                        foreach (string record in records)
                        {
                            Record newRecord = new Record(record);
                            records1.Add(newRecord);
                        }
                        Table table = new Table(records1);
                        Console.WriteLine("Examining file" + (++j));
                        if (TableFiltering.tableCleanUp(table) && TableFiltering.runTime_good_table(table))
                        {
                            i++;
                            string tableOutputFilePath = tableOutputDir + "table_" + i + ".txt";
                            string sequenceOutputFilePath = sequenceOutputDir + "table_" + i + ".txt";
                            table.printTableToFile(tableOutputFilePath);
                            table.printTableConcatenatedToFile(sequenceOutputFilePath);
                            if (i >= numTables)
                                break;
                        }
                       
                       
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

        }
        public static void read_WikiTables(int numTables)
        {
            string path = @"D:\xuchu\TableStats\WebTableFileParsed_10perc_filtered_wikipedia.txt";
            string tableOutputDir = Parameter.inputTabular_directory;
            string sequenceOutputDir = Parameter.inputSequence_directory;

            if (Directory.Exists(tableOutputDir))
            {
                Directory.Delete(tableOutputDir, true);
            }
            if (Directory.Exists(sequenceOutputDir))
            {
                Directory.Delete(sequenceOutputDir, true);
            }
            Directory.CreateDirectory(tableOutputDir);
            Directory.CreateDirectory(sequenceOutputDir);
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine(path + " does not exist!!");
                }
                int j = 0;
                int i = 0;
                using (StreamReader sr = new StreamReader(path))
                {
                    while (sr.Peek() >= 0)
                    {
                        string tableLine = sr.ReadLine();
                        if (path.Contains(@"10"))
                            tableLine = tableLine.Split('\t')[1];


                        string[] records = tableLine.Split(new string[] { "___" }, StringSplitOptions.None);
                        List<Record> records1 = new List<Record>();
                        foreach (string record in records)
                        {
                            Record newRecord = new Record(record);
                            records1.Add(newRecord);
                        }
                        Table table = new Table(records1);
                        Console.WriteLine("Examining file" + (++j));
                        if (TableFiltering.tableCleanUp(table) && TableFiltering.runTime_good_table(table))
                        {
                            i++;
                            string tableOutputFilePath = tableOutputDir + "table_" + i + ".txt";
                            string sequenceOutputFilePath = sequenceOutputDir + "table_" + i + ".txt";
                            table.printTableToFile(tableOutputFilePath);
                            table.printTableConcatenatedToFile(sequenceOutputFilePath);
                            if (i >= numTables)
                                break;
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

        }
       

        public static void readExcelTables( int numTables)
        {
            string dir = @"C:\Users\t-xuchu\Documents\EXPDATA\ExcelCorpus2-Tabular\";
            string tableOutputDir = @"C:\Users\t-xuchu\Documents\EXPDATA\ExcelCorpus10k_Examine\Tabular\";// Parameter.inputTabular_directory;
            string sequenceOutputDir = @"C:\Users\t-xuchu\Documents\EXPDATA\ExcelCorpus10k_Examine\Sequence\";// Parameter.inputSequence_directory;
            if (Directory.Exists(tableOutputDir))
            {
                Directory.Delete(tableOutputDir, true);
            }
            if (Directory.Exists(sequenceOutputDir))
            {
                Directory.Delete(sequenceOutputDir, true);
            }
            Directory.CreateDirectory(tableOutputDir);
            Directory.CreateDirectory(sequenceOutputDir);
            try
            {
                int i = 0;
                int j = 0;
                List<string> allFiles = FileUtil.getAllFiles(dir);

                string previous_good_file = null;

                foreach(string file in allFiles)
                {
                    string[] lines = System.IO.File.ReadAllLines(file);
                    List<Record> records = new List<Record>();
                    foreach(string line in lines)
                    {
                        records.Add(new Record(line));
                    }
                    Table table = new Table(records);
                    Console.WriteLine("Examining file : " + (++j) + "  " + file);
                    if(TableFiltering.tableCleanUp(table))
                    {
                        if (previous_good_file != null && Path.GetFileName(previous_good_file).Split(new string[] { @"-table" }, StringSplitOptions.None)[0] ==
                            Path.GetFileName(file).Split(new string[] { @"-table" }, StringSplitOptions.None)[0])
                        {
                            continue;
                        }


                        i++;
                        string tableOutputFilePath = tableOutputDir + "table_" + i + ".txt" + "_" + Path.GetFileName(file);
                        string sequenceOutputFilePath = sequenceOutputDir + "table_" + i + ".txt" + "_" + Path.GetFileName(file);
                        table.printTableToFile(tableOutputFilePath);
                        table.printTableConcatenatedToFile(sequenceOutputFilePath);

                        previous_good_file = file;

                        if (i >= numTables)
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

        }





        public static void readEnterpriseTables( int numTables)
        {
            string dir = @"D:\xuchu\EnterpriseTables\Tabular";
            string tableOutputDir = @"D:\xuchu\TableStats\EnterpriseTables_10k\Tabular\";
            string sequenceOutputDir = @"D:\xuchu\TableStats\EnterpriseTables_10k\Sequence\";
            if (Directory.Exists(tableOutputDir))
            {
                Directory.Delete(tableOutputDir, true);
            }
            if (Directory.Exists(sequenceOutputDir))
            {
                Directory.Delete(sequenceOutputDir, true);
            }
            Directory.CreateDirectory(tableOutputDir);
            Directory.CreateDirectory(sequenceOutputDir);
            try
            {
                int i = 0;
                int j = 0;
                List<string> allFiles = FileUtil.getAllFiles(dir);
                foreach (string file in allFiles)
                {
                    string[] lines = System.IO.File.ReadAllLines(file);
                    List<Record> records = new List<Record>();
                    foreach (string line in lines)
                    {
                        records.Add(new Record(line));
                    }
                    Table table = new Table(records);
                    Console.WriteLine("Examining file : " + (++j) + "  " + file);
                    if (TableFiltering.tableCleanUp(table) && TableFiltering.runTime_good_table(table))
                    {
                        i++;
                        string tableOutputFilePath = tableOutputDir + "table_" + i + ".txt";
                        string sequenceOutputFilePath = sequenceOutputDir + "table_" + i + ".txt";
                        table.printTableToFile(tableOutputFilePath);
                        table.printTableConcatenatedToFile(sequenceOutputFilePath);
                        if (i >= numTables)
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }
        
        
        
        
        
        public static void truncate_double_cell_score_to_fit_cosmos(int numFiles)
        {
            string[] lines = File.ReadAllLines(@"F:\Running Results\NewStats\doubleCell_occur_combined.txt_joined.txt");
            if (numFiles == 1)
            {
                //truncate
                StreamWriter sw = new StreamWriter(@"F:\Running Results\NewStats\doubleCell_occur_combined.txt_joined_reduced.txt");
                Console.WriteLine("Total number of lines before reducing: " + lines.Length);
                int total = 0;
                foreach (string line in lines)
                {
                    string[] split = line.Split('\t');
                    int count = Convert.ToInt32(split[2]);
                    if (count <= 100)
                        continue;
                    total++;
                    sw.WriteLine(line);

                }
                Console.WriteLine("Total number of lines after reducing: " + total);
                sw.Close();
            }
            if (numFiles == 2)
            {
                StreamWriter sw_1 = new StreamWriter(@"F:\Running Results\NewStats\doubleCell_occur_combined.txt_joined_1.txt");
                StreamWriter sw_2 = new StreamWriter(@"F:\Running Results\NewStats\doubleCell_occur_combined.txt_joined_2.txt");

                int i = 0;
                foreach (string line in lines)
                {
                    i++;
                    if (i % 2 == 0)
                    {
                        sw_1.WriteLine(line);
                    }
                    else
                    {
                        sw_2.WriteLine(line);
                    }

                }
                sw_2.Close();
                sw_1.Close();
            }
            
        }

        public static void prep_dataset_for_cosmos()
        {
            List<string> allSequenceFiles = FileUtil.getAllFiles(Parameter.inputSequence_directory);

            List<string> allTabularFiles = FileUtil.getAllFiles(Parameter.inputTabular_directory);

            int total = 0;
            int skipped = 0;
            string input_sequence_files_one_file_path = Directory.GetParent(Parameter.inputSequence_directory) + "all_sequences.txt";
            StreamWriter sw_input_sequence_files_one_file_path = new StreamWriter(input_sequence_files_one_file_path);
            foreach (string file in allSequenceFiles)
            {
                total++;
                Table groundTable = new Table(file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory));
                if (!TableFiltering.runTime_good_table(groundTable))
                {
                    skipped++;
                    continue;
                }

                if (total - skipped > 10000)
                    break;
                sw_input_sequence_files_one_file_path.WriteLine(single_line_table(file));
            }
            sw_input_sequence_files_one_file_path.Close();
            Console.WriteLine("Sequences: " + " total: " + total  + " skipped: " + skipped);

             total = 0;
             skipped = 0;
            string input_tabular_files_one_file_path = Directory.GetParent(Parameter.inputTabular_directory) + "all_tabulars.txt";
            StreamWriter sw_input_tabular_files_one_file_path = new StreamWriter(input_tabular_files_one_file_path);
            foreach (string file in allTabularFiles)
            {
                total++;
                Table groundTable = new Table(file);
                if (!TableFiltering.runTime_good_table(groundTable))
                {
                    skipped++;
                    continue;
                }
                if (total - skipped > 10000)
                    break;

                sw_input_tabular_files_one_file_path.WriteLine(single_line_table(file));
            }
            sw_input_tabular_files_one_file_path.Close();
            Console.WriteLine("tabulars: " + " total: " + total + " skipped: " + skipped);


            string[] sequences_all = File.ReadAllLines(input_sequence_files_one_file_path);
            string[] tabulars_all = File.ReadAllLines(input_tabular_files_one_file_path);
            string combined_file_path = Directory.GetParent(Parameter.inputSequence_directory) + "all_sequences_tabulars.txt";
            StreamWriter sw_combined = new StreamWriter(combined_file_path);
            for (int i = 0; i < sequences_all.Length; i++)
            {
                string xxx = sequences_all[i] + '\t' + tabulars_all[i].Split('\t')[1];
                sw_combined.WriteLine(xxx);

                if (xxx.Contains(@"Mundos Opuestos 2"))
                {
                    Console.WriteLine(xxx);
                }
                string[] test = xxx.Split('\t');
                if (test.Length != 3)
                {
                    Console.Write("error");
                }
            }
            sw_combined.Close();
            string[] aaa = File.ReadAllLines(combined_file_path);
            Console.WriteLine("combined lengh: " + aaa.Length);
        }

        private static string single_line_table(string table_file)
        {
            string[] lines = System.IO.File.ReadAllLines(table_file);
            StringBuilder sb = new StringBuilder();
            string fileName = Path.GetFileName(table_file);
            sb.Append(fileName);
            sb.Append('\t');
            for (int i = 0; i < lines.Length; i++)
            {
                sb.Append(lines[i]);
                if (i != lines.Length - 1)
                {
                    sb.Append("__________");
                }
            }
            return sb.ToString();
        }

    }
}
