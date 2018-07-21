﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace BlerghMerge
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1 && args.Length != 2)
            {
                Console.WriteLine("Usage: BlerghMerge.exe [SOURCE DIRECTORY] [OPTIONAL TARGET DIRECTORY]");
                return;
            }

            // Get and clean up the paths
            string sourcePath = args[0];
            if (sourcePath.Last() != '\\')
            {
                sourcePath = sourcePath+'\\';
            }

            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine("Directory {0} not found!", sourcePath);
                return;
            }

            string targetPath = args.Length == 2 ?
                args[1] :
                Path.GetFullPath(Path.Combine(sourcePath, "..\\", "blergh output\\"));
            
            // Delete the previous directory if it already exists
            if (Directory.Exists(targetPath))
            {
                try
                {
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


        private static void PreProcessFiles(string directoryPath)
        {
            DirectoryInfo dir = new DirectoryInfo(directoryPath);

            // Get the files in the directory and run the update on them
            foreach (FileInfo file in dir.GetFiles())
            {
                string[] possibleFiles = new string[] { "html", "htm", "js", "css", "txt", "json"};
                string ext = file.Extension.Substring(1);
                if (possibleFiles.Contains(ext))
                {
                    PreProcessFile(file.FullName);
                }
            }

            // Recursively call the folders
            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                PreProcessFiles(subdir.FullName);
            }
        }

        private static void PreProcessFile(string filePath)
        {
            List<string> content = File.ReadAllLines(filePath).ToList();
            bool insideIgnore = false;
            int ignoreStart = -1;

            for (int i=0;i<content.Count;i++)
            {
                string line = content[i].ToLower();
                if (!insideIgnore)
                {
                    if (line.Contains("//") && !line.Contains("end")
                    && (line.Contains("blergh ignore") || line.Contains("blergh! ignore")))
                    {
                        insideIgnore = true;
                        ignoreStart = i;
                    }
                } else
                {
                    if (line.Contains("//") && line.Contains("end")
                    && (line.Contains("blergh ignore") || line.Contains("blergh! ignore")))
                    {
                        insideIgnore = false;
                        content.RemoveRange(ignoreStart, i-ignoreStart+1);
                        i = ignoreStart;
                    }
                }
            }

            File.WriteAllLines(filePath, content);
        }

        private static void UpdateFiles(string directoryPath)
        {
            DirectoryInfo dir = new DirectoryInfo(directoryPath);

            // Get the files in the directory and run the update on them
            foreach (FileInfo file in dir.GetFiles())
            {
                string ext = file.Extension;
                if (ext == ".html" || ext == ".htm")
                {
                    UpdateFile(file.FullName);
                }
            }

            // Recursively call the folders
            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                UpdateFiles(subdir.FullName);
            }
        }
 
        private static void UpdateFile(string filePath)
        {
            List<string> content = File.ReadAllLines(filePath).ToList();

            for (int i=0;i<content.Count;i++)
            {
                string ogLine = content[i];
                string line = content[i].Trim().ToLower();
                if (line == "<!--blergh!-->" || line == "<!-- blergh! -->"
                 || line == "<!--blergh-->" || line == "<!-- blergh -->")
                {
                    //Remove the blergh line
                    content.RemoveAt(i);
                    //The next line is now i

                    line = content[i];

                    content.RemoveAt(i);
                    //i is now after the remove lines

                    //Check for indentation
                    char firstChar = ogLine.First();
                    int indentation = 0;
                    if (firstChar == '\t' || firstChar == ' ')
                    {
                        indentation = CountRepeatChars(ogLine);
                    }

                    // If it's a css file
                    if (line.Contains("link rel=\"stylesheet\""))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(line);
                        XmlNode newNode = doc.DocumentElement;
                        string path = newNode.Attributes["href"].Value;
                        path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filePath), path));
                        string[] lines = File.ReadAllLines(path);
                        IndentStrings(lines, indentation+1, firstChar);
                        content.Insert(i++, new string(firstChar, indentation) + "<style>");
                        content.InsertRange(i, lines);
                        i += lines.Length;
                        content.Insert(i, new string(firstChar, indentation) + "</style>");
                    }
                    // A script file
                    else if (line.Contains("<script"))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(line);
                        XmlNode newNode = doc.DocumentElement;
                        string path = newNode.Attributes["src"].Value;
                        path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filePath), path));
                        string[] lines = File.ReadAllLines(path);
                        IndentStrings(lines, indentation+1, firstChar);
                        content.Insert(i++, new string(firstChar, indentation) + "<script>");
                        content.InsertRange(i, lines);
                        i += lines.Length;
                        content.Insert(i, new string(firstChar, indentation) + "</script>");
                    }
                    // Or a html file
                    else if (line.Contains("class = \"imported") || line.Contains("class=\"imported"))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(line);
                        XmlNode newNode = doc.DocumentElement;
                        string path = newNode.InnerText;
                        path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filePath), path));
                        string[] lines = File.ReadAllLines(path);
                        IndentStrings(lines, indentation, firstChar);
                        content.InsertRange(i, lines);
                    }
                }
            }

            File.WriteAllLines(filePath, content);
        }

        /// <summary>
        /// Copies a directory from one location to another
        /// </summary>
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Taken from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static int CountRepeatChars(string str)
        {
            if (str.Length == 0)
            {
                return 0;
            }
            char firstChar = str.First();
            int count = 1;
            for (; count<str.Length; count++)
            {
                if (str[count] != firstChar)
                {
                    break;
                }
            }
            return count;
        }

        private static void IndentStrings(string[] strings, int indention, char indentChar)
        {
            int len = strings.Length;
            for (int i = 0; i < len; i++)
            {
                strings[i] = new string(indentChar, indention) + strings[i];
            }
        }
    }
}
