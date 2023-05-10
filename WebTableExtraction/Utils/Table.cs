using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTableExtraction.Utils
{
    public class Table
    {
        private List<Record> records = new List<Record>();



        public Table(string inputFilePath)
        {
            string[] temps = System.IO.File.ReadAllLines(inputFilePath);
            foreach (string temp in temps)
            {
                Record record = new Record(temp);
                records.Add(record);
            }
        }

        public Table(List<Record> records)
        {
            this.records = records;
        }

        public void addTuple(Record t)
        {
            records.Add(t);
        }

        public void delete(int index)
        {
            records.RemoveAt(index);
        }

        public int getNumRecords()
        {
            return records.Count();
        }

        public int getNumCols()
        {
            int numCols = 0;
            foreach (Record record in records)
            {
                int curNumCols = record.getNumCells();
                if (numCols == 0)
                {
                    numCols = curNumCols;
                }

                if (numCols != 0 && numCols != curNumCols)
                {
                    Console.WriteLine("The table is not properly formatted, there exist two rows with different number of columns");
                    //System.Diagnostics.Debug.Assert(false);
                    
                }
                
            }
            return numCols;
        }
        public bool isValidTable()
        {
            int numCols = 0;
            foreach (Record record in records)
            {
                int curNumCols = record.getNumCells();
                if (numCols == 0)
                {
                    numCols = curNumCols;
                }

                if (numCols != 0 && numCols != curNumCols)
                {
                    return false;
                }

            }
            return true;
        }

        public List<Record> getRecords()
        {
            return records;
        }

        public Record getRecord(int index)
        {
            return records.ElementAt(index);
        }

        public Cell getCell(int rowNum, int colNum)
        {
            return records.ElementAt(rowNum).getCell(colNum);
        }

        public string toString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Record tuple in records)
            {
                sb.AppendLine(tuple.toString());

            }
            return sb.ToString();
        }

        public void printTableToFile(String outputFile)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@outputFile))
            {
                foreach (Record tuple in records)
                {
                    file.WriteLine(tuple.toString());
                   
                }
                file.Close();
                
            }

        }
        public void printTableConcatenatedToFile(String outputFile)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@outputFile))
            {
                foreach (Record tuple in records)
                {
                    StringBuilder sb = new StringBuilder();
                    
                    for (int i = 0; i < tuple.getNumCells(); i++)
                    {
                        Cell cell = tuple.getCell(i);
                        if (i != tuple.getNumCells() - 1)
                        {
                            sb.Append(cell.getCellValue());
                            sb.Append(" ");
                        }
                        else
                        {
                            sb.Append(cell.getCellValue());

                        }

                    }

                
                    file.WriteLine(sb);

                }
                file.Close();

            }

        }


        public Dictionary<int, double> col2Score = new Dictionary<int, double>();
        public double getAvePerColumnScore(LocalScoringInfo localScoring)
        {
            //col 2 score
            col2Score = new Dictionary<int, double>();
            for (int j = 0; j < getNumCols(); j++)
            {
                double colJScore = 0;
                foreach (Record r1 in records)
                {
                    foreach (Record r2 in records)
                    {
                        if (r1 == r2)
                            continue;
                        else
                        {
                            colJScore += localScoring.doubleCellScore(r1.getCell(j).getCellValue(), r2.getCell(j).getCellValue());
                        }
                    }
                }
                col2Score[j] = colJScore / (getNumRecords() * (getNumRecords() - 1));
            }



            double average = 0;
            for(int j = 0; j < getNumCols(); j++)
            {
                average += col2Score[j];
            }
            return average / getNumCols();

        }
       

    
    }
}
