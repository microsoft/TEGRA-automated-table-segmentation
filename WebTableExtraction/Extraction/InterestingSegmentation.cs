using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using WebTableExtraction.Utils;
namespace WebTableExtraction.Extraction
{
    class InterestingSegmentation
    {

        //input
        Line anchorLine;
        int maxNumCols;
        List<Line> lines;
        Dictionary<Line, Record> examples;
        LocalScoringInfo localScoring;
      
        
        //my internal scoring storage
        Dictionary<string, double> anchorColumn2BestScore = new Dictionary<string, double>();
        double nullColumn2BestScore = 0;
        
    
        //output (1): give me your best guess
        //Record firstAnchorRecord = null;
        //double firstAnchorRecordBestScore = 0;
        Dictionary<int, Record> numCols2FirstAnchorRecord = new Dictionary<int, Record>();
        Dictionary<int, double> numCols2FirstAnchorRecordBestScore = new Dictionary<int, double>();


        //output (2): the h() function
        //the best score of the last k columns, starting from w
        double[,] bestScore_Backwards = null;
        

        public InterestingSegmentation(Line anchorLine, int maxNumCols, List<Line> lines, LocalScoringInfo localScoring, Dictionary<Line, Record> examples)
        {
            this.anchorLine = anchorLine;
            this.maxNumCols = maxNumCols;
            this.lines = lines;
            this.localScoring = localScoring;
            this.examples = examples;
           
            Console.WriteLine("Segmenting Line : {0} in order to produce all possible segmentations", anchorLine.ToString());

            DateTime t1 = DateTime.Now;
            setAnchorColumn2BestScore();
            DateTime t2 = DateTime.Now;
            //Console.WriteLine("Done setting the best score of any anchor column ");

            bestScore_Backwards = new double[maxNumCols + 1, anchorLine.getNumWords() + 1];
            setBestScore_Backwards();
            DateTime t3 = DateTime.Now;
            //Console.WriteLine("Init segmentation: best score init" + (t2 - t1).TotalMilliseconds + " backwards: " + (t3 - t2).TotalMilliseconds);
            //Console.WriteLine("Done Initialization the h() function for anchor line: " + anchorLine.ToString());
           
            for(int i = 0; i <= maxNumCols; i++)
            {
                for(int j = 0; j <= anchorLine.getNumWords(); j++)
                {
                    if(i != 0)
                    {
                        System.Diagnostics.Debug.Assert(bestScore_Backwards[i,j] < double.MaxValue);
                    }
                }
            }

        }

        /*
        private void setAnchorColumn2BestScore()
        {

            //set null2BestScore 
            //set nonInteresting2BestScore
            foreach (Line line in lines)
            {
                if (anchorLine == line)
                    continue;

                if (examples.ContainsKey(line))
                {
                    Record record = examples[line];
                    double nullBestScoreWithExample = localScoring.doubleCellScore(null, null);
                    double nonInterestingBestScoreWithExample = localScoring.doubleCellScore(null, null);
                    for (int i = 0; i < record.getNumCells(); i++)
                    {
                        string cell = record.getCell(i).getCellValue();

                        double curNullScore = localScoring.doubleCellScore(null, cell);
                        if (curNullScore < nullBestScoreWithExample)
                        {
                            nullBestScoreWithExample = curNullScore;
                        }

                        double curNonInteScore = localScoring.bestDoubleCellScoreGivenNonInterestingNonNull(cell);
                        if (curNonInteScore < nonInterestingBestScoreWithExample)
                        {
                            nonInterestingBestScoreWithExample = curNonInteScore;
                        }

                    }
                    nullColumn2BestScore += Parameter.exampleWeight * nullBestScoreWithExample;
                    nonInterestingColumn2BestScore += Parameter.exampleWeight * nonInterestingBestScoreWithExample;
                }
                else
                {
                    double nullBestScoreWithCurLine = localScoring.doubleCellScore(null, null);
                    double nonInterestingBestScoreWithCurLine = localScoring.doubleCellScore(null, null);
                    for (int k = 0; k < line.getNumWords(); k++)
                    {
                        for (int l = k; l < line.getNumWords(); l++)
                        {
                            string key = line.getCellValue(k, l);
                            double curNullScore = localScoring.doubleCellScore(null, key);
                            if (curNullScore < nullBestScoreWithCurLine)
                            {
                                nullBestScoreWithCurLine = curNullScore;
                            }

                            double curNonInteScore = localScoring.bestDoubleCellScoreGivenNonInterestingNonNull(key);
                            if (curNonInteScore < nonInterestingBestScoreWithCurLine)
                            {
                                nonInterestingBestScoreWithCurLine = curNonInteScore;
                            }
                        }
                    }

                    nullColumn2BestScore += nullBestScoreWithCurLine;
                    nonInterestingColumn2BestScore += nonInterestingBestScoreWithCurLine;
                     
                    //nullColumn2BestScore += localScoring.bestDoubleCellScoreGivenNull();
                    //nonInterestingColumn2BestScore += localScoring.bestDoubleCellScoreGivenNonInterestingNonNull();
                }
               
            }

            HashSet<string> keys = new HashSet<string>();
            for (int i = 0; i < anchorLine.getNumWords(); i++)
            {
                for (int j = i; j < anchorLine.getNumWords(); j++)
                {

                    string key = anchorLine.getCellValue(i, j);
                    keys.Add(key);
                }
            }

            foreach (string key in keys)
            {
                double result = 0;
                //If this substring is not interesting
                if (!localScoring.isInteresting(key))
                    continue;
                foreach (Line line2 in lines)
                {
                    if (anchorLine == line2)
                        continue;
                    double line2Best = 0;
                    if (examples.ContainsKey(line2))
                    {
                        line2Best = Parameter.exampleWeight * localScoring.doubleCellScore(key, null);
                        Record record = examples[line2];
                        for (int i = 0; i < record.getNumCells(); i++)
                        {
                            string key2 = record.getCell(i).getCellValue();
                            double cur = Parameter.exampleWeight * localScoring.doubleCellScore(key, key2);
                            if (cur < line2Best)
                            {
                                line2Best = cur;
                            }
                        }
                    }
                    else
                    {
                        line2Best = localScoring.doubleCellScore(key, null);
                        for (int k = 0; k < line2.getNumWords(); k++)
                        {
                            for (int l = k; l < line2.getNumWords(); l++)
                            {
                                string key2 = line2.getCellValue(k, l);
                                double cur = localScoring.doubleCellScore(key, key2);
                                if (cur < line2Best)
                                {
                                    line2Best = cur;
                                }
                            }
                        }
                    }
                    
                    result += line2Best;
                }

                anchorColumn2BestScore[key] = result;
            }




        }
         * */
        private void setAnchorColumn2BestScore()
        {

            //set null2BestScore 
            foreach (Line line in lines)
            {
                if (anchorLine == line)
                    continue;

                if (examples.ContainsKey(line))
                {
                    Record record = examples[line];
                    double nullBestScoreWithExample = localScoring.doubleCellScore(null, null);
                    for (int i = 0; i < record.getNumCells(); i++)
                    {
                        string cell = record.getCell(i).getCellValue();

                        double curNullScore = localScoring.doubleCellScore(null, cell);
                        if (curNullScore < nullBestScoreWithExample)
                        {
                            nullBestScoreWithExample = curNullScore;
                        }
                    }
                    nullColumn2BestScore += Parameter.exampleWeight * nullBestScoreWithExample;
                }
                else
                {
                    double nullBestScoreWithCurLine = localScoring.doubleCellScore(null, null);

                    for (int k = 0; k < line.getNumWords(); k++)
                    {
                        int maxEnd = k + Parameter.maxTokensPerColumn;
                        for (int l = k; l < line.getNumWords() && l < maxEnd; l++)
                        {
                            string key = line.getCellValue(k, l);
                            double curNullScore = localScoring.doubleCellScore(null, key);
                            if (curNullScore < nullBestScoreWithCurLine)
                            {
                                nullBestScoreWithCurLine = curNullScore;
                            }

                        }
                    }

                    nullColumn2BestScore += nullBestScoreWithCurLine;
                }

            }

            HashSet<string> keys = new HashSet<string>();
            for (int k = 0; k < anchorLine.getNumWords(); k++)
            {
                int maxEnd = k + Parameter.maxTokensPerColumn;
                for (int l = k; l < anchorLine.getNumWords() && l < maxEnd; l++)
                {

                    string key = anchorLine.getCellValue(k, l);
                    keys.Add(key);
                }
            }

            foreach (string key in keys)
            {
               
                double result = 0;
              
                foreach (Line line2 in lines)
                {
                    if (anchorLine == line2)
                        continue;
                    double line2Best = 0;
                    string bestkey = null;
                    if (examples.ContainsKey(line2))
                    {
                        line2Best = Parameter.exampleWeight * localScoring.doubleCellScore(key, null);
                        Record record = examples[line2];
                        for (int i = 0; i < record.getNumCells(); i++)
                        {
                            string key2 = record.getCell(i).getCellValue();
                            double cur = Parameter.exampleWeight * localScoring.doubleCellScore(key, key2);
                            if (cur < line2Best)
                            {
                                line2Best = cur;
                            }
                        }
                    }
                    else
                    {
                        line2Best = localScoring.doubleCellScore(key, null);
                        for (int k = 0; k < line2.getNumWords(); k++)
                        {
                            int maxEnd = k + Parameter.maxTokensPerColumn;
                            for (int l = k; l < line2.getNumWords() && l < maxEnd; l++)
                            {
                                string key2 = line2.getCellValue(k, l);
                                double cur = localScoring.doubleCellScore(key, key2);
                                if (cur < line2Best)
                                {
                                    line2Best = cur;
                                    bestkey = key2;
                                }
                            }
                        }
                    }
                   
                    if(Parameter.design_running_example)
                        Console.WriteLine("Key: " + key + " :has the best score with " + bestkey + " in line" + line2.ToString() + " fd: " + line2Best);
                    result += line2Best;
                }

                anchorColumn2BestScore[key] = result;
            }




        }

        public double getAnchorColumn2BestScore(string s)
        {
            if (s == null)
                return nullColumn2BestScore;
            //else if (!localScoring.isInteresting(s))
              //  return nonInterestingColumn2BestScore;
            else
            {
                if (anchorColumn2BestScore.ContainsKey(s))
                    return anchorColumn2BestScore[s];
                else
                    return Parameter.exampleWeight * examples.Count() + 1 * (lines.Count() - examples.Count() - 1);
            }
        }

        private void setBestScore_Backwards()
        {
            //bestScore_Backwards[k,w] is the best possible score of the last k columns starting from [w, w is 0-based in anchorLine
            for (int w = 0; w <= anchorLine.getNumWords(); w++)
            {
                bestScore_Backwards[0, w] = Double.MaxValue;
                if (w == anchorLine.getNumWords())
                    bestScore_Backwards[0, w] = 0;
            }
            for (int k = 1; k <= maxNumCols; k++)
            {
                bestScore_Backwards[k, anchorLine.getNumWords()] = bestScore_Backwards[k - 1, anchorLine.getNumWords()] + nullColumn2BestScore;
            }
            prePointers[,] track = new prePointers[maxNumCols + 1, anchorLine.getNumWords() + 1];
            for (int k = 1; k <= maxNumCols; k++)
            {
                for (int w = anchorLine.getNumWords() - 1; w >= 0; w--)
                {
                   
                    //The kth column is from [w, i]
                    double minKW = Double.MaxValue;
                    for (int i = w - 1; i < anchorLine.getNumWords(); i++)
                    {
                        if (i == w - 1)
                        {
                            //kth column is null
                            double curKW = bestScore_Backwards[k - 1, i + 1] + nullColumn2BestScore;
                            if (curKW < minKW)
                            {
                                minKW = curKW;
                                track[k, w].preK = k - 1;
                                track[k, w].preW = i + 1;
                            }
                               
                        }
                        else
                        {
                            string kthColumn = anchorLine.getCellValue(w, i);
                           
                            double curKW = bestScore_Backwards[k - 1, i + 1] + getAnchorColumn2BestScore(kthColumn);
                            if (curKW < minKW)
                            {
                                minKW = curKW;
                                track[k, w].preK = k - 1;
                                track[k, w].preW = i + 1;
                            }
                                
                        }
                    }
                    bestScore_Backwards[k, w] = minKW;
                }
            }
            //best overall should be bestScore_Backwards[k,0]
            //construct the firstAnchorRecord
            for(int numCols = 1; numCols <= maxNumCols; numCols++)
            {
                List<Cell> newCells = new List<Cell>();
                int curK = numCols;
                int curW = 0;
                while (curK <= numCols)
                {
                    if (curW == anchorLine.getNumWords())
                    {
                        for (int i = curK; i >= 1; i--)
                        {
                            Cell cell = new Cell(anchorLine, null);
                            newCells.Add(cell);
                        }
                        break;
                    }
                    else
                    {
                        int preK = track[curK, curW].preK;
                        int preW = track[curK, curW].preW;
                        if (preK != curK - 1)
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }

                        if (preW == curW)
                        {
                            //this column is null
                            Cell cell = new Cell(anchorLine, null);
                            newCells.Add(cell);
                        }
                        else
                        {
                            Cell cell = new Cell(anchorLine, curW, preW - 1);
                            newCells.Add(cell);
                        }

                        curK = preK;
                        curW = preW;
                    }


                }
                numCols2FirstAnchorRecord[numCols] = new Record(newCells);
                numCols2FirstAnchorRecordBestScore[numCols] = bestScore_Backwards[numCols, 0];
                //firstAnchorRecord = new Record(newCells);
                //firstAnchorRecordBestScore = bestScore_Backwards[maxNumCols, 0];

                System.Diagnostics.Debug.Assert(numCols2FirstAnchorRecord[numCols].isValidRecord(anchorLine));
            }
           

        }
        struct prePointers
        {
           public int preK;
           public int preW;
        }

        public double getBestScore_Backwards(int k, int w)
        {
            return bestScore_Backwards[k, w];
        }
        public int getMaxNumCols()
        {
            return maxNumCols;
        }


        public Record getFirstAnchorRecord(int numCols)
        {
           
            return numCols2FirstAnchorRecord[numCols];
        }
        public double getFirstAnchorRecordBestScore(int numCols)
        {
            return numCols2FirstAnchorRecordBestScore[numCols];
        }


    }
}
