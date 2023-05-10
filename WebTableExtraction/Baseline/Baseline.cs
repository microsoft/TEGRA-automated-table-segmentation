using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using WebTableExtraction.Utils;

namespace WebTableExtraction.Baseline
{
    public class Baseline
    {

        //input
        List<Line> lines = new List<Line>();
        LocalScoringInfo localScoring;
        Dictionary<Line, Record> examples = new Dictionary<Line, Record>();

        //output
        Dictionary<Line, Record> line2Record = new Dictionary<Line, Record>();
        double table_score = 0;
        //the number of columns if no examples were provided
        int guess_NumCol = 0;

        public Baseline(string filePath)
        {
            string[] temps = System.IO.File.ReadAllLines(filePath);
            foreach (string temp in temps)
            {
                Line line = new Line(temp);
                lines.Add(line);
            }
           

            //Add examples from 
            string groundFile = filePath.Replace(Parameter.inputSequence_directory, Parameter.inputTabular_directory);
            List<Record> records = new List<Record>();
            string[] temps2 = System.IO.File.ReadAllLines(groundFile);
            foreach (string temp in temps2)
            {
                Record record = new Record(temp);
                records.Add(record);
            }

            //select number of examples from the middle to avoid bad cases
            List<Line> linesForSelectingExamples = new List<Line>(lines);
            int myNumExamples = Parameter.numExamples;
            if (Parameter.numExamples > linesForSelectingExamples.Count())
            {
                myNumExamples = linesForSelectingExamples.Count();
            }
            for (int i = 0; i < myNumExamples; i++)
            {
                //int rnd = new Random().Next(linesForSelectingExamples.Count());
                int rnd = linesForSelectingExamples.Count() / 2;
                Line line = linesForSelectingExamples.ElementAt(rnd);
                linesForSelectingExamples.RemoveAt(rnd);

                int index = lines.IndexOf(line);
                examples[lines.ElementAt(index)] = records.ElementAt(index);
            }


            this.localScoring = new LocalScoringInfo(lines, examples.Count());
        }
        public Baseline(List<Line> lines)
        {
            this.lines = lines;
            this.localScoring = new LocalScoringInfo(lines, 0);
        }
        public Baseline(List<Line> lines, Dictionary<Line,Record> examples)
        {
            this.lines = lines;
            this.examples = examples;
            localScoring = new LocalScoringInfo(lines, examples.Count());
        }
        public Record getAlignedRecord(Line line)
        {
            return line2Record[line];
        }
        public int getGuessNumCols()
        {
            foreach (Line line in lines)
            {
                Record record = line2Record[line];
                return record.getNumCells();
            }
            return 0;
        }

        private void setGuessNumCols()
        {
            //after independent splitting phase
            Dictionary<int, int> numCols2Support = new Dictionary<int, int>();
            foreach(Line line in lines)
            {
                System.Diagnostics.Debug.Assert(line2Record.ContainsKey(line));
                Record record = line2Record[line];
                int curNumcols = record.getNumCells();

                if(numCols2Support.ContainsKey(curNumcols))
                {
                    numCols2Support[curNumcols] = numCols2Support[curNumcols] + 1;
                }
                else
                {
                    numCols2Support[curNumcols] = 1;
                }
            }

            //I prefer shorter number of columns, if two number of columns have the same support
            List<int> allNumCols = new List<int>(numCols2Support.Keys);
            allNumCols.Remove(0);
            allNumCols = allNumCols.OrderByDescending(x => numCols2Support[x]).ThenBy(x => x).ToList();
            if (allNumCols.Count > 0)
                guess_NumCol = allNumCols.ElementAt(0);
            else
                guess_NumCol = 1;
        }

        //Align without number of columns as given
        public void align()
        {

            System.Diagnostics.Debug.Assert(examples.Count() == 0);
            //Step 1: Independent splitting phase

            foreach (Line line in lines)
            {
                Record splittedRecord = new Splitting(line, localScoring).getSplittedRecord();
                if (!splittedRecord.isValidRecord(line))
                {
                    Console.WriteLine(" problem in the indepdent splitting");
                }
                line2Record.Add(line, splittedRecord);
            }

            setGuessNumCols();
            
            align(guess_NumCol);
        }

        public void align(int numCols)
        {
            //Step 1: Independent splitting phase

            foreach (Line line in lines)
            {
                Record splittedRecord = new Splitting(line, localScoring).getSplittedRecord();
                if (!splittedRecord.isValidRecord(line))
                {
                    Console.WriteLine(" problem in the indepdent splitting");
                }
                line2Record[line] = splittedRecord;
            }

            //Step 2: Alignment phase
            FieldSummary fs = new FieldSummary(numCols, localScoring,examples);

            //align long records first
            foreach (Line line in lines)
            {
                Record record = line2Record[line];
                if (record.getNumCells() > numCols)
                {
                    Record alignRecord = new Alignment(line, record, numCols, fs, localScoring).getAlignedRecord();
                    line2Record[line] = alignRecord;
                }
            }
            //sort the record in descending number of cols
            List<Line> sortedLines = lines.OrderByDescending(x => line2Record[x].getNumCells()).ToList();

            foreach (Line line in sortedLines)
            {
                Record record = line2Record[line];

                if (record.getNumCells() < numCols)
                {
                    Record alignRecord = new Alignment(line, record, numCols, fs, localScoring).getAlignedRecord();
                    line2Record[line] = alignRecord;
                    if (!alignRecord.isValidRecord(line))
                    {
                        Console.WriteLine(" problem in the short alignment ");
                    }
                }

                fs.updateFieldSummary(line2Record[line]);

            }

            //Step 3: refinement
            Refinement refinement = new Refinement(line2Record.Values.ToList(), fs, localScoring);
            foreach (Line line in lines)
            {
                Record refinedRecord = refinement.getRefinedRecord(line2Record[line]);

                if (!refinedRecord.isValidRecord(line))
                {
                    Console.WriteLine(" problem in the refinement");
                }
                line2Record[line] = refinedRecord;
            }

            //Step 4: set the score of the extracted table
            foreach (Line line in lines)
            {
                Record record = line2Record[line];
                for(int i = 0; i < record.getNumCells(); i++)
                {
                    table_score += fs.getF2FCScore(i, record.getCell(i));
                }
            }
            table_score  = table_score / (lines.Count() * numCols);

        }



        public double getExtractedTable_score()
        {
            return table_score;
        }
        public Table get_Extracted_Table()
        {
            List<Record> records = new List<Record>();
            foreach (Line line in lines)
            {
                Record record = null;
                if (examples.ContainsKey(line))
                {
                    record = examples[line];
                }
                else
                {
                    record = line2Record[line];
                }
                records.Add(record);
            }
            Table table = new Table(records);
            return table;
        }

        public void writeExtractedToFile(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            StreamWriter sw = new StreamWriter(filePath);

            foreach (Line line in lines)
            {
                Record record = null;
                if (examples.ContainsKey(line))
                {
                    record = examples[line];
                }
                else
                {
                    record = line2Record[line];
                }
                sw.WriteLine(record);
            }
            sw.Close();

            //after writing the extracted file, free the memory
            localScoring.freeMemory();
        }

    }
}
