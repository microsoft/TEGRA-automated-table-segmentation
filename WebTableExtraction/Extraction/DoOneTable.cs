using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTableExtraction.Extraction;
using WebTableExtraction.Utils;
using WebTableExtraction.Baseline;
using WebTableExtraction.Judie;
using WebTableExtraction.Experiments;
using System.Threading;
using System.IO;
namespace WebTableExtraction.Extraction
{
    class DoOneTable
    {
        //input
        private ManualResetEvent _doneEvent;
        bool do_MRA;
        bool do_ListExtract;
        bool do_Judie;


        //outupt
        public string log;

        public DoOneTable(ManualResetEvent _doneEvent, bool do_MRA, bool do_ListExtract, bool do_Judie)
        {
            this._doneEvent = _doneEvent;
            this.do_MRA = do_MRA;
            this.do_ListExtract = do_ListExtract;
            this.do_Judie = do_Judie;
        }

        private int getNumCols(String groundFile)
        {
            string[] temps = File.ReadAllLines(groundFile);
            string[] stringSeparators = new string[] { Seperator.getTabularSeperator() };
            string[] temp = temps[0].Split(stringSeparators, StringSplitOptions.None);
            return temp.Count();

        }
        private int getNumRows(String groundFile)
        {
            string[] temps = File.ReadAllLines(groundFile);
            return temps.Length;

        }


        public void ThreadPoolCallback(Object threadContext)
        {

            string file = (string)threadContext;

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


            //Get the number of columns 
            int numCols = getNumCols(groundFile);
            Console.WriteLine("Align File {0}, with ground num of cols: {1}", file, numCols);

            int numRows = getNumRows(groundFile);
            //Set the weights of the scoring functions

            Table groundTable = new Table(groundFile);
            GlobalScoringInfo.subtractTableStats(groundTable);



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
            
            
            log = file_info + "," + msa_log + "," + baseline_log + "," + judie_log;

            Console.WriteLine("***********************************************************");


            GlobalScoringInfo.addBackTableStats(groundTable);


            _doneEvent.Set();
        }

    }
}
