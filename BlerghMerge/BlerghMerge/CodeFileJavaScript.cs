using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlerghMerge {
    class CodeFileJavaScript : CodeFile {

        public CodeFileJavaScript(string source, string sourcePath) : base(source, sourcePath) {
            XmlNode newNode = ReadXmlNode(source);
            path = newNode.Attributes["src"].Value;
            path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath), path));
        }

        public override string WrapContent(List<string> content) {
            StringBuilder sb = new StringBuilder();
            sb.Append(new string(indentationChar, indentation) + "<script>\n");
            foreach(string s in content) {
                sb.Append(s);
            }
            sb.Append(new string(indentationChar, indentation) + "</script>");

            return sb.ToString();
        }
    }
}
