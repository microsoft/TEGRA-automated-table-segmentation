using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTableExtraction.Utils;
namespace WebTableExtraction.Judie
{
    class StructureFreeLabelling
    {
        //input
        Line line;
        LocalScoringInfo localScoring;
        

        //output
        LabelledRecord splittedRecord;

        public StructureFreeLabelling(Line line, LocalScoringInfo localScoring)
        {
            this.line = line;
            this.localScoring = localScoring;
           
            doSplitting();
        }
        public LabelledRecord getSplittedRecord()
        {
            return splittedRecord;
        }

        private void doSplitting()
        {
            Dictionary<Cell, double> cell2Score = new Dictionary<Cell, double>();
            List<Cell> candidateCells = new List<Cell>();
            List<string> cellLables = new List<string>();

            int i = 0;
            while(i < line.getNumWords())
            {
                int j = line.getNumWords() - 1;
                Cell cell = null;
                for(j = line.getNumWords() - 1; j >= i ; j--)
                {
                    cell = new Cell(line, i, j);
                    string cellValue = cell.getCellValue();
                    if(KB.getAttris(cellValue).Count() != 0)
                    {
                        break;
                    }
                    if(j == i)
                    {
                        break;
                    }
                }
                candidateCells.Add(cell);
                cellLables.Add(assignLabel(cell));
                i = j + 1;
            }

            splittedRecord = new LabelledRecord(candidateCells, cellLables);
            
        }

        private string assignLabel(Cell cell)
        {
            string cellValue = cell.getCellValue();

            HashSet<string> attris = KB.getAttris(cellValue);


            string best_attri = null;
            double best_score = Double.MaxValue;
            foreach(string attri in attris)
            {
                //attribute vocabulary
                double score_1 = 0;
                foreach(string repre_value in KB.getValues(attri))
                {
                    if (Parameter.d_occur_type == 1)
                        score_1 += 2 * localScoring.occurrenceDistance(cellValue, repre_value) - 1;
                    else if (Parameter.d_occur_type == 2)
                        score_1 += localScoring.occurrenceDistance(cellValue, repre_value);
                    else
                        score_1 += localScoring.occurrenceDistance(cellValue, repre_value);
                }
                score_1  = score_1 / KB.getValues(attri).Count();


                //attribute syntactic similarity
                double score_2 = 0;
                foreach(string repre_value in KB.getValues(attri))
                {
                    score_2 += localScoring.syntacticDistance(cellValue, repre_value);
                }
                score_2 = score_2 / KB.getValues(attri).Count();


                double cur_score = score_1 * score_2;

                if(cur_score < best_score)
                {
                    best_score = cur_score;
                    best_attri = attri;
                }
            }
            return best_attri;
        }
        
    }
}
