using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WebTableExtraction.Utils;
namespace WebTableExtraction.Judie
{
    public class Judie
    {
        //input
        List<Line> lines = new List<Line>();
        Dictionary<Line, Record> examples = new Dictionary<Line, Record>();

        //my internal data strucutre
        LocalScoringInfo localScoring;
        

        //output
        Dictionary<Line, Record> line2Record = new Dictionary<Line, Record>();
        double table_score = 0;
        //the number of columns if no examples were provided
        int guess_NumCol = 0;

        public Judie(string filePath)
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
        public Judie(List<Line> lines)
        {
            this.lines = lines;
            this.localScoring = new LocalScoringInfo(lines, 0);

        }
        public Judie(List<Line> lines, Dictionary<Line,Record> examples)
        {
            this.lines = lines;
            this.examples = examples;
            localScoring = new LocalScoringInfo(lines, examples.Count());
        }
        public void align()
        {
            //Step 1: structure free labelling
            Dictionary<Line, LabelledRecord> line2LabelledRecord = new Dictionary<Line, LabelledRecord>();
            foreach(Line line in lines)
            {
                StructureFreeLabelling sfLabelling = new StructureFreeLabelling(line, localScoring);
                LabelledRecord labelledRecord = sfLabelling.getSplittedRecord();
                line2LabelledRecord[line] = labelledRecord;
            }

            //Step 2: Detect structures, cycles. 
            

            //Step 3: Build the PSM
            PSM psm = new PSM(line2LabelledRecord);

            //Step 4: structure aware labelling
            Dictionary<Line, LabelledRecord> line2LabelledRecord_structure_aware = new Dictionary<Line, LabelledRecord>();
            foreach(Line line in lines)
            {
                StructureAwareLabelling saLabelling = new StructureAwareLabelling(line,line2LabelledRecord[line],localScoring,psm);
                LabelledRecord labelledRecord = saLabelling.getLabelledRecord();
                line2LabelledRecord_structure_aware[line] = labelledRecord;
            }


            //Step 5: Set the number of columns, and the best labelling for that 
            Dictionary<string, int> labelling2Count = new Dictionary<string, int>();
            foreach(Line line in lines)
            {
                string labelling = line2LabelledRecord_structure_aware[line].getConcatenatedLabels();
                if(labelling2Count.ContainsKey(labelling))
                {
                    labelling2Count[labelling] = labelling2Count[labelling] + 1;
                }
                else
                {
                    labelling2Count[labelling] = 1;
                }
            }
            string best_labelling = null;
            int best_support = 0;
            foreach(string labelling in labelling2Count.Keys)
            {
                if (labelling2Count[labelling] > best_support)
                {
                    best_support = labelling2Count[labelling];
                    best_labelling = labelling;
                }
            }
            string[] tempSplits = new string[] { "||" };
            List<string> desiredLabelling = best_labelling.Split(tempSplits, StringSplitOptions.None).ToList();

            guess_NumCol = desiredLabelling.Count();

            //Step 6: Align all the labelling to the best labelling
           
            foreach(Line line in lines)
            {
                AlignmentLabelling al = new AlignmentLabelling(line, line2LabelledRecord_structure_aware[line], desiredLabelling);
                line2Record[line] = al.getLabelledRecord();
            }



        }

        
            


        public void align(int numCols)
        {
            //Step 1: structure free labelling
            Dictionary<Line, LabelledRecord> line2LabelledRecord = new Dictionary<Line, LabelledRecord>();
            foreach (Line line in lines)
            {
                StructureFreeLabelling sfLabelling = new StructureFreeLabelling(line, localScoring);
                LabelledRecord labelledRecord = sfLabelling.getSplittedRecord();
                line2LabelledRecord[line] = labelledRecord;
            }

            //Step 2: Detect structures, cycles. 


            //Step 3: Build the PSM
            PSM psm = new PSM(line2LabelledRecord);

            //Step 4: structure aware labelling
            Dictionary<Line, LabelledRecord> line2LabelledRecord_structure_aware = new Dictionary<Line, LabelledRecord>();
            foreach (Line line in lines)
            {
                StructureAwareLabelling saLabelling = new StructureAwareLabelling(line, line2LabelledRecord[line], localScoring,  psm);
                LabelledRecord labelledRecord = saLabelling.getLabelledRecord();
                line2LabelledRecord_structure_aware[line] = labelledRecord;
            }


            //Step 5: Set the number of columns, and the best labelling for that 
            Dictionary<string, int> labelling2Count = new Dictionary<string, int>();
            foreach (Line line in lines)
            {
                string labelling = line2LabelledRecord_structure_aware[line].getConcatenatedLabels();
                if (labelling2Count.ContainsKey(labelling))
                {
                    labelling2Count[labelling] = labelling2Count[labelling] + 1;
                }
                else
                {
                    labelling2Count[labelling] = 1;
                }
            }
            string best_labelling = null;
            int best_support = 0;
            foreach (string labelling in labelling2Count.Keys)
            {
                string[] xxxSplits = new string[] { "||" };
                List<string> labellingAftersplit = labelling.Split(xxxSplits, StringSplitOptions.None).ToList();
                if (labellingAftersplit.Count() != numCols)
                    continue;
                if (labelling2Count[labelling] > best_support)
                {
                    best_support = labelling2Count[labelling];
                    best_labelling = labelling;
                }
            }
            List<string> desiredLabelling = null;
            if(best_labelling != null)
            {
                string[] tempSplits = new string[] { "||" };
                desiredLabelling = best_labelling.Split(tempSplits, StringSplitOptions.None).ToList();
            }
            else
            {
                desiredLabelling = new List<string>();
                for(int i = 0; i < numCols;i++)
                {
                    desiredLabelling.Add("");
                }
            }

            guess_NumCol = desiredLabelling.Count();

            //Step 6: Align all the labelling to the best labelling

            foreach (Line line in lines)
            {
                AlignmentLabelling al = new AlignmentLabelling(line, line2LabelledRecord_structure_aware[line], desiredLabelling);
                line2Record[line] = al.getLabelledRecord();
            }
        }

        public int getGuessNumCols()
        {
            return guess_NumCol;
        }
        public double getExtractedTableScore()
        {
            return 0;
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
