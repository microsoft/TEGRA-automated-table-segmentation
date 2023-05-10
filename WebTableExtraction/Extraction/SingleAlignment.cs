using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using WebTableExtraction.Utils;


namespace WebTableExtraction.Extraction
{
   
    class SingleAlignment
    {
        Line line;

        Record record;

        LocalScoringInfo localScoring;

        public SingleAlignment(Line line, LocalScoringInfo localScoring)
        {
            this.line = line;
            this.localScoring = localScoring;
        }

        public Record getAlignedRecord()
        {
            return record;
        }


        /**
         * Get the best alignment of the single record, given the number of columns.
         */
        public double alignMinimize(int numCols)
        {
            List<Cell> dummyCells = new List<Cell>();
            for (int i = 0; i < numCols; i++)
                dummyCells.Add(new Cell(line,null));
            Record dummy = new Record(dummyCells);
            return alignMinimize(numCols, dummy);
        }

      


        public double alignMinimize(int numCols, Record refRecord)
        {
            int numWords = line.getNumWords();

            //The scores[k,w] is the best score of aligning first k columns, where w is the last word of the last non-null column
            //k, w are all one-based, so be careful

            //Initialize 
            double[,] scores = new double[numCols + 1, numWords + 1];
            for (int w = 0; w <= numWords; w++)
            {
                if (w == 0)
                    scores[0, w] = 0;
                else
                    scores[0, w] = Double.MaxValue; //the 0th column has words, impossible
            }

            for (int k = 1; k <= numCols; k++)
            {
                //up the kth column are null columns
                Cell cell = refRecord.getCell(k - 1);
                string value = cell.getCellValue();
                double kthColumnScore = localScoring.doubleCellScore(null, value);
                scores[k, 0] = scores[k - 1, 0] + kthColumnScore;
            }
            prePointers[,] track = new prePointers[numCols + 1, numWords + 1];
           
            for (int k = 1; k <= numCols; k++)
            {
                //string kthRefColumn = refRecord.getCell(k - 1).getCellValue();
                for (int w = 1; w <= numWords; w++)
                {
                    //calculate scores[k,w]
                    double minKW = Double.MaxValue;
                    for (int i = w; i >= 0; i--)
                    //for (int i = 0; i <=w ; i++)
                    {
                        double curScore = 0;
                        if(i < w)
                        {
                            //the k the cell is from [i+1,w]
                            curScore = scores[k - 1, i] + localScoring.doubleCellScore(line.getCellValue(i, w - 1), refRecord.getCell(k - 1).getCellValue());
                            if (curScore < minKW)
                            {
                                minKW = curScore;
                                track[k, w].preK = k - 1;
                                track[k, w].preW = i;
                            }
                          
                        }else
                        {
                            curScore = scores[k - 1, i] + localScoring.doubleCellScore(null, refRecord.getCell(k - 1).getCellValue());
                            if (curScore < minKW)
                            {
                                minKW = curScore;
                                track[k, w].preK = k - 1;
                                track[k, w].preW = i;
                            }
                            
                        }
                        
                    }
                    scores[k, w] = minKW;
                }
            }
            //backtrack
            int curK = numCols;
            int curW = numWords;
            List<Cell> newCells = new List<Cell>();
            int count = 0;
            while (curK >= 1)
            {
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
                    System.Diagnostics.Debug.Assert(preK == curK - 1);
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

                count++;
                if (count > numCols)
                {
                    Console.WriteLine("got stuck in here");
                }
            }
            record = new Record(newCells);
            System.Diagnostics.Debug.Assert(record.getNumCells() == numCols);
            //Console.WriteLine("[Method 2]The aligned record is : {0} with score {1} ", record.toString(), scores[numCols, numWords]);
            return scores[numCols, numWords];
        }

       
        struct prePointers{
           public int preK;
           public int preW;
        }


    }
}
