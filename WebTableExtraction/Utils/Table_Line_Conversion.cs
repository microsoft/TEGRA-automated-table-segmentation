using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebTableExtraction.Utils
{
    class Table_Line_Conversion
    {
        public static Table line_2_table(string line)
        {
            List<Record> records = new List<Record>();
            foreach (string temp in line.Split(new string[] { "__________" }, StringSplitOptions.None))
            {
                Record record = new Record(temp);
                records.Add(record);
            }

            return new Table(records);
        }


        public static string table_2_line(Table table)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < table.getNumRecords(); i++)
            {
                Record record = table.getRecord(i);
                record.toString();

                sb.Append(record.toString());
                if (i != table.getNumRecords() - 1)
                    sb.Append("__________");
            }
            return sb.ToString();
        }
    }
}
