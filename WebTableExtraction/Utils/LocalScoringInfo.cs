using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
namespace WebTableExtraction.Utils
{

    /*
     * This class is specific for each dataset.
     */ 

    public class LocalScoringInfo
    {


        private Dictionary<string, int> singleCell2Type = new Dictionary<string, int>();
        private static double language_model_normalization_factor = 1;
        private static double table_corpus_support_normalization_factor = 1;

        //private Dictionary<string, double> cache_singleCell_2_occur = new Dictionary<string, double>();
        //private Dictionary<string, double> cache_doubleCell_2_occur = new Dictionary<string, double>();


        private double my_single_score_weight = 1.0;

        private  Dictionary<string, double> singleCell2Score = new Dictionary<string, double>();
        private  Dictionary<string, double> doubleCell2Score = new Dictionary<string, double>();

        public LocalScoringInfo(List<Line> lines, int numExamples)
        {
            Console.WriteLine("-------------------------------------------");
            //init the relative weight of singleScores
            if (lines.Count() > 1)
                my_single_score_weight = Parameter.singleScoreWeight * (1.0 / (lines.Count() - 1));
            else
                my_single_score_weight = Parameter.singleScoreWeight * 1.0;

            System.Diagnostics.Debug.Assert(lines.Count() >= numExamples);
            if (lines.Count() - numExamples > 1 && numExamples > 0)
                Parameter.exampleWeight = ((double)(lines.Count() - numExamples - 1)) / numExamples;
            else
                Parameter.exampleWeight = 1.0;

            if (Parameter.exampleWeight < 1.0)
                Parameter.exampleWeight = 1.0;
            

            //init the types information for each cell, once and for all
            foreach (Line line in lines)
            {
                for (int i = 0; i < line.getNumWords(); i++)
                {
                    for (int j = i; j < line.getNumWords(); j++)
                    {

                        string key = line.getCellValue(i, j);
                      
                        int type = GlobalScoringInfo.getType(key);
                        if (type != 0)
                        {
                            singleCell2Type[key] = type;
                        }


                        double occCount = GlobalScoringInfo.getOccurCount(key);
                        if (occCount > table_corpus_support_normalization_factor)
                            table_corpus_support_normalization_factor = occCount;

                        double table_prob = occCount / GlobalScoringInfo.totalStringOccurrence;
                        double document_prob = GlobalScoringInfo.getStringLangugeModelProb(key);
                        if(table_prob > 0 && document_prob > 0)
                        {
                            double result = table_prob / document_prob;
                            if (result > language_model_normalization_factor)
                            {
                                language_model_normalization_factor = result;

                            }
                        }
                            
                    }
                }
               
            }
            
            
            Console.WriteLine("finished loading local scoring: " );

            //init_double_cell_score_cache(lines);
            
        }




        public void init_double_cell_score_cache(List<Line> lines)
        {
            DateTime t1 = DateTime.Now;
            for(int i = 0; i < lines.Count; i++)
            {
                for(int j = i+1; j < lines.Count; j++)
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
                                    double cur = doubleCellScore_init_cache(key, key2,doubleCell2Score);
                                    
                                }
                            }
                        }
                    }
                }
            }
            DateTime t2 = DateTime.Now;
            Console.WriteLine("finished loading local scoring, initialize double cell cache: " + (t2 - t1).TotalMilliseconds);
        }

       



        public void init_double_cell_score_cache_parallel(List<Line> lines)
        {

            if (Parameter.max_num_threads_per_machine == 0)
            {
                init_double_cell_score_cache(lines);
                return;
            }

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
                ThreadStart starter = delegate { init_double_cell_score_cache_one_thread(lines_one_thread, lines, doubleCell2Score_one_thread); };
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
            DateTime t3 = DateTime.Now;
            foreach (Thread thread in thread2_lines.Keys)
            {
                foreach (string value in thread2_lines[thread].Keys)
                {
                    double score = thread2_lines[thread][value];
                    doubleCell2Score[value] = score;
                }
            }

            DateTime t2 = DateTime.Now;
            Console.WriteLine("finished loading local scoring, initialize double cell cache in parallel using: " + Parameter.max_num_threads_per_machine
                + " threads:" + (t2 - t1).TotalMilliseconds + " with time spent in collecting results: " + (t2 - t3).TotalMilliseconds);
        }

      
        public void init_double_cell_score_cache_one_thread(List<string> lines_one_thread,
            List<Line> lines, Dictionary<string, double> doubleCell2Score_one_thread)
        {

            
            //distribute anchors to threads
            /*
            List<List<string>> distribute_lines = new List<List<string>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<string> this_anchors = new List<string>();
                distribute_lines.Add(this_anchors);
            }
            int overall_count = 0;
            foreach(string line in lines_one_thread)
            {
               int which_thread = overall_count % Parameter.max_num_threads_per_machine;
                    distribute_lines[which_thread].Add(line);
                    overall_count++;
                

            }
            //create the threads, and the returning data
            Dictionary<Thread, Dictionary<string, double>> thread2_lines = new Dictionary<Thread, Dictionary<string, double>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<string> lines_one_thread_further = distribute_lines[i];
                Dictionary<string, double> doubleCell2Score_one_thread_further = new Dictionary<string, double>();
                ThreadStart starter = delegate { init_double_cell_score_cache_one_thread_futher_split(lines_one_thread_further, lines, doubleCell2Score_one_thread_further); };
                Thread thread = new Thread(starter);
                thread.Start();
                thread2_lines[thread] = doubleCell2Score_one_thread_further;
            }
            //wait for the thread to finish
            foreach (Thread thread in thread2_lines.Keys)
            {
                thread.Join();

            }
            //collect the result from each thread
            DateTime t3 = DateTime.Now;
            foreach (Thread thread in thread2_lines.Keys)
            {
                foreach (string value in thread2_lines[thread].Keys)
                {
                    double score = thread2_lines[thread][value];
                    doubleCell2Score_one_thread[value] = score;
                }
            }*/
          
            
            for (int i = 0; i < lines_one_thread.Count; i++ )
            {
                string linepair = lines_one_thread[i];
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
                                double cur = doubleCellScore_init_cache(key, key2, doubleCell2Score_one_thread);

                            }
                        }
                    }
                }
            }
             
            
        }


        public void init_double_cell_score_cache_one_thread_futher_split(List<string> lines_one_thread,
            List<Line> lines, Dictionary<string, double> doubleCell2Score_one_thread)
        {
            for (int i = 0; i < lines_one_thread.Count; i++)
            {
                string linepair = lines_one_thread[i];
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
                                double cur = doubleCellScore_init_cache(key, key2, doubleCell2Score_one_thread);

                            }
                        }
                    }
                }
            }
        }



        public  double singleCellScore(string s)
        {
            if (s == null)
                return 1;


            if (Parameter.s_languageWeight == 0 && Parameter.s_typeWeight == 0 && Parameter.s_occurWeight == 0)
                return 0;

            if (singleCell2Score.ContainsKey(s))
                return singleCell2Score[s];


            
            double typeDistanceScore = (Parameter.s_typeWeight == 0)?0:typeDistance(s);
            double languageModelScore = (Parameter.s_languageWeight == 0)?0:languageModelDistance(s);
            double occurrenceScore = (Parameter.s_occurWeight == 0)?0:occurrenceDistance(s);
           
            double result = Parameter.s_languageWeight * languageModelScore + Parameter.s_typeWeight * typeDistanceScore + Parameter.s_occurWeight * occurrenceScore;

            if (!(result >= 0 && result <= 1))
            {
                Debug.Assert(result >= 0 && result <= 1);
            }
            


            singleCell2Score[s] = result;
            return result;

        }
        private double occurrenceDistance(string s)
        {
            double oc = GlobalScoringInfo.getOccurCount(s);
           
            double normalized =  oc / table_corpus_support_normalization_factor;
            if (normalized < 0)
                normalized = 0;
            if (normalized > 1)
                normalized = 1;

            return 1.0 - normalized;
        }
        private double languageModelDistance(string s)
        {
            double unnomalized_table_over_docu = GlobalScoringInfo.getStringTableProOverDocuProb(s);

            double nomalized_table_over_docu = unnomalized_table_over_docu / language_model_normalization_factor;

            System.Diagnostics.Debug.Assert(nomalized_table_over_docu >= 0 && nomalized_table_over_docu <= 1);

            return 1.0 - nomalized_table_over_docu;
        }
        public double typeDistance(string s1)
        {

            int type = singleCell2Type.ContainsKey(s1) ? singleCell2Type[s1] : 0;

            /*
            if (type >= 1 && type <= 10)
                return 0;
            else if (type == 11 || type == 12)
                return 0;
            else if (type == 13)
                return 0.5;
            else
            {
                Debug.Assert(type == 0);
                return 1.0;
            }
             */
            if (type != 0)
                return 0;
            else
                return 1;
               
        }


        public double doubleCellScore_init_cache(string s1, string s2, Dictionary<string, double> doubleCell2Score)
        {

            string mys1 = s1;
            string mys2 = s2;

            double pairScore = pairWiseScore(mys1, mys2);

            double singleScore1 = singleCellScore(mys1);
            double singleScore2 = singleCellScore(mys2);

            double singleScoreWeight = my_single_score_weight;
            double pairScoreWeight = Parameter.pairScoreWeight;

            double result = singleScoreWeight * singleScore1 + singleScoreWeight * singleScore2 + pairScoreWeight * pairScore;
            Debug.Assert(result >= 0 && result <= 3);


            if (s1 != null && s2 != null)
            {
                if (s1.CompareTo(s2) >= 0)
                    doubleCell2Score[s1 + "\t" + s2] = result;
                else
                    doubleCell2Score[s2 + "\t" + s1] = result;
            }


            return result;

        }

        public  double doubleCellScore(string s1, string s2)
        {

            string mys1 = s1;
            string mys2 = s2;


           
            if (s1 != null && s2 != null)
            {
                if (s1.CompareTo(s2) >= 0 && doubleCell2Score.ContainsKey(s1 + "\t" + s2))
                    return doubleCell2Score[s1 + "\t" + s2];
                else if (doubleCell2Score.ContainsKey(s2 + "\t" + s1))
                    return doubleCell2Score[s2 + "\t" + s1];
            } 
            



            double pairScore = pairWiseScore(mys1, mys2);

            double singleScore1 = singleCellScore(mys1);
            double singleScore2 = singleCellScore(mys2);

            double singleScoreWeight = my_single_score_weight;
            double pairScoreWeight = Parameter.pairScoreWeight;

            double result = singleScoreWeight * singleScore1 + singleScoreWeight * singleScore2 + pairScoreWeight * pairScore;
            Debug.Assert(result >= 0 && result <= 3);

            
            /*
            if (s1 != null && s2 != null)
            {
                if (s1.CompareTo(s2) >= 0)
                    doubleCell2Score[s1 + "\t" + s2] = result;
                else
                    doubleCell2Score[s2 + "\t" + s1] = result;
            }
            */
            
           


            return result;

        }
        public double pairWiseScore(string s1, string s2)
        {
           
            double d1 = (Parameter.d_occurWeight == 0)?0: occurrenceDistance(s1, s2);
            
            double d2 = (Parameter.d_syntacticWeight == 0)?0: syntacticDistance(s1,s2);

            double result = Parameter.d_occurWeight * d1  + Parameter.d_syntacticWeight * d2;
            return result;
        }
        public double occurrenceDistance(string s1, string s2)
        {
            
            if (s1 == null || s2 == null)
                return 1.0;
            /*
            if (s1 == null && s2 == null)
                return 1.0;
            else if (s1 == null && s2 != null)
                return occurrenceDistance("", s2);
            else if (s1 != null && s2 == null)
                return occurrenceDistance(s1, "");
             **/
            

            double s1Count = 0;
            double s2Count = 0;
            double cooccurCount = 0;

            s1Count = GlobalScoringInfo.getOccurCount(s1);
            s2Count = GlobalScoringInfo.getOccurCount(s2);

           
            cooccurCount = GlobalScoringInfo.getCooccurCount(s1, s2);
            if (cooccurCount == 0)
                return 1.0;

            if(Parameter.d_occur_type == 1) //PMI
            {
                s1Count /= GlobalScoringInfo.totalStringOccurrence;
                s2Count /= GlobalScoringInfo.totalStringOccurrence;
                cooccurCount /= GlobalScoringInfo.totalStringOccurrence;

                double pmi = Math.Log(cooccurCount / (s1Count * s2Count), 2);
                double npmi = pmi / (-1 * Math.Log(cooccurCount, 2));


                //just in case we go out of range [-1,1]
                if (npmi > 1)
                {
                    npmi = 1;
                }
                else if (npmi < -1)
                {
                    npmi = -1;
                }


                double result = 0.75 - 0.25 * npmi;
                return result;
            }
            else if(Parameter.d_occur_type == 2) //Jarrcard
            {
                double denominator = s1Count + s2Count - cooccurCount;
              
                double result = 1.0 - (cooccurCount / denominator);
                if (result > 1)
                    result = 1.0;
                else if (result < 0)
                    result = 0;

                return result ;
            }
            else
            {
                //undefined
                return 1.0;
            }
          
           
        }
     


        public double syntacticDistance(string s1, string s2)
        {
            /*
            if (s1 == null && s2 == null)
                return 0.5;
            else if (s1 == null || s2 == null)
                return 1.0;
            else if (s1 == s2)
                return 0.5;
             */

            /*
            if (s1 == null || s2 == null)
                return 1.0;
            */
            
            if (s1 == null && s2 == null)
                return syntacticDistance("", "");
            else if (s1 == null && s2 != null)
                return syntacticDistance("",s2);
            else if (s1 != null && s2 == null)
                return syntacticDistance(s1, "");
            



            double numDigits_Dis = 1.0;
            int numDigits1 = 0;
            int numDigits2 = 0;

            double numCapitalLetters_Dis = 1.0;
            int numCapitalLetters1 = 0;
            int numCapitalLetters2 = 0;


            double numLetters_Dis = 1.0;
            int numLetters1 = 0;
            int numLetters2 = 0;

            double numSymbols_Dis = 1.0;
            int numSymbols1 = 0;
            int numSymbols2 = 0;

            double numPunctuations_Dis = 1.0;
            int numPunctuations1 = 0;
            int numPunctuations2 = 0;

            double lengthDiff_Dis = 1.0;
            int length1 = s1.Split(Seperator.getSeperators(),StringSplitOptions.None).Count();
            int length2 = s2.Split(Seperator.getSeperators(), StringSplitOptions.None).Count();
            if (length1 >= length2)
                lengthDiff_Dis = (double)(length1 - length2) / length1;
            else
                lengthDiff_Dis = (double)(length2 - length1) /  length2;
            foreach (char c in s1)
            {
                if (Char.IsDigit(c))
                    numDigits1++;

                if (Char.IsLetter(c))
                    numLetters1++;

                if (Char.IsUpper(c))
                    numCapitalLetters1++;

                if (Char.IsSymbol(c))
                    numSymbols1++;

                if (Char.IsPunctuation(c))
                    numPunctuations1++;
                
            }
            foreach (char c in s2)
            {
                if (Char.IsDigit(c))
                    numDigits2++;

                if (Char.IsLetter(c))
                    numLetters2++;

                if (Char.IsUpper(c))
                    numCapitalLetters2++;

                if (Char.IsSymbol(c))
                    numSymbols2++;

                if (Char.IsPunctuation(c))
                    numPunctuations2++;

            }

            if (numDigits1 == numDigits2 )
                numDigits_Dis = 0;
            if (numLetters1 == numLetters2 )
                numLetters_Dis = 0;
            if (numCapitalLetters1 == numCapitalLetters2 )
                numCapitalLetters_Dis = 0;
            if (numSymbols1 == numSymbols2 )
                numSymbols_Dis = 0;
            if (numPunctuations1 == numPunctuations2 )
                numPunctuations_Dis = 0;

            /*
            double total_Dis = 0;
            int total_Dis_Count = 0;
            if (numDigits1 != 0 || numDigits2 != 0)
            {
                total_Dis_Count++;
                total_Dis += numDigits_Dis;
            }
            if (numLetters1 != 0 || numLetters2 != 0)
            {
                total_Dis_Count++;
                total_Dis += numLetters_Dis;
            }
            if (numCapitalLetters1 != 0 || numCapitalLetters2 != 0)
            {
                total_Dis_Count++;
                total_Dis += numCapitalLetters_Dis;
            }
            if (numSymbols1 != 0 || numSymbols2 != 0)
            {
                total_Dis_Count++;
                total_Dis += numSymbols_Dis++;
            }
            if (numPunctuations1 != 0 || numPunctuations2 != 0)
            {
                total_Dis_Count++;
                total_Dis += numPunctuations_Dis;
            }

            result = total_Dis / total_Dis_Count;
            */
            double char_Diff = (numDigits_Dis + numLetters_Dis + numCapitalLetters_Dis + numSymbols_Dis + numPunctuations_Dis) / 5;
           // return result;


            int type1 = singleCell2Type.ContainsKey(s1) ? singleCell2Type[s1] : 0;
            int type2 = singleCell2Type.ContainsKey(s2) ? singleCell2Type[s2] : 0;
            double typeDiff = 1;
            if (type1 == type2 && type1 != 0)
                typeDiff = 0;

            if(true)
            {
                double result = (char_Diff + lengthDiff_Dis + typeDiff) / 3;
                return result;
                //double result = (char_Diff + typeDiff) / 2;
                //return result;
            }
            else
            {
                if (type1 == type2 && type1 != 0)
                    return 0;
                else
                    return (char_Diff + lengthDiff_Dis) / 2;
            }
          
         
        }






        public double twoRecordScore(Record r1, Record r2)
        {
            if (r1.getNumCells() != r2.getNumCells())
            {
                System.Diagnostics.Debug.Assert(false);
            }

            double result = 0;
            for (int i = 0; i < r1.getNumCells(); i++)
            {
                //Console.WriteLine(r1.getCell(i).getCellValue() + " and " + r2.getCell(i).getCellValue() + " have score" + doubleCellScore(r1.getCell(i).getCellValue(), r2.getCell(i).getCellValue()));
                result += doubleCellScore(r1.getCell(i).getCellValue(), r2.getCell(i).getCellValue());

            }
            return result;
        }
       

        public void freeMemory()
        {
            singleCell2Score.Clear();
            singleCell2Score = null;
            singleCell2Type.Clear();
            singleCell2Type = null;
            doubleCell2Score.Clear();
            doubleCell2Score = null;
            //GC.Collect();
        }



    }
}
