using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebTableExtraction.Utils;

namespace WebTableExtraction.Baseline
{
    class FieldSummary
    {
        int numCols;
        LocalScoringInfo localScoring;
        Dictionary<Line, Record> examples;

        List<HashSet<Cell>> fs;

        public FieldSummary(int numCols, LocalScoringInfo localScoring, Dictionary<Line, Record> examples)
        {
            this.numCols = numCols;
            this.localScoring = localScoring;
            this.examples = examples;

            fs = new List<HashSet<Cell>>();
            for (int i = 0; i < numCols; i++)
            {
                fs.Add(new HashSet<Cell>());
            }
        }

        

        public void updateFieldSummary(Record record)
        {
            for (int i = 0; i < numCols; i++)
            {
                fs.ElementAt(i).Add(record.getCell(i));
                if (fs.ElementAt(i).Count() > Parameter.max_n_reps)
                {
                    updateFieldSummary(i, fs.ElementAt(i));
                }
            }
        }


        private void updateFieldSummary(int colNum, HashSet<Cell> cells)
        {
            double maxDistance = Double.MinValue;
            Cell worstCell = null;
            
            foreach (Cell cell in cells)
            {
                double f2fc = getF2FCScore(colNum, cell);
                if (f2fc > maxDistance)
                {
                    maxDistance = f2fc;
                    worstCell = cell;
                }
            }

            cells.Remove(worstCell);
        }


        public double getF2FCScore(int colNum, Cell cell)
        {
            double result = 0;
            foreach (Cell temp in fs.ElementAt(colNum))
            {
                result += localScoring.pairWiseScore(cell.getCellValue(), temp.getCellValue());
            }
            //Add the score with the data examples as well
            foreach (Line line in examples.Keys)
            {
                Record record = examples[line];
                result += Parameter.exampleWeight * localScoring.pairWiseScore(cell.getCellValue(), record.getCell(colNum).getCellValue());
            }


            if (fs.ElementAt(colNum).Count() + examples.Count() == 0)
                result = 1;
            else
                result = result / (fs.ElementAt(colNum).Count() + examples.Count());

            return result;
        }


        public FieldSummary getSubFieldSummary(int startCol, int endCol)
        {
            System.Diagnostics.Debug.Assert(endCol >= startCol && endCol < numCols);

            FieldSummary result = new FieldSummary(endCol - startCol + 1, localScoring,examples);
            result.fs.Clear();
            for (int i = 0; i < result.numCols; i++)
            {
                result.fs.Add(this.fs.ElementAt(i + startCol));

            }

            return result;
        }
    }
}
