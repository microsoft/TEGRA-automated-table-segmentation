using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTableExtraction.Utils;

using WebTableExtraction.Experiments;

using WebTableExtraction.ExtractionAStar;


namespace WebTableExtraction.Extraction
{
    class MSASplitting
    {

        //input
        Line line;
        InterestingSegmentation inteSeg;


        //output, independenly split this line, greedily
        Record record;

        public MSASplitting (Line line, InterestingSegmentation inteSeg)
        {
            this.line = line;
            this.inteSeg = inteSeg;

            //doSplitting();
        }

        private void doSplitting()
        {
            Dictionary<Cell, double> cell2Score = new Dictionary<Cell, double>();
            List<Cell> candidateCells = new List<Cell>();

            for (int startIndex = 0; startIndex < line.getNumWords(); startIndex++)
            {
                for (int endIndex = startIndex; endIndex < line.getNumWords(); endIndex++)
                {
                    Cell cell = new Cell(line, startIndex, endIndex);
                    string s = cell.getCellValue();

                    double score = inteSeg.getAnchorColumn2BestScore(s);
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

            record = new Record(orderedCells);

            Console.WriteLine("MSA The record after indepdent splitting is: " + record.toString());
        }
    
        
        public int getIndependentNumCols()
        {
            //return record.getNumCells();

            double minAve = Double.MaxValue;
            int result = 0;
            for(int k =1; k <= inteSeg.getMaxNumCols(); k++)
            {
                double minScore = inteSeg.getBestScore_Backwards(k, 0);
                minScore = minScore / k;
                if (minScore < minAve)
                {
                    minAve = minScore;
                    result = k;
                }
            }
            return result;
        }
    
    }
}
