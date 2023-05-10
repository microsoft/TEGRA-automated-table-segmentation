using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WebTableExtraction.Utils;
using WebTableExtraction.Extraction;
using WebTableExtraction.Baseline;
using WebTableExtraction.Judie;

namespace WebTableExtraction.Experiments
{
    class NumRowsonQuality
    {
        int numTablesUsed = 1000;
        int[] all_numThreads = new int[] { };
        private string result_dir;

        //for varying number of rows
        int using_num_cols = 8;
        int[] numRows = new int[] { 2, 3,4,5,6,7,8,9,10 };

        //
       

        public NumRowsonQuality(string result_dir)
        {
            this.result_dir = result_dir;
            if (!Directory.Exists(result_dir))
                Directory.CreateDirectory(result_dir);

            GlobalScoringInfo.load();
            KB.load();

        }
        public void Varying_num_rows(string datasetFilePath, bool isSupervised)
        {
            StreamWriter sw = new StreamWriter(result_dir + @"\quality_varying_numrows_given_" + using_num_cols + "_" + isSupervised + "_columns.csv");
            StringBuilder sb = new StringBuilder();
            foreach (int numThreads in all_numThreads)
            {
                sb.Append("MRA+" + numThreads + ", ");
            }
            sw.WriteLine("NumRows, MRA, " + sb.ToString() + "ListExtract, Judie");
          


            Dictionary<int, List<Dictionary<int, double>>> numThreads_2_MRA_numRows_2_quality = new Dictionary<int, List<Dictionary<int, double>>>();
            foreach (int numThreads in all_numThreads)
                numThreads_2_MRA_numRows_2_quality[numThreads] = new List<Dictionary<int, double>>();


            List<Dictionary<int, double>> MRA_numRows_2_quality = new List<Dictionary<int, double>>();
            List<Dictionary<int, double>> ListEx_numRows_2_quality = new List<Dictionary<int, double>>();
            List<Dictionary<int, double>> Judie_numRows_2_quality = new List<Dictionary<int, double>>();

            string[] tables = File.ReadAllLines(datasetFilePath);
            int i = 0;
            foreach (string table in tables)
            {
                string[] splits = table.Split('\t');
                string table_name = splits[0];
                string input_table_line = splits[1];
                string ground_table_line = splits[2];

                Table ground_table = Table_Line_Conversion.line_2_table(ground_table_line);

                if (ground_table.getNumRecords() < numRows[numRows.Length - 1] || ground_table.getNumCols() < using_num_cols)
                    continue;

                i++;

                MRA_numRows_2_quality.Add(Varying_num_rows_one_table(input_table_line, ground_table_line, isSupervised, "MRA"));

                foreach (int numThreads in all_numThreads)
                {
                    numThreads_2_MRA_numRows_2_quality[numThreads].Add(Varying_num_rows_one_table(input_table_line, ground_table_line, isSupervised, "MRA", numThreads));
                }

                ListEx_numRows_2_quality.Add(Varying_num_rows_one_table(input_table_line, ground_table_line, isSupervised, "ListExtract"));
                Judie_numRows_2_quality.Add(Varying_num_rows_one_table(input_table_line, ground_table_line, isSupervised, "Judie"));
                if (i == numTablesUsed)
                {
                    break;
                }
            }


            //Analyze the result



            Dictionary<int, Dictionary<int, double>> numThreads_2_MRA_ave_numrows_2_quality = new Dictionary<int, Dictionary<int, double>>();
            foreach (int numThreads in all_numThreads)
                numThreads_2_MRA_ave_numrows_2_quality[numThreads] = new Dictionary<int, double>();

            Dictionary<int, double> MRA_ave_numrows_2_quality = new Dictionary<int, double>();
            Dictionary<int, double> ListEx_ave_numrows_2_quality = new Dictionary<int, double>();
            Dictionary<int, double> Judie_ave_numrows_2_quality = new Dictionary<int, double>();


            foreach (int x in numRows)
            {
                double MRA_sumTime = 0;
                int MRA_count = 0;
                foreach (Dictionary<int, double> onerun in MRA_numRows_2_quality)
                {
                    if (onerun.ContainsKey(x))
                    {
                        MRA_sumTime += onerun[x];
                        MRA_count++;
                    }

                }
                MRA_ave_numrows_2_quality[x] = MRA_sumTime / MRA_count;


                foreach (int numThreads in all_numThreads)
                {
                    MRA_sumTime = 0;
                    MRA_count = 0;
                    foreach (Dictionary<int, double> onerun in numThreads_2_MRA_numRows_2_quality[numThreads])
                    {
                        if (onerun.ContainsKey(x))
                        {
                            MRA_sumTime += onerun[x];
                            MRA_count++;
                        }

                    }
                    numThreads_2_MRA_ave_numrows_2_quality[numThreads][x] = MRA_sumTime / MRA_count;
                }


                double ListEx_sumTime = 0;
                int ListEx_count = 0;
                foreach (Dictionary<int, double> onerun in ListEx_numRows_2_quality)
                {
                    if (onerun.ContainsKey(x))
                    {
                        ListEx_sumTime += onerun[x];
                        ListEx_count++;
                    }

                }
                ListEx_ave_numrows_2_quality[x] = ListEx_sumTime / ListEx_count;

                double Judie_sumTime = 0;
                int Judie_count = 0;
                foreach (Dictionary<int, double> onerun in Judie_numRows_2_quality)
                {
                    if (onerun.ContainsKey(x))
                    {
                        Judie_sumTime += onerun[x];
                        Judie_count++;
                    }

                }
                Judie_ave_numrows_2_quality[x] = Judie_sumTime / Judie_count;


                sb = new StringBuilder();
                sb.Append(x + "," + MRA_ave_numrows_2_quality[x] + ",");
                foreach (int numThreads in all_numThreads)
                {
                    sb.Append(numThreads_2_MRA_ave_numrows_2_quality[numThreads][x] + ",");
                }
                sb.Append(ListEx_ave_numrows_2_quality[x] + "," + Judie_ave_numrows_2_quality[x]);
                sw.WriteLine(sb.ToString());

            }
            sw.Close();



        }




        public Dictionary<int, double> Varying_num_rows_one_table(string input_table_line, string ground_table_line, bool supervised, string algorithm)
        {
            return Varying_num_rows_one_table(input_table_line, ground_table_line, supervised, algorithm, 0);
        }


        public Dictionary<int, double> Varying_num_rows_one_table(string input_table_line, string ground_table_line, bool supervised, string algorithm, int numThreads)
        {
            //For one table, with 10 columns, varying numrows 1-//Construct a table from the table_line



            List<Line> allLines = new List<Line>();
            foreach (string temp in input_table_line.Split(new string[] { "__________" }, StringSplitOptions.None))
            {
                Line line = new Line(temp);
                allLines.Add(line);
            }
            List<Record> allRecords = new List<Record>();
            foreach (string temp in ground_table_line.Split(new string[] { "__________" }, StringSplitOptions.None))
            {
                Record record = new Record(temp);
                allRecords.Add(record);
            }
           

            Dictionary<int, double> numRows_2_time = new Dictionary<int, double>();

            foreach (int i in numRows)
            {
                List<Line> lines = new List<Line>();
                List<Record> records = new List<Record>();
                for (int j = 1; j <= i; j++)
                {

                    Record subRecord = allRecords[j - 1].getSubRecord(using_num_cols);
                    Line subLine = subRecord.toLine();
                    lines.Add(subLine);
                    records.Add(subRecord);
                }
                Table ground_table = new Table(records);

                bool numColsGiven = false;
                int numExamples = 0;
                if (supervised)
                {
                    numColsGiven = true;
                    numExamples = 0;
                }
                Parameter.numColsGiven = numColsGiven;
                Parameter.numExamples = numExamples;
                Parameter.s_languageWeight = 0;
                Parameter.s_occurWeight = 0;
                Parameter.s_typeWeight = 0;
                Parameter.d_occurWeight = 0.5;
                Parameter.d_syntacticWeight = 0.5;
                Parameter.max_num_threads_per_machine = numThreads;

                //Construct numExamples, and numCols from the ground truth table
                Dictionary<Line, Record> examples = new Dictionary<Line, Record>();
                int numCols = 0;
                if (numColsGiven == true)
                {
                    List<Line> linesForSelectingExamples = new List<Line>(lines);
                    int myNumExamples = numExamples;
                    if (numExamples > linesForSelectingExamples.Count)
                    {
                        myNumExamples = linesForSelectingExamples.Count;
                    }
                    for (int x = 0; x < myNumExamples; x++)
                    {
                        //int rnd = new Random().Next(linesForSelectingExamples.Count());
                        int rnd = linesForSelectingExamples.Count / 2;
                        Line line = linesForSelectingExamples[rnd];
                        linesForSelectingExamples.RemoveAt(rnd);

                        int index = lines.IndexOf(line);
                        examples[lines[index]] = records[index];
                    }
                    numCols = records[0].getNumCells();
                }

                double timeTaken = do_one_table(lines, examples, numCols, algorithm,ground_table);
                numRows_2_time[i] = timeTaken;

            }
            return numRows_2_time;







        }
        private double do_one_table(List<Line> lines, Dictionary<Line, Record> examples, int numCols, string algorithm, Table ground_table)
        {

            DateTime start = DateTime.Now;

            Table aligned_table = null;
            if (algorithm == "MRA")
            {
                //Do the MSA
                MSAAppro msaAppro = new MSAAppro(lines, examples);
                if (Parameter.numColsGiven)
                    msaAppro.alignAStar_with_numcols(numCols);
                else
                    msaAppro.alignAStar_without_numcols();
                aligned_table = msaAppro.getExtracted_Table();
            }
            else if (algorithm == "ListExtract")
            {
                Baseline.Baseline baseline = new Baseline.Baseline(lines, examples);
                if (Parameter.numColsGiven)
                    baseline.align(numCols);
                else
                    baseline.align();
                aligned_table = baseline.get_Extracted_Table();
            }
            else if (algorithm == "Judie")
            {
                Judie.Judie judie = new Judie.Judie(lines, examples);
                if (Parameter.numColsGiven)
                    judie.align(numCols);
                else
                    judie.align();
                aligned_table = judie.get_Extracted_Table();
            }

            DateTime end = DateTime.Now;


            TableComparator tc = new TableComparator(aligned_table, ground_table);
            return tc.flex_fmeasure;
        }
    }
}
