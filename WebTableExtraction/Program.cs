using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTableExtraction.Utils;
using WebTableExtraction.Extraction;
using WebTableExtraction.Experiments;
using System.Text.RegularExpressions;
using WebTableExtraction.Baseline;
using System.Configuration;
using System.Threading;

using WebTableExtraction.Judie;

namespace WebTableExtraction
{
    class Program
    {
       


        private static void design_running_example()
        {
            Parameter.singleCellOccurrenceFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_joined.txt";
            Parameter.doubleCellOccurrenceFilepath = @"F:\Running Results\NewStats\doubleCell_occur_combined.txt_joined.txt";
            Parameter.singleCellLanguageModelFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_language_model.txt";
            Parameter.kb_attri2valuesFilePath = @"F:\Running Results\NewStats\kb_attri_2_values.txt";
            Parameter.kb_value2attrisFilePath = @"F:\Running Results\NewStats\kb_value_2_attris.txt";
            Parameter.design_running_example = true;

            int tag = 1;
            List<Line> lines = new List<Line>();
            if(tag == 1)
            {
                /*
                Rashard Mendenhall||Arizona Cardinals||1 year/$2.5 million
                Keenan Lewis||New Orleans Saints||5 year/$25.5 million
                Ryan Mundy||New York Giants||1 year/$780,000
                Will Allen||Dallas Cowboys||1 year/$905,000
                Max Starks||San Diego Chargers||1 year/$840,000
                 * */

                lines = new List<Line>()
                {
                    new Line(@"Rashard Mendenhall Arizona Cardinals 1 year/$2.5 million"),
                    new Line(@"Keenan Lewis New Orleans Saints 5 year/$25.5 million"),
                    //new Line(@"Ryan Mundy New York Giants 1 year/$780,000"),
                    new Line(@"Ryan Mundy New York  1 year/$780,000"),
               
                };
            }
            else if(tag == 2)
            {
                /*
                   May On Motors||Virgin Books||2007
                    Notes From The Hard Shoulder||Virgin Books||2007
                    James May's 20th Century||Hodder & Stoughton||2007
                    James May's Magnificent Machines||Hodder & Stoughton||2008
                    James May's Car Fever (P/B)||Hodder & Stoughton||2009
                    James May's Car Fever (H/B)||Hodder & Stoughton||2009
                    James May's Toy Stories||Hodder & Stoughton||2009


                 */
                lines = new List<Line>()
                {
                    new Line(@"May On Motors Virgin Books 2007"),
                    new Line(@"James May's 20th Century Hodder & Stoughton 2007"),
                    new Line(@"James May's Toy Stories  2009"),
                };
            }
            else if(tag == 3)
            {
                /*
                   Moreno (Buenos Aires) 149,317
Concordia (Entre Ríos) 147,046
La Rioja (La Rioja) 146,418
Río Cuarto (Córdoba) 144,140
San Fernando del Valle de Catamarca (Catamarca) 140,741
                 * 
                 */
                lines = new List<Line>()
                {
                    new Line(@"Moreno (Buenos Aires) 149,317"),
                    new Line(@"La Rioja (La Rioja) 146,418"),
                    new Line(@"Río Cuarto (Córdoba) 144,140"),
                };
            };
           

            Parameter.numColsGiven = true;
            Parameter.numExamples = 0;
            Parameter.s_languageWeight = 0;
            Parameter.s_occurWeight = 0;
            Parameter.s_typeWeight = 0;
            Parameter.d_occurWeight = 0.5;
            Parameter.d_syntacticWeight = 0.5;
            Parameter.max_num_threads_per_machine = 0;

            Table aligned_table = null;
           
            //Do the MSA
            MSAAppro msaAppro = new MSAAppro(lines, new Dictionary<Line,Record>());
            if (Parameter.numColsGiven)
                msaAppro.alignAStar_with_numcols(3);
            else
                msaAppro.alignAStar_without_numcols();
            aligned_table = msaAppro.getExtracted_Table();


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("*********************");
            Console.WriteLine("The aligned table is: " + aligned_table.toString());
            Console.WriteLine("*********************");
        }




        static void Main(string[] args)
        {

           


            //FilterLists.filter(@"F:\WebLists\2.partial", @"F:\WebLists\Dump2\");
            Parameter.setWhichExp(100);
            Benchmark benchList = new Benchmark(true, true, true, int.MaxValue);
            return;

            Parameter.singleCellOccurrenceFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_joined.txt";
            Parameter.doubleCellOccurrenceFilepath = @"F:\Running Results\NewStats\doubleCell_occur_combined.txt_joined.txt";
            Parameter.singleCellLanguageModelFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_language_model.txt";
            Parameter.kb_attri2valuesFilePath = @"F:\Running Results\NewStats\kb_attri_2_values.txt";
            Parameter.kb_value2attrisFilePath = @"F:\Running Results\NewStats\kb_value_2_attris.txt";

            //exp_scalability();
            //exp_numRows_on_quality();

            DatasetAnalysis da = new DatasetAnalysis(@"F:\Running Results\June 27\General_WebTablesCorpus_10k\Sequenceall_sequences_tabulars.txt");

            //design_running_example();

            //RunningExample re = new RunningExample(@"F:\Running Results\June 27\WebTablesCorpus_200k\Sequenceall_sequences_tabulars.txt");

            //generate_result_analysis();
            //draw_graphs();

            //generate_test();

            //quality_analysis();
            

            //DataPrep.requiredStats_add_empty_strings();

            //test_paralllization_dummy();

            Console.WriteLine("I HAVE FINISHED ALL TASKS");
            Console.ReadLine();
        }
       



        private static void draw_graphs()
        {
            string graph_dir = @"F:\Running Results\July 14\Graphs\";

            GraphProduction3.exp_1_supervised_typical_setting(@"F:\Running Results\July 14\supervised_typical_one.csv", graph_dir);
            //GraphProduction3.exp_2_unsupervised_typical_setting(@"F:\Running Results\July 14\unsupervised_typical_one.csv", graph_dir);
            
            //GraphProduction3.exp_3_MRA_varying_double_cell_score(@"F:\Running Results\June 30\MRA_varying_double_cell_weight.csv", graph_dir);
            //GraphProduction3.exp_4_MRA_varying_super_level(@"F:\Running Results\June 30\varying_super_level.csv", graph_dir);
            return;

        }

        private static void exp_scalability()
        {
            /*
             Parameter.singleCellOccurrenceFilePath = @"D:\xuchu\TableStats\combined for yeye\singleCell_occur_combined.txt_numwildcardFalse_joined.txt";
             Parameter.doubleCellOccurrenceFilepath = @"D:\xuchu\TableStats\combined for yeye\doubleCell_occur_combined.txt_numwildcardFalse_joined.txt";
             Parameter.singleCellLanguageModelFilePath = @"D:\xuchu\TableStats\combined for yeye\singleCell_occur_combined.txt_language_model.txt";
             Parameter.kb_attri2valuesFilePath = @"D:\xuchu\TableStats\combined for xu\kb_attri_2_values.txt";
             Parameter.kb_value2attrisFilePath = @"D:\xuchu\TableStats\combined for xu\kb_value_2_attris.txt";

           

             Scalability scal = new Scalability(@"D:\xuchu\TableStats\Graphs\");
             string dataset = @"D:\xuchu\TableStats\WebTablesCorpus_200k\Sequenceall_sequences_tabulars.txt";
             scal.Varying_num_cols(dataset);
             scal.Varying_num_rows(dataset);*/


            Parameter.singleCellOccurrenceFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_joined.txt";
            Parameter.doubleCellOccurrenceFilepath = @"F:\Running Results\NewStats\doubleCell_occur_combined.txt_joined.txt";
            Parameter.singleCellLanguageModelFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_language_model.txt";
            Parameter.kb_attri2valuesFilePath = @"F:\Running Results\NewStats\kb_attri_2_values.txt";
            Parameter.kb_value2attrisFilePath = @"F:\Running Results\NewStats\kb_value_2_attris.txt";

            Scalability scal = new Scalability(@"F:\Running Results\ScalabilityGraphs\");
            string dataset = @"F:\Running Results\June 27\WebTablesCorpus_200k\Sequenceall_sequences_tabulars.txt";
            //scal.Varying_num_cols(dataset);
            scal.Varying_num_rows(dataset);

        }
        private static void exp_numRows_on_quality()
        {
            /*
             Parameter.singleCellOccurrenceFilePath = @"D:\xuchu\TableStats\combined for yeye\singleCell_occur_combined.txt_numwildcardFalse_joined.txt";
             Parameter.doubleCellOccurrenceFilepath = @"D:\xuchu\TableStats\combined for yeye\doubleCell_occur_combined.txt_numwildcardFalse_joined.txt";
             Parameter.singleCellLanguageModelFilePath = @"D:\xuchu\TableStats\combined for yeye\singleCell_occur_combined.txt_language_model.txt";
             Parameter.kb_attri2valuesFilePath = @"D:\xuchu\TableStats\combined for xu\kb_attri_2_values.txt";
             Parameter.kb_value2attrisFilePath = @"D:\xuchu\TableStats\combined for xu\kb_value_2_attris.txt";

           

             Scalability scal = new Scalability(@"D:\xuchu\TableStats\Graphs\");
             string dataset = @"D:\xuchu\TableStats\WebTablesCorpus_200k\Sequenceall_sequences_tabulars.txt";
             scal.Varying_num_cols(dataset);
             scal.Varying_num_rows(dataset);*/


            Parameter.singleCellOccurrenceFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_joined.txt";
            Parameter.doubleCellOccurrenceFilepath = @"F:\Running Results\NewStats\doubleCell_occur_combined.txt_joined.txt";
            Parameter.singleCellLanguageModelFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_language_model.txt";
            Parameter.kb_attri2valuesFilePath = @"F:\Running Results\NewStats\kb_attri_2_values.txt";
            Parameter.kb_value2attrisFilePath = @"F:\Running Results\NewStats\kb_value_2_attris.txt";

           
            string dataset = @"F:\Running Results\June 27\WebTablesCorpus_200k\Sequenceall_sequences_tabulars.txt";
            
            NumRowsonQuality numRowsonQualtiy = new NumRowsonQuality(@"F:\Running Results\ScalabilityGraphs\");
            numRowsonQualtiy.Varying_num_rows(dataset,true);

            numRowsonQualtiy.Varying_num_rows(dataset, false);

           
        }



       

      
        public static void test_paralllization_dummy()
        {
           

            DateTime t1 = DateTime.Now;

            for (int i = 0; i < 1024; i++)
                computation("key1", "key2");

            DateTime t2 = DateTime.Now;

            Console.WriteLine("NO Parallel: Time " + (t2 - t1).TotalMilliseconds);
            int[] numThreads_all = new int[] { 2, 4,8,16 ,32,64,128,256,512,1024};
           
            foreach (int numThreads in numThreads_all)
            {
                List<Thread> threads = new List<Thread>();


                DateTime t4 = DateTime.Now;
                Parameter.max_num_threads_per_machine = numThreads;

                for(int i = 0; i < numThreads; i++)
                {
                    ThreadStart starter = delegate { helper(numThreads); };
                    Thread thread = new Thread(starter);
                    thread.Start();
                    threads.Add(thread);
                }
                foreach (Thread thread in threads)
                    thread.Join();
               

                DateTime t3 = DateTime.Now;
                Console.WriteLine("Parallel" + Parameter.max_num_threads_per_machine + " time: " + (t3 - t4).TotalMilliseconds);
            }

        }

        public static void helper(int numThreads)
        {
            for (int i = 0; i < 1024 / numThreads; i++)
            {
                computation("key1  ", "key2");
            }
        }


        public static void test_paralllization()
        {
            List<Line> lines = new List<Line>();
            for(int i = 0; i < 5; i++)
            {
                lines.Add(new Line("aaaaaaaaaabbbbbbbbbbccccccccc" + i));
            }

            DateTime t1 = DateTime.Now;
            init_double_cell_score_cache(lines);
            
            DateTime t2 = DateTime.Now;

            Console.WriteLine("NO Parallel: Time " + (t2 - t1).TotalMilliseconds);
            int[] numThreads_all = new int[] { 2, 4, 6, 8, 10 };
            foreach(int numThreads in numThreads_all)
            {
                DateTime t4 = DateTime.Now;
                Parameter.max_num_threads_per_machine = numThreads;
                init_double_cell_score_cache_parallel(lines);
                DateTime t3 = DateTime.Now;
                Console.WriteLine("Parallel" + Parameter.max_num_threads_per_machine + " time: " + (t3 - t4).TotalMilliseconds);
            }

           
           

        }

        public static void init_double_cell_score_cache(List<Line> lines)
        {
            DateTime t1 = DateTime.Now;
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    Line line1 = lines[i];
                    Line line2 = lines[j];

                    for (int k_1 = 0; k_1 < line1.getNumWords(); k_1++)
                    {
                        int maxEnd_1 = k_1 + Parameter.maxTokensPerColumn;
                        for (int l_1 = k_1; l_1 < line1.getNumWords() && l_1 < maxEnd_1; l_1++)
                        {

                            string key = line1.getCellValue(k_1, l_1);

                            for (int k_2 = 0; k_2 < line2.getNumWords(); k_2++)
                            {
                                int maxEnd_2 = k_2 + Parameter.maxTokensPerColumn;
                                for (int l_2 = k_2; l_2 < line2.getNumWords() && l_2 < maxEnd_2; l_2++)
                                {
                                    string key2 = line2.getCellValue(k_2, l_2);

                                    computation(key, key2);
                                }
                            }
                        }
                    }
                }
            }
            DateTime t2 = DateTime.Now;
            //Console.WriteLine("finished loading local scoring, initialize double cell cache: " + (t2 - t1).TotalMilliseconds);
        }

        public static void init_double_cell_score_cache_parallel(List<Line> lines)
        {

           
            DateTime t1 = DateTime.Now;

            //distribute anchors to threads
            List<List<string>> distribute_lines = new List<List<string>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<string> this_anchors = new List<string>();
                distribute_lines.Add(this_anchors);
            }
            int overall_count = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    int which_thread = overall_count % Parameter.max_num_threads_per_machine;
                    distribute_lines[which_thread].Add(i + "," + j);
                    overall_count++;
                }

            }
            //create the threads, and the returning data
            Dictionary<Thread, Dictionary<string, double>> thread2_lines = new Dictionary<Thread, Dictionary<string, double>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<string> lines_one_thread = distribute_lines[i];
                Dictionary<string, double> doubleCell2Score_one_thread = new Dictionary<string, double>();

                List<Line> linesCopy = new List<Line>();
                foreach(Line line in lines)
                {
                    linesCopy.Add(new Line(line.ToString()));
                }

                ThreadStart starter = delegate { init_double_cell_score_cache_one_thread(lines_one_thread, linesCopy, doubleCell2Score_one_thread); };
                Thread thread = new Thread(starter);
                thread.Start();
                thread2_lines[thread] = doubleCell2Score_one_thread;
            }
            //wait for the thread to finish
            foreach (Thread thread in thread2_lines.Keys)
            {
                thread.Join();
            }
            //collect the result from each thread

            foreach (Thread thread in thread2_lines.Keys)
            {
                foreach (string value in thread2_lines[thread].Keys)
                {
                    double score = thread2_lines[thread][value];
                   
                }
            }
            DateTime t2 = DateTime.Now;
            //Console.WriteLine("finished loading local scoring, initialize double cell cache in parallel using: " + Parameter.max_num_threads_per_machine + " threads:" + (t2 - t1).TotalMilliseconds);
        }
        public static void init_double_cell_score_cache_one_thread(List<string> lines_one_thread,
            List<Line> lines, Dictionary<string, double> doubleCell2Score_one_thread)
        {

            foreach (string linepair in lines_one_thread)
            {
                int index_1 = Convert.ToInt32(linepair.Split(',')[0]);
                int index_2 = Convert.ToInt32(linepair.Split(',')[1]);
                Line line1 = lines[index_1];
                Line line2 = lines[index_2];
                for (int k_1 = 0; k_1 < line1.getNumWords(); k_1++)
                {
                    int maxEnd_1 = k_1 + Parameter.maxTokensPerColumn;
                    for (int l_1 = k_1; l_1 < line1.getNumWords() && l_1 < maxEnd_1; l_1++)
                    {

                        string key = line1.getCellValue(k_1, l_1);

                        for (int k_2 = 0; k_2 < line2.getNumWords(); k_2++)
                        {
                            int maxEnd_2 = k_2 + Parameter.maxTokensPerColumn;
                            for (int l_2 = k_2; l_2 < line2.getNumWords() && l_2 < maxEnd_2; l_2++)
                            {
                                string key2 = line2.getCellValue(k_2, l_2);
                                computation(key,key2);
                            }
                        }
                    }
                }
            }

        }
        static int global_count = 1;
        private static void computation(string value1, string value2)
        {
            int sum = 0;
            for(int i = 0; i < 5000000; i++)
            {
                sum += i;
            }
        }
    }
}
