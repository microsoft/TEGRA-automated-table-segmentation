using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTableExtraction.Utils;

namespace WebTableExtraction.Judie
{
    class LabelledRecord: Record
    {

        List<string> labels;
        public LabelledRecord(List<Cell> cells, List<string> labels): base(cells)
        {
            this.labels = labels;
        }

        public string getLabel(int i)
        {
            return labels.ElementAt(i);
        }

        public string getConcatenatedLabels()
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0 ; i < getNumCells(); i++)
            {
                sb.Append(getLabel(i));
                if (i < getNumCells() - 1)
                    sb.Append("||");
            }
            return sb.ToString();
        }

    }
}
