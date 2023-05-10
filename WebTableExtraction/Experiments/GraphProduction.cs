using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebTableExtraction.Experiments
{
    class GraphProduction
    {

        /*
         * This is for drawing graphs using analyzed log file
         */ 
        Dictionary<string, string[]> log2exps = new Dictionary<string, string[]>();
        string graph_dir;

        public GraphProduction( List<string> allLogAnalyzed_filePaths, string graph_dir)
        {
            this.graph_dir = graph_dir;
            foreach(string logAnalyzed_filePath in allLogAnalyzed_filePaths)
            {
                string[] exps = System.IO.File.ReadAllLines(logAnalyzed_filePath);
                
                Console.WriteLine(exps[0]);
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
                log2exps[logAnalyzed_filePath] = exps;
            }
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

            msa_varying_pairwise_score();
            listextract_varying_pairwise_score();

            listextract_varying_singlecell_score();
        }

        private string get_Excel_Tables_log()
        {
            foreach(string s in log2exps.Keys)
            {
                if (s.Contains(@"Excel"))
                    return s;
            }
            return null;
        }
        private string get_Web_Tables_log()
        {
            foreach (string s in log2exps.Keys)
            {
                if (s.Contains(@"General_WebTables"))
                    return s;
            }
            return null;
        }
        private string get_Wiki_Tables_log()
        {
            foreach (string s in log2exps.Keys)
            {
                if (s.Contains(@"WebTablesCorpus_200k"))
                    return s;
            }
            return null;
        }

        public void msa_top_percentage_fmeasure_supervised()
        {
            StreamWriter sw = new StreamWriter(graph_dir + "msa_top_percentage_fmeasure_supervised.csv");
            // Percentage	MRA+ExcelTables MRA+WebTables	MRA+WikiTables	

            string[] col_per = new string[] { "0.2", "0.4", "0.6", "0.8", "1.0"};

            string[] col_1 = new string[5];
            string cell_1 = log2exps[get_Excel_Tables_log()][10].Split(',')[18];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = log2exps[get_Web_Tables_log()][10].Split(',')[18];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = log2exps[get_Wiki_Tables_log()][10].Split(',')[18];
            string[] cell_3_split = cell_3.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_3[i] = cell_3_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            for (int l = 0; l < col_per.Length; l++ )
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
            string cell_1 = log2exps[get_Excel_Tables_log()][1].Split(',')[18];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = log2exps[get_Web_Tables_log()][1].Split(',')[18];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = log2exps[get_Wiki_Tables_log()][1].Split(',')[18];
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
            string cell_1 = log2exps[get_Excel_Tables_log()][10].Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = log2exps[get_Excel_Tables_log()][11].Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = log2exps[get_Excel_Tables_log()][12].Split(',')[26];
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
            string cell_1 = log2exps[get_Web_Tables_log()][10].Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = log2exps[get_Web_Tables_log()][11].Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = log2exps[get_Web_Tables_log()][12].Split(',')[26];
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
            string cell_1 = log2exps[get_Wiki_Tables_log()][10].Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = log2exps[get_Wiki_Tables_log()][11].Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = log2exps[get_Wiki_Tables_log()][12].Split(',')[26];
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
            string cell_1 = log2exps[get_Excel_Tables_log()][1].Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = log2exps[get_Excel_Tables_log()][2].Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = log2exps[get_Excel_Tables_log()][3].Split(',')[26];
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
            string cell_1 = log2exps[get_Web_Tables_log()][1].Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = log2exps[get_Web_Tables_log()][2].Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = log2exps[get_Web_Tables_log()][3].Split(',')[26];
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
            string cell_1 = log2exps[get_Wiki_Tables_log()][1].Split(',')[24];
            string[] cell_1_split = cell_1.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_1[i] = cell_1_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_2 = new string[5];
            string cell_2 = log2exps[get_Wiki_Tables_log()][2].Split(',')[25];
            string[] cell_2_split = cell_2.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 5; i++)
            {
                col_2[i] = cell_2_split[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string[] col_3 = new string[5];
            string cell_3 = log2exps[get_Wiki_Tables_log()][3].Split(',')[26];
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
            StreamWriter sw = new  StreamWriter(graph_dir + @"msa_varying_supervision_level.csv");
            string[] col_per = new string[] { "-1", "0", "1", "2", "3", "4" };
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++ )
            {
                col_1[l] = log2exps[get_Excel_Tables_log()][ 1 + l * 3].Split(',')[15];
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_2[l] = log2exps[get_Web_Tables_log()][1 + l * 3].Split(',')[15];
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_3[l] = log2exps[get_Wiki_Tables_log()][1 + l * 3].Split(',')[15];
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
            string[] col_per = new string[] { "-1", "0", "1", "2", "3", "4"};
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_1[l] = log2exps[get_Excel_Tables_log()][2 + l * 3].Split(',')[16];
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_2[l] = log2exps[get_Web_Tables_log()][2 + l * 3].Split(',')[16];
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_3[l] = log2exps[get_Wiki_Tables_log()][2 + l * 3].Split(',')[16];
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
                col_1[l] = log2exps[get_Excel_Tables_log()][3 + l * 3].Split(',')[17];
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_2[l] = log2exps[get_Web_Tables_log()][3 + l * 3].Split(',')[17];
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_3[l] = log2exps[get_Wiki_Tables_log()][3 + l * 3].Split(',')[17];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }
    
    
    
        
        public void msa_varying_pairwise_score()
        {
            //Supervision Level	Excel	Web	Wiki
            StreamWriter sw = new StreamWriter(graph_dir + @"msa_varying_pairwise_score.csv");
            string[] col_per = new string[] { "0", "0.3", "0.5", "0.7", "1.0"};
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_1[l] = log2exps[get_Excel_Tables_log()][19 + l].Split(',')[15];
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_2[l] = log2exps[get_Web_Tables_log()][19 + l].Split(',')[15];
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_3[l] = log2exps[get_Wiki_Tables_log()][19 + l].Split(',')[15];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }
        public void listextract_varying_pairwise_score()
        {
            //Supervision Level	Excel	Web	Wiki
            StreamWriter sw = new StreamWriter(graph_dir + @"listextract_varying_pairwise_score.csv");
            string[] col_per = new string[] { "0", "0.3", "0.5", "0.7", "1.0" };
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_1[l] = log2exps[get_Excel_Tables_log()][19 + l].Split(',')[16];
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_2[l] = log2exps[get_Web_Tables_log()][19 + l].Split(',')[16];
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_3[l] = log2exps[get_Wiki_Tables_log()][19 + l].Split(',')[16];
            }

            for (int l = 0; l < col_per.Length; l++)
            {
                string line = col_per[l] + "," + col_1[l] + "," + col_2[l] + "," + col_3[l];
                sw.WriteLine(line);
            }
            sw.Close();
        }
        public void listextract_varying_singlecell_score()
        {
            //Supervision Level	Excel	Web	Wiki
            StreamWriter sw = new StreamWriter(graph_dir + @"listextract_varying_singlecell_score.csv");
            string[] col_per = new string[] { "0", "0.3", "0.5", "0.7", "1.0" };
            int numLines = col_per.Length;

            string[] col_1 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_1[l] = log2exps[get_Excel_Tables_log()][24 + l].Split(',')[16];
            }
            string[] col_2 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_2[l] = log2exps[get_Web_Tables_log()][24 + l].Split(',')[16];
            }
            string[] col_3 = new string[numLines];
            for (int l = 0; l < numLines; l++)
            {
                col_3[l] = log2exps[get_Wiki_Tables_log()][24 + l].Split(',')[16];
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
