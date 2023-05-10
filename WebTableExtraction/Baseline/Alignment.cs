using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTableExtraction.Utils;


namespace WebTableExtraction.Baseline
{
    class Alignment
    {
        //input: a line, with an independently split record, and a number of columns
        Line line;
        Record record;
        int numCols;
        LocalScoringInfo localScoring;


        FieldSummary fs;


        bool insideRefinement;


        //output: get a newly aligned record with exactly numCols
        Record alignedRecord;

        public Alignment(Line line, Record record, int numCols, FieldSummary fs, LocalScoringInfo localScoring)
        {
            this.line = line;
            this.record = record;
            this.numCols = numCols;
            this.fs = fs;
            this.localScoring = localScoring;

            this.insideRefinement = false;

            align();
        }

        public Alignment(Line line, Record record, int numCols, FieldSummary fs, bool insideRefinement, LocalScoringInfo localScoring)
        {
            this.line = line;
            this.record = record;
            this.numCols = numCols;
            this.fs = fs;
            this.localScoring = localScoring;

            this.insideRefinement = insideRefinement;

            align();
        }

        private void align()
        {
            if (record.getNumCells() == numCols)
                return;
            else if (record.getNumCells() > numCols)
                alignLong();
            else
                alignShort();
        }

        private void alignLong()
        {
            Dictionary<Cell, double> cell2Score = new Dictionary<Cell, double>();
            List<Cell> candidateCells = new List<Cell>();

            for (int startIndex = 0; startIndex < line.getNumWords(); startIndex++)
            {
                for (int endIndex = startIndex; endIndex < line.getNumWords(); endIndex++)
                {
                    Cell cell = new Cell(line, startIndex, endIndex);
                    double score = localScoring.singleCellScore(cell.getCellValue());

                    //If this alignment is inside the refinement phase, then, add the list support score
                    if (insideRefinement)
                    {
                        score = 0.5 * score + 0.5 * listSupportScore(cell);
                    }


                    cell2Score.Add(cell, score);
                    candidateCells.Add(cell);

                }
            }
            //order candidate cells in asending order of the distance
            candidateCells = candidateCells.OrderBy(x => cell2Score[x]).ToList();

            //unordered
            HashSet<Cell> unordered = new HashSet<Cell>();

            while (candidateCells.Count() != 0) 
            {
                Cell cell = candidateCells.ElementAt(0);
                candidateCells.RemoveAt(0);


                if (cell.getCellValue() == @"chuxu1989@gmail.com China IL AAA 456")
                {
                    Console.WriteLine("Debug");
                }
                //estimate the minimum number of fields if cell were to be included in r
                unordered.Add(cell);
                int numGaps  = numberOfGaps(line, unordered);
                int minNumCols = unordered.Count() + numGaps;
                unordered.Remove(cell);

                //Add the cell if qualifies
                if (minNumCols <= numCols)
                {
                    unordered.Add(cell);
                    //remove from candidate cells that overlap with 
                    List<Cell> newCandidateCells = new List<Cell>();
                    foreach (Cell temp in candidateCells)
                    {
                        if (!temp.overlapping(cell))
                        {
                            newCandidateCells.Add(temp);
                        }
                    }
                    candidateCells = newCandidateCells;
                }



            }

            //re-arrange the unordered cells to form a record
            List<Cell> orderedCells = unordered.OrderBy(x => x.getStartIndex()).ToList();

            alignedRecord = new Record(orderedCells);

            //Console.WriteLine("Alignment: long record splitting is: " + alignedRecord.toString());

           

        }

        private double listSupportScore(Cell cell)
        {
            double result = Double.MinValue;
            for (int i = 0; i < numCols; i++)
            {
                double curScore = fs.getF2FCScore(i, cell);
                if (curScore < result)
                    result = curScore;
            }
            return result;
        }
  

        private int numberOfGaps(Line line, HashSet<Cell> cells)
        {
            List<Cell> orderedCells = cells.OrderBy(x => x.getStartIndex()).ToList();

            int gaps = 0;
            for (int i = 0; i < orderedCells.Count(); i++)
            {
                Cell cell = orderedCells.ElementAt(i);
                if (i == 0)
                {
                    if (cell.getStartIndex() > 0)
                        gaps++;
                }
                else if (i > 0 && i < orderedCells.Count() - 1)
                {
                    if (cell.getStartIndex() > orderedCells.ElementAt(i - 1).getEndIndex() + 1)
                        gaps++;
                }
                else
                {
                    if (cell.getStartIndex() > orderedCells.ElementAt(i - 1).getEndIndex() + 1)
                        gaps++;

                }

                //If this is also the last cell
                if (i == orderedCells.Count() - 1)
                {
                    if (cell.getEndIndex() < line.getNumWords() - 1)
                        gaps++;
                }
            }

            return gaps;
        }

        

        private void alignShort()
        {
            int curNumCols = record.getNumCells();
            System.Diagnostics.Debug.Assert(curNumCols < numCols);
            
            double[,] distances = new double[curNumCols + 1, numCols + 1];
            distances[0, 0] = 0;

            for (int i = 1; i < curNumCols; i++)
            {
                distances[i, 0] = distances[i - 1, 0] + 10000; //Since every field must match a column , therefore, no match has the highest distance
            }

            for (int j = 1; j < numCols; j++)
            {
                distances[0, j] = distances[0, j - 1] + 1; //For each unmathced column, we have a distance 1 
            }

            prePointers[,] track = new prePointers[curNumCols + 1, numCols + 1];

            if (record.toString() == "WSAF||Pandas")
            {
                Console.WriteLine("debug");
            }
            for (int i = 1; i <= curNumCols; i++)
            {
                for (int j = 1; j <= numCols; j++)
                {
                    if (i > j)
                        continue;

                    double d1 = Double.MaxValue;
                    if(i <= j-1 )
                        d1 = distances[i, j - 1] + 1;

                    //double d2 = distances[i - 1, j] + 10000;

                    double f2fc = fs.getF2FCScore(j - 1, record.getCell(i - 1));
                    if (f2fc > 1)
                        f2fc = 1;
                    else if (f2fc < 0)
                        f2fc = 0;

                    if (!(f2fc >= 0 && f2fc <= 1))
                    {
                        System.Diagnostics.Debug.Assert(f2fc >= 0 && f2fc <= 1);
                    }
                   
                   
                  
                    double d3 = distances[i - 1, j - 1] + f2fc;


                    //System.Diagnostics.Debug.Assert(d1 <= d2 || d3 <= d2);
                  
                    if (d3 <= d1 )
                    {
                        //d3 is the smallest
                        distances[i, j] = d3;
                        track[i, j].preI = i - 1;
                        track[i, j].preJ = j - 1;
                    }
                    else if (d1 <= d3)
                    {
                        //d1 is the smallest
                        distances[i, j] = d1;
                        track[i, j].preI = i;
                        track[i, j].preJ = j - 1;
                    }
                   

                   

                }
            }

            //create a new record
            List<Cell> newCells = new List<Cell>();
            int m = curNumCols;
            int n = numCols;
            while (n >= 1)
            {
                int preM = track[m, n].preI;
                int preN = track[m, n].preJ;

                if (m == 0)
                {
                    for (int k = n; k >= 1; k--)
                    {
                        newCells.Insert(0, new Cell(line, null));
                    }
                    break;
                }
                System.Diagnostics.Debug.Assert(preN == n - 1);
                if (preM == m)
                {
                    newCells.Insert(0, new Cell(line, null));
                }
                else
                {
                    System.Diagnostics.Debug.Assert(preM == m - 1);
                    newCells.Insert(0, record.getCell(m - 1));
                }
                
                /*
                if (preN != 0)
                {
                    System.Diagnostics.Debug.Assert(preN == n - 1);
                    if (preM == m - 1)
                    {
                        newCells.Insert(0, record.getCell(m - 1));
                    }
                    else if (preM == m)
                    {
                        newCells.Insert(0, new Cell(line, null));
                    }
                }
                else
                {
                    //The records from n-to column 1 are null columns
                    for (int k = n; k >= 1; k--)
                    {
                        newCells.Insert(0, new Cell(line, null));
                    }
                    break;
                }
                 * */
                

                m = preM;
                n = preN;
            }
            alignedRecord = new Record(newCells);

            Console.WriteLine("Alignment: short record inserting null is: " + alignedRecord.toString());
        }
        struct prePointers
        {
            public int preI;
            public int preJ;
        }


        public Record getAlignedRecord()
        {
            return alignedRecord;
        }




    }
}
