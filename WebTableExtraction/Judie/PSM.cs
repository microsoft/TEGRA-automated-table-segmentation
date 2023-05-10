using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTableExtraction.Utils;

namespace WebTableExtraction.Judie
{
    class PSM
    {
        //input
        Dictionary<Line, LabelledRecord> line2LabelledRecord;


        //output
        public PSM(Dictionary<Line, LabelledRecord> line2LabelledRecord)
        {
            this.line2LabelledRecord = line2LabelledRecord;
        }

        public double transitition_prob(string label1, string  label2)
        {
            double result = 0;
            if(label2 == null)
            {
                double label_1_end = 0;
                double label_1_total = 0;
                foreach (Line line in line2LabelledRecord.Keys)
                {
                    LabelledRecord labelledRecord = line2LabelledRecord[line];

                    for (int i = 0; i < labelledRecord.getNumCells(); i++)
                    {
                        string label_i = labelledRecord.getLabel(i);

                        if (label_i == label1)
                        {
                            label_1_total++;
                            if (i + 1 == labelledRecord.getNumCells())
                            {
                                label_1_end++;
                            }
                        }
                        
                    }
                }
                result = label_1_end / label_1_total;
            }
            else
            {
                double label1_2_label2 = 0;
                double label1_2 = 0;

                foreach (Line line in line2LabelledRecord.Keys)
                {
                    LabelledRecord labelledRecord = line2LabelledRecord[line];

                    for (int i = 0; i < labelledRecord.getNumCells(); i++)
                    {
                        string label_i = labelledRecord.getLabel(i);

                        if (label_i == label1)
                        {
                            label1_2++;
                        }
                        if (i + 1 < labelledRecord.getNumCells())
                        {
                            if (labelledRecord.getLabel(i + 1) == label2)
                            {
                                label1_2_label2++;
                            }
                        }
                    }
                }
                 result = label1_2_label2 / label1_2;
            }
            
            

            return result;
        }

        public double position_prob(string label, int k)
        {

            double label_k = 0;
            double total_k = 0;

            foreach (Line line in line2LabelledRecord.Keys)
            {
                LabelledRecord labelledRecord = line2LabelledRecord[line];

                if( k < labelledRecord.getNumCells())
                {
                    if(label == labelledRecord.getLabel(k))
                    {
                        label_k++;
                    }
                    total_k++;
                }
            }

            double result = 0;
            if(total_k != 0)
            {
                result = label_k / total_k;
            }
            return result;
            
        }


    }
}
