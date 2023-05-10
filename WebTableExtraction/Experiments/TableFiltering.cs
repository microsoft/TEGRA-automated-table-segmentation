using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebTableExtraction.Utils;

namespace WebTableExtraction.Experiments
{
    class TableFiltering
    {

        public static bool tableCleanUp(Table table)
        {
            


            //(4) Num of rows [5,1000]
            if (table.getNumRecords() < 7 || table.getNumRecords() > 52)
                return false;

            //throw awasy the first and last row
            table.delete(0);
            table.delete(table.getNumRecords() - 1);

            //(1) Have to be a valid table
            if (!table.isValidTable())
                return false;

            //(2) Non English chars
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    foreach (char c in cellValue)
                    {
                        if (c >= 256)
                        {

                            return false;
                        }
                       
                    }
                }
            }

            //(3) more than 5 tokens on any column
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                Record record = table.getRecord(i);

                for (int j = 0; j < record.getNumCells(); j++)
                {
                    string cellValue = record.getCell(j).getCellValue();
                    int numTokens = cellValue.Split(Seperator.getSeperators(), StringSplitOptions.None).Count();
                    if (numTokens > Parameter.maxTokensPerColumn)
                        return false;
                }

            }


            //(5) any column is more than 50 percent null

            for (int j = 0; j < table.getNumCols(); j++)
            {
                int numNULL = 0;
                for (int i = 0; i < table.getNumRecords(); i++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    if (cellValue == null || cellValue == "")
                        numNULL++;
                }
                if (numNULL >= 0.5 * table.getNumRecords())
                    return false;
            }


           //(6) all numbers
            bool allNumbers = true;
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    double d;
                    if (!Double.TryParse(cellValue, out d))
                    {
                        allNumbers = false;
                    }
                }
            }


            if (allNumbers)
                return false;


            //(7): all nulls in any row
            bool allNullsInARow = false;
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                bool allNUllsCurRow = true;
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    if (cellValue != "")
                    {
                        allNUllsCurRow = false;
                        break;
                    }
                }
                if (allNUllsCurRow)
                    allNullsInARow = true;
            }
            if (allNullsInARow)
                return false;

            return true;
        }

        public static bool runTime_good_table(Table table)
        {
            //***************
            //Some additional concerns
            //*************
            //If the table contains "|", get rid of it, causing splitting problem
            //If the table has more than 50 columns, get rid of it, causing out of memory errors
            if (table.getNumCols() > 20)
                return false;


            if (table.getNumCols() > 20)
                return false;

            if (table.getNumCols() == 1)
                return false;

            //(2) Non English chars
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                for (int j = 0; j < table.getNumCols(); j++)
                {
                    string cellValue = table.getCell(i, j).getCellValue();
                    if (cellValue.Contains("__________"))
                        return false;
                    foreach (char c in cellValue)
                    {
                        if (c >= 256)
                        {

                            return false;
                        }
                        if (c == '|')
                        {
                            return false;
                        }
                        
                        if (c == '\t')
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
