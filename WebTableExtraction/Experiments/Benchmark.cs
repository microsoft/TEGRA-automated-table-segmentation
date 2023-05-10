using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using WebTableExtraction.Extraction;
using WebTableExtraction.Utils;
using WebTableExtraction.Baseline;
using WebTableExtraction.Judie;

namespace WebTableExtraction.Experiments
{
    class Benchmark
    {
       
        StreamWriter swLogger = new StreamWriter(Parameter.logFile);
        HashSet<string> processingFiles = new HashSet<string>();
        bool do_MRA;
        bool do_ListExtract;
        bool do_Judie;
        int do_numTables;

        public Benchmark():this(true,true,true,Int32.MaxValue)
        {

        }
        public Benchmark(bool MRA, bool ListExtract, bool Judie, int numTables)
        {

            this.do_MRA = MRA;
            this.do_ListExtract = ListExtract;
            this.do_Judie = Judie;
            this.do_numTables = numTables;
            swLogger.WriteLine("File,Ground_NumRows,Ground_NumCols,"
                + "MSA_NumAnchors_Done, MSA_Time, MSA_NumCols, MSA_Precision,MSA_Recall,MSA_FMeasure,MSA_Score,"
                + "Baseline_Time,Baseline_NumCols,Baseline_Precision,Baseline_Recall,Baseline_FMeasure,Baseline_Score,"
                + "Judie_Time,Judie_NumCols,Judie_Precision,Judie_Recall,Judie_FMeasure,Judie_Score,");


            
            if(File.Exists(Parameter.processingFilePath))
            {
                foreach(string file in System.IO.File.ReadAllLines(Parameter.processingFilePath))
                {
                   
                        processingFiles.Add(file);
                  
                }
            }

            doIt();
        }
       
        public void doIt()
        {
            if (!Parameter.numColsGiven)
                Parameter.numExamples = 0;
            

            List<string> allfiles = FileUtil.getAllFiles(Parameter.inputSequence_directory);
          
            int doneNumTables = 0;
            int skippedNumTables = 0;
           
            GlobalScoringInfo.load();
            KB.load();

            
            //distribute files to threads
            List<Dictionary<string, string>> distribute_files2log = new List<Dictionary<string, string>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                Dictionary<string, string> files2log = new Dictionary<string, string>();
                distribute_files2log.Add(files2log);
            }

            foreach (string file in allfiles)
            {
                /*
                string groundFile = file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory);
                Table groundTable = new Table(groundFile);
                if (!TableFiltering.runTime_good_table(groundTable))
                {
                    skippedNumTables++;
                    continue;
                }
                 */ 

                int which_thread = doneNumTables % Parameter.max_num_threads_per_machine;
                distribute_files2log.ElementAt(which_thread)[file] = "";
                
                doneNumTables++;
                if (doneNumTables >= do_numTables)
                    break;
            }

            Dictionary<Thread, Dictionary<string, string>> thread2_files2log = new Dictionary<Thread, Dictionary<string, string>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++ )
            {
                Dictionary<string, string> files2log = distribute_files2log.ElementAt(i);
                ThreadStart starter = delegate { do_many_tables(files2log); };
                Thread thread = new Thread(starter);
                thread.Start();
                thread2_files2log[thread] = files2log;
            }

            foreach (Thread thread in thread2_files2log.Keys)
            {
                thread.Join();
            }

            Console.WriteLine("Files total processed:  " + doneNumTables);
            Console.WriteLine("Files total skipped:  " + skippedNumTables);


            foreach (Thread thread in thread2_files2log.Keys)
            {
                foreach(string file in thread2_files2log[thread].Keys)
                {
                    swLogger.WriteLine(thread2_files2log[thread][file]);
                }
            }

            swLogger.Close();
        }


        private void do_many_tables(Dictionary<string,string> file2log)
        {
            List<string> files = file2log.Keys.ToList();
            foreach (string file in files)
            {
                string log = do_one_table(file);
                file2log[file] = log;
            }
            GC.Collect();
        }


        private string do_one_table(string file)
        {
            Console.WriteLine("***********************************************************");


            string groundFile = file.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory);
            string outputFile_MSA = file.Replace(Parameter.inputSequence_directory, Parameter.outputTabular_MSA_directory);
            string outputFile_Baseline = file.Replace(Parameter.inputSequence_directory, Parameter.outputTabular_Baseline_directory);
            string outputFile_Judie = file.Replace(Parameter.inputSequence_directory, Parameter.outputTabular_Judie_directory);

            if (File.Exists(outputFile_MSA))
                File.Delete(outputFile_MSA);
            if (File.Exists(outputFile_Judie))
                File.Delete(outputFile_Judie);
            if (File.Exists(outputFile_Baseline))
                File.Delete(outputFile_Baseline);

            int numCols = 0;
            int numRows = getNumRows(file);
            if(groundFile != file)
            {
                //Get the number of columns 
                 numCols = getNumCols(groundFile);
                Console.WriteLine("Align File {0}, with ground num of cols: {1}", file, numCols);

                 numRows = getNumRows(groundFile);
                //Set the weights of the scoring functions

                Table groundTable = new Table(groundFile);
                GlobalScoringInfo.subtractTableStats(groundTable);
            }

           



            //Judie
            string judie_log = "0," + "0," + "0," + "0," + "0," + "0";
            if (do_Judie)
            {
                DateTime time0 = DateTime.Now;
                Judie.Judie judie = new Judie.Judie(file);
                if (Parameter.numColsGiven)
                    judie.align(numCols);
                else
                    judie.align();
                judie.writeExtractedToFile(outputFile_Judie);
                TableComparator tc_Judie = new TableComparator(outputFile_Judie, groundFile);
                DateTime time1 = DateTime.Now;
                Console.WriteLine("*********Judie Done with Time {0}***********", (time1 - time0).TotalMilliseconds);
                judie_log = (time1 - time0).TotalMilliseconds + "," + judie.getGuessNumCols() + "," + tc_Judie.flex_precision + "," + tc_Judie.flex_recall + "," + tc_Judie.flex_fmeasure + "," + judie.getExtractedTableScore();
            }



            string baseline_log = "0," + "0," + "0," + "0," + "0," + "0";
            if (do_ListExtract)
            {
                //baseline
                DateTime time0 = DateTime.Now;
                Baseline.Baseline baseline = new Baseline.Baseline(file);
                if (Parameter.numColsGiven)
                    baseline.align(numCols);
                else
                    baseline.align();
                baseline.writeExtractedToFile(outputFile_Baseline);
                TableComparator tc_Baseline = new TableComparator(outputFile_Baseline, groundFile);
                DateTime time1 = DateTime.Now;
                Console.WriteLine("*********Baseline Done with Time {0}***********", (time1 - time0).TotalMilliseconds);
                baseline_log = (time1 - time0).TotalMilliseconds + "," + baseline.getGuessNumCols() + "," + tc_Baseline.flex_precision + "," + tc_Baseline.flex_recall + "," + tc_Baseline.flex_fmeasure + "," + baseline.getExtractedTable_score();

            }

            string msa_log = "0," + "0," + "0," + "0," + "0," + "0," + "0";
            if (do_MRA)
            {
                //Init the program
                DateTime time0 = DateTime.Now;
                MSAAppro msaAppro = new MSAAppro(file);
                if (Parameter.numColsGiven)
                    msaAppro.alignAStar_with_numcols(numCols);
                else
                    msaAppro.alignAStar_without_numcols();
                //msaAppro.alignAStar_with_numcols(baseline.getGuessNumCols());
                msaAppro.writeExtractedToFile(outputFile_MSA);
                //Compare the quality
                TableComparator tc_MSA = new TableComparator(outputFile_MSA, groundFile);
                DateTime time1 = DateTime.Now;
                Console.WriteLine("**********MSA Done with Time {0}***********", (time1 - time0).TotalMilliseconds);
                msa_log = msaAppro.numAnchors_done + "," +
                    (time1 - time0).TotalMilliseconds + "," + msaAppro.getGuessNumCols() + "," + tc_MSA.flex_precision + "," + tc_MSA.flex_recall + "," + tc_MSA.flex_fmeasure + "," + msaAppro.getExtractedTable_Score();
            }



            string file_info = file.Replace(",", "_") + "," + numRows + "," + numCols;
            string log = file_info + "," + msa_log + "," + baseline_log + "," + judie_log;
           
            Console.WriteLine("***********************************************************");


            //GlobalScoringInfo.addBackTableStats(groundTable);

            return log;
           
        }


        
        private int getNumCols(String groundFile)
        {
            string[] temps = File.ReadAllLines(groundFile);
            string[] stringSeparators = new string[] {Seperator.getTabularSeperator()};
            string[] temp = temps[0].Split(stringSeparators,StringSplitOptions.None);
            return temp.Count();
            
        }
        private int getNumRows(String groundFile)
        {
            string[] temps = File.ReadAllLines(groundFile);
            return temps.Length;

        }



    }
}
