using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace BlerghMerge {
    class Program {
        static void Main(string[] args) {
            if (args.Length != 1 && args.Length != 2) {
                Console.WriteLine("Usage: BlerghMerge [SOURCE DIRECTORY] [OPTIONAL TARGET DIRECTORY]");
                return;
            }

            // Get and clean up the paths
            string sourcePath = args[0];
            if (sourcePath.Last() != '\\') {
                sourcePath = sourcePath + '\\';
            }

            if (!Directory.Exists(sourcePath)) {
                Console.WriteLine("Directory {0} not found!", sourcePath);
                return;
            }

            string targetPath = args.Length == 2 ?
                args[1] :
                Path.GetFullPath(Path.Combine(sourcePath, "..\\", "blergh output\\"));

            // Delete the previous directory if it already exists
            if (Directory.Exists(targetPath)) {
                try {
                    Directory.Delete(targetPath, true);
                } catch { }
            }

            //Copy all files
            DirectoryCopy(sourcePath, targetPath, true);

            //Do the preprocessing
            PreProcessFiles(targetPath);

            //Move through all files applying our thingies
            UpdateFiles(targetPath);
        }


        private static void PreProcessFiles(string directoryPath) {
            DirectoryInfo dir = new DirectoryInfo(directoryPath);

            // Get the files in the directory and run the update on them
            foreach (FileInfo file in dir.GetFiles()) {
                string[] possibleFiles = new string[] { "html", "htm", "js", "css", "txt", "json" };
                string ext = file.Extension.Substring(1);
                if (possibleFiles.Contains(ext)) {
                    PreProcessFile(file.FullName, ext);
                }
            }

            // Recursively call the folders
            foreach (DirectoryInfo subdir in dir.GetDirectories()) {
                PreProcessFiles(subdir.FullName);
            }
        }

        private static void PreProcessFile(string filePath, string filetype) {
            List<string> content = File.ReadAllLines(filePath).ToList();
            bool insideIgnore = false;
            int ignoreStart = -1;

            string commentIdentifier = filetype == "html" || filetype == "htm" ? "<!--" : "//";

            for (int i = 0; i < content.Count; i++) {
                string line = content[i].ToLower();
                if (!insideIgnore) {
                    if (line.Contains(commentIdentifier) && !line.Contains("end")
                    && (line.Contains("blergh ignore") || line.Contains("blergh! ignore"))) {
                        insideIgnore = true;
                        ignoreStart = i;
                    }
                } else {
                    if (line.Contains(commentIdentifier) && line.Contains("end")
                    && (line.Contains("blergh ignore") || line.Contains("blergh! ignore"))) {
                        insideIgnore = false;
                        content.RemoveRange(ignoreStart, i - ignoreStart + 1);
                        i = ignoreStart;
                    }
                }
            }

            File.WriteAllLines(filePath, content);
        }

        private static void UpdateFiles(string directoryPath) {
            DirectoryInfo dir = new DirectoryInfo(directoryPath);

            // Get the files in the directory and run the update on them
            foreach (FileInfo file in dir.GetFiles()) {
                string ext = file.Extension;
                if (ext == ".html" || ext == ".htm") {
                    UpdateFile(file.FullName);
                }
            }

            // Recursively call the folders
            foreach (DirectoryInfo subdir in dir.GetDirectories()) {
                UpdateFiles(subdir.FullName);
            }
        }

        private static void UpdateFile(string filePath) {
            List<string> content = File.ReadAllLines(filePath).ToList();

            for (int i = 0; i < content.Count; i++) {
                string line = content[i].Trim().ToLower();
                if (line == "<!--blergh!-->" || line == "<!-- blergh! -->"
                 || line == "<!--blergh-->" || line == "<!-- blergh -->") {
                    //Remove the blergh line
                    content.RemoveAt(i);

                    List<CodeFile> files = new List<CodeFile>();
                    CodeFile previous = null;

                    // Scan until it finds something that can't be imported
                    for (; i < content.Count; i++) {
                        line = content[i];

                        CodeFile file = null;

                        // If it's a css file
                        if (line.Contains("link rel=\"stylesheet\"")) {
                            file = new CodeFileCSS(line, filePath);
                        }
                        // A script file
                        else if (line.Contains("<script")) {
                            file = new CodeFileJavaScript(line, filePath);
                        }
                        // Or a html file
                        else if (line.Contains("class = \"imported") || line.Contains("class=\"imported")) {
                            file = new CodeFileHtml(line, filePath);
                        }

                        if (file == null) {
                            break;
                        }

                        content.RemoveAt(i--);
                        files.Add(file);
                    }

                    if (files.Count == 0) {
                        continue;
                    }

                    List<string> insert = new List<string>();

                    foreach (CodeFile f in files) {
                        insert.AddRange(f.ExportLines());
                    }

                    //Replace the line with the new content
                    
                    content.Insert(i, files[0].WrapContent(insert));
                }
            }

            File.WriteAllLines(filePath, content);
        }


        /// <summary>
        /// Copies a directory from one location to another
        /// </summary>
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
            // Taken from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files) {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs) {
                foreach (DirectoryInfo subdir in dirs) {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
