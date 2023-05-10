using System;
using System.Collections.Generic;
using WebTableExtraction.Utils;
using System.Threading;
namespace WebTableExtraction.ExtractionAStar
{
    class AStarNode 
    {

        //first k column, ending at w
        Line anchorLine;
       

        private int k;
        private int w;

        private int preK;
        private int preW;


        private double f;
        private double g;
        private double h;


        Dictionary<Line,double[,]> line2Scores;
        Dictionary<Line, Record> examples;


        public double tentativeG(AStarNode parent, int numCols, Dictionary<Line, double[,]>  tentative_line2Scores, LocalScoringInfo localScoring)
        {
           
            //from this and parent, I can construct current anchor record, every other line must align with the current anchor record
            string curAnchorCol = null;
            if (w != parent.getW())
                curAnchorCol = anchorLine.getCellValue(parent.getW(), w - 1);
            foreach (Line line in parent.line2Scores.Keys)
            {
                double[,] scores = new double[k + 1, line.getNumWords() + 1];
                //init
                for (int j = 0; j <= line.getNumWords(); j++)
                {
                    if (j == 0)
                        scores[0, j] = 0;
                    else
                        scores[0, j] = Double.MaxValue; //the 0th column has words, impossible
                }

                for (int i = 1; i <= k; i++)
                {
                    //up the kth column are null columns
                    double ithColumnScore = localScoring.doubleCellScore(null, curAnchorCol);
                    scores[i, 0] = scores[i - 1, 0] + ithColumnScore;
                }

                for (int i = 1; i <= k; i++)
                {
                    for (int j = 1; j <= line.getNumWords(); j++)
                    {
                        if (i < k)
                        {
                            scores[i, j] = parent.line2Scores[line][i, j];
                        }
                        else
                        {
                            double mxnij = Double.MaxValue;
                            for (int x = j; x >= 0; x--)
                            {
                                double curScore = 0;
                                if (x < j)
                                {
                                    if (j - x > Parameter.maxTokensPerColumn)
                                        continue;

                                    //the i the cell xs from [x+1,j]
                                    curScore = scores[i - 1, x] + localScoring.doubleCellScore(line.getCellValue(x, j - 1), curAnchorCol);
                                    if (curScore < mxnij)
                                        mxnij = curScore;
                                }
                                else
                                {
                                    curScore = scores[i - 1, x] + localScoring.doubleCellScore(null, curAnchorCol);
                                    if (curScore < mxnij)
                                        mxnij = curScore;
                                }
                            }
                            scores[i, j] = mxnij;
                        }
                    }
                }
                tentative_line2Scores[line] = scores;
            }


            //Set G
            double tentative_g = 0;
            foreach (Line line in tentative_line2Scores.Keys)
            {
                System.Diagnostics.Debug.Assert(!examples.ContainsKey(line));
                double curMin = Double.MaxValue;
                if (k == numCols)
                {
                    //the last column must consume all the words
                    if (tentative_line2Scores[line][k, line.getNumWords()] < curMin)
                    {
                        curMin = tentative_line2Scores[line][k, line.getNumWords()];
                    }
                }
                else
                {
                    for (int j = 0; j <= line.getNumWords(); j++)
                    {
                        if (tentative_line2Scores[line][k, j] < curMin)
                        {
                            curMin = tentative_line2Scores[line][k, j];
                        }
                    }
                }
                tentative_g += curMin;
            }
           

            //Add the tentative_g with the examples

            foreach (Line line in examples.Keys)
            {
                Record record = examples[line];

                tentative_g += Parameter.exampleWeight * localScoring.doubleCellScore(curAnchorCol, record.getCell(k - 1).getCellValue());
                
            }

            if (Double.IsInfinity(tentative_g))
            {
                tentative_g = Double.MaxValue;
            }
            return tentative_g;
        }
        public double tentativeG_parallel(AStarNode parent, int numCols, Dictionary<Line, double[,]> tentative_line2Scores, LocalScoringInfo localScoring)
        {
            return tentativeG(parent, numCols, tentative_line2Scores, localScoring);


            if(Parameter.max_num_threads_per_machine == 0)
            {
                return tentativeG( parent,  numCols, tentative_line2Scores,  localScoring);
            }
            //from this and parent, I can construct current anchor record, every other line must align with the current anchor record
            string curAnchorCol = null;
            if (w != parent.getW())
                curAnchorCol = anchorLine.getCellValue(parent.getW(), w - 1);

            List<List<Line>> distribute_lines = new List<List<Line>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<Line> this_anchors = new List<Line>();
                distribute_lines.Add(this_anchors);
            }
            int temp_count = 0;
            foreach (Line line in parent.line2Scores.Keys)
            {
                int which_thread = temp_count % Parameter.max_num_threads_per_machine;
                distribute_lines[which_thread].Add(line);
                temp_count++;
            }
            Dictionary<Thread, Dictionary<Line, double[,]>> thread_2_tentative_line2Scores = new Dictionary<Thread, Dictionary<Line, double[,]>>();
            for (int i = 0; i < Parameter.max_num_threads_per_machine; i++)
            {
                List<Line> distribute_lines_one_thread = distribute_lines[i];
                Dictionary<Line, double[,]> tentative_line2Scores_one_thread = new Dictionary<Line, double[,]>();
                ThreadStart starter = delegate { tentativeG_one_thread(distribute_lines_one_thread, localScoring, curAnchorCol, parent, tentative_line2Scores_one_thread); };
                Thread thread = new Thread(starter);
                thread.Start();
                thread_2_tentative_line2Scores[thread] = tentative_line2Scores_one_thread;
            }
            foreach (Thread thread in thread_2_tentative_line2Scores.Keys)
            {
                thread.Join();
            }
            //collect the result from each thread
            foreach (Thread thread in thread_2_tentative_line2Scores.Keys)
            {
                foreach (Line line in thread_2_tentative_line2Scores[thread].Keys)
                {
                    tentative_line2Scores[line] = thread_2_tentative_line2Scores[thread][line];
                }

            }
            //Set G
            double tentative_g = 0;
            foreach (Line line in tentative_line2Scores.Keys)
            {
                System.Diagnostics.Debug.Assert(!examples.ContainsKey(line));
                double curMin = Double.MaxValue;
                if (k == numCols)
                {
                    //the last column must consume all the words
                    if (tentative_line2Scores[line][k, line.getNumWords()] < curMin)
                    {
                        curMin = tentative_line2Scores[line][k, line.getNumWords()];
                    }
                }
                else
                {
                    for (int j = 0; j <= line.getNumWords(); j++)
                    {
                        if (tentative_line2Scores[line][k, j] < curMin)
                        {
                            curMin = tentative_line2Scores[line][k, j];
                        }
                    }
                }
                tentative_g += curMin;
            }


            //Add the tentative_g with the examples

            foreach (Line line in examples.Keys)
            {
                Record record = examples[line];

                tentative_g += Parameter.exampleWeight * localScoring.doubleCellScore(curAnchorCol, record.getCell(k - 1).getCellValue());

            }

            if (Double.IsInfinity(tentative_g))
            {
                tentative_g = Double.MaxValue;
            }
            return tentative_g;
        }


        public void tentativeG_one_thread(List<Line> distribute_lines_one_thread , 
            LocalScoringInfo localScoring, string curAnchorCol, AStarNode parent,Dictionary<Line, double[,]> tentative_line2Scores_one_thread)
        {
            foreach (Line line in distribute_lines_one_thread)
            {
                double[,] scores = new double[k + 1, line.getNumWords() + 1];
                //init
                for (int j = 0; j <= line.getNumWords(); j++)
                {
                    if (j == 0)
                        scores[0, j] = 0;
                    else
                        scores[0, j] = Double.MaxValue; //the 0th column has words, impossible
                }

                for (int i = 1; i <= k; i++)
                {
                    //up the kth column are null columns
                    double ithColumnScore = localScoring.doubleCellScore(null, curAnchorCol);
                    scores[i, 0] = scores[i - 1, 0] + ithColumnScore;
                }

                for (int i = 1; i <= k; i++)
                {
                    for (int j = 1; j <= line.getNumWords(); j++)
                    {
                        if (i < k)
                        {
                            scores[i, j] = parent.line2Scores[line][i, j];
                        }
                        else
                        {
                            double mxnij = Double.MaxValue;
                            for (int x = j; x >= 0; x--)
                            {
                                double curScore = 0;
                                if (x < j)
                                {
                                    if (j - x > Parameter.maxTokensPerColumn)
                                        continue;

                                    //the i the cell xs from [x+1,j]
                                    curScore = scores[i - 1, x] + localScoring.doubleCellScore(line.getCellValue(x, j - 1), curAnchorCol);
                                    if (curScore < mxnij)
                                        mxnij = curScore;
                                }
                                else
                                {
                                    curScore = scores[i - 1, x] + localScoring.doubleCellScore(null, curAnchorCol);
                                    if (curScore < mxnij)
                                        mxnij = curScore;
                                }
                            }
                            scores[i, j] = mxnij;
                        }
                    }
                }
                tentative_line2Scores_one_thread[line] = scores;
            }
        }


        public void setLine2Scores(Dictionary<Line, double[,]> tentative_line2Scores)
        {
            line2Scores = tentative_line2Scores;            
        }



        public AStarNode(Line anchorLine, int k, int w, Dictionary<Line, Record> examples)
        {
            this.anchorLine = anchorLine;
            this.k = k;
            this.w = w;
            this.examples = examples;
        }

        public void setStartAStartNode(List<Line> lines)
        {
            System.Diagnostics.Debug.Assert(k == 0 && w == 0);
            line2Scores = new Dictionary<Line, double[,]>();
            foreach (Line line in lines)
            {
                if (line == anchorLine)
                    continue;

                if (examples.ContainsKey(line))
                    continue;

                double[,] scores = new double[1, line.getNumWords() + 1     ];

                for (int j = 0; j <= line.getNumWords(); j++)
                {
                    if (j == 0)
                        scores[0, j] = 0;
                    else
                        scores[0, j] = Double.MaxValue; //the 0th column has words, impossible
                }

                line2Scores[line] = scores;

            }
        }


        public int getPreK()
        {
            return preK;
        }
        public void setPreK(int preK)
        {
            this.preK = preK;
        }
        public int getPreW()
        {
            return preW;
        }
        public void setPreW(int preW)
        {
            this.preW = preW;
        }

        public int getK()
        {
            return k;
        }
        public int getW()
        {
            return w;
        }

        public double getF()
        {
            return f;
        }
        public void setF()
        {
            this.f = g + h;
        }
        public void setF(double f)
        {
            this.f = f;
        }
        public double getG()
        {
            return g;
        }
        public void setG(double g)
        {
            this.g = g;
        }
        public double getH()
        {
            return h;
        }
        public void setH(double h)
        {
          
            this.h = Parameter.A_Star_H_Appro * h;
            if (Double.IsInfinity(this.h))
            {
                this.h = Double.MaxValue;
            }
        }

    }
}
