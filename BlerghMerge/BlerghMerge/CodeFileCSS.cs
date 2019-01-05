using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlerghMerge {
    class CodeFileCSS : CodeFile {

        public CodeFileCSS(string source, string sourcePath) : base(source, sourcePath) {
            XmlNode newNode = ReadXmlNode(source);
            path = newNode.Attributes["href"].Value;
            path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath), path));
        }

        public override string WrapContent(List<string> content) {
            StringBuilder sb = new StringBuilder();
            sb.Append(new string(indentationChar, indentation) + "<style>\n");
            foreach(string s in content) {
                sb.Append(s + "\n");
            }
            sb.Append(new string(indentationChar, indentation) + "</style>");

            return sb.ToString();
        }
    }
}
