using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace WebTableExtraction.Utils
{
    class FileUtil
    {
        public static List<string> getAllFiles(string root_directory)
        {
            List<string> ret_json_file_paths = new List<string>();

            if (Directory.Exists(root_directory))
            {
                ProcessDirectory(root_directory, ref ret_json_file_paths);
            }

            return ret_json_file_paths;
        }
        public static void ProcessDirectory(string directory, ref List<string> ret_json_file_paths)
        {
            string[] fileEntries = Directory.GetFiles(directory);
            foreach (string fileName in fileEntries)
            {
                ret_json_file_paths.Add(fileName);
            }

            string[] subdirectoryEntries = Directory.GetDirectories(directory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, ref ret_json_file_paths);

        }

       

    }
}
