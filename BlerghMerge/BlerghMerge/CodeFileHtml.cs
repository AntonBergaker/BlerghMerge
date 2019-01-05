using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlerghMerge {
    class CodeFileHtml : CodeFile {
        public CodeFileHtml(string source, string sourcePath) : base(source, sourcePath) {
            XmlNode newNode = ReadXmlNode(source);
            path = newNode.InnerText;
            path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath), path));
        }

        public override string WrapContent(List<string> content) {
            StringBuilder sb = new StringBuilder();

            foreach (string s in content) {
                sb.Append(s + "\n");
            }

            return sb.ToString();
        }

        public override List<string> ExportLines() {
            // Overridden because this doesn't indent like others do
            string[] lines = File.ReadAllLines(path);
            Indent(lines, indentation, indentationChar);

            return new List<string>(lines);

        }
    }
}
