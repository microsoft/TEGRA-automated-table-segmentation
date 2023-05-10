using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebTableExtraction.Experiments;
using WebTableExtraction.Utils;
using WebTableExtraction.Extraction;

namespace WebTableExtraction.ExtractionAStar
{
    class MSAAStar
    {

        List<Line> lines;
        Line anchorLine;
        InterestingSegmentation inteSeg;
        int numCols;
        LocalScoringInfo localScoring;
        Dictionary<Line, Record> examples;


        //output
        double bestScore;
        Record bestAnchorRecord;



        public MSAAStar(List<Line> lines, int numCols, Line anchorLine, InterestingSegmentation inteSeg, LocalScoringInfo localScoring, Dictionary<Line, Record> examples)
        {
            this.lines = lines;
            this.numCols = numCols;
            this.anchorLine = anchorLine;
            this.inteSeg = inteSeg;
            this.localScoring = localScoring;
            this.examples = examples;
            
            for (int k = 1; k <= numCols; k++)
            {
                for (int w = 0; w <= anchorLine.getNumWords(); w++)
                {
                    //first k columns, ending at w, one-based index
                    AStarNode node = new AStarNode(anchorLine,k, w,examples);
                   
                    allNodes.Add(node);
                }
            }
        }

        HashSet<AStarNode> allNodes = new HashSet<AStarNode>();
        private HashSet<AStarNode> getNeighbors(AStarNode node)
        {
            HashSet<AStarNode> result = new HashSet<AStarNode>();
            foreach (AStarNode cur in allNodes)
            {
                if (cur.getK() == node.getK() + 1 && cur.getW() >= node.getW())
                {
                    result.Add(cur);
                }
            }
            return result;
        }
        private AStarNode getAStarNode(int k, int w)
        {
            foreach (AStarNode cur in allNodes)
            {
                if (cur.getK() == k && cur.getW() == w)
                {
                    return cur;
                }
            }
            return null;
        }


        private AStarNode getFirstNode(HashSet<AStarNode> openNodes)
        {
            double minF = double.MaxValue;
            AStarNode result = null;
            foreach (AStarNode node in openNodes)
            {
                if (node.getF() <= minF)
                {
                    minF = node.getF();
                    result = node;
                }
            }
           
            return result;
        }

        /*
         * Produce the best anchor segmentation, who has a score less than scoreThreshold
         */ 
        public double align(int numCols, double scoreThreshold)
        {

            

            HashSet<AStarNode> openNodes = new HashSet<AStarNode>();
            HashSet<AStarNode> closedNodes = new HashSet<AStarNode>();
            
            //SortedSet<AStarNode> openNodes = new SortedSet<AStarNode>(new AStarNodeComparer());
            //SortedSet<AStarNode> closedNodes = new SortedSet<AStarNode>(new AStarNodeComparer());

            AStarNode startNode = new AStarNode(anchorLine,0,0,examples);
            startNode.setStartAStartNode(lines);
            openNodes.Add(startNode);


            startNode.setG(0);
            startNode.setH(inteSeg.getBestScore_Backwards(numCols - startNode.getK(), startNode.getW()));
            startNode.setF();

            while (openNodes.Count() != 0)
            {
                
                AStarNode curNode = getFirstNode(openNodes);

                Console.WriteLine("Processing node: [{0} , {1}] with f,g, h ({2},{3},{4})", curNode.getK(), curNode.getW(), curNode.getF(),curNode.getG(),curNode.getH());
               

                if (curNode.getK() == numCols && curNode.getW() == anchorLine.getNumWords())
                {
                    if (curNode.getF() >= scoreThreshold)
                    {
                        bestScore = Double.MaxValue;
                        return Double.MaxValue;
                    }

                    constructAnchorSegmentation(curNode);
                    bestScore =  alignGivenAnchor(bestAnchorRecord);
                    return bestScore;
                }

              
                bool tag = openNodes.Remove(curNode);
                System.Diagnostics.Debug.Assert(tag);
                closedNodes.Add(curNode);

               
                if(curNode.getF() >= scoreThreshold)
                {
                    continue;
                }
            

                HashSet<AStarNode> neiborNodes = getNeighbors(curNode);
                foreach (AStarNode neiborNode in neiborNodes)
                {
                  
                    if (closedNodes.Contains(neiborNode))
                        continue;

                    Dictionary<Line, double[,]> tentative_line2Scores = new Dictionary<Line, double[,]>();
                    double tentative_g = neiborNode.tentativeG_parallel(curNode, numCols, tentative_line2Scores, localScoring);
                   
                    if(!openNodes.Contains(neiborNode) || tentative_g < neiborNode.getG())
                    {
                        //Set preK, PreW
                        neiborNode.setPreK(curNode.getK());
                        neiborNode.setPreW(curNode.getW());

                        //Set the line2Score and set G
                        neiborNode.setLine2Scores(tentative_line2Scores);
                        neiborNode.setG(tentative_g);
                        //Set H and F
                        neiborNode.setH(inteSeg.getBestScore_Backwards(numCols - neiborNode.getK(), neiborNode.getW()));
                        neiborNode.setF();
                        if (!openNodes.Contains(neiborNode))
                            openNodes.Add(neiborNode);
                    }

                }
                
            }
            bestScore = Double.MaxValue;
            return Double.MaxValue;
        }



        private void constructAnchorSegmentation(AStarNode finalNode)
        {
            List<Cell> cells = new List<Cell>();

            int curK = finalNode.getK();
            int curW = finalNode.getW();
            System.Diagnostics.Debug.Assert(curK == numCols && curW == anchorLine.getNumWords());
            AStarNode curNode = finalNode;

            while (curK >= 1)
            {
                curNode = getAStarNode(curK, curW);
                int preK = curNode.getPreK();
                int preW = curNode.getPreW();

                if (preW == curW)
                {
                    Cell cell = new Cell(anchorLine, null);
                    cells.Insert(0, cell);
                }
                else
                {
                    Cell cell = new Cell(anchorLine, preW, curW - 1);
                    cells.Insert(0, cell);
                }
                curK = preK;
                curW = preW;
            }


            bestAnchorRecord = new Record(cells);
             
        }



        private double alignGivenAnchor(Record anchorRecord)
        {
            System.Diagnostics.Debug.Assert(anchorRecord.isValidRecord(anchorLine));
            double curScore = 0;
            //align all records in lines to record
            foreach (Line line in lines)
            {
                if (line != anchorLine)
                {

                    SingleAlignment sa2 = new SingleAlignment(line, localScoring);
                    double tempScore2 = sa2.alignMinimize(numCols, anchorRecord);
                    Record temp2 = sa2.getAlignedRecord();
                    curScore += tempScore2;

                }
            }
          
            return curScore;
        }

        public Record getBestAnchorRecord()
        {
            return bestAnchorRecord;
        }

        public double getBestScore()
        {
            return bestScore;
        }

    }
}
