using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlerghMerge {
    class CodeFile {
        protected int indentation;
        protected char indentationChar;
        protected string path;
        

        public CodeFile(string source, string documentPath) {
            (indentation, indentationChar) = GetIndentionData(source);
        }

        public virtual List<string> ExportLines() {

            string[] lines = File.ReadAllLines(path);
            Indent(lines, indentation + 1, indentationChar);

            return new List<string>(lines);
        
        }

        public virtual string WrapContent(List<string> content) {
            throw new NotImplementedException();
        }

        protected void Indent(string[] content, int indentation, char indentChar) {
            for (int i = 0; i < content.Length; i++) {
                content[i] = new string(indentChar, indentation) + content[i];
            }
        }

        protected void Indent(List<string> content, int indentation, char indentChar) {
            for (int i=0;i<content.Count;i++) {
                content[i] = new string(indentChar, indentation) + content[i];
            }
        }

        protected static XmlNode ReadXmlNode(string line) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(line);
            return doc.DocumentElement;
        }

        protected static (int indentation, char indentationChar) GetIndentionData(string input) {
            if (input.Length == 0) {
                return (0, '\t');
            }

            char indentationChar = input.First();
            int indentation = 0;
            if (indentationChar == '\t' || indentationChar == ' ') {
                indentation = CountRepeatChars(input);
            }

            return (indentation, indentationChar);
        }

        private static int CountRepeatChars(string str) {
            if (str.Length == 0) {
                return 0;
            }
            char firstChar = str.First();
            int count = 1;
            for (; count < str.Length; count++) {
                if (str[count] != firstChar) {
                    break;
                }
            }
            return count;
        }
    }
}
