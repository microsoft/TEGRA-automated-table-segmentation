using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ScopeRuntime;
using WebTableExtraction.Utils;
using WebTableExtraction.Extraction;
using WebTableExtraction.Baseline;
using WebTableExtraction.Judie;
using WebTableExtraction.Experiments;


public class MyCombine_StringString : Aggregate2<string, string, string>
{
    StringBuilder result = null;

    public override void Initialize()
    {
        result = new StringBuilder();
    }

    public override void Add(string value1, string value2)
    {

        result.Append(value1 + Seperator.getAdHoc() + value2);
        result.Append("#AAAAAAA#");
    }

    public override string Finalize()
    {
        string temp = result.ToString();
        temp = temp.Remove(temp.Length - 9);
        return temp;
    }


}














public class HighParallelProcessor: Processor
{
    public override void GetProperties(RowSetProperties.RSPCategory category, List<RowSetProperties> inputRowsetProps, RowSetProperties outputRowsetProp)
    {
        // sample user code for overriding the function
        switch (category)
        {
            case RowSetProperties.RSPCategory.Partition:
                {
                    // Parallelism related properties
                    outputRowsetProp.PartitionCount = 250;
                }
                break;

            default:
                break;
        }

    }



    public override Schema Produces(string[] requested_columns, string[] args, Schema input_schema)
    {

        var output_schema = new Schema();

        
        output_schema.Add(input_schema[0]);


        var newcol = new ColumnInfo("alignedTable", typeof(string));
        output_schema.Add(newcol);

        return output_schema;

    }


    public override IEnumerable<Row> Process(RowSet input_rowset, Row output_row, string[] args)
    {

        foreach (Row input_row in input_rowset.Rows)
        {
            string table_name = input_row[0].String;
            string input_table_line = input_row[1].String;
            string ground_table_line = input_row[2].String;

            string aligned_table = MyHelper.align_table(input_table_line, ground_table_line, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);

            //input_row.CopyTo(output_row);
            output_row[0].Set(table_name);
            output_row[1].Set(aligned_table);

            yield return output_row;

        }

    } 

}



public static class MyHelper
{
    public static string align_table(
        string input_table_line, string ground_table_line, string whichdataset, string all_parameters,
        string singleCellOccurrenceFilePath, string doubleCellOccurrenceFilepath, string singleCellLanguageModelFilePath,
         string kbAttri2Values, string kbValue2Attris)
    {
        //Set the approximate parameter for the program
        string[] splits_parametrs = all_parameters.Split(new string[] { "__" }, StringSplitOptions.None);
        string algorithm = splits_parametrs[0];
        bool numColsGiven = Convert.ToBoolean(splits_parametrs[1]);
        int numExamples = Convert.ToInt32(splits_parametrs[2]);
        double s_occurWeight = Convert.ToDouble(splits_parametrs[3]);
        double s_typeWeight = Convert.ToDouble(splits_parametrs[4]);
        double d_occurWeight = Convert.ToDouble(splits_parametrs[5]);
        double d_syntacticWeight = Convert.ToDouble(splits_parametrs[6]);

        int whichStats = Convert.ToInt32(splits_parametrs[7]);

        Parameter.numColsGiven = numColsGiven;
        Parameter.numExamples = numExamples;
        Parameter.s_languageWeight = 0;
        Parameter.s_occurWeight = s_occurWeight;
        Parameter.s_typeWeight = s_typeWeight;
        Parameter.d_occurWeight = d_occurWeight;
        Parameter.d_syntacticWeight = d_syntacticWeight;
        Parameter.whichStats = whichStats;


        string parentDir = System.IO.Directory.GetParent(whichdataset).ToString();



        Parameter.singleCellOccurrenceFilePath = singleCellOccurrenceFilePath;
        Parameter.doubleCellOccurrenceFilepath = doubleCellOccurrenceFilepath;
        Parameter.singleCellLanguageModelFilePath = singleCellLanguageModelFilePath;

        //remote setting
        if (singleCellOccurrenceFilePath.Contains("my"))
            Parameter.singleCellOccurrenceFilePath = Path.GetFileName(singleCellOccurrenceFilePath);
        if (doubleCellOccurrenceFilepath.Contains("my"))
            Parameter.doubleCellOccurrenceFilepath = Path.GetFileName(doubleCellOccurrenceFilepath); // Path.GetFileName(doubleCellOccurrenceFilepath.Split('\t')[0]) + "\t" + Path.GetFileName(doubleCellOccurrenceFilepath.Split('\t')[1]);
        if (singleCellLanguageModelFilePath.Contains("my"))
            Parameter.singleCellLanguageModelFilePath = Path.GetFileName(singleCellLanguageModelFilePath);

        Parameter.kb_value2attrisFilePath = parentDir + @"ScoringStats/kb_value_2_attris.txt";
        Parameter.kb_attri2valuesFilePath = parentDir + @"ScoringStats/kb_attri_2_values.txt";

        if (parentDir.Contains("my"))
        {
            Parameter.kb_value2attrisFilePath = Path.GetFileName(kbValue2Attris);
            Parameter.kb_attri2valuesFilePath = Path.GetFileName(kbAttri2Values);
        }

        //Construct a table from the table_line
        List<Line> lines = new List<Line>();
        foreach (string temp in input_table_line.Split(new string[] { "__________" }, StringSplitOptions.None))
        {
            Line line = new Line(temp);
            lines.Add(line);
        }
        List<Record> records = new List<Record>();
        foreach (string temp in ground_table_line.Split(new string[] { "__________" }, StringSplitOptions.None))
        {
            Record record = new Record(temp);
            records.Add(record);
        }

        //Construct numExamples, and numCols from the ground truth table
        Dictionary<Line, Record> examples = new Dictionary<Line, Record>();
        int numCols = 0;
        if (numColsGiven == true)
        {
            List<Line> linesForSelectingExamples = new List<Line>(lines);
            int myNumExamples = numExamples;
            if (numExamples > linesForSelectingExamples.Count)
            {
                myNumExamples = linesForSelectingExamples.Count;
            }
            for (int i = 0; i < myNumExamples; i++)
            {
                //int rnd = new Random().Next(linesForSelectingExamples.Count());
                int rnd = linesForSelectingExamples.Count / 2;
                Line line = linesForSelectingExamples[rnd];
                linesForSelectingExamples.RemoveAt(rnd);

                int index = lines.IndexOf(line);
                examples[lines[index]] = records[index];
            }
            numCols = records[0].getNumCells();
        }






        //Do the alignment
        Table aligned_table = null;
        if (algorithm == "MRA")
        {
            //Do the MSA
            MSAAppro msaAppro = new MSAAppro(lines, examples);
            if (Parameter.numColsGiven)
                msaAppro.alignAStar_with_numcols(numCols);
            else
                msaAppro.alignAStar_without_numcols();
            aligned_table = msaAppro.getExtracted_Table();
        }
        else if (algorithm == "ListExtract")
        {
            Baseline baseline = new Baseline(lines, examples);
            if (Parameter.numColsGiven)
                baseline.align(numCols);
            else
                baseline.align();
            aligned_table = baseline.get_Extracted_Table();
        }
        else if (algorithm == "Judie")
        {
            Judie judie = new Judie(lines, examples);
            if (Parameter.numColsGiven)
                judie.align(numCols);
            else
                judie.align();
            aligned_table = judie.get_Extracted_Table();
        }






        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < aligned_table.getNumRecords(); i++)
        {
            Record record = aligned_table.getRecord(i);
            record.toString();

            sb.Append(record.toString());
            if (i != aligned_table.getNumRecords() - 1)
                sb.Append("__________");
        }
        return sb.ToString();

    }




    public static string analyze(string whichdataset, string all_parameters, string dataset_ground_line, string dataset_aligned_line,
        string singleCellOccurrenceFilePath, string doubleCellOccurrenceFilepath, string singleCellLanguageModelFilePath)
    {
        Parameter.singleCellOccurrenceFilePath = singleCellOccurrenceFilePath;
        Parameter.doubleCellOccurrenceFilepath = doubleCellOccurrenceFilepath;
        Parameter.singleCellLanguageModelFilePath = singleCellLanguageModelFilePath;

        //remote setting
        if (singleCellOccurrenceFilePath.Contains("my"))
            Parameter.singleCellOccurrenceFilePath = Path.GetFileName(singleCellOccurrenceFilePath);
        if (doubleCellOccurrenceFilepath.Contains("my"))
            Parameter.doubleCellOccurrenceFilepath = Path.GetFileName(doubleCellOccurrenceFilepath); // Path.GetFileName(doubleCellOccurrenceFilepath.Split('\t')[0]) + "\t" + Path.GetFileName(doubleCellOccurrenceFilepath.Split('\t')[1]);
        if (singleCellLanguageModelFilePath.Contains("my"))
            Parameter.singleCellLanguageModelFilePath = Path.GetFileName(singleCellLanguageModelFilePath);
        

        ResultAnalysis ra = new ResultAnalysis();
       
        string whichdataset_tag = "";
        if (whichdataset.Contains(@"Enterprise_10k"))
            whichdataset_tag = "Enterprise";
            else if (whichdataset.Contains(@"General_WebTablesCorpus_10k"))
            whichdataset_tag = "Web";
            else if (whichdataset.Contains(@"WebTablesCorpus_200k"))
            whichdataset_tag = "Wiki";
        string[] splits_parametrs = all_parameters.Split(new string[] { "__" }, StringSplitOptions.None);


        string[] temps = dataset_aligned_line.Split(new string[]{"#AAAAAAA#"}, StringSplitOptions.None);
        string[] ground_tables = dataset_ground_line.Split(new string[] { "#AAAAAAA#" }, StringSplitOptions.None);

        //performan some sanity check

        if(temps.Length != ground_tables.Length)
        {
            return "two datasets streams do not have the same size";
        }

        string analysis = ra.scope_analyze_one_file(whichdataset_tag, splits_parametrs, temps, ground_tables);
        return analysis;
    }



}