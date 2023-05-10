using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebTableExtraction.Experiments
{
    class GraphProduction2
    {

        /*
         * This is for drawing graphs using analyzed log file
         */
        string[] scope_analyzed = null;
        string graph_dir;

        public GraphProduction2(string scope_analyzed_file_path, string graph_dir)
        {
            this.graph_dir = graph_dir;

            scope_analyzed = System.IO.File.ReadAllLines(scope_analyzed_file_path);

          
            /*
            string header = "# of Files" + ","
            +"# anchors, # Cols Given?, # Examples" + ","
            + "s_type_weight, s_language_weight, s_table_corpus" + ","
            + "d_syntatic_weight, d_table_corpus_pmi" + ","
            + "MSA_precision, Baseline_precision, Judie_precision" + ","
            + "MSA_recall, Baseline_recall, Judie_recall" + ","
            + "MSA_fmeasure, Baseline_fmeasure, Judie_fmeasure" + ","
            + "MSA_top_score_fmeasure, Baseline_top_score_fmeasure, Judie_top_score_fmeasure" + ","
            + "MSA_top_files_fmeasure, Baseline_top_files_fmeasure, Judie_top_files_fmeasure" + ","
            + "MSA_aveTokens_fmeasure, Baseline_aveTokens_fmeasure, Judie_aveTokens_fmeasure";
                * */
           
            
            msa_top_percentage_fmeasure_supervised();
            msa_top_percentage_fmeasure_unsupervised();
            ave_tokens_fmeasure_supervised_excel_dataset();
            ave_tokens_fmeasure_supervised_webtables_dataset();
            ave_tokens_fmeasure_supervised_wikitables_dataset();
            ave_tokens_fmeasure_unsupervised_excel_dataset();
            ave_tokens_fmeasure_unsupervised_webtables_dataset();
            ave_tokens_fmeasure_unsupervised_wikitables_dataset();

            msa_varying_supervision_level();
            listextract_varying_supervision_level();
            judie_varying_supervision_level();

            msa_varying_pairwise_score(true);
            listextract_varying_pairwise_score(true);

            msa_varying_single_cell_score(true);
            listextract_varying_single_cell_score(true);

            msa_varying_pairwise_score(false);
            listextract_varying_pairwise_score(false);

            msa_varying_single_cell_score(false);
            listextract_varying_single_cell_score(false);

        }
        public GraphProduction2(string supervised_all_filepath, string unsupervised_all_filepath, string varying_super_level_file_path, string graph_dir)
        {
            this.graph_dir = graph_dir;


           


            scope_analyzed = System.IO.File.ReadAllLines(varying_super_level_file_path);
            
            msa_varying_supervision_level();
            listextract_varying_supervision_level();
            judie_varying_supervision_level();




            scope_analyzed = System.IO.File.ReadAllLines(supervised_all_filepath);
            string[] scope_analyzed_judie = System.IO.File.ReadAllLines(varying_super_level_file_path);
            foreach(string temp in scope_analyzed_judie)
            {
                if(temp.Contains("Judie"))
                {
                    string[] scope_analyzed_temp = new string[scope_analyzed.Length + 1];
                    for (int i = 0; i < scope_analyzed_temp.Length; i++)
                    {
                        if (i != scope_analyzed_temp.Length - 1)
                            scope_analyzed_temp[i] = scope_analyzed[i];
                        else
                            scope_analyzed_temp[i] = temp;
                    }
                    scope_analyzed = scope_analyzed_temp;

                }
               
            }

            msa_top_percentage_fmeasure_supervised();
            ave_tokens_fmeasure_supervised_excel_dataset();
            ave_tokens_fmeasure_supervised_webtables_dataset();
            ave_tokens_fmeasure_supervised_wikitables_dataset();
            msa_varying_pairwise_score(true);
            listextract_varying_pairwise_score(true);
            msa_varying_single_cell_score(true);
            listextract_varying_single_cell_score(true);



            scope_analyzed = System.IO.File.ReadAllLines(unsupervised_all_filepath);
            scope_analyzed_judie = System.IO.File.ReadAllLines(varying_super_level_file_path);
            foreach (string temp in scope_analyzed_judie)
            {
                if (temp.Contains("Judie"))
                {
                    string[] scope_analyzed_temp = new string[scope_analyzed.Length + 1];
                    for (int i = 0; i < scope_analyzed_temp.Length; i++)
                    {
                        if (i != scope_analyzed_temp.Length - 1)
                            scope_analyzed_temp[i] = scope_analyzed[i];
                        else
                            scope_analyzed_temp[i] = temp;
                    }
                    scope_analyzed = scope_analyzed_temp;

                }

            }

            msa_top_percentage_fmeasure_unsupervised();
            ave_tokens_fmeasure_unsupervised_excel_dataset();
            ave_tokens_fmeasure_unsupervised_webtables_dataset();
            ave_tokens_fmeasure_unsupervised_wikitables_dataset();
            msa_varying_pairwise_score(false);
            listextract_varying_pairwise_score(false);
            msa_varying_single_cell_score(false);
            listextract_varying_single_cell_score(false);
            


        }
        private string get_setting_log(string dataset, bool numColsGiven, int numExamples, 
            double s_type_weight,
            double  s_table_corpus,
            double d_syntatic_weight,
            double  d_table_corpus_pmi,
            string algorithm)
        {
            foreach(string log in scope_analyzed)
            {
                string[] split = log.Split(',');

                if(split[0].Contains(dataset)
                    && Convert.ToBoolean(split[2]) == numColsGiven
                    && Convert.ToInt32(split[3]) == numExamples
                    && Convert.ToDouble(split[4]) == Math.Round(s_type_weight,3)
                    && Convert.ToDouble(split[6]) == Math.Round(s_table_corpus,3)
                    && Convert.ToDouble(split[7]) == Math.Round(d_syntatic_weight,3)
                    && Convert.ToDouble(split[8]) == Math.Round(d_table_corpus_pmi,3)
                    )
                {
                    if (algorithm == "MRA" && Convert.ToDouble(split[9]) != 0)  
                        return log;
                    if (algorithm == "ListExtract" && Convert.ToDouble(split[10]) != 0)
                        return log;
                    if (algorithm == "Judie" && Convert.ToDouble(split[11]) != 0)
                        return log;
                }
            }
            //No data regarding this setting
            return dataset + ","
                  + 2147483647 + ","
                  + numColsGiven + ","
                  + numExamples + ","
                  + s_type_weight + ","
                  + 0 + ","
                  + s_table_corpus + ","
                  + d_syntatic_weight + ","
                  + d_table_corpus_pmi + ","
                  + 0 + ","
                  + 0 + ","
                  + 0 + ","
                  + 0 + ","
                  + 0 + ","
                  + 0 + ","
                  + 0 + ","
                  + 0 + ","
                  + 0 + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< [1-1.5] :0>< [1.5-2.0] :0>< [2.0-2.5] :0>< [2.5-3.0] :0>< [3.0-100] :0>" + ","
                  + @"< [1-1.5] :0>< [1.5-2.0] :0>< [2.0-2.5] :0>< [2.5-3.0] :0>< [3.0-100] :0>" + ","
                  + @"< [1-1.5] :0>< [1.5-2.0] :0>< [2.0-2.5] :0>< [2.5-3.0] :0>< [3.0-100] :0>"
                  ;
        }


        public void msa_top_percentage_fmeasure_supervised()
        {
            StreamWriter sw = new StreamWriter(graph_dir + "msa_top_percentage_fmeasure_supervised.csv");
            // Percentage	MRA+ExcelTables MRA+WebTables	MRA+WikiTables	

            string[] col_per = new string[] { "0.2", "0.4", "0.6", "0.8", "1.0" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log("Excel",true,2,0.5,0.5,0.5,0.5,"MRA").Split(',')[18];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log("Web", true, 2, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[18];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log("Wiki", true, 2, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[18];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        public void msa_top_percentage_fmeasure_unsupervised()
        {
            StreamWriter sw = new StreamWriter(graph_dir + "msa_top_percentage_fmeasure_unsupervised.csv");
            // Percentage	MRA+ExcelTables MRA+WebTables	MRA+WikiTables	

            string[] col_per = new string[] { "0.2", "0.4", "0.6", "0.8", "1.0" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log("Excel", false, 0, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[18];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log("Web", false, 0, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[18];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log("Wiki", false, 0, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[18];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }


        public void ave_tokens_fmeasure_supervised_excel_dataset()
        {
            StreamWriter sw = new StreamWriter(graph_dir + "ave_tokens_fmeasure_supervised_excel_dataset.csv");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "1.5", "2", "2.5", "3", "3.5" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log("Excel", true, 2, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log("Excel", true, 2, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log("Excel", true, 2, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[26];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        public void ave_tokens_fmeasure_supervised_webtables_dataset()
        {
            StreamWriter sw = new StreamWriter(graph_dir + "ave_tokens_fmeasure_supervised_webtables_dataset.csv");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "1.5", "2", "2.5", "3", "3.5" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log("Web", true, 2, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log("Web", true, 2, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log("Web", true, 2, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[26];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        public void ave_tokens_fmeasure_supervised_wikitables_dataset()
        {
            StreamWriter sw = new StreamWriter(graph_dir + "ave_tokens_fmeasure_supervised_wikitables_dataset.csv");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "1.5", "2", "2.5", "3", "3.5" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log("Wiki", true, 2, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log("Wiki", true, 2, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log("Wiki", true, 2, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[26];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }

        public void ave_tokens_fmeasure_unsupervised_excel_dataset()
        {
            StreamWriter sw = new StreamWriter(graph_dir + "ave_tokens_fmeasure_unsupervised_excel_dataset.csv");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "1.5", "2", "2.5", "3", "3.5" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log("Excel", false, 0, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log("Excel", false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log("Excel", false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[26];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        public void ave_tokens_fmeasure_unsupervised_webtables_dataset()
        {
            StreamWriter sw = new StreamWriter(graph_dir + "ave_tokens_fmeasure_unsupervised_webtables_dataset.csv");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "1.5", "2", "2.5", "3", "3.5" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log("Web", false, 0, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log("Web", false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log("Web", false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[26];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        public void ave_tokens_fmeasure_unsupervised_wikitables_dataset()
        {
            StreamWriter sw = new StreamWriter(graph_dir + "ave_tokens_fmeasure_unsupervised_wikitables_dataset.csv");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "1.5", "2", "2.5", "3", "3.5" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log("Wiki", false, 0, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log("Wiki", false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log("Wiki", false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[26];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }




        public void msa_varying_supervision_level()
        {
            //Supervision Level	Excel	Web	Wiki
            StreamWriter sw = new StreamWriter(graph_dir + @"msa_varying_supervision_level.csv");
            string[] col_per = new string[] { "-1", "0", "1", "2", "3", "4" };
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)  
                    col_1[l] = get_setting_log("Excel", false, 0, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[15];
                else 
                    col_1[l] = get_setting_log("Excel", true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[15];
               
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)  
                    col_2[l] = get_setting_log("Web", false, 0, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[15];
                else
                    col_2[l] = get_setting_log("Web", true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[15];
               

               
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)  
                    col_3[l] = get_setting_log("Wiki", false, 0, 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[15];
                else
                    col_3[l] = get_setting_log("Wiki", true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "MRA").Split(',')[15];
               
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }

        public void listextract_varying_supervision_level()
        {
            //Supervision Level	Excel	Web	Wiki
            StreamWriter sw = new StreamWriter(graph_dir + @"listextract_varying_supervision_level.csv");
            string[] col_per = new string[] { "-1", "0", "1", "2", "3", "4" };
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)
                    col_1[l] = get_setting_log("Excel", false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[16];
                else
                    col_1[l] = get_setting_log("Excel", true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[16];

            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)
                    col_2[l] = get_setting_log("Web", false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[16];
                else
                    col_2[l] = get_setting_log("Web", true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[16];



            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)
                    col_3[l] = get_setting_log("Wiki", false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[16];
                else
                    col_3[l] = get_setting_log("Wiki", true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[16];

            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }
        public void judie_varying_supervision_level()
        {
            //Supervision Level	Excel	Web	Wiki
            StreamWriter sw = new StreamWriter(graph_dir + @"judie_varying_supervision_level.csv");
            string[] col_per = new string[] { "-1", "0", "1", "2", "3", "4" };
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)
                    col_1[l] = get_setting_log("Excel", false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[17];
                else
                    col_1[l] = get_setting_log("Excel", true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[17];

            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)
                    col_2[l] = get_setting_log("Web", false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[17];
                else
                    col_2[l] = get_setting_log("Web", true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[17];



            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)
                    col_3[l] = get_setting_log("Wiki", false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[17];
                else
                    col_3[l] = get_setting_log("Wiki", true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[17];

            }


            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }




        public void msa_varying_pairwise_score(bool supervised)
        {
            //Supervision Level	Excel	Web	Wiki
            StreamWriter sw = new StreamWriter(graph_dir + @"msa_varying_pairwise_score_unsupervised.csv");
            if(supervised)
                sw = new StreamWriter(graph_dir + @"msa_varying_pairwise_score_supervised.csv");
            
            string[] col_per = new string[] { "0", "0.3", "0.5", "0.7", "1.0" };
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_1[l] = get_setting_log("Excel", false, 0, 0.5, 0.5, temp, 1.0 - temp, "MRA").Split(',')[15];
                if (supervised)
                    col_1[l] = get_setting_log("Excel", true, 2, 0.5, 0.5, temp, 1.0 - temp, "MRA").Split(',')[15];
                
                    
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_2[l] = get_setting_log("Web", false, 0, 0.5, 0.5, temp, 1.0 - temp, "MRA").Split(',')[15];
                if(supervised)
                    col_2[l] = get_setting_log("Web", true, 2, 0.5, 0.5, temp, 1.0 - temp, "MRA").Split(',')[15];
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_3[l] = get_setting_log("Wiki", false, 0, 0.5, 0.5, temp, 1.0 - temp, "MRA").Split(',')[15];
                if(supervised)
                    col_3[l] = get_setting_log("Wiki", true, 2, 0.5, 0.5, temp, 1.0 - temp, "MRA").Split(',')[15];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }
        public void listextract_varying_pairwise_score(bool supervised)
        {
            //Supervision Level	Excel	Web	Wiki
            StreamWriter sw = new StreamWriter(graph_dir + @"listextract_varying_pairwise_score_unsupervised.csv");
            if (supervised)
                sw = new StreamWriter(graph_dir + @"listextract_varying_pairwise_score_supervised.csv");
            string[] col_per = new string[] { "0", "0.3", "0.5", "0.7", "1.0" };
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_1[l] = get_setting_log("Excel", false, 0, 0.5, 0.5, temp, 1.0 - temp, "ListExtract").Split(',')[16];
                if (supervised)
                    col_1[l] = get_setting_log("Excel", true, 2, 0.5, 0.5, temp, 1.0 - temp, "ListExtract").Split(',')[16];
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_2[l] = get_setting_log("Web", false, 0, 0.5, 0.5, temp, 1.0 - temp, "ListExtract").Split(',')[16];
                if (supervised)
                    col_2[l] = get_setting_log("Web", true, 2, 0.5, 0.5, temp, 1.0 - temp, "ListExtract").Split(',')[16];
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_3[l] = get_setting_log("Wiki", false, 0, 0.5, 0.5, temp, 1.0 - temp, "ListExtract").Split(',')[16];
                if (supervised)
                    col_3[l] = get_setting_log("Wiki", true, 2, 0.5, 0.5, temp, 1.0 - temp, "ListExtract").Split(',')[16];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }
        public void msa_varying_single_cell_score(bool supervised)
        {
            //Supervision Level	Excel	Web	Wiki
            StreamWriter sw = new StreamWriter(graph_dir + @"msa_varying_single_cell_score_unsupervised.csv");
            if (supervised)
                sw = new StreamWriter(graph_dir + @"msa_varying_single_cell_score_supervised.csv");

            string[] col_per = new string[] { "0", "0.3", "0.5", "0.7", "1.0" };
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_1[l] = get_setting_log("Excel", false, 0, temp, 1.0-temp, 0.5, 0.5, "MRA").Split(',')[15];
                if (supervised)
                    col_1[l] = get_setting_log("Excel", true, 2, temp, 1.0 - temp, 0.5, 0.5, "MRA").Split(',')[15];
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_2[l] = get_setting_log("Web", false, 0, temp, 1.0 - temp, 0.5, 0.5, "MRA").Split(',')[15];
                if (supervised)
                    col_2[l] = get_setting_log("Web", true, 2, temp, 1.0 - temp, 0.5, 0.5, "MRA").Split(',')[15];
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_3[l] = get_setting_log("Wiki", false, 0, temp, 1.0 - temp, 0.5, 0.5, "MRA").Split(',')[15];
                if (supervised)
                    col_3[l] = get_setting_log("Wiki", true, 2, temp, 1.0 - temp, 0.5, 0.5, "MRA").Split(',')[15];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }
        public void listextract_varying_single_cell_score(bool supervised)
        {
            //Supervision Level	Excel	Web	Wiki
            StreamWriter sw = new StreamWriter(graph_dir + @"listextract_varying_single_cell_score_unsupervised.csv");
            if (supervised)
                sw = new StreamWriter(graph_dir + @"listextract_varying_single_cell_score_supervised.csv");
            string[] col_per = new string[] { "0", "0.3", "0.5", "0.7", "1.0" };
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_1[l] = get_setting_log("Excel", false, 0, temp, 1.0 - temp, 0.5, 0.5, "ListExtract").Split(',')[16];
                if (supervised)
                    col_1[l] = get_setting_log("Excel", true, 2, temp, 1.0 - temp, 0.5, 0.5, "ListExtract").Split(',')[16];
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_2[l] = get_setting_log("Web", false, 0, temp, 1.0 - temp, 0.5, 0.5, "ListExtract").Split(',')[16];
                if (supervised)
                    col_2[l] = get_setting_log("Web", true, 2, temp, 1.0 - temp, 0.5, 0.5, "ListExtract").Split(',')[16];
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                double temp = Convert.ToDouble(col_per[l]);
                col_3[l] = get_setting_log("Wiki", false, 0, temp, 1.0 - temp, 0.5, 0.5, "ListExtract").Split(',')[16];
                if (supervised)
                    col_3[l] = get_setting_log("Wiki", true, 2, temp, 1.0 - temp, 0.5, 0.5, "ListExtract").Split(',')[16];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }

    }
}
