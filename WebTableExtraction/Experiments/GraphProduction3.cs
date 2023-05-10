using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WebTableExtraction.Experiments
{
    class GraphProduction3
    {

        private static string algorithm_1 = "MRA";
        private static string algorithm_2 = "ListExtract";
        private static string algorithm_3 = "Judie";

        private static string[] algorithms = new string[] { algorithm_1, algorithm_2, algorithm_3 };

        private static string dataset_1 = "Enterprise";
        private static string dataset_2 = "Web";
        private static string dataset_3 = "Wiki";

        private static string[] datasets = new string[] { dataset_1, dataset_2, dataset_3 };


        public static void exp_1_supervised_typical_setting(string inputFile, string outputDir)
        {
            //Generate the table, recording the P,R,F measure of all three algorithms of three setttings
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string[] scope_analyzed = System.IO.File.ReadAllLines(inputFile);

            typical_setting_all_measures(scope_analyzed, true, outputDir + @"supervised_all_measures.csv");


            foreach (string dataset in datasets)
                ave_tokens_fmeasure_dataset(scope_analyzed, dataset, true, outputDir + @"/supervised_ave_token_fmeasure" + dataset + "_.csv");
            foreach (string dataset in datasets)
                top_num_tokens_fmeasure_dataset(scope_analyzed, dataset, true, outputDir + @"/supervised_top_tokens_fmeasure" + dataset + "_.csv");


            foreach (string dataset in datasets)
                num_cols_fmeasure_dataset(scope_analyzed, dataset, true, outputDir + @"/supervised_num_cols_fmeasure" + dataset + "_.csv");

            foreach (string dataset in datasets)
                num_rows_fmeasure_dataset(scope_analyzed, dataset, true, outputDir + @"/supervised_num_rows_fmeasure" + dataset + "_.csv");

            foreach (string dataset in datasets)
                top_num_rows_fmeasure_dataset(scope_analyzed, dataset, true, outputDir + @"/supervised_top_rows_fmeasure" + dataset + "_.csv");

            foreach (string dataset in datasets)
                top_num_cols_fmeasure_dataset(scope_analyzed, dataset, true, outputDir + @"/supervised_top_cols_fmeasure" + dataset + "_.csv");


            MRA_Scoring_Analysis(scope_analyzed, true, outputDir + @"/supervised_scoring_analysis" + "_.csv");
        }

        public static void exp_2_unsupervised_typical_setting(string inputFile, string outputDir)
        {
            //Generate the table, recording the P,R,F measure of all three algorithms of three setttings
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string[] scope_analyzed = System.IO.File.ReadAllLines(inputFile);

            typical_setting_all_measures(scope_analyzed, false, outputDir + @"unsupervised_all_measures.csv");


            foreach(string dataset in datasets)
                ave_tokens_fmeasure_dataset(scope_analyzed, dataset, false, outputDir + @"/unsupervised_ave_token_fmeasure" + dataset + "_.csv");
            foreach (string dataset in datasets)
                top_num_tokens_fmeasure_dataset(scope_analyzed, dataset, false, outputDir + @"/unsupervised_top_tokens_fmeasure" + dataset + "_.csv");



            foreach (string dataset in datasets)
                num_cols_fmeasure_dataset(scope_analyzed, dataset, false, outputDir + @"/unsupervised_num_cols_fmeasure" + dataset + "_.csv");

            foreach (string dataset in datasets)
                num_rows_fmeasure_dataset(scope_analyzed, dataset, false, outputDir + @"/unsupervised_num_rows_fmeasure" + dataset + "_.csv");

            foreach (string dataset in datasets)
                top_num_rows_fmeasure_dataset(scope_analyzed, dataset, false, outputDir + @"/unsupervised_top_rows_fmeasure" + dataset + "_.csv");

            foreach (string dataset in datasets)
                top_num_cols_fmeasure_dataset(scope_analyzed, dataset, false, outputDir + @"/unsupervised_top_cols_fmeasure" + dataset + "_.csv");


            MRA_Scoring_Analysis(scope_analyzed, false, outputDir + @"/unsupervised_scoring_analysis" + "_.csv");
        }

        public static void exp_3_MRA_varying_double_cell_score(string inputFile, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string[] scope_analyzed = System.IO.File.ReadAllLines(inputFile);


            //unsupervised, PMI
            MRA_varying_double_cell_weights(scope_analyzed, false, "PMI", outputDir + @"/unsupervised_double_cell_score_weight_analysis_PMI" + "_.csv");

            //supervised, PMI

            MRA_varying_double_cell_weights(scope_analyzed, true, "PMI", outputDir + @"/supervised_double_cell_score_weight_analysis_PMI" + "_.csv");



            MRA_varying_double_cell_weights(scope_analyzed, false, "Jarrcard", outputDir + @"/unsupervised_double_cell_score_weight_analysis_Jarrcad" + "_.csv");


            MRA_varying_double_cell_weights(scope_analyzed, true, "Jarrcard", outputDir + @"/supervised_double_cell_score_weight_analysis_Jarrcad" + "_.csv");

        }
        public static void exp_4_MRA_varying_super_level(string inputFile, string outputDir)
        {
            //good
            //
            string[] scope_analyzed = System.IO.File.ReadAllLines(inputFile);
            foreach(string dataset in datasets)
            {
                varying_super_level_fmeasure_dataset(scope_analyzed, dataset, outputDir + "/varying_super_leve" + dataset + "_.csv");
            }

           
        }



        private static void typical_setting_all_measures(string[] scope_analyzed, bool supervised, string outputFilePath)
        {
            StreamWriter sw = new StreamWriter(outputFilePath);


            sw.WriteLine("dataset-measure, MRA, ListExtract, Judie");

            foreach(string dataset in datasets)
            {
                //precision line
                StringBuilder sb = new StringBuilder();
                sb.Append(dataset + "-precision,");
                for(int i = 0; i < algorithms.Count(); i++)
                {
                    string cell_1 = null;
                    if(algorithms[i] == "MRA")
                    {
                        cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0, 0, 0.5, 0.5, algorithms[i]);
                        if (!supervised)
                            cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0, 0, 0.5, 0.5, algorithms[i]);
                    }
                    else
                    {
                        cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, algorithms[i]);
                        if (!supervised)
                            cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, algorithms[i]);
                    }
                 

                    string cur_precision = cell_1.Split(',')[i + 9];

                    sb.Append(cur_precision);
                    if (i != algorithms.Count() - 1)
                        sb.Append(",");
                        
                }
                sw.WriteLine(sb.ToString());
               

                //recall line
                 sb = new StringBuilder();
                 sb.Append(dataset + "-recall,");
                 for (int i = 0; i < algorithms.Count(); i++)
                {
                    string cell_1 = null;
                    if (algorithms[i] == "MRA")
                    {
                        cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0, 0, 0.5, 0.5, algorithms[i]);
                        if (!supervised)
                            cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0, 0, 0.5, 0.5, algorithms[i]);
                    }
                    else
                    {
                        cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, algorithms[i]);
                        if (!supervised)
                            cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, algorithms[i]);
                    }

                    string cur_recall = cell_1.Split(',')[i + 12];

                    sb.Append(cur_recall);
                    if (i != algorithms.Count() - 1)
                        sb.Append(",");

                }
                sw.WriteLine(sb.ToString());
               

                //fmeasure line
                 sb = new StringBuilder();
                 sb.Append(dataset + "-fmeasure,");
                for (int i = 0; i < algorithms.Count(); i++)
                {
                    string cell_1 = null;
                    if (algorithms[i] == "MRA")
                    {
                        cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0, 0, 0.5, 0.5, algorithms[i]);
                        if (!supervised)
                            cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0, 0, 0.5, 0.5, algorithms[i]);
                    }
                    else
                    {
                        cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, algorithms[i]);
                        if (!supervised)
                            cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, algorithms[i]);
                    }

                    string cur_fmeasure = cell_1.Split(',')[i + 15];

                    sb.Append(cur_fmeasure);
                    if (i != algorithms.Count() - 1)
                        sb.Append(",");

                }
                sw.WriteLine(sb.ToString());
               
            }


            sw.Close();
        }

        private static void ave_tokens_fmeasure_dataset(string[] scope_analyzed, string dataset, bool supervised, string outputFilePath)
        {
            StreamWriter sw = new StreamWriter(outputFilePath);

            sw.WriteLine("Average Tokens, MRA, ListExtract, Judie");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "1.5", "2", "2.5", "3", "3.5" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log(scope_analyzed,dataset, true, 2, 0, 0, 0.5, 0.5, "MRA").Split(',')[24];
            if(!supervised)
                cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0, 0, 0.5, 0.5, "MRA").Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[25];
            if(!supervised)
                cell_2 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[26];
            if(!supervised)
                cell_3 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[26];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        private static void top_num_tokens_fmeasure_dataset(string[] scope_analyzed, string dataset, bool supervised, string outputFilePath)
        {
            StreamWriter sw = new StreamWriter(outputFilePath);

            sw.WriteLine("Ranked Based on Num Tokens Per Cell, MRA, ListExtract, Judie");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "0.2", "0.4", "0.6", "0.8", "1" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0, 0, 0.5, 0.5, "MRA").Split(',')[21];
            if (!supervised)
                cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0, 0, 0.5, 0.5, "MRA").Split(',')[21];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[22];
            if (!supervised)
                cell_2 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[22];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[23];
            if (!supervised)
                cell_3 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[23];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        private static void num_cols_fmeasure_dataset(string[] scope_analyzed, string dataset, bool supervised, string outputFilePath)
        {
            StreamWriter sw = new StreamWriter(outputFilePath);

            sw.WriteLine("Num Cols, MRA, ListExtract, Judie");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "4", "8", "12", "16", "20" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0, 0, 0.5, 0.5, "MRA").Split(',')[29];
            if (!supervised)
                cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0, 0, 0.5, 0.5, "MRA").Split(',')[29];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[30];
            if (!supervised)
                cell_2 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[30];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[31];
            if (!supervised)
                cell_3 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[31];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        private static void num_rows_fmeasure_dataset(string[] scope_analyzed, string dataset, bool supervised, string outputFilePath)
        {
            StreamWriter sw = new StreamWriter(outputFilePath);

            sw.WriteLine("Num Rows, MRA, ListExtract, Judie");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "10", "20", "30", "40", "50" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0, 0, 0.5, 0.5, "MRA").Split(',')[32];
            if (!supervised)
                cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0, 0, 0.5, 0.5, "MRA").Split(',')[32];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[33];
            if (!supervised)
                cell_2 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[33];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[34];
            if (!supervised)
                cell_3 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[34];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }


        private static void top_num_rows_fmeasure_dataset(string[] scope_analyzed, string dataset, bool supervised, string outputFilePath)
        {
            StreamWriter sw = new StreamWriter(outputFilePath);

            sw.WriteLine("Ranked Based on Num Rows, MRA, ListExtract, Judie");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "0.2", "0.4", "0.6", "0.8", "1" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0, 0, 0.5, 0.5, "MRA").Split(',')[35];
            if (!supervised)
                cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0, 0, 0.5, 0.5, "MRA").Split(',')[35];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[36];
            if (!supervised)
                cell_2 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[36];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[37];
            if (!supervised)
                cell_3 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[37];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        private static void top_num_cols_fmeasure_dataset(string[] scope_analyzed, string dataset, bool supervised, string outputFilePath)
        {
            StreamWriter sw = new StreamWriter(outputFilePath);

            sw.WriteLine("Ranked Based on Num Cols, MRA, ListExtract, Judie");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "0.2", "0.4", "0.6", "0.8", "1" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log(scope_analyzed, dataset, true, 2, 0, 0, 0.5, 0.5, "MRA").Split(',')[38];
            if (!supervised)
                cell_1 = get_setting_log(scope_analyzed, dataset, false, 0, 0, 0, 0.5, 0.5, "MRA").Split(',')[38];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[39];
            if (!supervised)
                cell_2 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[39];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log(scope_analyzed, dataset, true, 2, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[40];
            if (!supervised)
                cell_3 = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[40];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }

        private static void MRA_Scoring_Analysis(string[] scope_analyzed, bool supervised, string outputFilePath)
        {
            StreamWriter sw = new StreamWriter(outputFilePath);
            // Percentage	MRA+EnterpriseTables MRA+WebTables	MRA+WikiTables	

            sw.WriteLine("Top-Percentage, Enterprise, Web, Wiki");
            string[] col_per = new string[] { "0.2", "0.4", "0.6", "0.8", "1.0" };

            string[] col_1 = new string[5];
            string cell_1 = get_setting_log(scope_analyzed,"Enterprise", true, 2, 0, 0, 0.5, 0.5, "MRA").Split(',')[18];
            if(!supervised)
                cell_1 = get_setting_log(scope_analyzed, "Enterprise", false, 0, 0, 0, 0.5, 0.5, "MRA").Split(',')[18];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_2 = new string[5];
            string cell_2 = get_setting_log(scope_analyzed,"Web", true, 2, 0, 0, 0.5, 0.5, "MRA").Split(',')[18];
            if(!supervised)
                cell_2 = get_setting_log(scope_analyzed, "Web", false, 0, 0, 0, 0.5, 0.5, "MRA").Split(',')[18];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            string[] col_3 = new string[5];
            string cell_3 = get_setting_log(scope_analyzed,"Wiki", true, 2, 0, 0, 0.5, 0.5, "MRA").Split(',')[18];
            if(!supervised)
                cell_3 = get_setting_log(scope_analyzed, "Wiki", false, 0, 0, 0, 0.5, 0.5, "MRA").Split(',')[18];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('-')[0];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        
        
        
        
        private static void varying_super_level_fmeasure_dataset(string[] scope_analyzed, string dataset, string outputFilePath)
        {
            StreamWriter sw = new StreamWriter(outputFilePath);

            sw.WriteLine("Supervision Level, MRA, ListExtract, Judie");
            // Average Tokens per Cell	, MRA	ListExtract	Judie
            string[] col_per = new string[] { "-1", "0", "1", "2", "3", "4" };

            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)
                    col_1[l] = get_setting_log(scope_analyzed, dataset, false, 0, 0, 0, 0.5, 0.5, "MRA").Split(',')[15];
                else
                    col_1[l] = get_setting_log(scope_analyzed, dataset, true, Convert.ToInt32(col_per[l]), 0, 0, 0.5, 0.5, "MRA").Split(',')[15];

            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)
                    col_2[l] = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[16];
                else
                    col_2[l] = get_setting_log(scope_analyzed, dataset, true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "ListExtract").Split(',')[16];



            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                if (Convert.ToInt32(col_per[l]) == -1)
                    col_3[l] = get_setting_log(scope_analyzed, dataset, false, 0, 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[17];
                else
                    col_3[l] = get_setting_log(scope_analyzed, dataset, true, Convert.ToInt32(col_per[l]), 0.5, 0.5, 0.5, 0.5, "Judie").Split(',')[17];

            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }
        
        
        
        
        
        
        private static void MRA_varying_double_cell_weights(string[] scope_analyzed, bool supervised, string d_semantic_score_type, string outputFilePath)
        {
            StreamWriter sw = new StreamWriter(outputFilePath);
            // Percentage	MRA+EnterpriseTables MRA+WebTables	MRA+WikiTables	

            sw.WriteLine("Weights, Enterprise, Web, Wiki");
            string[] col_per = new string[] { "0", "0.3", "0.5", "0.7", "1.0" };
            int numLines = col_per.Length;
            string[] col_1 = new string[numLines];
            for (int i = 0; i < numLines; i++)
            {
                double temp = Convert.ToDouble(col_per[i]);
                string cell_1 = get_setting_log(scope_analyzed, "Enterprise", true, 2, 0, 0, temp, 1.0 - temp, "MRA", d_semantic_score_type);
                if (!supervised)
                    cell_1 = get_setting_log(scope_analyzed, "Enterprise", false, 0, 0, 0, temp, 1.0 - temp, "MRA", d_semantic_score_type);
                col_1[i] = cell_1.Split(',')[15];
            }

            string[] col_2 = new string[numLines];
            for (int i = 0; i < numLines; i++)
            {
                double temp = Convert.ToDouble(col_per[i]);
                string cell_2 = get_setting_log(scope_analyzed, "Web", true, 2, 0, 0, temp, 1.0 - temp, "MRA", d_semantic_score_type);
                if (!supervised)
                    cell_2 = get_setting_log(scope_analyzed, "Web", false, 0, 0, 0, temp, 1.0 - temp, "MRA", d_semantic_score_type);

                col_2[i] = cell_2.Split(',')[15];
            }

            string[] col_3 = new string[numLines];
           
            for (int i = 0; i < numLines; i++)
            {
                double temp = Convert.ToDouble(col_per[i]);
                string cell_3 = get_setting_log(scope_analyzed, "Wiki", true, 2, 0, 0, temp, 1.0 - temp, "MRA", d_semantic_score_type);
                if (!supervised)
                    cell_3 = get_setting_log(scope_analyzed, "Wiki", false, 0, 0, 0, temp, 1.0 - temp, "MRA", d_semantic_score_type);

                col_3[i] = cell_3.Split(',')[15];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }

            sw.Close();
        }
        
        
        
        
        
        
        
        private static string get_setting_log(string[] scope_analyzed, 
           string dataset, bool numColsGiven, int numExamples,
           double s_type_weight,
           double s_table_corpus,
           double d_syntatic_weight,
           double d_table_corpus_pmi,
           string algorithm)
        {
            foreach (string log in scope_analyzed)
            {
                string[] split = log.Split(',');

                if (split[0].Contains(dataset)
                    && Convert.ToBoolean(split[2]) == numColsGiven
                    && Convert.ToInt32(split[3]) == numExamples
                    && Convert.ToDouble(split[4]) == Math.Round(s_type_weight, 3)
                    && Convert.ToDouble(split[6]) == Math.Round(s_table_corpus, 3)
                    && Convert.ToDouble(split[7]) == Math.Round(d_syntatic_weight, 3)
                    && Convert.ToDouble(split[8]) == Math.Round(d_table_corpus_pmi, 3)
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
                  + @"< [1-1.5] :0>< [1.5-2.0] :0>< [2.0-2.5] :0>< [2.5-3.0] :0>< [3.0-100] :0>" + ","
                  + 1 + ","
                  + 1 + ","
                  + @"< [1-5) :0>< [5-9) :0>< [9-13) :0>< [13-17) :0>< [17-21) :0>" + ","
                  + @"< [1-5) :0>< [5-9) :0>< [9-13) :0>< [13-17) :0>< [17-21) :0>" + ","
                  + @"< [1-5) :0>< [5-9) :0>< [9-13) :0>< [13-17) :0>< [17-21) :0>" + ","
                  + @"< [1-10) :0>< [11-20) :0>< [21-30) :0>< [31-40) :0>< [41-50) :0>" + ","
                  + @"< [1-10) :0>< [11-20) :0>< [21-30) :0>< [31-40) :0>< [41-50) :0>" + ","
                  + @"< [1-10) :0>< [11-20) :0>< [21-30) :0>< [31-40) :0>< [41-50) :0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                  + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>"
                  ;
        }

        private static string get_setting_log(string[] scope_analyzed,
          string dataset, bool numColsGiven, int numExamples,
          double s_type_weight,
          double s_table_corpus,
          double d_syntatic_weight,
          double d_table_corpus_pmi,
          string algorithm, 
            string d_semantic_score_type)
        {
            foreach (string log in scope_analyzed)
            {
                string[] split = log.Split(',');

                if (split[0].Contains(dataset)
                    && Convert.ToBoolean(split[2]) == numColsGiven
                    && Convert.ToInt32(split[3]) == numExamples
                    && Convert.ToDouble(split[4]) == Math.Round(s_type_weight, 3)
                    && Convert.ToDouble(split[6]) == Math.Round(s_table_corpus, 3)
                    && Convert.ToDouble(split[7]) == Math.Round(d_syntatic_weight, 3)
                    && Convert.ToDouble(split[8]) == Math.Round(d_table_corpus_pmi, 3)
                    && split[0].Contains(d_semantic_score_type))
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
                   + @"< [1-1.5] :0>< [1.5-2.0] :0>< [2.0-2.5] :0>< [2.5-3.0] :0>< [3.0-100] :0>" + ","
                   + 1 + ","
                   + 1 + ","
                   + @"< [1-5) :0>< [5-9) :0>< [9-13) :0>< [13-17) :0>< [17-21) :0>" + ","
                   + @"< [1-5) :0>< [5-9) :0>< [9-13) :0>< [13-17) :0>< [17-21) :0>" + ","
                   + @"< [1-5) :0>< [5-9) :0>< [9-13) :0>< [13-17) :0>< [17-21) :0>" + ","
                   + @"< [1-10) :0>< [11-20) :0>< [21-30) :0>< [31-40) :0>< [41-50) :0>" + ","
                   + @"< [1-10) :0>< [11-20) :0>< [21-30) :0>< [31-40) :0>< [41-50) :0>" + ","
                   + @"< [1-10) :0>< [11-20) :0>< [21-30) :0>< [31-40) :0>< [41-50) :0>" + ","
                   + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                   + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                   + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                   + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                   + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>" + ","
                   + @"< 0.2:0>< 0.4:0>< 0.6:0>< 0.8:0>< 1:0>"
                   ;
        }
    }
}
