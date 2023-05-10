using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTableExtraction.Utils;
using System.Threading;

using WebTableExtraction.Experiments;

using WebTableExtraction.ExtractionAStar;

namespace WebTableExtraction.Extraction
{
    public class MSAAppro
    {
        string filePath = "";

        //inputs
        List<Line> lines = new List<Line>();
        Dictionary<Line, Record> examples = new Dictionary<Line, Record>();

        //outputs
        Dictionary<Line, Record> line2Record = new Dictionary<Line, Record>();
        public int numAnchors_done = 0;
        double table_score = 0;
        Table extracted_table = null;

        //internal data structure
        LocalScoringInfo localScoring;


        public MSAAppro(string filePath)
        {
            this.filePath = filePath;
            string[] temps = System.IO.File.ReadAllLines(filePath);
            foreach (string temp in temps)
            {
                Line line = new Line(temp);
                lines.Add(line);
            }



            //Add examples from 
            string groundFile = filePath.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory);
            List<Record> records = new List<Record>();
            string[] temps2 = System.IO.File.ReadAllLines(groundFile);
            foreach (string temp in temps2)
            {
                Record record = new Record(temp);
                records.Add(record);
            }

            //select number of examples from the middle to avoid bad cases
            List<Line> linesForSelectingExamples = new List<Line>(lines);

            int myNumExamples = Parameter.numExamples;
            if (Parameter.numExamples > linesForSelectingExamples.Count())
            {
                myNumExamples = linesForSelectingExamples.Count();
            }
            for (int i = 0; i < myNumExamples; i++)
            {
                //int rnd = new Random().Next(linesForSelectingExamples.Count());
                int rnd = linesForSelectingExamples.Count() / 2;
                Line line = linesForSelectingExamples.ElementAt(rnd);
                linesForSelectingExamples.RemoveAt(rnd);

                int index = lines.IndexOf(line);
                examples[lines.ElementAt(index)] = records.ElementAt(index);
            }


            localScoring = new LocalScoringInfo(lines, examples.Count());


            localScoring.init_double_cell_score_cache_parallel(lines);
        }




        public MSAAppro(List<Line> lines)
        {
            this.lines = lines;
            localScoring = new LocalScoringInfo(lines, 0);

            localScoring.init_double_cell_score_cache_parallel(lines);
        
        }
        public MSAAppro(List<Line> lines, Dictionary<Line, Record> examples)
        {
            this.lines = lines;
            this.examples = examples;
            localScoring = new LocalScoringInfo(lines, examples.Count());
            localScoring.init_double_cell_score_cache_parallel(lines);
        
        }



        /*
       
         * */


        public void alignAStar_without_numcols()
        {

            System.Diagnostics.Debug.Assert(examples.Count() == 0);
            int minNumCols = 1;
            int maxNumCols = 1;
            foreach (Line line in lines)
            {
                if (line.getNumWords() > maxNumCols)
                    maxNumCols = line.getNumWords();
            }
            List<Line> anchors = select_anchors_parallel(maxNumCols);
            int myGuessNumCols = maxNumCols;

            //Different ways of determining the number of columns
            //

            if (false)
            {
                int guess_num_cols_1 = alignAStar_without_numcols_method_1(minNumCols, maxNumCols, anchors, true);
                int guess_num_cols_2 = alignAStar_without_numcols_method_2(minNumCols, maxNumCols, anchors, true);

                Console.WriteLine("Method1: " + guess_num_cols_1);
                Console.WriteLine("Method2: " + guess_num_cols_2);


                if (guess_num_cols_1 > guess_num_cols_2)
                {
                    //Unsupervised_Test.method_1_more++;
                }
                else if (guess_num_cols_1 < guess_num_cols_2)
                {
                    //Unsupervised_Test.method_2_more++;
                }
                else
                {

                }
            }


            int how_to_determine_num_cols = Parameter.determine_num_cols_method;
            if (how_to_determine_num_cols == 1)
                alignAStar_without_numcols_method_1(minNumCols, maxNumCols, anchors, false);
            else if (how_to_determine_num_cols == 2)
                alignAStar_without_numcols_method_2(minNumCols, maxNumCols, anchors, false);
            else if (how_to_determine_num_cols == 3)
                alignAStar_without_numcols_method_3(minNumCols, maxNumCols, anchors);

        }




        public int alignAStar_without_numcols_method_1(int minNumCols, int maxNumCols, List<Line> anchors, bool debugMode)
        {
            Dictionary<int, int> numCols2Support = new Dictionary<int, int>();
            foreach (Line line in lines)
            {
                MSASplitting msaSplitting = new MSASplitting(line, anchorLine2InterSeg[line]);

                int curNumcols = msaSplitting.getIndependentNumCols();

                if (numCols2Support.ContainsKey(curNumcols))
                {
                    numCols2Support[curNumcols] = numCols2Support[curNumcols] + 1;
                }
                else
                {
                    numCols2Support[curNumcols] = 1;
                }
            }

            //I prefer shorter number of columns, if two number of columns have the same support
            List<int> allNumCols = new List<int>(numCols2Support.Keys);
            allNumCols.Remove(0);
            allNumCols = allNumCols.OrderByDescending(x => numCols2Support[x]).ThenBy(x => x).ToList();


            int guess_NumCol = allNumCols.ElementAt(0);
            //Console.WriteLine("My guess number of cols is: " + guess_NumCol);

            if (!debugMode)
                alignAStar_parallel(guess_NumCol, anchors, false);

            return guess_NumCol;
        }

        public int alignAStar_without_numcols_method_2(int minNumCols, int maxNumCols, List<Line> anchors, bool debugMode)
        {
            double best_ave_per_column = Double.MaxValue;
            Table best_table = null;


            Dictionary<int, double> numCols_2_estimate_score = new Dictionary<int, double>();
            for (int numCols = maxNumCols; numCols >= minNumCols; numCols--)
            {
                double cur_prune = 0;
                foreach (Line line in lines)
                {
                    InterestingSegmentation inteSeg = anchorLine2InterSeg[line];
                    cur_prune += inteSeg.getBestScore_Backwards(numCols, 0);
                }
                cur_prune = cur_prune / (lines.Count * numCols * (lines.Count - 1));
                numCols_2_estimate_score[numCols] = cur_prune;
            }
            List<int> all_numCols = numCols_2_estimate_score.Keys.OrderBy(x => numCols_2_estimate_score[x]).ToList();

            foreach (int numCols in all_numCols)
            {

                //If I can prune this number of columns without doing it?
                double cur_prune = numCols_2_estimate_score[numCols];
                if (cur_prune >= best_ave_per_column)
                {
                    continue;
                }





                alignAStar_parallel(numCols, anchors, true);
                Table table = getExtracted_Table();
                Console.WriteLine("Table with " + numCols + " cols have a score:" + table.getAvePerColumnScore(localScoring));
                if (table.getAvePerColumnScore(localScoring) < best_ave_per_column)
                {
                    best_ave_per_column = table.getAvePerColumnScore(localScoring);
                    best_table = table;
                }
            }


            if (!debugMode)
                alignAStar_parallel(best_table.getNumCols(), anchors, false);

            if(filePath.Contains("7"))
            {

            }

            return best_table.getNumCols();
        }



        public List<Table> unsupervised_accepted_tables = new List<Table>();
        private void alignAStar_without_numcols_method_3(int minNumCols, int maxNumCols, List<Line> anchors)
        {
            Dictionary<int, Table> numCols2Table = new Dictionary<int, Table>();
            Dictionary<double, int> histogram = new Dictionary<double, int>();
            //Do an A Star on every possible numCols, and collect the histogram


            for (int numCols = minNumCols; numCols <= maxNumCols; numCols++)
            {
                alignAStar_parallel(numCols, anchors, true);
                Table table = getExtracted_Table();
                numCols2Table[numCols] = table;
                double curAveScore = table.getAvePerColumnScore(localScoring);
                if (histogram.ContainsKey(curAveScore))
                    histogram[curAveScore] = histogram[curAveScore] + 1;
                else
                    histogram[curAveScore] = 1;
            }
            //determine the  column score threshold, any score below or equal are considered good
            double thresholdScore = good_bad_thresholding(histogram);
            Console.WriteLine("---Best Threshold Score:  " + thresholdScore);

            //get the accepted tables according to the threshold score
            List<Table> acceptedTables = new List<Table>();
            for (int numCols = minNumCols; numCols <= maxNumCols; numCols++)
            {
                Table table = numCols2Table[numCols];
                Console.WriteLine("Table with " + numCols + " has a score of" + table.getAvePerColumnScore(localScoring));
                if (table.getAvePerColumnScore(localScoring) <= thresholdScore)
                {
                    acceptedTables.Add(table);
                }
            }
            unsupervised_accepted_tables = acceptedTables;
            if (acceptedTables.Count > 0)
            {
                double tempPos = acceptedTables.Count * ((double)Parameter.determine_num_cols_aggressiveness / 5);
                int index = Convert.ToInt32(tempPos);
                if (index < 0)
                    index = 0;
                else if (index > acceptedTables.Count - 1)
                    index = acceptedTables.Count - 1;
                Table best_table = acceptedTables[index];


                //Do a real A star

                alignAStar_parallel(best_table.getNumCols(), anchors, false);
            }
            else
            {
                alignAStar_parallel(1, anchors, false);
            }



        }
        private double good_bad_thresholding(Dictionary<double, int> histogram)
        {
            int totalCount = 0;
            foreach (double key in histogram.Keys)
                totalCount += histogram[key];


            List<double> points = histogram.Keys.OrderBy(x => x).ToList();

            int smaller_Equal_Count = 0;
            double max = 0;
            double thre1 = 0;
            double thre2 = 0;
            for (int i = 0; i < points.Count(); i++)
            {
                double point = points.ElementAt(i);
                smaller_Equal_Count += histogram[point];

                double w_smaller_equal = (double)smaller_Equal_Count / totalCount;
                double w_bigger = (double)(totalCount - smaller_Equal_Count) / totalCount;

                double sum_smaller_equal = 0;
                double sum_bigger = 0;

                for (int j = 0; j < points.Count(); j++)
                {
                    if (j <= i)
                    {
                        sum_smaller_equal += points.ElementAt(j) * histogram[points.ElementAt(j)];
                    }
                    else
                    {
                        sum_bigger += points.ElementAt(j) * histogram[points.ElementAt(j)];
                    }
                }

                double mean_smaller_equal = sum_smaller_equal / smaller_Equal_Count;
                double mean_bigger = sum_bigger / (totalCount - smaller_Equal_Count);

                double between_class_variance = w_smaller_equal * w_bigger * Math.Pow((mean_bigger - mean_smaller_equal), 2);

                if (between_class_variance >= max)
                {
                    thre1 = point;
                    if (between_class_variance > max)
                        thre2 = point;
                    max = between_class_variance;
                }

            }

            return (thre2 + thre1) / 2;
        }




        public void alignAStar_with_numcols(int numCols)
        {

            //Step 1: select numAchors randomly from lines
            List<Line> anchors = select_anchors_parallel(numCols);
            alignAStar_parallel(numCols, anchors, false);
        }



        private void alignAStar(int numCols, List<Line> anchors, bool heuristic)
        {
            Line minLine = null;
            Record minRecord = null;
            double minScore = Double.MaxValue;

            DateTime t1 = DateTime.Now;
            if(Parameter.A_Star_H_Appro != 0)
            {
                //do a sort on anchors
                anchors = anchors.OrderBy(x => anchorLine2InterSeg[x].getFirstAnchorRecordBestScore(numCols)).ToList();
                foreach (Line anchorLine in anchors)
                {
                    double h = anchorLine2InterSeg[anchorLine].getFirstAnchorRecordBestScore(numCols);
                    double curScore = alignGivenAnchor(anchorLine, anchorLine2InterSeg[anchorLine].getFirstAnchorRecord(numCols), numCols);
                    if (curScore < minScore)
                    {
                        minScore = curScore;
                        minLine = anchorLine;
                        minRecord = anchorLine2InterSeg[anchorLine].getFirstAnchorRecord(numCols);
                    }
                    Console.WriteLine("START GUESS FOR: " + anchorLine);
                    Console.WriteLine(" -----GUESS SEGMENTATION---- " + anchorLine2InterSeg[anchorLine].getFirstAnchorRecord(numCols));
                    Console.WriteLine(" -----GUESS SEGMENTATION---- H score: " + h);
                    Console.WriteLine(" -----GUESS SEGMENTATION---- anchor score: " + curScore);
                }
            }
           
            DateTime t2 = DateTime.Now;
            
            Console.WriteLine("Done the guess best score for every line using time: " + (t2 - t1).TotalMilliseconds);
            Console.WriteLine();
            //Do A star on each line
           
            if (!heuristic)
            {
                DateTime s_time = DateTime.Now;
                foreach (Line anchorLine in anchors)
                {
                    Console.WriteLine("START A STAR FOR: " + anchorLine);
                    MSAAStar msaAStar = new MSAAStar(lines, numCols, anchorLine, anchorLine2InterSeg[anchorLine], localScoring, examples);
                    double curScore = 0;
                    if(!Parameter.design_running_example)
                        curScore = msaAStar.align(numCols, minScore);
                    else
                        curScore = msaAStar.align(numCols, Double.MaxValue);
                   
                    Console.WriteLine(" -----A STAR SEGMENTATION---- : " + msaAStar.getBestAnchorRecord());
                    Console.WriteLine(" -----A STAR SEGMENTATION---- anchor score: " + curScore);
                   
                    if (curScore < minScore)
                    {
                        minLine = anchorLine;
                        minScore = curScore;
                        minRecord = msaAStar.getBestAnchorRecord();
                    }
                    DateTime e_time = DateTime.Now;
                    numAnchors_done++;
                    if ((e_time - s_time).TotalMilliseconds >= Parameter.maxTimeMSA)
                        break;


                }

            }
            DateTime t3 = DateTime.Now;
            Console.WriteLine("Done A star search for every line using time: " + (t3 - t2).TotalMilliseconds);
            if (minLine != null)
                line2Record[minLine] = minRecord;
            foreach (Line line in lines)
            {
                if (line == minLine)
                    continue;

                if (!examples.ContainsKey(line))
                {
                    SingleAlignment sa = new SingleAlignment(line, localScoring);
                    sa.alignMinimize(numCols, minRecord);
                    Record record = sa.getAlignedRecord();
                    line2Record[line] = record;

                }
                else
                {
                    line2Record[line] = examples[line];
                }
            }

            //set the score of the extracted table
            List<Record> records = new List<Record>();
            foreach (Line line in lines)
                records.Add(line2Record[line]);

            extracted_table = new Table(records);
            table_score = extracted_table.getAvePerColumnScore(localScoring);
        }
        private void alignAStar_parallel(int numCols, List<Line> anchors, bool heuristic)
        {
           
            if(Parameter.max_num_threads_per_machine == 0)
            {
                alignAStar(numCols, anchors, heuristic);
                return;
            }



            Line minLine = null;
            Record minRecord = null;
            double minScore = Double.MaxValue;

            DateTime t1 = DateTime.Now;
            //do a sort on anchors
            anchors = anchors.OrderBy(x => anchorLine2InterSeg[x].getFirstAnchorRecordBestScore(numCols)).ToList();

            //distribute anchors to threads
            List<List<Line>> distribute_anchors = new List<List<Line>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<Line> this_anchors = new List<Line>();
                distribute_anchors.Add(this_anchors);
            }
            for (int i = 0; i < anchors.Count; i++)
            {
                int which_thread = i % Parameter.max_num_threads_per_machine;
                distribute_anchors[which_thread].Add(anchors[i]);
            }
            //create the threads, and the returning data
            Dictionary<Thread, Dictionary<Line, double>> thread2_line2guess = new Dictionary<Thread, Dictionary<Line, double>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<Line> anchors_one_thread = distribute_anchors[i];
                Dictionary<Line, double> line2guess = new Dictionary<Line, double>();
                ThreadStart starter = delegate { alignAStar_one_thread_guess(anchors_one_thread, numCols, line2guess); };
                Thread thread = new Thread(starter);
                thread.Start();
                thread2_line2guess[thread] = line2guess;
            }
            //wait for the thread to finish
            foreach (Thread thread in thread2_line2guess.Keys)
            {
                thread.Join();
            }
            //collect the result from each thread
            foreach (Thread thread in thread2_line2guess.Keys)
            {
                foreach (Line anchorLine in thread2_line2guess[thread].Keys)
                {
                    double curScore = thread2_line2guess[thread][anchorLine];
                    if (curScore < minScore)
                    {
                        minScore = curScore;
                        minLine = anchorLine;
                        minRecord = anchorLine2InterSeg[anchorLine].getFirstAnchorRecord(numCols);
                    }
                }
            }
            DateTime t2 = DateTime.Now;
            Console.WriteLine("Done the guess best score for every line in parallel using time: " + (t2 - t1).TotalMilliseconds);


            //Do A star on each line
            
            if (!heuristic)
            {
                if(false)
                {
                    //create the threads, and the returning data
                    Dictionary<Thread, Dictionary<Line, MSAAStar>> thread2_line2MSAAStar = new Dictionary<Thread, Dictionary<Line, MSAAStar>>();
                    for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
                    {
                        List<Line> anchors_one_thread = distribute_anchors[i];
                        Dictionary<Line, MSAAStar> line2MSAAStar_one_thread = new Dictionary<Line, MSAAStar>();
                        ThreadStart starter = delegate { alignAStar_one_thread_search(anchors_one_thread, numCols, minScore, line2MSAAStar_one_thread); };
                        Thread thread = new Thread(starter);
                        thread.Start();
                        thread2_line2MSAAStar[thread] = line2MSAAStar_one_thread;
                    }
                    //wait for the thread to finish
                    foreach (Thread thread in thread2_line2MSAAStar.Keys)
                    {
                        thread.Join();
                    }
                    //collect the result from each thread
                    foreach (Thread thread in thread2_line2MSAAStar.Keys)
                    {
                        foreach (Line anchorLine in thread2_line2MSAAStar[thread].Keys)
                        {
                            MSAAStar msaAStar = thread2_line2MSAAStar[thread][anchorLine];
                            double curScore = msaAStar.getBestScore();
                            if (curScore < minScore)
                            {
                                minLine = anchorLine;
                                minScore = curScore;
                                minRecord = msaAStar.getBestAnchorRecord();
                            }
                            numAnchors_done++;
                        }
                    }
                }
                else
                {
                    foreach (Line anchorLine in anchors)
                    {
                        MSAAStar msaAStar = new MSAAStar(lines, numCols, anchorLine, anchorLine2InterSeg[anchorLine], localScoring, examples);
                        double curScore = msaAStar.align(numCols, minScore);
                        //double curScore = msaAStar.align(numCols, Double.MaxValue);
                        //Console.WriteLine(anchorLine + " real sum: " + curScore);
                        if (curScore < minScore)
                        {
                            minLine = anchorLine;
                            minScore = curScore;
                            minRecord = msaAStar.getBestAnchorRecord();
                        }
                       
                    }
                }


               

            }
            DateTime t3 = DateTime.Now;
            Console.WriteLine("Done A star search for every line in parallel using time: " + (t3-t2).TotalMilliseconds);
            if (minLine != null)
                line2Record[minLine] = minRecord;
            foreach (Line line in lines)
            {
                if (line == minLine)
                    continue;

                if (!examples.ContainsKey(line))
                {
                    SingleAlignment sa = new SingleAlignment(line, localScoring);
                    sa.alignMinimize(numCols, minRecord);
                    Record record = sa.getAlignedRecord();
                    line2Record[line] = record;

                }
                else
                {
                    line2Record[line] = examples[line];
                }
            }

            //set the score of the extracted table
            List<Record> records = new List<Record>();
            foreach (Line line in lines)
                records.Add(line2Record[line]);

            extracted_table = new Table(records);
            table_score = extracted_table.getAvePerColumnScore(localScoring);
        }



        private void alignAStar_one_thread_guess(List<Line> anchors_one_thread, int numCols, Dictionary<Line, double> anchor_2_best_guess)
        {
            foreach (Line anchorLine in anchors_one_thread)
            {
                double h = anchorLine2InterSeg[anchorLine].getFirstAnchorRecordBestScore(numCols);
                double curScore = alignGivenAnchor(anchorLine, anchorLine2InterSeg[anchorLine].getFirstAnchorRecord(numCols), numCols);
                anchor_2_best_guess[anchorLine] = curScore;
            }
        }
        private void alignAStar_one_thread_search(List<Line> anchors_one_thread, int numCols, double minScore, Dictionary<Line, MSAAStar> anchor_2_msaastar)
        {
            double minScore_one_thread = minScore;
            foreach (Line anchorLine in anchors_one_thread)
            {
                MSAAStar msaAStar = new MSAAStar(lines, numCols, anchorLine, anchorLine2InterSeg[anchorLine], localScoring, examples);
                double curScore_one_thread = msaAStar.align(numCols, minScore_one_thread);
                anchor_2_msaastar[anchorLine] = msaAStar;
                if(curScore_one_thread < minScore_one_thread)
                {
                    minScore_one_thread = curScore_one_thread;
                }
            }
        }








        private Dictionary<Line, InterestingSegmentation> anchorLine2InterSeg;
        /**
         * select numAnchors from all lines, exlcuding those in the examples
         */
        private List<Line> select_anchors(int maxNumCols)
        {
            DateTime start = DateTime.Now;
            List<Line> linesExcludingExamples = new List<Line>();
            foreach (Line line in lines)
            {
                if (!examples.ContainsKey(line))
                    linesExcludingExamples.Add(line);
            }
            List<Line> anchors = new List<Line>();
            if (Parameter.numAnchorLines > linesExcludingExamples.Count())
                anchors = linesExcludingExamples;
            else
            {
                for (int i = 0; i < Parameter.numAnchorLines; i++)
                {
                    int rnd = new Random().Next(linesExcludingExamples.Count());
                    anchors.Add(linesExcludingExamples.ElementAt(rnd));
                    linesExcludingExamples.RemoveAt(rnd);
                }
            }

            //init the h() function for every anchor
            anchorLine2InterSeg = new Dictionary<Line, InterestingSegmentation>();
            foreach (Line anchorLine in anchors)
            {

                InterestingSegmentation inteSeg = new InterestingSegmentation(anchorLine, maxNumCols, lines, localScoring, examples);
                anchorLine2InterSeg[anchorLine] = inteSeg;
            }
            DateTime end = DateTime.Now;
            Console.WriteLine("done init h()  for all anchor lines, given the maximum number of columns " + maxNumCols + " using time" + (end - start).TotalMilliseconds);
            return anchors;
        }
        private List<Line> select_anchors_parallel(int maxNumCols)
        {

            DateTime start = DateTime.Now;
            if(Parameter.max_num_threads_per_machine == 0)
            {
                return select_anchors(maxNumCols);
            }
            

            List<Line> linesExcludingExamples = new List<Line>();
            foreach (Line line in lines)
            {
                if (!examples.ContainsKey(line))
                    linesExcludingExamples.Add(line);
            }
            List<Line> anchors = new List<Line>();
            if (Parameter.numAnchorLines > linesExcludingExamples.Count())
                anchors = linesExcludingExamples;
            else
            {
                for (int i = 0; i < Parameter.numAnchorLines; i++)
                {
                    int rnd = new Random().Next(linesExcludingExamples.Count());
                    anchors.Add(linesExcludingExamples.ElementAt(rnd));
                    linesExcludingExamples.RemoveAt(rnd);
                }
            }
            anchorLine2InterSeg = new Dictionary<Line, InterestingSegmentation>();

            //distribute anchors to threads
            List<List<Line>> distribute_anchors = new List<List<Line>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<Line> this_anchors = new List<Line>();
                distribute_anchors.Add(this_anchors);
            }
            for (int i = 0; i < anchors.Count; i++)
            {
                int which_thread = i % Parameter.max_num_threads_per_machine;
                distribute_anchors[which_thread].Add(anchors[i]);
            }
            //create the threads, and the returning data
            Dictionary<Thread, Dictionary<Line, InterestingSegmentation>> thread2_line2Seg = new Dictionary<Thread, Dictionary<Line, InterestingSegmentation>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<Line> anchors_one_thread = distribute_anchors[i];
                Dictionary<Line, InterestingSegmentation> line2Seg_one_thread = new Dictionary<Line, InterestingSegmentation>();
                ThreadStart starter = delegate { select_anchors_one_thread(anchors_one_thread, maxNumCols, line2Seg_one_thread); };
                Thread thread = new Thread(starter);
                thread.Start();
                thread2_line2Seg[thread] = line2Seg_one_thread;
            }
            //wait for the thread to finish
            foreach (Thread thread in thread2_line2Seg.Keys)
            {
                thread.Join();
            }

            if (Parameter.max_num_threads_per_machine == 10)
            {

            }

            //collect the result from each thread
            foreach (Thread thread in thread2_line2Seg.Keys)
            {
                foreach (Line line in thread2_line2Seg[thread].Keys)
                {
                    anchorLine2InterSeg[line] = thread2_line2Seg[thread][line];
                }

            }
            DateTime end = DateTime.Now;
            Console.WriteLine("done init h()  for all anchor lines, given the maximum number of columns " + maxNumCols + " using time" + (end - start).TotalMilliseconds);
            return anchors;
        }
        private void select_anchors_one_thread(List<Line> anchors_one_thread, int maxNumCols, Dictionary<Line, InterestingSegmentation> line2Seg_one_thread)
        {
            foreach (Line anchorLine in anchors_one_thread)
            {

                InterestingSegmentation inteSeg = new InterestingSegmentation(anchorLine, maxNumCols, lines,
                    localScoring, examples);
                line2Seg_one_thread[anchorLine] = inteSeg;
            }
        }




        double totalTimeInAligning = 0;
        private double alignGivenAnchor(Line anchorLine, Record anchorRecord, int numCols)
        {
            DateTime time1 = DateTime.Now;
            double curScore = 0;
            //align all records in lines to record
            foreach (Line line in lines)
            {
                if (line == anchorLine)
                    continue;
                if (!examples.ContainsKey(line))
                {
                    SingleAlignment sa = new SingleAlignment(line, localScoring);
                    double tempScore = sa.alignMinimize(numCols, anchorRecord);
                    curScore += tempScore;

                }
                else
                {
                    double tempScore = Parameter.exampleWeight * localScoring.twoRecordScore(anchorRecord, examples[line]);
                    curScore += tempScore;
                }
            }
            DateTime time2 = DateTime.Now;
            totalTimeInAligning += (time2 - time1).TotalMilliseconds;
            return curScore;

        }

        public double getExtractedTable_Score()
        {
            return table_score;
        }
        public Table getExtracted_Table()
        {
            return extracted_table;
        }

        public void writeExtractedToFile(string filePath)
        {

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            StreamWriter sw = new StreamWriter(filePath);

            foreach (Line line in lines)
            {
                Record record = line2Record[line];
                sw.WriteLine(record);
            }
            sw.Close();


            //after writing the extracted file, free the memory
            localScoring.freeMemory();
        }

        public int getGuessNumCols()
        {
            foreach (Line line in lines)
            {
                Record record = line2Record[line];
                return record.getNumCells();
            }
            return 0;
        }
    }
}
