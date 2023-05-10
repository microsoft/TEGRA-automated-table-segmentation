using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WebTableExtraction.Utils;
using WebTableExtraction.Extraction;
using WebTableExtraction.Baseline;
using WebTableExtraction.Judie;

namespace WebTableExtraction.Experiments
{
    class RunningExample
    {

        public RunningExample(string datasetFilePath)
        {
            Parameter.singleCellOccurrenceFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_joined.txt";
            Parameter.doubleCellOccurrenceFilepath = @"F:\Running Results\NewStats\doubleCell_occur_combined.txt_joined.txt";
            Parameter.singleCellLanguageModelFilePath = @"F:\Running Results\NewStats\singleCell_occur_combined.txt_language_model.txt";
            Parameter.kb_attri2valuesFilePath = @"F:\Running Results\NewStats\kb_attri_2_values.txt";
            Parameter.kb_value2attrisFilePath = @"F:\Running Results\NewStats\kb_value_2_attris.txt";


            chooseTable(datasetFilePath);
        }

        public void chooseTable(string datasetFilePath)
        {
            string[] tables = File.ReadAllLines(datasetFilePath);
            int i = 1;
            foreach (string table in tables)
            {
                string[] splits = table.Split('\t');
                string table_name = splits[0];
                string input_table_line = splits[1];
                string ground_table_line = splits[2];
          
                
                Table ground_table = Table_Line_Conversion.line_2_table(ground_table_line);

                if (!good_for_example(ground_table))
                    continue;

               
                //do some filtering based on groud_table
                Table mra_table = do_one_table(input_table_line, ground_table_line, "MRA");
                Table listex_table = do_one_table(input_table_line, ground_table_line, "ListExtract");


                TableComparator mra_tc = new TableComparator(mra_table, ground_table);
                TableComparator listex_tc = new TableComparator(listex_table, ground_table);

                if(mra_tc.flex_fmeasure - listex_tc.flex_fmeasure > 0.3)
                {
                    Console.WriteLine("take a look");
                    ground_table.printTableToFile(@"F:\Running Results\RunningExample\table_" + i + ".txt");
                }
                i++;
            }

        }

        public bool good_for_example(Table table)
        {
            //not too big

            if (table.getNumCols() != 4)
                return false;


            int num_string_cells = 0;
            int num_numerical_cells = 0;
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    double d;
                    if (!Double.TryParse(cellValue, out d))
                    {
                        num_string_cells++;
                    }
                    else
                    {
                        num_numerical_cells++;
                    }
                }
            }

            //I need more string cells
            if (num_numerical_cells > num_string_cells)
                return false;



            //average tokens per col
            double totalTokens = 0;
            int totalNumCells = 0;


            for (int j = 0; j < table.getNumCols(); j++) 
            {
                double aveTokens = 0;
                int numCells = 0;
                for (int i = 0; i < table.getNumRecords(); i++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    numCells++;

                    aveTokens += cellValue.Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries).Count();

                    totalTokens += cellValue.Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries).Count();
                    totalNumCells++;
                }
                aveTokens = aveTokens / numCells;
                if (aveTokens > 2)
                    return false;
            }

            totalTokens = totalTokens / totalNumCells;
            //if (totalTokens < 1.6)
              //  return false;




            return true;

        }



        public Table do_one_table(string input_table_line, string ground_table_line, string algorithm)
        {
            List<Line> lines = new List<Line>();
            foreach (string temp in input_table_line.Split(new string[] { "__________" }, StringSplitOptions.None))
            {
                Line line = new Line(temp);
                lines.Add(line);
            }
            List<Record> records = new List<Record>();
            foreach (string temp in ground_table_line.Split(new string[] { "__________" }, StringSplitOptions.None))
            {
                Record record = new Record(temp);
                records.Add(record);
            }
            int numCols = records[0].getNumCells();
            Dictionary<Line, Record> examples = new Dictionary<Line, Record>();

            Parameter.numColsGiven = false;
            Parameter.numExamples = 0;
            Parameter.s_languageWeight = 0;
            Parameter.s_occurWeight = 0;
            Parameter.s_typeWeight = 0;
            Parameter.d_occurWeight = 0.5;
            Parameter.d_syntacticWeight = 0.5;
            Parameter.max_num_threads_per_machine = 0;

            Table aligned_table = null;
            if (algorithm == "MRA")
            {
                //Do the MSA
                MSAAppro msaAppro = new MSAAppro(lines, examples);
                if (Parameter.numColsGiven)
                    msaAppro.alignAStar_with_numcols(numCols);
                else
                    msaAppro.alignAStar_without_numcols();
                aligned_table = msaAppro.getExtracted_Table();
            }
            else if (algorithm == "ListExtract")
            {
                Baseline.Baseline baseline = new Baseline.Baseline(lines, examples);
                if (Parameter.numColsGiven)
                    baseline.align(numCols);
                else
                    baseline.align();
                aligned_table = baseline.get_Extracted_Table();
            }
            else if (algorithm == "Judie")
            {
                Judie.Judie judie = new Judie.Judie(lines, examples);
                if (Parameter.numColsGiven)
                    judie.align(numCols);
                else
                    judie.align();
                aligned_table = judie.get_Extracted_Table();
            }
            return aligned_table;

        }

        

    }
}
