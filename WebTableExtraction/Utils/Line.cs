using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTableExtraction.Utils
{
    public class Line
    {
        private string original;

        private string[] words;
        private string[] delimiters;

        /**
         * Seperate the original records into words, according to a set of pre-defined seperators
         * for example: Redmond WA, United States
         * words: <Redmond> <WA> <United> <States>
         * delimiters: < > <, > < >
         * 
         * WARNING: Have not consider the case where there are delimiters on the very left or on the very right., for example [Redmond WA, United States]
         */
        public Line(string original)
        {
            this.original = original;
            //init words, and seperators

            if (original == null)
                original = "";

            if (original.StartsWith(@"1. Louisiana Tech University"))
            {

            }

            words = original.Split(Seperator.getSeperators(), StringSplitOptions.RemoveEmptyEntries);

            if (words.Count() == 0)
            {
                //There is not words on this line
                return;
            }

            delimiters = new string[words.Count() - 1];

            int startIndex = 0;
            for (int i = 0; i < words.Count(); i++)
            {
                if (i == words.Count() - 1)
                    continue;

                //find the first occurance of the i word

                int index1 = original.IndexOf(words[i], startIndex);

                startIndex = index1 + words[i].Count();

                //find the first occrance of the i+1 word

                int index2 = original.IndexOf(words[i + 1], startIndex);

                if (index2 - index1 - words[i].Count() < 0)
                {
                }
                delimiters[i] = original.Substring(index1 + words[i].Count(), index2 - index1 - words[i].Count());
            
            }

            /*
            for (int i = 0; i < words.Count(); i++)
            {
                Console.WriteLine("Words: " + i + " is: " + words[i]);
            }
            for (int i = 0; i < delimiters.Count(); i++)
            {
                Console.WriteLine(" delimiters:  " + i + " is: " + delimiters[i]);
            }
             **/
            delimiters_special_treatment();
        }

        private void delimiters_special_treatment()
        {
            List<int> toBeRemovedIndex = new List<int>();
            for (int i = 0; i < delimiters.Length; i++)
            {
                if (delimiters[i] != ",")
                    continue;

                string left = words[i];
                string right = words[i+1];
                int n_left;
                int n_right;
                bool isNumericLeft = int.TryParse(left, out n_left);
                bool isNumericalRight = int.TryParse(right, out n_right);
                if(isNumericLeft && isNumericalRight)
                {
                    toBeRemovedIndex.Add(i);
                    continue;
                }

                if(left.StartsWith(@"$"))
                {
                    bool isNumericLeft_Dollar = int.TryParse(left.Substring(1), out n_left);
                    if (isNumericLeft_Dollar && isNumericalRight)
                    {
                        toBeRemovedIndex.Add(i);
                        continue;
                    }
                }

            }
            string[] newWords = new string[words.Length - toBeRemovedIndex.Count];
            string[] newDelimiters = new string[delimiters.Length - toBeRemovedIndex.Count];
            int myIndex = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < delimiters.Length; i++)
            {
                sb.Append(words[i]);
                if(!toBeRemovedIndex.Contains(i))
                {
                    newWords[myIndex] = sb.ToString();
                    myIndex++;
                    sb = new StringBuilder();
                }
                else
                {
                    sb.Append(",");

                }
            }
            sb.Append(words[words.Length - 1]);
            newWords[myIndex] = sb.ToString();


            myIndex = 0;
            for (int i = 0; i < delimiters.Length; i++)
            {
                if(!toBeRemovedIndex.Contains(i))
                {
                    newDelimiters[myIndex] = delimiters[i];
                    myIndex++;
                }
            }

            words = newWords;
            delimiters = newDelimiters;
            
        }


        public int getNumWords()
        {
            return words.Count();
        }

        /**
         * words values are 0-based
         * Get the cell value starting from [startIndex,endIndex]
         */ 
        public string getCellValue(int startIndex, int endIndex)
        {
            if (startIndex < 0 || endIndex < 0)
                return null;

            if (endIndex < startIndex)
                return null;

            StringBuilder sb = new StringBuilder();
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (i != endIndex)
                {
                    sb.Append(words[i]);
                    sb.Append(delimiters[i]);
                }
                else
                {
                    sb.Append(words[i]);
                }
            }

            return sb.ToString();
        }

        public Line subLine(int startIndex, int endIndex)
        {
            Line result = new Line(getCellValue(startIndex,endIndex));
            return result;
        }

        public override string ToString()
        {
            return original;
        }
    }
}
