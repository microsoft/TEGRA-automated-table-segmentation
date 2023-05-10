using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WebTableExtraction.Utils;

namespace WebTableExtraction.Experiments
{
    class DatasetAnalysis
    {
        /*
        "< [1-1.5] " 
                  "< [1.5-2.0] " 
                  "< [2.0-2.5] " 
                  "< [2.5-3.0] " 
                  "< [3.0-100] " 
         * */

        List<List<string>> partitions = new List<List<string>>();


        Dictionary<int, List<string>> partitions_rows = new Dictionary<int, List<string>>();

        int totalCells = 0;
        int totalNumericalCells = 0;

        int num_tables_with_many_same_cells = 0;
        double tables_with_many_same_cells_ave_tokens = 0;

        public DatasetAnalysis(string datasetfilepath)
        {

            GlobalScoringInfo.load();

            string[] tables = System.IO.File.ReadAllLines(datasetfilepath);
            for (int i = 1; i <= 5; i++ )
            {
                List<string> partition = new List<string>();
                partitions.Add(partition);
            }
            List<string> allpartitions = new List<string>();
                foreach (string table in tables)
                {
                    string table_name = table.Split('\t')[0];
                    string table_input = table.Split('\t')[1];
                    string table_ground = table.Split('\t')[2];

                    int numrows = num_rows(table_ground);
                    if (partitions_rows.ContainsKey(numrows))
                    {
                        partitions_rows[numrows].Add(table_ground);
                    }
                    else
                    {
                        List<string> newPar = new List<string>();
                        newPar.Add(table_ground);
                        partitions_rows[numrows] = newPar;
                    }

                    allpartitions.Add(table_ground);
                    double aveTokens = average_tokens_per_cell(table_ground);

                    if (aveTokens < 1.5)
                    {
                        partitions[0].Add(table_ground);
                    }
                    else if (aveTokens < 2.0)
                    {
                        partitions[1].Add(table_ground);
                    }
                    else if (aveTokens < 2.5)
                    {
                        partitions[2].Add(table_ground);
                    }
                    else if (aveTokens < 3.0)
                    {
                        partitions[3].Add(table_ground);
                    }
                    else if (aveTokens < 100)
                    {
                        partitions[4].Add(table_ground);
                    }

                    count_cells(table_ground);

                    if (table_many_same_cells(table_ground, 0.5))
                        num_tables_with_many_same_cells++;
                }

            Console.WriteLine("Dataset: " + datasetfilepath);

            Console.WriteLine("Total cells: " + totalCells);

            Console.WriteLine("Total numerical cells: " + totalNumericalCells + " percentage: " + (double)totalNumericalCells / totalCells);

            Console.WriteLine("Number of tables distinct cells less than 50% same cells " + num_tables_with_many_same_cells + " average tokens: " + tables_with_many_same_cells_ave_tokens / num_tables_with_many_same_cells);

            Console.WriteLine("Num files having ave tokens per cell: [1-1.5]: " + partitions[0].Count
                + " average number of rows: " + average_rows_count(partitions[0])
                + " average number of columns: " + average_columns_count(partitions[0])
                + " average occurenc count per cell for string cells: " + average_single_cell_occurrence_count(partitions[0]));

            Console.WriteLine("Num files having ave tokens per cell: [1.5-2]: " + partitions[1].Count
                 + " average number of rows: " + average_rows_count(partitions[1])
                  + " average number of columns: " + average_columns_count(partitions[1])
                + " average occurenc count per cell for string cells: " + average_single_cell_occurrence_count(partitions[1]));

            Console.WriteLine("Num files having ave tokens per cell: [2-2.5]: " + partitions[2].Count
                 + " average number of rows: " + average_rows_count(partitions[2])
                  + " average number of columns: " + average_columns_count(partitions[2])
                + " average occurenc count per cell for string cells: " + average_single_cell_occurrence_count(partitions[2]));

            Console.WriteLine("Num files having ave tokens per cell: [2.5-3]: " + partitions[3].Count
                 + " average number of rows: " + average_rows_count(partitions[3])
                  + " average number of columns: " + average_columns_count(partitions[3])
                + " average occurenc count per cell for string cells: " + average_single_cell_occurrence_count(partitions[3]));

            Console.WriteLine("Num files having ave tokens per cell: [3-100]: " + partitions[4].Count
                 + " average number of rows: " + average_rows_count(partitions[4])
                  + " average number of columns: " + average_columns_count(partitions[4])
                + " average occurenc count per cell for string cells: " + average_single_cell_occurrence_count(partitions[4]));

            //print_a_partition(System.IO.Directory.GetParent(datasetfilepath).ToString() + "/partition4/", partitions[4]);

            Console.WriteLine("average occurenc count per cell for string cells: " + average_single_cell_occurrence_count(allpartitions));


            foreach(int numRows in partitions_rows.Keys)
            {
                Console.WriteLine("Number of rows: " + numRows + " num tables: " + partitions_rows[numRows].Count);
            }


            column_count_distributino(allpartitions);

        }
        private Dictionary<string, double> ground_table_line2_ave_tokens = new Dictionary<string, double>();
        private double average_tokens_per_cell(string ground_table_line)
        {


            if (ground_table_line2_ave_tokens.ContainsKey(ground_table_line))
                return ground_table_line2_ave_tokens[ground_table_line];


            double result = 0;
            int numCells = 0;
            Table table = Table_Line_Conversion.line_2_table(ground_table_line);
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    numCells++;

                    result += cellValue.Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries).Count();

                }
            }
            result = result / numCells;
            ground_table_line2_ave_tokens[ground_table_line] = result;
            return result;
        }

        private int num_rows(string ground_table_line)
        {
            Table table = Table_Line_Conversion.line_2_table(ground_table_line);
            return table.getNumRecords();

        }

        private void count_cells(string ground_table_line)
        {
            Table table = Table_Line_Conversion.line_2_table(ground_table_line);
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();

                    double d1;
                    if (Double.TryParse(cellValue, out d1))
                    {
                        totalNumericalCells++;
                    }
                    totalCells++;
                }
            }
        }


        private bool table_many_same_cells(string ground_table_line, double percent)
        {
            int myTotal = 0;
            HashSet<string> myCells = new HashSet<string>();
            Table table = Table_Line_Conversion.line_2_table(ground_table_line);
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();

                    myCells.Add(cellValue);
                    myTotal++;
                }
            }

            double temp = average_tokens_per_cell(ground_table_line);
            if(temp > 2)
            {

            }
            tables_with_many_same_cells_ave_tokens += temp;

            if ((double)myCells.Count / myTotal  < percent)
                return true;
            else
                return false;
        }
    
    
    
    
    
        private int subset_tables_with_many_same_cells(List<string> inputtables)
        {
            List<string> outputTables = new List<string>();

            foreach(string table in inputtables)
            {
                if(table_many_same_cells(table,0.5))
                {
                    outputTables.Add(table);
                }
            }
            return outputTables.Count;
        }
    

    
      
        private double average_single_cell_occurrence_count(List<string> inputtables)
        {
            double all_occurrence_count = 0;
            int myTotalCells = 0;
            foreach(string ground_table_line in inputtables)
            {
                Table table = Table_Line_Conversion.line_2_table(ground_table_line);
                for (int i = 0; i < table.getNumRecords(); i++)
                {
                    for (int j = 0; j < table.getNumCols(); j++)
                    {
                        string cellValue = table.getCell(i, j).getCellValue();
                        double d1;
                        if (Double.TryParse(cellValue, out d1))
                        {
                            continue;
                        }
                        all_occurrence_count += GlobalScoringInfo.getOccurCount(cellValue);
                        myTotalCells++;
                    }
                }
            }
            return all_occurrence_count / myTotalCells;
           
        }
        private double average_rows_count(List<string> inputtables)
        {
            double totalNumRows = 0;
            foreach (string ground_table_line in inputtables)
            {
                Table table = Table_Line_Conversion.line_2_table(ground_table_line);
                totalNumRows += table.getNumRecords();
            }
            return totalNumRows / inputtables.Count;
        }
        private double average_columns_count(List<string> inputtables)
        {
            double totalNumColumns = 0;
            foreach (string ground_table_line in inputtables)
            {
                Table table = Table_Line_Conversion.line_2_table(ground_table_line);
                totalNumColumns += table.getNumCols();
            }
            return totalNumColumns / inputtables.Count;
        }

        private void column_count_distributino(List<string> inputtables)
        {
            Dictionary<int, int> column_2_Numbers = new Dictionary<int, int>();
            int maxNumCols = 0;
            foreach (string ground_table_line in inputtables)
            {
                Table table = Table_Line_Conversion.line_2_table(ground_table_line);
                int numCols = table.getNumCols();
                if (numCols > maxNumCols)
                    maxNumCols = numCols;
                if (column_2_Numbers.ContainsKey(numCols))
                    column_2_Numbers[numCols] = column_2_Numbers[numCols] + 1;
                else
                    column_2_Numbers[numCols] = 1;

            }
            for(int i = 1; i <= 20; i++)
            {
                Console.WriteLine("Number of tables with " + i + " number of columsn is: " + (column_2_Numbers.ContainsKey(i) ? column_2_Numbers[i] : 0));
            }
        }

        private void print_a_partition(string dir, List<string> inputtables)
        {
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            int i = 0;
            foreach (string ground_table_line in inputtables)
            {
                Table table = Table_Line_Conversion.line_2_table(ground_table_line);
                table.printTableToFile(dir + @"table_partition_" + i + ".txt");
                i++;
            }
        }




        public static  void compare_MRA_ListExtract(string mra_result, string list_extract_result, string ground,
            string output_Dir)
        {

            if (!System.IO.Directory.Exists(output_Dir))
                System.IO.Directory.CreateDirectory(output_Dir);

            Dictionary<string, Table> tableName_2_ground = new Dictionary<string, Table>();
            Dictionary<string, Table> tablename_2_MRA = new Dictionary<string, Table>();
            Dictionary<string, Table> tablename_2_ListEx = new Dictionary<string, Table>();

            
            foreach(string line in System.IO.File.ReadAllLines(mra_result))
            {
                string table_name = line.Split('\t')[0];
                string table_result = line.Split('\t')[1];
                Table table = Table_Line_Conversion.line_2_table(table_result);
                tablename_2_MRA[table_name] = table;
            }
            foreach (string line in System.IO.File.ReadAllLines(list_extract_result))
            {
                string table_name = line.Split('\t')[0];
                string table_result = line.Split('\t')[1];
                Table table = Table_Line_Conversion.line_2_table(table_result);
                tablename_2_ListEx[table_name] = table;
            }
            foreach (string line in System.IO.File.ReadAllLines(ground))
            {
                string table_name = line.Split('\t')[0];
                string table_result = line.Split('\t')[2];
                Table table = Table_Line_Conversion.line_2_table(table_result);
                tableName_2_ground[table_name] = table;
            }
            int[] recall_winners = new int[3];
             int[] precision_winners = new int[3];
             int[] fmeasure_winners = new int[3];
            int i = 0;
            double total_precision_mra = 0;
            double total_precision_listex = 0;
            double total_recall_mra = 0;
            double total_recall_listex = 0;
            double total_fmeasure_mra = 0;
            double total_fmeasure_listex = 0;
            foreach (string table_name in tablename_2_MRA.Keys)
            {
                
                Table ground_table = tableName_2_ground[table_name];
                Table mra_table = tablename_2_MRA[table_name];
                Table listex_table = tablename_2_ListEx[table_name];

                if(mra_table.getNumCols() == 1)
                {
                    string xx = ground_table.toString();
                }
                
                i++;

                TableComparator tc_mra = new TableComparator(mra_table, ground_table);
                TableComparator tc_listex = new TableComparator(listex_table, ground_table);

                total_precision_mra += tc_mra.flex_precision;
                total_precision_listex += tc_listex.flex_precision;

                total_recall_mra += tc_mra.flex_recall;
                total_recall_listex += tc_listex.flex_recall;

                total_fmeasure_mra += tc_mra.flex_fmeasure;
                total_fmeasure_listex += tc_listex.flex_fmeasure;

                if (average_tokens_per_cell(ground_table) >= 2 && ground_table.getNumCols() >=4)
                {
                    if (tc_mra.flex_fmeasure == 1)
                    {

                    }
                }
                

                
                if (tc_mra.flex_recall > tc_listex.flex_recall)
                {
                    recall_winners[0]++;
                }
                else if (tc_mra.flex_recall < tc_listex.flex_recall)
                {
                    recall_winners[1]++;
                    Console.WriteLine(i + " Precision, Recall, F-measure of MRA is: " + tc_mra.flex_precision
                           + "\t" + tc_mra.flex_recall + "\t" + tc_mra.flex_fmeasure);
                    Console.WriteLine(i + " Precision, Recall, F-measure ListEx is: " + tc_listex.flex_precision
                        + "\t" + tc_listex.flex_recall + "\t" + tc_listex.flex_fmeasure);


                    mra_table.printTableToFile(output_Dir + @"/" + i + "_MRA.txt");
                    ground_table.printTableToFile(output_Dir + @"/" + i + "_Ground.txt");
                    listex_table.printTableToFile(output_Dir + @"/" + i + "_ListEx.txt");
                }
                else
                {
                    recall_winners[2]++;
                }
               
                if (tc_mra.flex_precision > tc_listex.flex_precision)
                {
                    precision_winners[0]++;
                }
                else if (tc_mra.flex_precision < tc_listex.flex_precision)
                {
                    precision_winners[1]++;
                }
                else
                {
                    precision_winners[2]++;
                }

               
                if (tc_mra.flex_fmeasure > tc_listex.flex_fmeasure)
                {
                    fmeasure_winners[0]++;
                }
                else if (tc_mra.flex_fmeasure < tc_listex.flex_fmeasure)
                {
                    fmeasure_winners[1]++;
                }
                else
                {
                    fmeasure_winners[2]++;
                }
            }
            Console.WriteLine("Recall winners MRA:ListExtract:Draw: " + recall_winners[0] + " : " + recall_winners[1] + " : " + recall_winners[2]);

            Console.WriteLine("Precision winners MRA:ListExtract:Draw: " + precision_winners[0] + " : " + precision_winners[1] + " : " + precision_winners[2]);

            Console.WriteLine("fmeasure winners MRA:ListExtract:Draw: " + fmeasure_winners[0] + " : " + fmeasure_winners[1] + " : " + fmeasure_winners[2]);

            Console.WriteLine("AVE Precision, Recall, F measure, MRA: "  + total_precision_mra / i
                + "\t" + total_recall_mra / i
                + "\t" + total_fmeasure_mra / i);

            Console.WriteLine("AVE Precision, Recall, F measure, ListEx: " + total_precision_listex / i
              + "\t" + total_recall_listex /i
              + "\t" + total_fmeasure_listex / i);

        }

        private static double average_tokens_per_cell(Table table)
        {
            double result = 0;
            int numCells = 0;
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    numCells++;

                    result += cellValue.Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries).Count();

                }
            }
            result = result / numCells;
            return result;
        }





    }
}
