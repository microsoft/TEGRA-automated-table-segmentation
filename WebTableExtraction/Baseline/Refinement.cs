using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebTableExtraction.Utils;

namespace WebTableExtraction.Baseline
{
    class Refinement
    {
        //input, a list of already aligned records
        List<Record> records;
        FieldSummary fs;
        LocalScoringInfo localScoring;


        //output, a list of refined record
        Dictionary<Record, Record> record2RefinedRecord = new Dictionary<Record,Record>();

        public Refinement(List<Record> records, FieldSummary fs, LocalScoringInfo localScoring)
        {
            this.records = records;
            this.fs = fs;
            this.localScoring = localScoring;

            foreach (Record record in records)
            {
                record2RefinedRecord[record] = record;
            }
            
            refine();
        }



        private void refine()
        {
            HashSet<Streak> streaks = detectInconsistentStreaks();

            foreach (Streak streak in streaks)
            {
                //we only consider streaks that have at least two non-null columns
                if (!isGoodStreak(streak))
                    continue;

                //construct the new subline to resplit, realign
                Record beforeRefinement = record2RefinedRecord[streak.record];
                Line line = beforeRefinement.getCell(0).getLine();
                string subLineString = null;
                int subLineStartIndex = Int32.MaxValue;
                int subLineEndIndex = -1;
                for (int i = streak.startCol; i <= streak.endCol; i++)
                {
                    Cell cell = streak.record.getCell(i);
                    if (cell.getStartIndex() != -1 && cell.getStartIndex() < subLineStartIndex)
                    {
                        subLineStartIndex = cell.getStartIndex();
                    }
                    if (cell.getEndIndex() != -1 && cell.getEndIndex() > subLineEndIndex)
                    {
                        subLineEndIndex = cell.getEndIndex();
                    }
                }
                subLineString = line.getCellValue(subLineStartIndex, subLineEndIndex);

                Line subLine = new Line(subLineString);
                FieldSummary subFS = fs.getSubFieldSummary(streak.startCol, streak.endCol);
                int subNumCols = streak.endCol - streak.startCol + 1;

                Record subRecord = null;

                subRecord =  new Splitting(subLine, localScoring).getSplittedRecord();
                if (subRecord.getNumCells() > subNumCols)
                {
                    subRecord = new Alignment(subLine, subRecord, subNumCols, subFS, true, localScoring).getAlignedRecord(); 
                }
                if (subRecord.getNumCells() < subNumCols)
                {
                    subRecord = new Alignment(subLine, subRecord, subNumCols, subFS, true, localScoring).getAlignedRecord();
                }

                //Merge the subrecord into the original record
                //WARNING: the index of the cells in the subLine is not the same w.r.t. the original line
                //Console.WriteLine("--------------------");
                //Console.WriteLine("Before streak refinement: " + beforeRefinement.toString());
                //Console.WriteLine("The streak is: " + subLineString);

                if (beforeRefinement.toString() == "ID||NEZ||PERCE ID||208937"
                    && subLineString == "NEZ PERCE ID 208937")
                {
                    Console.WriteLine("debug");
                }

                Record afterRefinement = new Record();
                for (int i = 0; i < streak.record.getNumCells(); i++)
                {
                    if( i < streak.startCol || i > streak.endCol)
                    {
                        afterRefinement.addCell(streak.record.getCell(i));
                    }
                    else
                    {
                        Cell subCell = subRecord.getCell(i - streak.startCol);
                        int newCellStartIndex = -1;
                        int newCellEndIndex = -1;
                        if (subCell.getStartIndex() != -1)
                        {
                            newCellStartIndex = afterRefinement.getNextAvailableWordIndex();
                            newCellEndIndex = afterRefinement.getNextAvailableWordIndex() + (subCell.getEndIndex() - subCell.getStartIndex());
                        }
                        Cell newCell = new Cell(line, newCellStartIndex, newCellEndIndex);
                        afterRefinement.addCell(newCell);
                    }
                }
                record2RefinedRecord[streak.record] = afterRefinement;

              
                
                //Console.WriteLine("After streak refinement: " + afterRefinement.toString());
               // Console.WriteLine("--------------------");
            }

        }




        private bool isGoodStreak(Streak streak)
        {
            Record record = streak.record;

            int numNonNull = 0;

            for (int i = streak.startCol; i <= streak.endCol; i++)
            {
                Cell cell = streak.record.getCell(i);
                if (cell.getCellValue() != null)
                {
                    numNonNull++;
                }
            }

            if (numNonNull >= 2)
                return true;
            else
                return false;
        }

        private HashSet<Streak> detectInconsistentStreaks()
        {
            HashSet<Streak> result = new HashSet<Streak>();

            Dictionary<Cell, double> cell2F2FC = new Dictionary<Cell, double>();

            List<Cell> inconsistentCells = new List<Cell>();

            foreach (Record record in records)
            {
                for (int i = 0; i < record.getNumCells(); i++)
                {
                    Cell cell = record.getCell(i);
                    double f2fc = fs.getF2FCScore(i, cell);

                    cell2F2FC[cell] = f2fc;
                    inconsistentCells.Add(cell);
                }
            }

            //rank the cells
            inconsistentCells = inconsistentCells.OrderByDescending(x => cell2F2FC[x]).ToList();
            //int startRemoveIndex = inconsistentCells.Count() / 2;
            //int endRemoveIndex = inconsistentCells.Count() - 1;
            //inconsistentCells.RemoveRange(startRemoveIndex, (endRemoveIndex - startRemoveIndex + 1));

            //Any distance greater than curScore is considered to be bad;
            double cutScore = cell2F2FC[inconsistentCells.ElementAt((inconsistentCells.Count() + 1) / 2)];


            foreach (Record record in records)
            {
                int i = 0;

                while (i < record.getNumCells() )
                {
                    double f2fc = cell2F2FC[record.getCell(i)];
                    if (f2fc >= cutScore)
                    {
                        int j = i;
                        for (j = i; j < record.getNumCells(); j++)
                        {
                            if (cell2F2FC[record.getCell(j)] < cutScore)
                                break;
                        }
                        
                        //we have detected a streak from [i,j - 1]
                        Streak streak = new Streak();
                        streak.record = record;
                        streak.startCol = i;
                        streak.endCol = j - 1;
                        result.Add(streak);

                        i = j;

                    }
                    else
                    {
                        i++;
                    }
                }


            }

            return result;

        }

        private struct Streak
        {
            public Record record;
            public int startCol;
            public  int endCol;
        }


        public Record getRefinedRecord(Record record)
        {
            return record2RefinedRecord[record];
        }

    }
}
