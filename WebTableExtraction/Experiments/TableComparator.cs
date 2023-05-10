using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTableExtraction.Utils;
using System.IO;

namespace WebTableExtraction.Experiments
{
    class TableComparator
    {
        private string extractedTableFilePath;
        private string goldenTableFilePath;

        

        private Table extractedTable;
        private Table goldenTable;



        //For each records, get the maximum number of matching cells
        //These are for number of columns are the same for result and ground table
        public double maxCellPrecision; 
        //compare each cell exactly
        public double cellPrecision;


        //The precision and recall
        public double flex_precision;
        public double flex_recall;
        public double flex_fmeasure;

        private double num_total_matches = 0;
        private int num_1_to_1_matches = 0;
        private int num_n_to_1_matches = 0;
        
     

        public TableComparator(string extractedTableFilePath, string goldenTableFilePath)
        {
            this.extractedTableFilePath = extractedTableFilePath;
            this.goldenTableFilePath = goldenTableFilePath;
          
            
            if(File.Exists(extractedTableFilePath))
            {
                this.extractedTable = new Table(extractedTableFilePath);
                this.goldenTable = new Table(goldenTableFilePath);

                removeSeperators(extractedTable);
                removeSeperators(goldenTable);

                compare();
                compare_flex3();
            }
            
        }
        public TableComparator(Table extracted_Table, Table ground_Table)
        {
           
                this.extractedTable = extracted_Table;
                this.goldenTable = ground_Table;

                if(extracted_Table != null)
                {

                    removeSeperators(extractedTable);
                    removeSeperators(goldenTable);


                    compare();
                    compare_flex3();
                }
               
            
            
        }

        //remove seperators
        private void removeSeperators(Table table)
        {
            for(int i = 0; i < table.getNumRecords(); i++)
            {
                for(int j = 0; j < table.getNumCols(); j++)
                {
                    Cell cell = table.getCell(i, j);
                    string value = cell.getCellValue();
                    string newValue = value;
                    foreach(string sep in Seperator.getSeperators())
                    {
                        newValue = newValue.Replace(sep, " ");
                    }
                    cell.setCellValue(newValue);
                }
            }
        }

       

        /*
        private void compare_flex()
        {
            int numRows = extractedTable.getNumRecords();

            int numCols_extracted = extractedTable.getNumCols();
            int numCols_golden = goldenTable.getNumCols();
            System.Diagnostics.Debug.Assert(numRows == goldenTable.getNumRecords());

            int precision_count = 0;
            int recall_count = 0;
            
            for (int i = 0; i < numRows; i++)
            {
                Record record = extractedTable.getRecord(i);
                Record goldRecord = goldenTable.getRecord(i);

                 if (!record.isSame(goldRecord))
                 {

                 }

                Dictionary<int, int> tokenID2ColID_extracted = new Dictionary<int, int>();
                Dictionary<int, int> tokenID2Col2ID_golden = new Dictionary<int, int>();
                int curTokenID_extracted = 0;
                for(int j = 0; j < numCols_extracted; j++)
                {
                    Cell cell = extractedTable.getCell(i, j);
                    string[] tokens = cell.getCellValue().Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries);
                    foreach(string token in tokens)
                    {
                        tokenID2ColID_extracted[curTokenID_extracted] = j;
                        curTokenID_extracted++;
                    }
                }
                int curTokenID_golden = 0;
                for(int j = 0;j < numCols_golden; j++)
                {
                    Cell cell = goldenTable.getCell(i, j);
                    string[] tokens = cell.getCellValue().Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string token in tokens)
                    {
                        tokenID2Col2ID_golden[curTokenID_golden] = j;
                        curTokenID_golden++;
                    }
                }
                System.Diagnostics.Debug.Assert(curTokenID_extracted == curTokenID_golden);
                //count precision
                curTokenID_extracted = 0;
                for (int j = 0; j < numCols_extracted; j++)
                {
                    Cell cell = extractedTable.getCell(i, j);
                    string[] tokens = cell.getCellValue().Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries);
                    HashSet<int> mappingCols = new HashSet<int>();
                    foreach (string token in tokens)
                    {
                        mappingCols.Add(tokenID2Col2ID_golden[curTokenID_extracted]);
                        curTokenID_extracted++;
                    }
                    if (mappingCols.Count() <= 1)
                        precision_count++;
                }

                //count recall
                curTokenID_golden = 0;
                for(int j = 0; j < numCols_golden; j++)
                {
                    Cell cell = goldenTable.getCell(i, j);
                    string[] tokens = cell.getCellValue().Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries);
                    HashSet<int> mappingCols = new HashSet<int>();
                    foreach (string token in tokens)
                    {
                        mappingCols.Add(tokenID2ColID_extracted[curTokenID_golden]);
                        curTokenID_golden++;
                    }
                    if (mappingCols.Count() <= 1)
                        recall_count++;
                }
            }
            flex_precision = (double)precision_count / (numRows * numCols_extracted);
            flex_recall = (double)recall_count / (numRows * numCols_golden);
            if (flex_precision == 0 || flex_recall == 0)
                flex_fmeasure = 0;
            else
                flex_fmeasure = 2 * flex_precision * flex_recall / (flex_precision + flex_recall);
        }
         */ 

        /**
         * Google's conversative approach
         */ 
        /*
        private void compare_flex2()
        {
            
            int numRows = extractedTable.getNumRecords();

            int numCols_extracted = extractedTable.getNumCols();
            int numCols_golden = goldenTable.getNumCols();
            System.Diagnostics.Debug.Assert(numRows == goldenTable.getNumRecords());


            int total_matches = 0;


            Dictionary<string, int> colpair2Matches = new Dictionary<string, int>();
            for (int k1 = 0; k1 < numCols_extracted; k1++)
            {
                for(int k2 = 0; k2< numCols_golden; k2++)
                {
                    string colpair = k1 + "," + k2;
                    int matches = 0;
                    for (int i = 0; i < numRows; i++)
                    {
                        Record record = extractedTable.getRecord(i);
                        Record goldRecord = goldenTable.getRecord(i);
                        if (record.getCell(k1).getCellValue() == goldRecord.getCell(k2).getCellValue())
                            matches++;
                    }
                    colpair2Matches[colpair] = matches;
                }
            }

            List<string> colPairsSorted = colpair2Matches.Keys.OrderByDescending(x => colpair2Matches[x]).ToList();
            while(colPairsSorted.Count() > 0)
            {
                string colPair = colPairsSorted[0];
                string k1 = colPair.Split(',')[0];
                string k2 = colPair.Split(',')[1];

                total_matches += colpair2Matches[colPair];

                //remove all overlapping col pairs
                List<string> toBeRemoved = new List<string>();
                foreach(string temp in colPairsSorted)
                {
                    string tempK1 = temp.Split(',')[0];
                    string tempK2 = temp.Split(',')[1];
                    if (k1 == tempK1 || k2 == tempK2)
                        toBeRemoved.Add(temp);
                }
                foreach (string temp in toBeRemoved)
                    colPairsSorted.Remove(temp);
            }

            flex_precision = (double)total_matches / (numRows * numCols_extracted);


            flex_recall = (double)total_matches / (numRows * numCols_golden);
            if (flex_precision == 0 || flex_recall == 0)
                flex_fmeasure = 0;
            else
                flex_fmeasure = 2 * flex_precision * flex_recall / (flex_precision + flex_recall);
        }
        */
        private void compare_flex3()
        {

            int numRows = extractedTable.getNumRecords();

            int numCols_extracted = extractedTable.getNumCols();
            int numCols_golden = goldenTable.getNumCols();
            System.Diagnostics.Debug.Assert(numRows == goldenTable.getNumRecords());


            double total_matches = max_number_matches_flex_number_cols();
            num_total_matches = total_matches;

            flex_precision = (double)total_matches / (numRows * numCols_extracted);


            flex_recall = (double)total_matches / (numRows * numCols_golden);
            if (flex_precision == 0 || flex_recall == 0)
                flex_fmeasure = 0;
            else
                flex_fmeasure = 2 * flex_precision * flex_recall / (flex_precision + flex_recall);
        }



        private double max_number_matches_flex_number_cols()
        {
            
            int numRows = extractedTable.getNumRecords();

            int numCols_extracted = extractedTable.getNumCols();
            int numCols_golden = goldenTable.getNumCols();

            double[,] matches = new double[numCols_extracted + 1, numCols_golden + 1];
            for (int i = 0; i <= numCols_extracted; i++)
            {
                matches[i, 0] = 0;
            }
            for (int j = 0; j <= numCols_golden; j++)
            {
                matches[0, j] = 0;
            }
            for (int i = 1; i <= numCols_extracted; i++)
            {
                for (int j = 1; j <= numCols_golden; j++)
                {
                    double ijBest = -1;
                    for (int m = 1; m <= i; m++)
                    {
                        for (int n = 1; n <= j; n++)
                        {
                            //[m,i] for extracted
                            //[n-j] for golend
                            
                            double curNumMatches = matches[m - 1, n - 1] + max_number_matches_flex_number_cols_helper(m-1, i-1, n-1, j-1);
                            if (curNumMatches > ijBest)
                            {
                                ijBest = curNumMatches;
                            }
                        }
                    }
                    matches[i, j] = ijBest;
                }
            }

            return matches[numCols_extracted, numCols_golden];
        }

        private double max_number_matches_flex_number_cols_helper(int m, int i, int n, int j)
        {

            if (i - m + 1 > 1 && j - n + 1 > 1)
                return 0;

            double result = 0;
            int numRows = extractedTable.getNumRecords();
            

            for (int row = 0; row < numRows; row++)
            {
                Record record = extractedTable.getRecord(row);
                Record goldRecord = goldenTable.getRecord(row);
                StringBuilder sb1 = new StringBuilder();
                for (int x = m; x <= i; x++)
                {
                    sb1.Append(record.getCell(x).getCellValue() + " ");
                }
                StringBuilder sb2 = new StringBuilder();
                for (int y = n; y <= j; y++)
                {
                    sb2.Append(goldRecord.getCell(y).getCellValue() + " ");
                }
                if (sb1.ToString() == sb2.ToString())
                {
                   
                    if (i - m + 1 > 1 || j - n + 1 > 1)
                    {
                        result += 1;
                    }
                    else
                        result++;
                }
                    
            }
            return result;
        }

        public double get_flex_precision_penalize_n_1_matches(double penalty)
        {
            double penalized_matches = num_1_to_1_matches + penalty * num_n_to_1_matches;

            int numRows = extractedTable.getNumRecords();

            int numCols_extracted = extractedTable.getNumCols();
            int numCols_golden = goldenTable.getNumCols();
            System.Diagnostics.Debug.Assert(numRows == goldenTable.getNumRecords());




            double penalized_flex_precision = (double)penalized_matches / (numRows * numCols_extracted);
            return penalized_flex_precision;

           
        }
        public double get_flex_recall_penalize_n_1_matches(double penalty)
        {
            double penalized_matches = num_1_to_1_matches + penalty * num_n_to_1_matches;

            int numRows = extractedTable.getNumRecords();

            int numCols_extracted = extractedTable.getNumCols();
            int numCols_golden = goldenTable.getNumCols();
            System.Diagnostics.Debug.Assert(numRows == goldenTable.getNumRecords());


            double penalized_flex_recall = (double)penalized_matches / (numRows * numCols_golden);
            return penalized_flex_recall;
         
        }
        public double get_flex_fmeasure_penalize_n_1_matches(double penalty)
        {
            double penalized_matches = num_1_to_1_matches + penalty * num_n_to_1_matches;

            int numRows = extractedTable.getNumRecords();

            int numCols_extracted = extractedTable.getNumCols();
            int numCols_golden = goldenTable.getNumCols();
            System.Diagnostics.Debug.Assert(numRows == goldenTable.getNumRecords());




            double penalized_flex_precision = (double)penalized_matches / (numRows * numCols_extracted);
          

            double penalized_flex_recall = (double)penalized_matches / (numRows * numCols_golden);
          
            if (penalized_flex_precision == 0 || penalized_flex_recall == 0)
                return 0;
            else
                return  (2 * penalized_flex_precision * penalized_flex_recall) / (penalized_flex_precision + penalized_flex_recall);
        }


        private void compare()
        {

            

            int numRows = extractedTable.getNumRecords();
            int numCols = extractedTable.getNumCols();

            if (numCols != goldenTable.getNumCols())
            {
                cellPrecision = 0;
                maxCellPrecision = 0;
                return;
            }

            System.Diagnostics.Debug.Assert(numRows == goldenTable.getNumRecords());
            System.Diagnostics.Debug.Assert(numCols == goldenTable.getNumCols());

            int numDifferentRecords = 0;
            int numDifferentCells = 0;

            int maxNumSameCells = 0;

            for (int i = 0; i < numRows; i++)
            {
                Record record = extractedTable.getRecord(i);
                Record goldRecord = goldenTable.getRecord(i);

                maxNumSameCells += maxMatching(goldRecord, record);

                if (!record.isSame(goldRecord))
                    numDifferentRecords++;

                for (int j = 0; j < numCols; j++)
                {
                    Cell cell = extractedTable.getCell(i, j);
                    Cell goldCell = goldenTable.getCell(i, j);
                    if (!cell.isSame(goldCell))
                        numDifferentCells++;
                }
            }

            cellPrecision = 1.0 - (double)(numDifferentCells) / (numRows * numCols);
           

            System.Diagnostics.Debug.Assert(maxNumSameCells >= numRows * numCols - numDifferentCells);
            maxCellPrecision = (double)maxNumSameCells / (numRows * numCols);


            
            
        }


        

        private int maxMatching(Record goldRecord, Record record)
        {
            
            Dictionary<string, int> processed = new Dictionary<string, int>();
            return maxMatchingHelper(goldRecord, record, 0, 0, processed);
        }



        private int maxMatchingHelper(Record goldRecord, Record record, int startGoldenIndex, int startRecordIndex,  Dictionary<string, int> processed)
        {

            if (startGoldenIndex >= goldRecord.getNumCells() || startRecordIndex >= record.getNumCells())
                return 0;

            if (processed.ContainsKey(startGoldenIndex + "," + startRecordIndex))
            {
                return processed[startGoldenIndex + "," + startRecordIndex];
            }

            //Get the matching indexes from goldRecord
            HashSet<int> matchingIndexes = new HashSet<int>();
            for (int i = startGoldenIndex; i < goldRecord.getNumCells(); i++)
            {
                if (goldRecord.getCell(i).isSame(record.getCell(startRecordIndex)))
                {
                    matchingIndexes.Add(i);
                    break;
                }
                    
            }
            //Match startRecordIndex, I  only need to do the first match
            int maxNumMatches = 0;
            System.Diagnostics.Debug.Assert(matchingIndexes.Count() <= 1);
            foreach (int matchingIndex in matchingIndexes)
            {
                int numMatches = 1 + maxMatchingHelper(goldRecord, record, matchingIndex + 1, startRecordIndex + 1,processed);
                if (numMatches > maxNumMatches)
                {
                    maxNumMatches = numMatches;
                }
            }
            //Do not match startRecordIndex
            int numMatches1 = maxMatchingHelper(goldRecord, record, startGoldenIndex, startRecordIndex + 1,processed);
            if (numMatches1 > maxNumMatches)
            {
                maxNumMatches = numMatches1;
            }

            processed[startGoldenIndex + "," + startRecordIndex] = maxNumMatches;

            return maxNumMatches;

            
        }

        
        public double getExactCellPrecision()
        {
            return cellPrecision;
        }
        public double getMaxCellPrecision()
        {
            return maxCellPrecision;
        }



    }
}
