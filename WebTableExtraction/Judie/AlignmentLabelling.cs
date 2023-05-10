using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebTableExtraction.Utils;

namespace WebTableExtraction.Judie
{
    class AlignmentLabelling
    {
        //input
        Line line;
        LabelledRecord labelledRecord;
        List<string> desiredLabelling;
       

        //output
        Record labellingRecord_aligned;

        public AlignmentLabelling(Line line, LabelledRecord labelledRecord, List<string> desiredLabelling)
        {
            this.line = line;
            this.labelledRecord = labelledRecord;
            this.desiredLabelling = desiredLabelling;
            

            alignLabelling();
        }

        public Record getLabelledRecord()
        {
            return labellingRecord_aligned;
        }

        public void alignLabelling()
        {
            //maximize the number of columns that have the correct label
            
            int[,] matches = new int[desiredLabelling.Count() + 1, line.getNumWords() + 1];
            for (int j = 1; j <= line.getNumWords(); j++)
                matches[0, j] = -1000;

            prePointers[,] track = new prePointers[desiredLabelling.Count() + 1, line.getNumWords() + 1];
            
            for(int i = 1; i <= desiredLabelling.Count(); i++)
            {
                string ithLabel = desiredLabelling.ElementAt(i - 1);
                for(int j = 1; j <= line.getNumWords();j++)
                {
                    int bestMatches = -1;
                   
                    for(int k = 1; k <= j + 1; k++)
                    {
                        //this column is from [k,j]
                        int curMatches = matches[i - 1, k - 1];
                        string ithCol = line.getCellValue(k - 1, j - 1);
                        HashSet<string> attri = KB.getAttris(ithCol);
                        if(ithLabel == null)
                        {
                            if(ithCol == null || attri.Count() == 0)
                            {
                                curMatches++;
                            }
                        }
                        else
                        {
                            if (attri.Contains(ithLabel))
                            {
                                curMatches++;
                            }
                        }
                        if(curMatches > bestMatches)
                        {
                            bestMatches = curMatches;
                            track[i, j].preK = i - 1;
                            track[i, j].preW = k - 1;
                        }
                       
                    }
                    matches[i, j] = bestMatches;

                }
            }

            //constract the record from 
            
            int curK = desiredLabelling.Count();
            int curW = line.getNumWords();
            List<Cell> newCells = new List<Cell>();
            while (curK >= 1)
            {
                if(curK == 1)
                {

                }
                if (curW == 0)
                {
                    for (int i = curK; i >= 1; i--)
                    {
                        Cell cell = new Cell(line, null);
                        newCells.Insert(0, cell);
                    }
                    break;
                }
                else
                {
                    int preK = track[curK, curW].preK;
                    int preW = track[curK, curW].preW;
                    if(preK != curK - 1)
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    if (preW == curW)
                    {
                        Cell cell = new Cell(line, null);
                        newCells.Insert(0, cell);
                    }
                    else
                    {
                        Cell cell = new Cell(line, preW, curW - 1);
                        newCells.Insert(0, cell);
                    }
                    curK = preK;
                    curW = preW;

                }

            }
            labellingRecord_aligned = new Record(newCells);
            if(!labellingRecord_aligned.isValidRecord(line))
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }

        struct prePointers
        {
            public int preK;
            public int preW;
        }


    }
}
