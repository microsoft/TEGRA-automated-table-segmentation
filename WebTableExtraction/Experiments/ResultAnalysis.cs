using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTableExtraction.Utils;
using System.IO;
using System.Threading;
namespace WebTableExtraction.Experiments
{
    public class ResultAnalysis
    {

        private string result_analysis_file_csv;
        string header = "# of Files" + ","
                      + "# anchors, # Cols Given?, # Examples" + ","
                      + "s_type_weight, s_language_weight, s_table_corpus" + ","
                      + "d_syntatic_weight, d_table_corpus_pmi" + ","
                      + "MSA_precision, Baseline_precision, Judie_precision" + ","
                      + "MSA_recall, Baseline_recall, Judie_recall" + ","
                      + "MSA_fmeasure, Baseline_fmeasure, Judie_fmeasure" + ","
                      + "MSA_top_score_fmeasure, Baseline_top_score_fmeasure, Judie_top_score_fmeasure" + ","
                      + "MSA_top_files_fmeasure, Baseline_top_files_fmeasure, Judie_top_files_fmeasure" + ","
                      + "MSA_aveTokens_fmeasure, Baseline_aveTokens_fmeasure, Judie_aveTokens_fmeasure" + ","
                      + "d_table_corpus_score_type" + ","
                      + "single_cell_score_weight" + ","
                      + "MSA_numCols_fmeasure, Baseline_numCols_fmeasure, Judie_numCols_fmeasure" + ","
                      + "MSA_numRows_fmeasure, Baseline_numRows_fmeasure, Judie_numRows_fmeasure" + ","
                      + "MSA_numRows_top_fmeasure, Baseline_numRows_top_fmeasure, Judie_numRows_top_fmeasure" + ","
                      + "MSA_numCols_top_fmeasure, Baseline_numCols_top_fmeasure, Judie_numCols_top_fmeasure" + ",";
        private string result_analysi_file_txt;
        StreamWriter sw_csv;
        StreamWriter sw_txt;
        string whichdataset = "";


        bool scope_analysis = false;
        Dictionary<string, string> data2_input_table_line = new Dictionary<string, string>();
        Dictionary<string, string> data2_ground_table_line = new Dictionary<string, string>();




        List<string> filesRanked_averageTokensPerColumn = new List<string>();
        List<string> filesRanked_numRows = new List<string>();
        List<string> filesRanked_numCols = new List<string>();

        Dictionary<string, double> file2MSAExactPrecision = new Dictionary<string, double>();
        Dictionary<string, double> file2MSAMaxPrecision = new Dictionary<string, double>();
        Dictionary<string, double> file2MSA_flex_precision = new Dictionary<string, double>();
        Dictionary<string, double> file2MSA_flex_recall = new Dictionary<string, double>();
        Dictionary<string, double> file2MSA_flex_f = new Dictionary<string, double>();


        Dictionary<string, double> file2BaselineExactPrecision = new Dictionary<string, double>();
        Dictionary<string, double> file2BaselineMaxPrecision = new Dictionary<string, double>();
        Dictionary<string, double> file2Baseline_flex_precision = new Dictionary<string, double>();
        Dictionary<string, double> file2Baseline_flex_recall = new Dictionary<string, double>();
        Dictionary<string, double> file2Baseline_flex_f = new Dictionary<string, double>();


        Dictionary<string, double> file2JudieExactPrecision = new Dictionary<string, double>();
        Dictionary<string, double> file2JudieMaxPrecision = new Dictionary<string, double>();
        Dictionary<string, double> file2Judie_flex_precision = new Dictionary<string, double>();
        Dictionary<string, double> file2Judie_flex_recall = new Dictionary<string, double>();
        Dictionary<string, double> file2Judie_flex_f = new Dictionary<string, double>();


        int msa_numcols_correct = 0;
        int msa_numcols_over = 0;
        int msa_numcols_under = 0;
        int baseline_numcols_correct = 0;
        int baseline_numcols_over = 0;
        int baseline_numcols_under = 0;
        int judie_numcols_correct = 0;
        int judie_numcols_over = 0;
        int judie_numcols_under = 0;


        Dictionary<string, double> file2MSA_score = new Dictionary<string, double>();
        Dictionary<string, double> file2Baseline_score = new Dictionary<string, double>();
        Dictionary<string, double> file2Judie_score = new Dictionary<string, double>();

        public ResultAnalysis(string inputLogfile)
        {

            if(inputLogfile.Contains(@"ExcelCorpus_10k"))
                whichdataset = "Excel";
            else if(inputLogfile.Contains(@"General_WebTablesCorpus_10k"))
                whichdataset = "Web";
            else if(inputLogfile.Contains(@"WebTablesCorpus_200k"))
                whichdataset = "Wiki";

            string[] temps = System.IO.File.ReadAllLines(inputLogfile);

            result_analysis_file_csv = inputLogfile +  "_analyzed.csv";
            result_analysi_file_txt = inputLogfile + "_analyzed.txt";
            if (File.Exists(result_analysis_file_csv))
            {
                sw_csv = new StreamWriter(result_analysis_file_csv, true);
            }
            else
            {
                sw_csv = new StreamWriter(result_analysis_file_csv, true);
               

                sw_csv.WriteLine(header);
            }
            sw_txt = new StreamWriter(result_analysi_file_txt, true);

           
            for (int i = 0; i < temps.Length; i++)
            {
                //i = 0, is the first line 
                if (i > 0)
                {
                    string temp = temps[i];
                    string[] sps = temp.Split(',');

                    string data = sps[0];
                    string msa_extracted_data = data.Replace(Parameter.inputSequence_directory, Parameter.outputTabular_MSA_directory);
                    string baseline_extracted_data = data.Replace(Parameter.inputSequence_directory, Parameter.outputTabular_Baseline_directory);
                    string judie_extracted_data = data.Replace(Parameter.inputSequence_directory,Parameter.outputTabular_Judie_directory);
                    string ground_data = data.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory);
                    TableComparator msa_tc = new TableComparator(msa_extracted_data, ground_data);
                    TableComparator baseline_tc = new TableComparator(baseline_extracted_data, ground_data);
                    TableComparator Judie_tc = new TableComparator(judie_extracted_data, ground_data);

                  
                 
                    file2MSAExactPrecision.Add(data, msa_tc.cellPrecision);
                    file2MSAMaxPrecision.Add(data, msa_tc.maxCellPrecision);
                    file2MSA_flex_precision.Add(data, msa_tc.flex_precision);
                    file2MSA_flex_recall.Add(data, msa_tc.flex_recall);
                    file2MSA_flex_f.Add(data, msa_tc.flex_fmeasure);


                    file2BaselineExactPrecision.Add(data, baseline_tc.cellPrecision);
                    file2BaselineMaxPrecision.Add(data, baseline_tc.maxCellPrecision);
                    file2Baseline_flex_precision.Add(data, baseline_tc.flex_precision);
                    file2Baseline_flex_recall.Add(data, baseline_tc.flex_recall);
                    file2Baseline_flex_f.Add(data, baseline_tc.flex_fmeasure);


                    file2JudieExactPrecision.Add(data, Judie_tc.cellPrecision);
                    file2JudieMaxPrecision.Add(data, Judie_tc.maxCellPrecision);
                    file2Judie_flex_precision.Add(data, Judie_tc.flex_precision);
                    file2Judie_flex_recall.Add(data, Judie_tc.flex_recall);
                    file2Judie_flex_f.Add(data, Judie_tc.flex_fmeasure);


                    int ground_numcols = Convert.ToInt32(sps[2]);
                    int msa_numcols = Convert.ToInt32(sps[5]);
                    int baseline_numcols = Convert.ToInt32(sps[11]);
                    int judie_numcols = Convert.ToInt32(sps[17]);
                    if (msa_numcols == ground_numcols)
                        msa_numcols_correct++;
                    else if (msa_numcols > ground_numcols)
                        msa_numcols_over++;
                    else
                        msa_numcols_under++;
                    if (baseline_numcols == ground_numcols)
                        baseline_numcols_correct++;
                    else if (baseline_numcols > ground_numcols)
                        baseline_numcols_over++;
                    else
                        baseline_numcols_under++;
                    if (judie_numcols == ground_numcols)
                        judie_numcols_correct++;
                    else if (judie_numcols > ground_numcols)
                        judie_numcols_over++;
                    else
                        judie_numcols_under++;

                    file2MSA_score.Add(data, Convert.ToDouble(sps[9]));
                    file2Baseline_score.Add(data, Convert.ToDouble(sps[15]));
                    file2Judie_score.Add(data, Convert.ToDouble(sps[21]));
                    
                }

            }
            filesRanked_averageTokensPerColumn = file2MSA_flex_f.Keys.OrderBy(x => average_tokens_per_cell(x.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory))).ToList();


            filesRanked_numRows = file2MSA_flex_f.Keys.OrderBy(x => new Table(x.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory)).getNumRecords()).ToList();
            filesRanked_numCols = file2MSA_flex_f.Keys.OrderBy(x => new Table(x.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory)).getNumCols()).ToList();


            string line = write_to_csv();
            sw_csv.WriteLine(line);
            write_to_txt( line);

            sw_csv.Close();
            sw_txt.Close();


        }
        //This is dedicated for analyzing scope result
        public ResultAnalysis(bool Scope_Result, string scope_result_dir, string scope_input_ground_file, string analyzed_file_path)
        {
            if (scope_result_dir.Contains(@"ExcelCorpus_10k"))
                whichdataset = "Excel";
            else if (scope_result_dir.Contains(@"General_WebTablesCorpus_10k"))
                whichdataset = "Web";
            else if (scope_result_dir.Contains(@"WebTablesCorpus_200k"))
                whichdataset = "Wiki";

            System.Diagnostics.Debug.Assert(Scope_Result == true);
            scope_analysis = true;
            result_analysis_file_csv = analyzed_file_path + "scope_result_analyzed.csv";
            result_analysi_file_txt = analyzed_file_path + "scope_result_analyzed.txt";
            if (File.Exists(result_analysis_file_csv))
            {
                sw_csv = new StreamWriter(result_analysis_file_csv, true);
            }
            else
            {
                sw_csv = new StreamWriter(result_analysis_file_csv, true);


                sw_csv.WriteLine(header);
            }
            sw_txt = new StreamWriter(result_analysi_file_txt, true);


           
            foreach(string s in File.ReadAllLines(scope_input_ground_file))
            {
                string data = s.Split('\t')[0];
                string input_table_line = s.Split('\t')[1];
                string ground_table_line = s.Split('\t')[2];
                data2_input_table_line[data] = input_table_line;
                data2_ground_table_line[data] = ground_table_line;


                //set the ground table lines, average token per cell
                average_tokens_per_cell(true, ground_table_line);

            }
          
            foreach(string one_setting_file in FileUtil.getAllFiles(scope_result_dir))
            {
                //Set the approximate parameter for the program
                string[] splits_parametrs = Path.GetFileName(one_setting_file).Split(new string[] { "__" }, StringSplitOptions.None);

                //not a scope run result file
                if (splits_parametrs.Length < 7)
                    continue;
                
                string one_setting_analysis = local_analyze_one_file(one_setting_file);
                sw_csv.WriteLine(one_setting_analysis);
                write_to_txt(one_setting_analysis);
                sw_csv.Flush();
                sw_txt.Flush();
            
            }
            sw_csv.Close();
            sw_txt.Close();
            
        }

        private string local_analyze_one_file(string one_setting_file)
        {
            //Set the approximate parameter for the program
            string[] splits_parametrs = Path.GetFileName(one_setting_file).Split(new string[] { "__" }, StringSplitOptions.None);

        
            string algorithm = splits_parametrs[0];
            bool numColsGiven = Convert.ToBoolean(splits_parametrs[1]);
            int numExamples = Convert.ToInt32(splits_parametrs[2]);
            double s_occurWeight = Convert.ToDouble(splits_parametrs[3]);
            double s_typeWeight = Convert.ToDouble(splits_parametrs[4]);
            double d_occurWeight = Convert.ToDouble(splits_parametrs[5]);
            double d_syntacticWeight = Convert.ToDouble(splits_parametrs[6]);
            Parameter.numColsGiven = numColsGiven;
            Parameter.numExamples = numExamples;
            Parameter.s_languageWeight = 0;
            Parameter.s_occurWeight = s_occurWeight;
            Parameter.s_typeWeight = s_typeWeight;
            Parameter.d_occurWeight = d_occurWeight;
            Parameter.d_syntacticWeight = d_syntacticWeight;


            string[] temps = System.IO.File.ReadAllLines(one_setting_file);
            Dictionary<string, TableComparator> table2TC = new Dictionary<string, TableComparator>();
            local_analyze_one_file_parallel_create_table_comparator(temps,table2TC);
            for (int i = 0; i < temps.Length; i++)
            {
                string temp = temps[i];
                string[] sps = temp.Split('\t');

                //data is the 
                string data = sps[0];
                string result_table_line = sps[1];
                string input_table_line = data2_input_table_line[data];
                string ground_table_line = data2_ground_table_line[data];

                Table result_table = Table_Line_Conversion.line_2_table(result_table_line);
                Table input_table = Table_Line_Conversion.line_2_table(input_table_line);
                Table ground_table = Table_Line_Conversion.line_2_table(ground_table_line);

                TableComparator tc = table2TC[temp];


                TableComparator msa_tc = null;
                TableComparator baseline_tc = null;
                TableComparator Judie_tc = null;
                if (algorithm == "MRA")
                    msa_tc = tc; //new TableComparator(result_table, ground_table);
                else
                    msa_tc = new TableComparator(null, ground_table);
                if (algorithm == "ListExtract")
                    baseline_tc = tc; //new TableComparator(result_table, ground_table);
                else
                    baseline_tc = new TableComparator(null, ground_table);
                if (algorithm == "Judie")
                    Judie_tc = tc; //new TableComparator(result_table, ground_table);
                else
                    Judie_tc = new TableComparator(null, ground_table);


                file2MSAExactPrecision[data] = msa_tc.cellPrecision;
                file2MSAMaxPrecision[data] = msa_tc.maxCellPrecision;
                file2MSA_flex_precision[data] = msa_tc.flex_precision;
                file2MSA_flex_recall[data] = msa_tc.flex_recall;
                file2MSA_flex_f[data] = msa_tc.flex_fmeasure;


                file2BaselineExactPrecision[data] = baseline_tc.cellPrecision;
                file2BaselineMaxPrecision[data] = baseline_tc.maxCellPrecision;
                file2Baseline_flex_precision[data] = baseline_tc.flex_precision;
                file2Baseline_flex_recall[data] = baseline_tc.flex_recall;
                file2Baseline_flex_f[data] = baseline_tc.flex_fmeasure;


                file2JudieExactPrecision[data] = Judie_tc.cellPrecision;
                file2JudieMaxPrecision[data] = Judie_tc.maxCellPrecision;
                file2Judie_flex_precision[data] = Judie_tc.flex_precision;
                file2Judie_flex_recall[data] = Judie_tc.flex_recall;
                file2Judie_flex_f[data] = Judie_tc.flex_fmeasure;


                int ground_numcols = ground_table.getNumCols();
                int result_numcols = result_table.getNumCols();
                int msa_numcols = 0;
                int baseline_numcols = 0;
                int judie_numcols = 0;
                if (algorithm == "MRA")
                    msa_numcols = result_numcols;
                else if (algorithm == "ListExtract")
                    baseline_numcols = result_numcols;
                else if (algorithm == "Judie")
                    judie_numcols = result_numcols;

                if (msa_numcols == ground_numcols)
                    msa_numcols_correct++;
                else if (msa_numcols > ground_numcols)
                    msa_numcols_over++;
                else
                    msa_numcols_under++;
                if (baseline_numcols == ground_numcols)
                    baseline_numcols_correct++;
                else if (baseline_numcols > ground_numcols)
                    baseline_numcols_over++;
                else
                    baseline_numcols_under++;
                if (judie_numcols == ground_numcols)
                    judie_numcols_correct++;
                else if (judie_numcols > ground_numcols)
                    judie_numcols_over++;
                else
                    judie_numcols_under++;


                file2MSA_score[data] = 0;
                file2Baseline_score[data] = 0;
                file2Judie_score[data] = 0;
                if (algorithm == "MRA")
                {
                    List<Line> lines = new List<Line>();
                    foreach (Record record in result_table.getRecords())
                    {
                        lines.Add(record.toLine());
                    }
                    LocalScoringInfo localScoring = new LocalScoringInfo(lines, numExamples);
                    file2MSA_score[data] = result_table.getAvePerColumnScore(localScoring);
                }
                else if (algorithm == "ListExtract")
                    file2Baseline_score[data] = 0;
                else if (algorithm == "Judie")
                    file2Judie_score[data] = 0;


            }
            filesRanked_averageTokensPerColumn = file2MSA_flex_f.Keys.OrderBy(x => average_tokens_per_cell(true,data2_ground_table_line[x])).ToList();
            filesRanked_numRows = file2MSA_flex_f.Keys.OrderBy(x => Table_Line_Conversion.line_2_table(data2_ground_table_line[x]).getNumRecords()).ToList();
            filesRanked_numCols = file2MSA_flex_f.Keys.OrderBy(x => Table_Line_Conversion.line_2_table(data2_ground_table_line[x]).getNumCols()).ToList();
            string one_setting_analysis = write_to_csv();
            Console.WriteLine("finishing one setting: " + one_setting_file);

            return one_setting_analysis;
        }



        private void local_analyze_one_file_parallel_create_table_comparator(string[] temps, Dictionary<string,TableComparator> table2TC)
        {

            List<List<string>> distribute_temps = new List<List<string>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<string> temps_one = new List<string>();
                distribute_temps.Add(temps_one);
            }
            int tag = 0;
            for (int i = 0; i < temps.Length; i++)
            {
                string temp = temps[i];
                int which_thread = tag % Parameter.max_num_threads_per_machine;
                distribute_temps.ElementAt(which_thread).Add(temp);

                tag++;
            }

            int total_size_after_distri = 0;
            foreach (List<string> temps_one in distribute_temps)
            {
                total_size_after_distri += temps_one.Count();
            }
            if (total_size_after_distri != temps.Length)
            {
                Console.WriteLine("wrong");
                System.Diagnostics.Debug.Assert(false);
            }

            Dictionary<Thread, Dictionary<string, TableComparator>> thread2_temps = new Dictionary<Thread, Dictionary<string, TableComparator>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<string> temps_one = distribute_temps.ElementAt(i);
                Dictionary<string, TableComparator> temps_one_table2TC = new Dictionary<string, TableComparator>();
                ThreadStart starter = delegate { local_analyze_one_file_parallel_create_table_comparator_helper(temps_one, temps_one_table2TC); };
                Thread thread = new Thread(starter);
                thread.Start();
                thread2_temps[thread] = temps_one_table2TC;
            }
            foreach (Thread thread in thread2_temps.Keys)
            {
                thread.Join();
            }
            foreach (Thread thread in thread2_temps.Keys)
            {
                foreach (string table in thread2_temps[thread].Keys)
                {
                    table2TC[table] = thread2_temps[thread][table];   
                }
            }
            Console.WriteLine("Done setting TC of all tables  " );
                       
    

            foreach(string temp in temps)
            {
                System.Diagnostics.Debug.Assert(table2TC.ContainsKey(temp));
            }

        }
        private void local_analyze_one_file_parallel_create_table_comparator_helper(List<string> temps, Dictionary<string, TableComparator> table2TC)
        {
            foreach(string temp in temps)
            {
                
                string[] sps = temp.Split('\t');

                //data is the 
                string data = sps[0];
                string result_table_line = sps[1];
                string input_table_line = data2_input_table_line[data];
                string ground_table_line = data2_ground_table_line[data];

                Table result_table = Table_Line_Conversion.line_2_table(result_table_line);
                Table input_table = Table_Line_Conversion.line_2_table(input_table_line);
                Table ground_table = Table_Line_Conversion.line_2_table(ground_table_line);

                TableComparator tc = new TableComparator(result_table, ground_table);


                table2TC[temp] = tc;
            }
            Console.WriteLine("One thread done");
        }




        public ResultAnalysis ()
        {
            scope_analysis = true;
        }
        public string scope_analyze_one_file(string whichdataset, 
            string[] splits_parametrs,
            string[] temps,
            //string scope_result_one_file, 
            string[] ground_tables
            //string scope_input_ground_file
                )
        {

            this.whichdataset = whichdataset;



            foreach (string s in ground_tables)
            {
                string data = s.Split(new string[]{Seperator.getAdHoc()},StringSplitOptions.None)[0];
                //string input_table_line = s.Split('\t')[1];
                string ground_table_line = s.Split(new string[] { Seperator.getAdHoc() }, StringSplitOptions.None)[1];
                //data2_input_table_line[data] = input_table_line;
                data2_ground_table_line[data] = ground_table_line;
            }


            //Set the approximate parameter for the program
            //string[] splits_parametrs = Path.GetFileName(scope_result_one_file).Split(new string[] { "__" }, StringSplitOptions.None);

            //not a scope run result file
            if (splits_parametrs.Length < 7)
                return null;

            string algorithm = splits_parametrs[0];
            bool numColsGiven = Convert.ToBoolean(splits_parametrs[1]);
            int numExamples = Convert.ToInt32(splits_parametrs[2]);
            double s_occurWeight = Convert.ToDouble(splits_parametrs[3]);
            double s_typeWeight = Convert.ToDouble(splits_parametrs[4]);
            double d_occurWeight = Convert.ToDouble(splits_parametrs[5]);
            double d_syntacticWeight = Convert.ToDouble(splits_parametrs[6]);
            Parameter.numColsGiven = numColsGiven;
            Parameter.numExamples = numExamples;
            Parameter.s_languageWeight = 0;
            Parameter.s_occurWeight = s_occurWeight;
            Parameter.s_typeWeight = s_typeWeight;
            Parameter.d_occurWeight = d_occurWeight;
            Parameter.d_syntacticWeight = d_syntacticWeight;


            //string[] temps = System.IO.File.ReadAllLines(scope_result_one_file);
            for (int i = 0; i < temps.Length; i++)
            {
                string temp = temps[i];
                string[] sps = temp.Split(new string[] { Seperator.getAdHoc() }, StringSplitOptions.None);

                //data is the 
                string data = sps[0];
                string result_table_line = sps[1];
                //string input_table_line = data2_input_table_line[data];
                string ground_table_line = data2_ground_table_line[data];

                Table result_table = Table_Line_Conversion.line_2_table(result_table_line);
                //Table input_table = Table_Line_Conversion.line_2_table(input_table_line);
                Table ground_table = Table_Line_Conversion.line_2_table(ground_table_line);


                TableComparator msa_tc = null;
                TableComparator baseline_tc = null;
                TableComparator Judie_tc = null;
                if (algorithm == "MRA")
                    msa_tc = new TableComparator(result_table, ground_table);
                else
                    msa_tc = new TableComparator(null, ground_table);
                if (algorithm == "ListExtract")
                    baseline_tc = new TableComparator(result_table, ground_table);
                else
                    baseline_tc = new TableComparator(null, ground_table);
                if (algorithm == "Judie")
                    Judie_tc = new TableComparator(result_table, ground_table);
                else
                    Judie_tc = new TableComparator(null, ground_table);


                file2MSAExactPrecision[data] = msa_tc.cellPrecision;
                file2MSAMaxPrecision[data] = msa_tc.maxCellPrecision;
                file2MSA_flex_precision[data] = msa_tc.flex_precision;
                file2MSA_flex_recall[data] = msa_tc.flex_recall;
                file2MSA_flex_f[data] = msa_tc.flex_fmeasure;


                file2BaselineExactPrecision[data] = baseline_tc.cellPrecision;
                file2BaselineMaxPrecision[data] = baseline_tc.maxCellPrecision;
                file2Baseline_flex_precision[data] = baseline_tc.flex_precision;
                file2Baseline_flex_recall[data] = baseline_tc.flex_recall;
                file2Baseline_flex_f[data] = baseline_tc.flex_fmeasure;


                file2JudieExactPrecision[data] = Judie_tc.cellPrecision;
                file2JudieMaxPrecision[data] = Judie_tc.maxCellPrecision;
                file2Judie_flex_precision[data] = Judie_tc.flex_precision;
                file2Judie_flex_recall[data] = Judie_tc.flex_recall;
                file2Judie_flex_f[data] = Judie_tc.flex_fmeasure;


                int ground_numcols = ground_table.getNumCols();
                int result_numcols = result_table.getNumCols();
                int msa_numcols = 0;
                int baseline_numcols = 0;
                int judie_numcols = 0;
                if (algorithm == "MRA")
                    msa_numcols = result_numcols;
                else if (algorithm == "ListExtract")
                    baseline_numcols = result_numcols;
                else if (algorithm == "Judie")
                    judie_numcols = result_numcols;

                if (msa_numcols == ground_numcols)
                    msa_numcols_correct++;
                else if (msa_numcols > ground_numcols)
                    msa_numcols_over++;
                else
                    msa_numcols_under++;
                if (baseline_numcols == ground_numcols)
                    baseline_numcols_correct++;
                else if (baseline_numcols > ground_numcols)
                    baseline_numcols_over++;
                else
                    baseline_numcols_under++;
                if (judie_numcols == ground_numcols)
                    judie_numcols_correct++;
                else if (judie_numcols > ground_numcols)
                    judie_numcols_over++;
                else
                    judie_numcols_under++;


                file2MSA_score[data] = 0;
                file2Baseline_score[data] = 0;
                file2Judie_score[data] = 0;
                if (algorithm == "MRA")
                {
                    List<Line> lines = new List<Line>();
                    foreach (Record record in result_table.getRecords())
                    {
                        lines.Add(record.toLine());
                    }
                    LocalScoringInfo localScoring = new LocalScoringInfo(lines, numExamples);
                    file2MSA_score[data] = result_table.getAvePerColumnScore(localScoring);
                }
                else if (algorithm == "ListExtract")
                    file2Baseline_score[data] = 0;
                else if (algorithm == "Judie")
                    file2Judie_score[data] = 0;


            }
            filesRanked_averageTokensPerColumn = file2MSA_flex_f.Keys.OrderBy(x => average_tokens_per_cell(true,data2_ground_table_line[x])).ToList();
            filesRanked_numRows = file2MSA_flex_f.Keys.OrderBy(x => Table_Line_Conversion.line_2_table(data2_ground_table_line[x]).getNumRecords()).ToList();
            filesRanked_numCols = file2MSA_flex_f.Keys.OrderBy(x => Table_Line_Conversion.line_2_table(data2_ground_table_line[x]).getNumCols()).ToList();
            string line = write_to_csv();
            return line;
           
          
        }



        private Dictionary<string, double> ground_table_line2_ave_tokens = new Dictionary<string, double>();
        private double average_tokens_per_cell(bool tag, string ground_table_line)
        {
            if (ground_table_line2_ave_tokens.ContainsKey(ground_table_line))
                return ground_table_line2_ave_tokens[ground_table_line];


            double result = 0;
            int numCells = 0;
            Table table = Table_Line_Conversion.line_2_table(ground_table_line);
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    numCells++;

                    result += cellValue.Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries).Count();

                }
            }
            result = result / numCells;
            ground_table_line2_ave_tokens[ground_table_line] = result;
            return result;
        }
        private double average_tokens_per_cell(string groundFile)
        {
            double result = 0;
            int numCells = 0;
            Table table = new Table(groundFile);
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    numCells++;

                    result += cellValue.Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries).Count();

                }
            }
            result  =  result / numCells;
            return result;
        }
       
        private string top_msa_percentage_average_fmeasure(double top_percent)
        {
            List<string> topFiles = file2MSA_score.Keys.OrderBy(x => file2MSA_score[x]).ToList();

            int index = Convert.ToInt32(topFiles.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > topFiles.Count() - 1)
                index = topFiles.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for(int i = 0; i <= index; i++)
            {
                result_f += file2MSA_flex_f[topFiles.ElementAt(i)];
                result_p += file2MSA_flex_precision[topFiles.ElementAt(i)];
                result_r += file2MSA_flex_recall[topFiles.ElementAt(i)];
            }
            //return result_f / (index + 1);
            return result_f / (index + 1) + "-" + result_p / (index + 1) + "-" + result_r / (index + 1);
        }
        private string top_baseline_percentage_average_fmeasure(double top_percent)
        {
            List<string> topFiles = file2Baseline_score.Keys.OrderBy(x => file2MSA_score[x]).ToList();

            int index = Convert.ToInt32(topFiles.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > topFiles.Count() - 1)
                index = topFiles.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for (int i = 0; i <= index; i++)
            {
                result_f += file2Baseline_flex_f[topFiles.ElementAt(i)];
                result_p += file2Baseline_flex_precision[topFiles.ElementAt(i)];
                result_r += file2Baseline_flex_recall[topFiles.ElementAt(i)];
            }
            //return result_f / (index + 1);
            return result_f / (index + 1) + "-" + result_p / (index + 1) + "-" +  result_r / (index + 1);
        }



        private string top_msa_averageTokensPerCell_average_fmeasure(double top_percent)
        {
            int index = Convert.ToInt32(filesRanked_averageTokensPerColumn.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > filesRanked_averageTokensPerColumn.Count() - 1)
                index = filesRanked_averageTokensPerColumn.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for (int i = 0; i <= index; i++)
            {
                result_f += file2MSA_flex_f[filesRanked_averageTokensPerColumn.ElementAt(i)];
                result_p += file2MSA_flex_precision[filesRanked_averageTokensPerColumn.ElementAt(i)];
                result_r += file2MSA_flex_recall[filesRanked_averageTokensPerColumn.ElementAt(i)];
                
            }
            return result_f / (index + 1) + "-" + result_p / (index + 1) + "-" + result_r / (index + 1);
        }
        private string msa_averageTokensPerCell_average_fmeasure(double min, double max)
        {
            //[min,max)
            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            double count = 0;
            foreach(string file in file2MSA_flex_f.Keys)
            {
                double ave = 0;
                if (scope_analysis == true)
                {
                    ave = average_tokens_per_cell(true, data2_ground_table_line[file]);
                }
                else
                {
                    ave = average_tokens_per_cell(file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory));
                }
                if (ave < min || ave >= max)
                    continue;

                result_f += file2MSA_flex_f[file];
                result_p += file2MSA_flex_precision[file];
                result_r += file2MSA_flex_recall[file];
                count += 1.0;
            }
            result_f = result_f / count;
            result_p = result_p / count;
            result_r = result_r / count;
            return result_f + "-" + result_p + "-" + result_r;
        }
       
        private string top_baseline_averageTokensPerCell_average_fmeasure(double top_percent)
        {

            int index = Convert.ToInt32(filesRanked_averageTokensPerColumn.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > filesRanked_averageTokensPerColumn.Count() - 1)
                index = filesRanked_averageTokensPerColumn.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for (int i = 0; i <= index; i++)
            {
                result_f += file2Baseline_flex_f[filesRanked_averageTokensPerColumn.ElementAt(i)];
                result_p += file2Baseline_flex_precision[filesRanked_averageTokensPerColumn.ElementAt(i)];
                result_r += file2Baseline_flex_recall[filesRanked_averageTokensPerColumn.ElementAt(i)];
            }
            return result_f / (index + 1) + "-" + result_p / (index + 1) + "-" + result_r / (index + 1);
        }
        private string baseline_averageTokensPerCell_average_fmeasure(double min, double max)
        {
            //[min,max)
            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            double count = 0;
            foreach (string file in file2Baseline_flex_f.Keys)
            {
                double ave = 0;
                if (scope_analysis == true)
                {
                    ave = average_tokens_per_cell(true, data2_ground_table_line[file]);
                }
                else
                {
                    ave = average_tokens_per_cell(file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory));
                }

               
                if (ave < min || ave >= max)
                    continue;

                result_f += file2Baseline_flex_f[file];
                result_p += file2Baseline_flex_precision[file];
                result_r += file2Baseline_flex_recall[file];
                count += 1.0;
            }
            result_f = result_f / count;
            result_p = result_p / count;
            result_r = result_r / count;
            return result_f + "-" + result_p + "-" + result_r;
        }
        private string top_judie_averageTokensPerCell_average_fmeasure(double top_percent)
        {

            int index = Convert.ToInt32(filesRanked_averageTokensPerColumn.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > filesRanked_averageTokensPerColumn.Count() - 1)
                index = filesRanked_averageTokensPerColumn.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for (int i = 0; i <= index; i++)
            {
                result_f += file2Judie_flex_f[filesRanked_averageTokensPerColumn.ElementAt(i)];
                result_p += file2Judie_flex_precision[filesRanked_averageTokensPerColumn.ElementAt(i)];
                result_r += file2Judie_flex_recall[filesRanked_averageTokensPerColumn.ElementAt(i)];
            }
            return result_f / (index + 1) + "-" + result_p / (index + 1)+ "-" + result_r / (index + 1);
        }

        private string judie_averageTokensPerCell_average_fmeasure(double min, double max)
        {
            //[min,max)
            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            double count = 0;
            foreach (string file in file2Judie_flex_f.Keys)
            {
                double ave = 0;
                if (scope_analysis == true)
                {
                    ave = average_tokens_per_cell(true, data2_ground_table_line[file]);
                }
                else
                {
                    ave = average_tokens_per_cell(file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory));
                }

                
                if (ave < min || ave >= max)
                    continue;

                result_f += file2Judie_flex_f[file];
                result_p += file2Judie_flex_precision[file];
                result_r += file2Judie_flex_recall[file];
                count += 1.0;
            }
            result_f = result_f / count;
            result_p = result_p / count;
            result_r = result_r / count;
            return result_f + "-" + result_p + "-" + result_r;
        }


        private string msa_numCols_average_fmeasure(double min, double max)
        {
            //[min,max)
            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            double count = 0;
            foreach (string file in file2MSA_flex_f.Keys)
            {
                double ave = 0;
                if (scope_analysis == true)
                {
                    ave = Table_Line_Conversion.line_2_table(data2_ground_table_line[file]).getNumCols();
                }
                else
                {
                    ave = new Table(file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory)).getNumCols();
                }
                if (ave < min || ave >= max)
                    continue;

                result_f += file2MSA_flex_f[file];
                result_p += file2MSA_flex_precision[file];
                result_r += file2MSA_flex_recall[file];
                count += 1.0;
            }
            result_f = result_f / count;
            result_p = result_p / count;
            result_r = result_r / count;
            return result_f + "-" + result_p + "-" + result_r;
        }
        private string baseline_numCols_average_fmeasure(double min, double max)
        {
            //[min,max)
            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            double count = 0;
            foreach (string file in file2Baseline_flex_f.Keys)
            {
                double ave = 0;
                if (scope_analysis == true)
                {
                    ave = Table_Line_Conversion.line_2_table(data2_ground_table_line[file]).getNumCols();
                }
                else
                {
                    ave = new Table(file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory)).getNumCols();
                }
                if (ave < min || ave >= max)
                    continue;

                result_f += file2Baseline_flex_f[file];
                result_p += file2Baseline_flex_precision[file];
                result_r += file2Baseline_flex_recall[file];
                count += 1.0;
            }
            result_f = result_f / count;
            result_p = result_p / count;
            result_r = result_r / count;
            return result_f + "-" + result_p + "-" + result_r;
        }
        private string judie_numCols_average_fmeasure(double min, double max)
        {
            //[min,max)
            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            double count = 0;
            foreach (string file in file2Judie_flex_f.Keys)
            {
                double ave = 0;
                if (scope_analysis == true)
                {
                    ave = Table_Line_Conversion.line_2_table(data2_ground_table_line[file]).getNumCols();
                }
                else
                {
                    ave = new Table(file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory)).getNumCols();
                }
                if (ave < min || ave >= max)
                    continue;

                result_f += file2Judie_flex_f[file];
                result_p += file2Judie_flex_precision[file];
                result_r += file2Judie_flex_recall[file];
                count += 1.0;
            }
            result_f = result_f / count;
            result_p = result_p / count;
            result_r = result_r / count;
            return result_f + "-" + result_p + "-" + result_r;
        }

        private string msa_numRows_average_fmeasure(double min, double max)
        {
            //[min,max)
            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            double count = 0;
            foreach (string file in file2MSA_flex_f.Keys)
            {
                double ave = 0;
                if (scope_analysis == true)
                {
                    ave = Table_Line_Conversion.line_2_table(data2_ground_table_line[file]).getNumRecords();
                }
                else
                {
                    ave = new Table(file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory)).getNumRecords();
                }
                if (ave < min || ave >= max)
                    continue;

                result_f += file2MSA_flex_f[file];
                result_p += file2MSA_flex_precision[file];
                result_r += file2MSA_flex_recall[file];
                count += 1.0;
            }
            result_f = result_f / count;
            result_p = result_p / count;
            result_r = result_r / count;
            return result_f + "-" + result_p + "-" + result_r;
        }
        private string baseline_numRows_average_fmeasure(double min, double max)
        {
            //[min,max)
            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            double count = 0;
            foreach (string file in file2Baseline_flex_f.Keys)
            {
                double ave = 0;
                if (scope_analysis == true)
                {
                    ave = Table_Line_Conversion.line_2_table(data2_ground_table_line[file]).getNumRecords();
                }
                else
                {
                    ave = new Table(file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory)).getNumRecords();
                }
                if (ave < min || ave >= max)
                    continue;

                result_f += file2Baseline_flex_f[file];
                result_p += file2Baseline_flex_precision[file];
                result_r += file2Baseline_flex_recall[file];
                count += 1.0;
            }
            result_f = result_f / count;
            result_p = result_p / count;
            result_r = result_r / count;
            return result_f + "-" + result_p + "-" + result_r;
        }
        private string judie_numRows_average_fmeasure(double min, double max)
        {
            //[min,max)
            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            double count = 0;
            foreach (string file in file2Judie_flex_f.Keys)
            {
                double ave = 0;
                if (scope_analysis == true)
                {
                    ave = Table_Line_Conversion.line_2_table(data2_ground_table_line[file]).getNumRecords();
                }
                else
                {
                    ave = new Table(file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory)).getNumRecords();
                }
                if (ave < min || ave >= max)
                    continue;

                result_f += file2Judie_flex_f[file];
                result_p += file2Judie_flex_precision[file];
                result_r += file2Judie_flex_recall[file];
                count += 1.0;
            }
            result_f = result_f / count;
            result_p = result_p / count;
            result_r = result_r / count;
            return result_f + "-" + result_p + "-" + result_r;
        }


        private string top_msa_numRows_average_fmeasure(double top_percent)
        {
            int index = Convert.ToInt32(filesRanked_numRows.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > filesRanked_numRows.Count() - 1)
                index = filesRanked_numRows.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for (int i = 0; i <= index; i++)
            {
                result_f += file2MSA_flex_f[filesRanked_numRows.ElementAt(i)];
                result_p += file2MSA_flex_precision[filesRanked_numRows.ElementAt(i)];
                result_r += file2MSA_flex_recall[filesRanked_numRows.ElementAt(i)];

            }
            return result_f / (index + 1) + "-" + result_p / (index + 1) + "-" + result_r / (index + 1);
        }

        private string top_baseline_numRows_average_fmeasure(double top_percent)
        {

            int index = Convert.ToInt32(filesRanked_numRows.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > filesRanked_numRows.Count() - 1)
                index = filesRanked_numRows.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for (int i = 0; i <= index; i++)
            {
                result_f += file2Baseline_flex_f[filesRanked_numRows.ElementAt(i)];
                result_p += file2Baseline_flex_precision[filesRanked_numRows.ElementAt(i)];
                result_r += file2Baseline_flex_recall[filesRanked_numRows.ElementAt(i)];
            }
            return result_f / (index + 1) + "-" + result_p / (index + 1) + "-" + result_r / (index + 1);
        }

        private string top_judie_numRows_average_fmeasure(double top_percent)
        {

            int index = Convert.ToInt32(filesRanked_numRows.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > filesRanked_numRows.Count() - 1)
                index = filesRanked_numRows.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for (int i = 0; i <= index; i++)
            {
                result_f += file2Judie_flex_f[filesRanked_numRows.ElementAt(i)];
                result_p += file2Judie_flex_precision[filesRanked_numRows.ElementAt(i)];
                result_r += file2Judie_flex_recall[filesRanked_numRows.ElementAt(i)];
            }
            return result_f / (index + 1) + "-" + result_p / (index + 1) + "-" + result_r / (index + 1);
        }
        private string top_msa_numCols_average_fmeasure(double top_percent)
        {
            int index = Convert.ToInt32(filesRanked_numCols.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > filesRanked_numCols.Count() - 1)
                index = filesRanked_numCols.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for (int i = 0; i <= index; i++)
            {
                result_f += file2MSA_flex_f[filesRanked_numCols.ElementAt(i)];
                result_p += file2MSA_flex_precision[filesRanked_numCols.ElementAt(i)];
                result_r += file2MSA_flex_recall[filesRanked_numCols.ElementAt(i)];

            }
            return result_f / (index + 1) + "-" + result_p / (index + 1) + "-" + result_r / (index + 1);
        }

        private string top_baseline_numCols_average_fmeasure(double top_percent)
        {

            int index = Convert.ToInt32(filesRanked_numCols.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > filesRanked_numCols.Count() - 1)
                index = filesRanked_numCols.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for (int i = 0; i <= index; i++)
            {
                result_f += file2Baseline_flex_f[filesRanked_numCols.ElementAt(i)];
                result_p += file2Baseline_flex_precision[filesRanked_numCols.ElementAt(i)];
                result_r += file2Baseline_flex_recall[filesRanked_numCols.ElementAt(i)];
            }
            return result_f / (index + 1) + "-" + result_p / (index + 1) + "-" + result_r / (index + 1);
        }

        private string top_judie_numCols_average_fmeasure(double top_percent)
        {

            int index = Convert.ToInt32(filesRanked_numCols.Count() * top_percent);
            if (index < 0)
                index = 0;
            if (index > filesRanked_numCols.Count() - 1)
                index = filesRanked_numCols.Count() - 1;

            double result_f = 0;
            double result_p = 0;
            double result_r = 0;
            for (int i = 0; i <= index; i++)
            {
                result_f += file2Judie_flex_f[filesRanked_numCols.ElementAt(i)];
                result_p += file2Judie_flex_precision[filesRanked_numCols.ElementAt(i)];
                result_r += file2Judie_flex_recall[filesRanked_numCols.ElementAt(i)];
            }
            return result_f / (index + 1) + "-" + result_p / (index + 1) + "-" + result_r / (index + 1);
        }
        private string write_to_csv()
        {
            string line = "";

            line += whichdataset + "-" + file2MSA_flex_f.Count() + ",";
            line += Parameter.numAnchorLines + "," + Parameter.numColsGiven + "," + Parameter.numExamples + ",";
            line += Parameter.s_typeWeight + "," + Parameter.s_languageWeight + "," + Parameter.s_occurWeight + ",";
            line += Parameter.d_syntacticWeight + "," + Parameter.d_occurWeight + ",";

            line += calculate_average(file2MSA_flex_precision) + "," + calculate_average(file2Baseline_flex_precision) + "," + calculate_average(file2Judie_flex_precision) + ",";
            line += calculate_average(file2MSA_flex_recall) + "," + calculate_average(file2Baseline_flex_recall) + "," + calculate_average(file2Judie_flex_recall) + ",";
            line += calculate_average(file2MSA_flex_f) + "," + calculate_average(file2Baseline_flex_f) + "," + calculate_average(file2Judie_flex_f) + ",";

            //ranked based on the score of extracted table
            line += "< " + 0.2 + ":" + top_msa_percentage_average_fmeasure(0.2) + ">" +
                    "< " + 0.4 + ":" + top_msa_percentage_average_fmeasure(0.4) + ">" +
                    "< " + 0.6 + ":" + top_msa_percentage_average_fmeasure(0.6) + ">" +
                    "< " + 0.8 + ":" + top_msa_percentage_average_fmeasure(0.8) + ">" +
                    "< " + 1 + ":" + top_msa_percentage_average_fmeasure(1) + ">" + ",";

            line += "< " + 0.2 + ":" + top_baseline_percentage_average_fmeasure(0.2) + ">" +
                   "< " + 0.4 + ":" + top_baseline_percentage_average_fmeasure(0.4) + ">" +
                   "< " + 0.6 + ":" + top_baseline_percentage_average_fmeasure(0.6) + ">" +
                   "< " + 0.8 + ":" + top_baseline_percentage_average_fmeasure(0.8) + ">" +
                   "< " + 1 + ":" + top_baseline_percentage_average_fmeasure(1) + ">" + ",";

            line += "NA,";

            //ranked based on the hardness of tables, average tokens per cell
            line += "< " + 0.2 + ":" + top_msa_averageTokensPerCell_average_fmeasure(0.2) + ">" +
                   "< " + 0.4 + ":" + top_msa_averageTokensPerCell_average_fmeasure(0.4) + ">" +
                   "< " + 0.6 + ":" + top_msa_averageTokensPerCell_average_fmeasure(0.6) + ">" +
                   "< " + 0.8 + ":" + top_msa_averageTokensPerCell_average_fmeasure(0.8) + ">" +
                   "< " + 1 + ":" + top_msa_averageTokensPerCell_average_fmeasure(1) + ">" + ",";

            line += "< " + 0.2 + ":" + top_baseline_averageTokensPerCell_average_fmeasure(0.2) + ">" +
                   "< " + 0.4 + ":" + top_baseline_averageTokensPerCell_average_fmeasure(0.4) + ">" +
                   "< " + 0.6 + ":" + top_baseline_averageTokensPerCell_average_fmeasure(0.6) + ">" +
                   "< " + 0.8 + ":" + top_baseline_averageTokensPerCell_average_fmeasure(0.8) + ">" +
                   "< " + 1 + ":" + top_baseline_averageTokensPerCell_average_fmeasure(1) + ">" + ",";

            line += "< " + 0.2 + ":" + top_judie_averageTokensPerCell_average_fmeasure(0.2) + ">" +
                 "< " + 0.4 + ":" + top_judie_averageTokensPerCell_average_fmeasure(0.4) + ">" +
                 "< " + 0.6 + ":" + top_judie_averageTokensPerCell_average_fmeasure(0.6) + ">" +
                 "< " + 0.8 + ":" + top_judie_averageTokensPerCell_average_fmeasure(0.8) + ">" +
                 "< " + 1 + ":" + top_judie_averageTokensPerCell_average_fmeasure(1) + ">" + ",";


            //ranked based on the average number of tokens per cell
            line += "< [1-1.5] " + ":" + msa_averageTokensPerCell_average_fmeasure(1,1.5) + ">" +
                  "< [1.5-2.0] " + ":" + msa_averageTokensPerCell_average_fmeasure(1.5, 2.0) + ">" +
                  "< [2.0-2.5] " + ":" + msa_averageTokensPerCell_average_fmeasure(2.0, 2.5) + ">" +
                  "< [2.5-3.0] " + ":" + msa_averageTokensPerCell_average_fmeasure(2.5, 3.0) + ">" +
                  "< [3.0-100] " + ":" + msa_averageTokensPerCell_average_fmeasure(3.0, 100) + ">"  +",";

            line += "< [1-1.5] " + ":" + baseline_averageTokensPerCell_average_fmeasure(1, 1.5) + ">" +
                  "< [1.5-2.0] " + ":" + baseline_averageTokensPerCell_average_fmeasure(1.5, 2.0) + ">" +
                  "< [2.0-2.5] " + ":" + baseline_averageTokensPerCell_average_fmeasure(2.0, 2.5) + ">" +
                  "< [2.5-3.0] " + ":" + baseline_averageTokensPerCell_average_fmeasure(2.5, 3.0) + ">" +
                  "< [3.0-100] " + ":" + baseline_averageTokensPerCell_average_fmeasure(3.0, 100) + ">" + ",";

            line += "< [1-1.5] " + ":" + judie_averageTokensPerCell_average_fmeasure(1, 1.5) + ">" +
                  "< [1.5-2.0] " + ":" + judie_averageTokensPerCell_average_fmeasure(1.5, 2.0) + ">" +
                  "< [2.0-2.5] " + ":" + judie_averageTokensPerCell_average_fmeasure(2.0, 2.5) + ">" +
                  "< [2.5-3.0] " + ":" + judie_averageTokensPerCell_average_fmeasure(2.5, 3.0) + ">" +
                  "< [3.0-100] " + ":" + judie_averageTokensPerCell_average_fmeasure(3.0, 100) + ">" + ",";


            line += Parameter.d_occur_type + ",";
            line += Parameter.singleScoreWeight + ",";

            int maxNumCols = 20;
            int step = 4;
            for (int numCols = 1; numCols <= maxNumCols; numCols += step)
            {
                line += "< [" + numCols + "-" + (numCols + step) + ") " + ":" + msa_numCols_average_fmeasure(numCols, numCols + step) + ">";
               
                    
            }
                line += ",";
            for (int numCols = 1; numCols <= maxNumCols; numCols += step)
            {
                line += "< [" + numCols + "-" + (numCols + step) + ") " + ":" + baseline_numCols_average_fmeasure(numCols, numCols + step) + ">";
               
            }
            line += ",";
            for (int numCols = 1; numCols <= maxNumCols; numCols += step)
            {
                line += "< [" + numCols + "-" + (numCols + step) + ") " + ":" + judie_numCols_average_fmeasure(numCols, numCols + step) + ">";
                
            }
            line += ",";



            int maxNumRows = 50;
            int step_rows = 10;
            for (int numRows = 1; numRows <= maxNumRows; numRows += step_rows)
            {
                line += "< [" + numRows + "-" + (numRows + step_rows) + ") " + ":" + msa_numRows_average_fmeasure(numRows, numRows + step_rows) + ">";
            }
            line += ",";
            for (int numRows = 1; numRows <= maxNumRows; numRows += step_rows)
            {
                line += "< [" + numRows + "-" + (numRows + step_rows) + ") " + ":" + baseline_numRows_average_fmeasure(numRows, numRows + step_rows) + ">";
            }
            line += ",";
            for (int numRows = 1; numRows <= maxNumRows; numRows += step_rows)
            {
                line += "< [" + numRows + "-" + (numRows + step_rows) + ") " + ":" + judie_numRows_average_fmeasure(numRows, numRows + step_rows) + ">";
            }
            line += ",";


            //ranked based on num rows per table
            line += "< " + 0.2 + ":" + top_msa_numRows_average_fmeasure(0.2) + ">" +
                   "< " + 0.4 + ":" + top_msa_numRows_average_fmeasure(0.4) + ">" +
                   "< " + 0.6 + ":" + top_msa_numRows_average_fmeasure(0.6) + ">" +
                   "< " + 0.8 + ":" + top_msa_numRows_average_fmeasure(0.8) + ">" +
                   "< " + 1 + ":" + top_msa_numRows_average_fmeasure(1) + ">" + ",";

            line += "< " + 0.2 + ":" + top_baseline_numRows_average_fmeasure(0.2) + ">" +
                   "< " + 0.4 + ":" + top_baseline_numRows_average_fmeasure(0.4) + ">" +
                   "< " + 0.6 + ":" + top_baseline_numRows_average_fmeasure(0.6) + ">" +
                   "< " + 0.8 + ":" + top_baseline_numRows_average_fmeasure(0.8) + ">" +
                   "< " + 1 + ":" + top_baseline_numRows_average_fmeasure(1) + ">" + ",";

            line += "< " + 0.2 + ":" + top_judie_numRows_average_fmeasure(0.2) + ">" +
                 "< " + 0.4 + ":" + top_judie_numRows_average_fmeasure(0.4) + ">" +
                 "< " + 0.6 + ":" + top_judie_numRows_average_fmeasure(0.6) + ">" +
                 "< " + 0.8 + ":" + top_judie_numRows_average_fmeasure(0.8) + ">" +
                 "< " + 1 + ":" + top_judie_numRows_average_fmeasure(1) + ">" + ",";


            //ranked based on num COLS per table
            line += "< " + 0.2 + ":" + top_msa_numCols_average_fmeasure(0.2) + ">" +
                   "< " + 0.4 + ":" + top_msa_numCols_average_fmeasure(0.4) + ">" +
                   "< " + 0.6 + ":" + top_msa_numCols_average_fmeasure(0.6) + ">" +
                   "< " + 0.8 + ":" + top_msa_numCols_average_fmeasure(0.8) + ">" +
                   "< " + 1 + ":" + top_msa_numCols_average_fmeasure(1) + ">" + ",";

            line += "< " + 0.2 + ":" + top_baseline_numCols_average_fmeasure(0.2) + ">" +
                   "< " + 0.4 + ":" + top_baseline_numCols_average_fmeasure(0.4) + ">" +
                   "< " + 0.6 + ":" + top_baseline_numCols_average_fmeasure(0.6) + ">" +
                   "< " + 0.8 + ":" + top_baseline_numCols_average_fmeasure(0.8) + ">" +
                   "< " + 1 + ":" + top_baseline_numCols_average_fmeasure(1) + ">" + ",";

            line += "< " + 0.2 + ":" + top_judie_numCols_average_fmeasure(0.2) + ">" +
                 "< " + 0.4 + ":" + top_judie_numCols_average_fmeasure(0.4) + ">" +
                 "< " + 0.6 + ":" + top_judie_numCols_average_fmeasure(0.6) + ">" +
                 "< " + 0.8 + ":" + top_judie_numCols_average_fmeasure(0.8) + ">" +
                 "< " + 1 + ":" + top_judie_numCols_average_fmeasure(1) + ">" + ",";


            return line;
        }

        private double calculate_average(Dictionary<string,double> input)
        {
            double result = 0;
            foreach(string key in input.Keys)
            {
                //Console.WriteLine("File: " + key + " has value: " + input[key]);
                result += input[key];
            }
            return result / input.Keys.Count();
        }


        private void write_to_txt(string line)
        {

            sw_txt.WriteLine(line);
            sw_txt.Write(Parameter.toString());

            sw_txt.WriteLine();

            sw_txt.WriteLine("(1) Precision Analysis using exact matching with ground truth");
            MyAnalysis myAnalysis = new MyAnalysis(file2MSAExactPrecision, file2BaselineExactPrecision, sw_txt);
            sw_txt.WriteLine();

            sw_txt.WriteLine("(2) Precision Analysis using max matching with ground truth for each line");
            MyAnalysis myAnalysis2 = new MyAnalysis(file2MSAMaxPrecision, file2BaselineMaxPrecision, sw_txt);
            sw_txt.WriteLine();

            sw_txt.WriteLine("(3) Precision Analysis using flexable number of columns");
            MyAnalysis myAnalysis3 = new MyAnalysis(file2MSA_flex_precision, file2Baseline_flex_precision, sw_txt);
            sw_txt.WriteLine();

            sw_txt.WriteLine("(4) recall Analysis using flexable number of columns");
            MyAnalysis myAnalysis4 = new MyAnalysis(file2MSA_flex_recall, file2Baseline_flex_recall, sw_txt);
            sw_txt.WriteLine();

            sw_txt.WriteLine("(5) f measure Analysis using flexable number of columns");
            MyAnalysis myAnalysis5 = new MyAnalysis(file2MSA_flex_f, file2Baseline_flex_f, sw_txt);
            sw_txt.WriteLine();


            sw_txt.WriteLine("(6) Number of columns predication comparasion");
            sw_txt.WriteLine(" MSA correctly predicts num of cols is " + msa_numcols_correct);
            sw_txt.WriteLine(" MSA over predicts num of cols is " + msa_numcols_over);
            sw_txt.WriteLine(" MSA under predicate num of cols is " + msa_numcols_under);

            sw_txt.WriteLine(" Baseline correctly predicts num of cols is " + baseline_numcols_correct);
            sw_txt.WriteLine(" Baseline over predicts num of cols is " + baseline_numcols_over);
            sw_txt.WriteLine(" Baseline under predicate num of cols is " + baseline_numcols_under);

            sw_txt.WriteLine(" Judie correctly predicts num of cols is " + judie_numcols_correct);
            sw_txt.WriteLine(" Judie over predicts num of cols is " + judie_numcols_over);
            sw_txt.WriteLine(" Judie under predicate num of cols is " + judie_numcols_under);
            
            
            
            
            
            sw_txt.WriteLine("*********************************************************");


            sw_txt.WriteLine("");

            
        }


        class MyAnalysis
        {
            Dictionary<string, double> file2_MSA_Precision;
            Dictionary<string, double> file2_Baseline_Precision;
            StreamWriter sw;
            public MyAnalysis(Dictionary<string, double> file2_MSA_Precision, Dictionary<string, double> file2_BaseLine_Precision, StreamWriter sw)
            {
                this.file2_Baseline_Precision = file2_BaseLine_Precision;
                this.file2_MSA_Precision = file2_MSA_Precision;
                this.sw = sw;
                analyze();
            }
            private void analyze()
            {
               
                numberOfWinners();
                averagePrecision();

                /*
                sw.WriteLine("---------------------------");
                numberOfWinnersWhenBothPrecisionGreaterThan(0.3);
                averagePrecisionWhenPrecisionGreaterThan(0.3);

                sw.WriteLine("---------------------------");
                numberOfWinnersWhenBothPrecisionGreaterThan(0.4);
                averagePrecisionWhenPrecisionGreaterThan(0.4);

                sw.WriteLine("---------------------------");
                numberOfWinnersWhenBothPrecisionGreaterThan(0.5);
                averagePrecisionWhenPrecisionGreaterThan(0.5);

                sw.WriteLine("---------------------------");
                numberOfWinnersWhenBothPrecisionGreaterThan(0.6);
                averagePrecisionWhenPrecisionGreaterThan(0.6);

                sw.WriteLine("---------------------------");
                numberOfWinnersWhenBothPrecisionGreaterThan(0.7);
                averagePrecisionWhenPrecisionGreaterThan(0.7);
                 * */
            }

            private void numberOfWinners()
            {
                int msaWins = 0;
                int baselineWins = 0;
                int ties = 0;
                foreach (string data in file2_Baseline_Precision.Keys)
                {
                    double msaPrecision = file2_MSA_Precision[data];
                    double baselinePrecision = file2_Baseline_Precision[data];
                    if (msaPrecision > baselinePrecision)
                        msaWins++;
                    else if (msaPrecision < baselinePrecision)
                    {
                        baselineWins++;
                    }
                    else
                    {
                        ties++;
                    }
                }
                System.Diagnostics.Debug.Assert(msaWins + baselineWins + ties == file2_Baseline_Precision.Keys.Count());
                sw.WriteLine("MSA:Baseline:Ties is {0}:{1}:{2}", msaWins, baselineWins, ties);

            }
            private void numberOfWinnersWhenBothPrecisionGreaterThan(double thre)
            {
                int msaWins = 0;
                int baselineWins = 0;
                int ties = 0;
                int total = 0;
                foreach (string data in file2_Baseline_Precision.Keys)
                {
                    double msaPrecision = file2_MSA_Precision[data];
                    double baselinePrecision = file2_Baseline_Precision[data];

                    if (msaPrecision < thre || baselinePrecision < thre)
                        continue;

                    total++;
                    if (msaPrecision > baselinePrecision)
                        msaWins++;
                    else if (msaPrecision < baselinePrecision)
                    {
                        baselineWins++;
                    }
                    else
                    {
                        ties++;
                    }
                }
                System.Diagnostics.Debug.Assert(msaWins + baselineWins + ties == total);
                sw.WriteLine("MSA:Baseline:Ties is {0}:{1}:{2} When both measure is greater than {3}", msaWins, baselineWins, ties, thre);

            }
            private void averagePrecision()
            {
                double msaTotalPrecision = 0;
                double baseTotalPrecision = 0;

                foreach (string data in file2_Baseline_Precision.Keys)
                {
                    double msaPrecision = file2_MSA_Precision[data];
                    double baselinePrecision = file2_Baseline_Precision[data];
                    msaTotalPrecision += msaPrecision;
                    baseTotalPrecision += baselinePrecision;
                }
                msaTotalPrecision /= file2_Baseline_Precision.Keys.Count();
                baseTotalPrecision /= file2_Baseline_Precision.Keys.Count();

                sw.WriteLine("The Average of all files: MSA: {0}. Baseline {1}", msaTotalPrecision, baseTotalPrecision);


            }
            private void averagePrecisionWhenPrecisionGreaterThan(double thre)
            {
                double msaTotalPrecision = 0;
                double baseTotalPrecision = 0;
                int total = 0;

                foreach (string data in file2_Baseline_Precision.Keys)
                {
                    double msaPrecision = file2_MSA_Precision[data];
                    double baselinePrecision = file2_Baseline_Precision[data];

                    if (msaPrecision < thre || baselinePrecision < thre)
                        continue;

                    total++;
                    msaTotalPrecision += msaPrecision;
                    baseTotalPrecision += baselinePrecision;
                }
                msaTotalPrecision /= total;
                baseTotalPrecision /= total;

                sw.WriteLine("The Average  of all files: MSA: {0}. Baseline {1} When both precision is greater than {2}", msaTotalPrecision, baseTotalPrecision, thre);


            }

        }


       

    }
}
