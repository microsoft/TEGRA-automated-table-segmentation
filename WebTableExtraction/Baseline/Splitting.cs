 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebTableExtraction.Utils;


namespace WebTableExtraction.Baseline
{
    /**
     * For each line, independently splitting them into fields
     */
    class Splitting
    {
        //input
        private Line line;
        LocalScoringInfo localScoring;

        //output
        private Record splittedRecord;


        public Splitting(Line line, LocalScoringInfo localScoring)
        {
            this.line = line;
            this.localScoring = localScoring;
            splittedRecord = doSplitting();
        }

        public Record getSplittedRecord()
        {
            return splittedRecord;
        }


        private Record doSplitting()
        {
            Dictionary<Cell, double> cell2Score = new Dictionary<Cell, double>();
            List<Cell> candidateCells = new List<Cell>();

            for (int startIndex = 0; startIndex < line.getNumWords(); startIndex++)
            {
                for (int endIndex = startIndex; endIndex < line.getNumWords(); endIndex++)
                {
                    Cell cell = new Cell(line, startIndex, endIndex);
                    string s = cell.getCellValue();
                    
                    double score = localScoring.singleCellScore(s);
                   
                    double numWords = (double)s.Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries).Count();
                    double scaling = (numWords > 0) ? Math.Log(numWords + 9, 10) : 1;
                    score = score / scaling;
                   
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

            //re-arrange the unordered cells to form a record
            List<Cell> orderedCells = unordered.OrderBy(x => x.getStartIndex()).ToList();

            Record record = new Record(orderedCells);

            //Console.WriteLine("Baseline: The record after splitting is: " + record.toString());

            return record;
        }


    }
}
