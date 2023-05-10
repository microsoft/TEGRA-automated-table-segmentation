using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTableExtraction.Utils
{
    public class Parameter
    {

        
        

        //input
        public static string inputSequence_directory = null;      //the input sequences to be extracted
        public static string inputTabular_directory = null;        // the groud truth table 


        //scoring
        public static string kb_directory = null;
        public static string kb_value2attrisFilePath = null;
        public static string kb_attri2valuesFilePath = null;
        public static string singleCellOccurrenceFilePath = null;
        public static string doubleCellOccurrenceFilepath = null;
        public static string singleCellLanguageModelFilePath = null;

        

        //output
        public static string outputTabular_MSA_directory = null;  //the output of our algorithm
        public static string outputTabular_Baseline_directory = null;  //the output of baseline, google's approach
        public static string outputTabular_Judie_directory = null; //the output of Judie, Eli's approach
        public static string logFile = null;



        //internal parameter

        public static int maxTokensPerColumn = 5;

        public static int max_n_reps = 3;  //For google's baseline approach

        public static double singleScoreWeight = 1.0;//Relative weights of scoring functions
        public static double pairScoreWeight = 1.0;

        public static double s_occurWeight = 0; //Relative weight of scoring dimensions when calculating singleScore
        public static double s_typeWeight = 0;
        public static double s_languageWeight = 0;

        public static double d_occurWeight = 0.7; //Relative weight of scoring dimensions when calculating pairScore
        public static int d_occur_type = 1; //1 is PMI default, 2 is Jarrcard distance
        public static double d_syntacticWeight = 0.3;


        public static bool numColsGiven = false; //Number of data examples
        public static int numExamples = 0;
        public static double exampleWeight = 1.0;

        public static int numAnchorLines = Int32.MaxValue;//Number of anchor records
        
        
      

        //unsupervised
        public static int determine_num_cols_method = 2; //1 for indepednet splitting, 2 for ave column, 3 for thresholding
        public static int determine_num_cols_aggressiveness = 3; //on a scale of [0,5]




        //other
        public static string processingFilePath = null;
        public static long maxTimeMSA = 2 * 60 * 1000;//10 minutes


        public static int global_scoring_options = 1; // 0 is for counting number of cells, 1 is for couting number of columns
        public static int whichStats = 3; // 1 is for web stats onlly, 2 is for excel only, 3 is for both

        public static int max_num_threads_per_machine = 0;


        public static double A_Star_H_Appro = 1.0;


        public static bool design_running_example = false;

        public static string toString()
        {
            StringBuilder sb = new StringBuilder();
           
            sb.AppendFormat("Max tokens for any column is: {0}", maxTokensPerColumn);
            sb.AppendLine();
            sb.AppendLine("Number of anchors: " + numAnchorLines);
            sb.AppendLine("Number of columns is given? : " + numColsGiven);
            sb.AppendLine("Number of examples: " + numExamples);
            sb.AppendLine("Example Weight: " + exampleWeight);
            sb.AppendFormat("Single Score = {0} * occurrence + {1} * type + {2} * language model", s_occurWeight, s_typeWeight, s_languageWeight);
           
            sb.AppendLine();
            sb.AppendFormat("PairWise Score = {0} * co-occurrence  + {1} * co-syntactic", d_occurWeight,d_syntacticWeight);
            sb.AppendLine();
            sb.AppendFormat("Total score of two cells = {0} * single cell score + {1} * pairwise score", singleScoreWeight, pairScoreWeight);
            sb.AppendLine();
            
            return sb.ToString();
        }

        public static int exp_num = 0;
        public static void setWhichExp(int whichExp)
        {
            exp_num = whichExp;
            if (whichExp == 1)
            {
                //input
                inputSequence_directory = @"D:\xuchu\TableStats\ExcelCorpus_10k\Sequence\";      //the input sequences to be extracted
                inputTabular_directory = @"D:\xuchu\TableStats\ExcelCorpus_10k\Tabular\";        // the groud truth table 


                //scoring stats
                kb_directory = @"D:\xuchu\TableStats\_Freebase\";
                kb_value2attrisFilePath = @"D:\xuchu\TableStats\ExcelCorpus_10k\ScoringStats\kb_value_2_attris.txt";
                kb_attri2valuesFilePath = @"D:\xuchu\TableStats\ExcelCorpus_10k\ScoringStats\kb_attri_2_values.txt";
                singleCellOccurrenceFilePath = @"D:\xuchu\TableStats\ExcelCorpus_10k\ScoringStats\singleCell_occur.txt";
                doubleCellOccurrenceFilepath = @"D:\xuchu\TableStats\ExcelCorpus_10k\ScoringStats\doubleCell_occur.txt";
                singleCellLanguageModelFilePath = @"D:\xuchu\TableStats\ExcelCorpus_10k\ScoringStats\singleCell_language_model.txt";

                //output
                outputTabular_MSA_directory = @"D:\xuchu\TableStats\ExcelCorpus_10k\Extracted_MSA\";  //the output of our algorithm
                outputTabular_Baseline_directory = @"D:\xuchu\TableStats\ExcelCorpus_10k\Extracted_Baseline\";  //the output of baseline
                outputTabular_Judie_directory = @"D:\xuchu\TableStats\ExcelCorpus_10k\Extracted_Judie\";
                logFile = @"D:\xuchu\TableStats\ExcelCorpus_10k\TableExtraction.log.csv";

                //other
                processingFilePath = @"D:\xuchu\TableStats\ExcelCorpus_10k\GoodFiles_Human.txt";
            }
            else if (whichExp == 2)
            {
                //input
                inputSequence_directory = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\Sequence\";      //the input sequences to be extracted
                inputTabular_directory = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\Tabular\";        // the groud truth table 


                //scoring stats
                kb_directory = @"D:\xuchu\TableStats\_Freebase\";
                kb_value2attrisFilePath = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\ScoringStats\kb_value_2_attris.txt";
                kb_attri2valuesFilePath = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\ScoringStats\kb_attri_2_values.txt";
                singleCellOccurrenceFilePath = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\ScoringStats\singleCell_occur.txt";
                doubleCellOccurrenceFilepath = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\ScoringStats\doubleCell_occur.txt";
                singleCellLanguageModelFilePath = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\ScoringStats\singleCell_language_model.txt";

                //output
                outputTabular_MSA_directory = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\Extracted_MSA\";  //the output of our algorithm
                outputTabular_Baseline_directory = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\Extracted_Baseline\";  //the output of baseline
                outputTabular_Judie_directory = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\Extracted_Judie\";
                logFile = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\TableExtraction.log.csv";

                //other
                processingFilePath = @"D:\xuchu\TableStats\General_WebTablesCorpus_10k\GoodFiles_Human.txt";

            }
            else if (whichExp == 3)
            {

                //input
                inputSequence_directory = @"D:\xuchu\TableStats\WebTablesCorpus_200k\Sequence\";      //the input sequences to be extracted
                inputTabular_directory = @"D:\xuchu\TableStats\WebTablesCorpus_200k\Tabular\";        // the groud truth table 


                //scoring stats
                kb_directory = @"D:\xuchu\TableStats\_Freebase\";
                kb_value2attrisFilePath = @"D:\xuchu\TableStats\WebTablesCorpus_200k\ScoringStats\kb_value_2_attris.txt";
                kb_attri2valuesFilePath = @"D:\xuchu\TableStats\WebTablesCorpus_200k\ScoringStats\kb_attri_2_values.txt";
                singleCellOccurrenceFilePath = @"D:\xuchu\TableStats\WebTablesCorpus_200k\ScoringStats\singleCell_occur.txt";
                doubleCellOccurrenceFilepath = @"D:\xuchu\TableStats\WebTablesCorpus_200k\ScoringStats\doubleCell_occur.txt";
                singleCellLanguageModelFilePath = @"D:\xuchu\TableStats\WebTablesCorpus_200k\ScoringStats\singleCell_language_model.txt";

                //output
                outputTabular_MSA_directory = @"D:\xuchu\TableStats\WebTablesCorpus_200k\Extracted_MSA\";  //the output of our algorithm
                outputTabular_Baseline_directory = @"D:\xuchu\TableStats\WebTablesCorpus_200k\Extracted_Baseline\";  //the output of baseline
                outputTabular_Judie_directory = @"D:\xuchu\TableStats\WebTablesCorpus_200k\Extracted_Judie\";
                logFile = @"D:\xuchu\TableStats\WebTablesCorpus_200k\TableExtraction.log.csv";

                //other
                processingFilePath = @"D:\xuchu\TableStats\WebTablesCorpus_200k\GoodFiles_Human.txt";


            }
            else if (whichExp == 4)
            {

                //input
                inputSequence_directory = @"D:\xuchu\TableStats\EnterpriseTables_10k\Sequence\";      //the input sequences to be extracted
                inputTabular_directory = @"D:\xuchu\TableStats\EnterpriseTables_10k\Tabular\";        // the groud truth table 


                //scoring stats
                kb_directory = @"D:\xuchu\TableStats\_Freebase\";
                kb_value2attrisFilePath = @"D:\xuchu\TableStats\EnterpriseTables_10k\ScoringStats\kb_value_2_attris.txt";
                kb_attri2valuesFilePath = @"D:\xuchu\TableStats\EnterpriseTables_10k\ScoringStats\kb_attri_2_values.txt";
                singleCellOccurrenceFilePath = @"D:\xuchu\TableStats\EnterpriseTables_10k\ScoringStats\singleCell_occur.txt";
                doubleCellOccurrenceFilepath = @"D:\xuchu\TableStats\EnterpriseTables_10k\ScoringStats\doubleCell_occur.txt";
                singleCellLanguageModelFilePath = @"D:\xuchu\TableStats\EnterpriseTables_10k\ScoringStats\singleCell_language_model.txt";

                //output
                outputTabular_MSA_directory = @"D:\xuchu\TableStats\EnterpriseTables_10k\Extracted_MSA\";  //the output of our algorithm
                outputTabular_Baseline_directory = @"D:\xuchu\TableStats\EnterpriseTables_10k\Extracted_Baseline\";  //the output of baseline
                outputTabular_Judie_directory = @"D:\xuchu\TableStats\EnterpriseTables_10k\Extracted_Judie\";
                logFile = @"D:\xuchu\TableStats\EnterpriseTables_10k\TableExtraction.log.csv";

                //other
                processingFilePath = @"D:\xuchu\TableStats\EnterpriseTables_10k\GoodFiles_Human.txt";


            }
            else if (whichExp == 10)
            {

                //input
                inputSequence_directory = @"F:\WebTablesCorpus_10k\Sequence\";      //the input sequences to be extracted
                inputTabular_directory = @"F:\WebTablesCorpus_10k\Tabular\";        // the groud truth table 


                //scoring stats
                kb_directory = @"F:\_Freebase\";
                kb_value2attrisFilePath = @"F:\WebTablesCorpus_10k\ScoringStats\kb_value_2_attris.txt";
                kb_attri2valuesFilePath = @"F:\WebTablesCorpus_10k\ScoringStats\kb_attri_2_values.txt";


                singleCellOccurrenceFilePath = @"F:\Stats\singleCell_occur_combined.txt_numwildcardFalse_joined.txt";
                doubleCellOccurrenceFilepath = @"F:\Stats\doubleCell_occur_combined.txt_numwildcardFalse_joined.txt";
                singleCellLanguageModelFilePath = @"F:\Stats\singleCell_occur_combined.txt_language_model.txt";

                //output
                outputTabular_MSA_directory = @"F:\WebTablesCorpus_10k\Extracted_MSA\";  //the output of our algorithm
                outputTabular_Baseline_directory = @"F:\WebTablesCorpus_10k\Extracted_Baseline\";  //the output of baseline
                outputTabular_Judie_directory = @"F:\WebTablesCorpus_10k\Extracted_Judie\";
                logFile = @"F:\WebTablesCorpus_10k\TableExtraction.log.csv";

                //other
                processingFilePath = @"F:\WebTablesCorpus_10k\GoodFiles_Human.txt";


            }
            else if (whichExp == 100)
            {
                //list exp
                Parameter.max_num_threads_per_machine = 1;
                //input
                inputSequence_directory = @"F:\WebLists\DumpManual\";      //the input sequences to be extracted
                inputTabular_directory = @"F:\WebLists\DumpManual_Ground\";        // the groud truth table 


                //scoring stats

                Parameter.singleCellOccurrenceFilePath = @"F:\WebLists\Stats2\alllists_single.txt_joined.txt";
                Parameter.doubleCellOccurrenceFilepath = @"F:\WebLists\Stats2\alllists_double.txt_joined.txt";
                Parameter.singleCellLanguageModelFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_language_model.txt";
                Parameter.kb_attri2valuesFilePath = @"F:\Running Results\NewStats\kb_attri_2_values.txt";
                Parameter.kb_value2attrisFilePath = @"F:\Running Results\NewStats\kb_value_2_attris.txt";

                //output
                outputTabular_MSA_directory = @"F:\WebLists\DumpManual_Extracted_MSA\";  //the output of our algorithm
                outputTabular_Baseline_directory = @"F:\WebLists\DumpManual_Extracted_Baseline\";  //the output of baseline
                outputTabular_Judie_directory = @"F:\WebLists\DumpManual_Extracted_Judie\";

                logFile = @"F:\WebLists\DumpManual_TableExtraction.log.csv";

                //other
                processingFilePath = @"F:\WebLists\DumpManual_GoodFiles_Human.txt";


            }
           


        }
       

        public static void setParameterForExcelAddin()
        {
            Parameter.singleCellOccurrenceFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_joined.txt";
            Parameter.doubleCellOccurrenceFilepath = @"F:\Running Results\NewStats\doubleCell_occur_combined.txt_joined_reduced.txt";
            Parameter.singleCellLanguageModelFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_language_model.txt";
            Parameter.kb_attri2valuesFilePath = @"F:\Running Results\NewStats\kb_attri_2_values.txt";
            Parameter.kb_value2attrisFilePath = @"F:\Running Results\NewStats\kb_value_2_attris.txt";
        }

    }
}
