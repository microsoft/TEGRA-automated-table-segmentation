using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebTableExtraction.Utils;

namespace WebTableExtraction.Judie
{
    class StructureAwareLabelling
    {
        //input
        Line line;
        LabelledRecord splittedRecord;
        LocalScoringInfo localScoring;
        
        PSM psm;

        //output
        LabelledRecord splittedRecord_structure_aware;

        public StructureAwareLabelling(Line line, LabelledRecord splittedRecord, LocalScoringInfo localScoring, PSM psm)
        {
            this.line = line;
            this.splittedRecord = splittedRecord;
            this.localScoring = localScoring;
           
            this.psm = psm;

            doSplitting();
        }
        public LabelledRecord getLabelledRecord()
        {
            return splittedRecord_structure_aware;
        }
        public void doSplitting()
        {
            //refine the assignment of labels

            List<Cell> newCells = new List<Cell>();
            List<string> newLabels = new List<string>();

            for(int i = 0; i < splittedRecord.getNumCells(); i++)
            {
                Cell cell = splittedRecord.getCell(i);
                string label = splittedRecord.getLabel(i);

                Cell newCell = new Cell(cell);
                string next_attri = null;
                if(i + 1 < splittedRecord.getNumCells())
                    next_attri = splittedRecord.getLabel(i+1);
                string newLabel = assignLabel(newCell, i, next_attri);

                newCells.Add(newCell);
                newLabels.Add(newLabel);
            }

            splittedRecord_structure_aware = new LabelledRecord(newCells, newLabels);
        }

        public string assignLabel(Cell cell, int k, string next_attri  )
        {
            string cellValue = cell.getCellValue();

            HashSet<string> attris = KB.getAttris(cellValue);


            string best_attri = null;
            double best_score = Double.MaxValue;
            foreach (string attri in attris)
            {
                //attribute vocabulary
                double score_1 = 0;
                foreach (string repre_value in KB.getValues(attri))
                {
                    if(Parameter.d_occur_type == 1) 
                        score_1 += 2 * localScoring.occurrenceDistance(cellValue, repre_value) - 1;
                    else if(Parameter.d_occur_type == 2)
                        score_1 += localScoring.occurrenceDistance(cellValue, repre_value);
                    else
                        score_1 += localScoring.occurrenceDistance(cellValue, repre_value);
                }
                score_1 = score_1 / KB.getValues(attri).Count();


                //attribute syntactic similarity
                double score_2 = 0;
                foreach (string repre_value in KB.getValues(attri))
                {
                    score_2 += localScoring.syntacticDistance(cellValue, repre_value);
                }
                score_2 = score_2 / KB.getValues(attri).Count();


                double score_3 = psm.position_prob(attri, k);
                double score_4 = psm.transitition_prob(attri, next_attri);


                double cur_score = score_1 * score_2 * score_3 * score_4;

                if (cur_score < best_score)
                {
                    best_score = cur_score;
                    best_attri = attri;
                }
            }
            return best_attri;
        }

    }
}
