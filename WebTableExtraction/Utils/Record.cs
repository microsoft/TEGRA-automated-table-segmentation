using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTableExtraction.Utils
{
    public class Record
    {

        private List<Cell> cells = new List<Cell>();

        public Record(string record)
        {
            string[] seps = new string[] { Seperator.getTabularSeperator()};
            string[] temps = record.Split(seps, StringSplitOptions.None);
            foreach (string temp in temps)
            {
                Cell cell = new Cell(temp);
                cells.Add(cell);
            }
        }

        public Record(List<Cell> cells)
        {
            this.cells = cells;
        }

        public Record(Cell cell)
        {
            cells.Add(cell);
        }

        public Record()
        {
            
        }

        /**
         * Deep copy a record
         */ 
        public Record(Record record)
        {
            foreach (Cell cell in record.cells)
            {
                this.cells.Add(cell);
            }
        }

        public void addCell(Cell cell)
        {
            cells.Add(cell);
        }

        public Cell getCell(int index)
        {
            return cells.ElementAt(index);
        }

        public void replaceCell(int index, Cell newCell)
        {
            System.Diagnostics.Debug.Assert(index >= 0 && index < cells.Count())   ;
            cells.RemoveAt(index);
            cells.Insert(index,newCell);
        }

        public int getNumCells()
        {
            return cells.Count();
        }

        public Record getSubRecord(int numCols)
        {
            List<Cell> subCells = new List<Cell>();
            for (int i = 0; i < numCols; i++)
            {
                subCells.Add(cells[i]);
            }
            return new Record(subCells);
        }

        

        public bool isSame(Record record)
        {
            if (getNumCells() != record.getNumCells())
                return false;

            for (int i = 0; i < getNumCells(); i++)
            {
                if (!getCell(i).isSame(record.getCell(i)))
                    return false;
            }
            return true;
        }

        /**
         * Get the last word in the line, that has been referenced in the current record
         */
        public int getNextAvailableWordIndex()
        {
            for (int i = cells.Count() - 1; i >= 0; i--)
            {
                Cell cell = cells.ElementAt(i);
                if (cell.getEndIndex() != -1)
                    return cell.getEndIndex() + 1;
            }
            return 0;
        }

        public Line toLine()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells.ElementAt(i);
                if (i != cells.Count - 1)
                {
                    sb.Append(cell.getCellValue());
                    sb.Append(" ");
                }
                else
                {
                    sb.Append(cell.getCellValue());

                }

            }
            return new Line(sb.ToString());
        }

        public String toString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells.ElementAt(i);
                if (i != cells.Count - 1)
                {
                    sb.Append(cell.getCellValue());
                    sb.Append(Seperator.getTabularSeperator());
                }
                else
                {
                    sb.Append(cell.getCellValue());
                    
                }
           
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            return toString();
        }
        

        /**
         *Test if current record is a valid segmentation of the line 
         */
        public bool isValidRecord(Line line)
        {
            if (cells.Count() == 0 && line.ToString() != null)
                return false;

            if (cells.Count() > 0)
            {
                Cell firstCell = cells.ElementAt(0);
                if (firstCell.getStartIndex() == -1 && firstCell.getCellValue() == null)
                {

                }
                else if (firstCell.getStartIndex() == 0 && firstCell.getCellValue() != null)
                {

                }
                else
                {
                    Console.WriteLine("there is a problem at the first cell");
                    return false;
                }
            }

            for (int i = 1; i < cells.Count(); i++)
            {
                Cell cell = cells.ElementAt(i);
                if (cell.getCellValue() == null && cell.getStartIndex() == -1 && cell.getEndIndex() == -1)
                {

                }
                else if (cell.getCellValue() != null && cell.getStartIndex() == isValidRecordHelper(i-1) )
                {

                }
                else
                {
                    Console.WriteLine("there is a problem at the first cell");
                    return false;
                }
            }

            //check the last record
            if (cells.Count() > 0)
            {
                if (isValidRecordHelper(cells.Count() - 1) != line.getNumWords())
                {
                    Console.WriteLine("there is a problem at the last non null cell");
                    return false;
                }
            }
            return true;
        }
        private int isValidRecordHelper(int col)
        {
            for (int i = col; i >= 0; i--)
            {
                Cell cell = cells.ElementAt(i);
                if (cell.getEndIndex() != -1)
                    return cell.getEndIndex() + 1;
            }
            return 0;
        }

    }
}
