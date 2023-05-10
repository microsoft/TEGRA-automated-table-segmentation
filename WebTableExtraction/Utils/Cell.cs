using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTableExtraction.Utils
{
    public class Cell
    {



        Line line;
        int startIndex;
        int endIndex;
        string value;


        public Cell(Line line, int startIndex, int endIndex)
        {
            this.line = line;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.value = line.getCellValue(startIndex, endIndex);
        } 
        public Cell(Line line, string value)
        {
            this.line = line;
            this.value = value;
            this.startIndex = -1;
            this.endIndex = -1;
        }

        /**
         * Deep copy a cell
         */ 
        public Cell(Cell cell)
        {
            this.line = cell.line;
            this.startIndex = cell.startIndex;
            this.endIndex = cell.endIndex;
            this.value = cell.value;
        }


        public Cell(string value)
        {
            this.value = value;
        }



        public int getStartIndex()
        {
            return startIndex;
        }

        public int getEndIndex()
        {
            return endIndex;
        }


        public string getCellValue()
        {
            return value;
        }

        public void setCellValue(string newValue)
        {
            this.value = newValue;
        }

        public Line getLine()
        {
            return line;
        }


        public bool overlapping(Cell cell)
        {
            if (cell.startIndex > endIndex)
                return false;
            else if (startIndex > cell.endIndex)
                return false;
            else
                return true;
        }

        public bool isSame(Cell cell)
        {
            return value == cell.value;
        }

        public override string ToString()
        {
            return value;
            //return base.ToString();
        }

    }
}
