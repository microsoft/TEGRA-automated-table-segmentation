using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTableExtraction.Utils
{
    public class Seperator
    {
        private static string tabularSeperator = "||";

        private static string[] seperators = new string[]{
            @" ",
            @",",
            @"-",
            @"(",
            @")",
            @"/",
            @"\",
            @";",
            @":",
            @"&",
            @"–",
        };


        /**
         * Possible list of seperators 
         */
        public static string[] getSeperators()
        {
            return seperators;
        }

        public static string getTabularSeperator()
        {
            return tabularSeperator;
        }



        public static string getAdHoc()
        {
            return @"#XAYXZ#";
            //return @"xuxuxuxuxuxuxuxuxuxuxuxuxuxuxu";
        }

    }
}
