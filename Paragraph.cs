using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfWriter
{
    public class Paragraph
    {
        List<string> content;
        int contentLength;

        public Paragraph()
        {
            content = new List<string>();
            content.Add("<</Length " + contentLength + ">>");
            content.Add("stream");

        }

        public void Add(string s)
        {
            List<string> extra;
            if(s.Length > 72)
            {
                extra = splitByLength(s, 72);
                string temp = "BT /F1 12 Tf 50 750 Td 12 TL (" + extra[0] + ") Tj";
                content.Add(temp);
                contentLength += Encoding.UTF8.GetByteCount(temp);
                for(int i = 1; i < extra.Count; i++)
                {
                    temp = "(" + extra[i] + ") '";
                    content.Add(temp);
                    contentLength += Encoding.UTF8.GetByteCount(temp);
                }
                content.Add("ET");
                contentLength += Encoding.UTF8.GetByteCount("ET");
                content[0] = "<</Length " + contentLength + ">>";
            }
            else
            {
                string temp = "BT /F1 12 Tf 50 750 (" + s + ") Tj ET";
                content.Add(temp);
                contentLength += Encoding.UTF8.GetByteCount(temp);
                content[0] = "<</Length " + contentLength + ">>";
            }
        }

        private List<string> splitByLength(string s, int length)
        {
            List<string> result = new List<string>();
            if(s.Length < length)
            {
                result.Add(s);
                return result;
            }
            else
            {
                while(s.Length >= length)
                {
                    result.Add(s.Substring(0, length));
                    s = s.Substring(length);
                }
                result.Add(s);
                return result;
            }
        }

        public void Close()
        {
            content.Add("endstream");
            content.Add("endobj");
        }

        public List<string> GetContent()
        {
            return content;
        }

        public int GetContentLength()
        {
            return contentLength;
        }

    }
}
