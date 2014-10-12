using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PdfWriter
{
    public class Document
    {
        List<string> content; // TODO: Implement as List<PdfObject>
        Xref xref;
        string location;
        StorageFile dataFile;
        int objectCounter = 6;

        public Document(string filename)
        {
            location = filename;
            content = new List<string>();
            xref = new Xref();

            // Add header and catalog object
            content.Add("%PDF-1.4");
            content.Add("1 0 obj <</Type /Catalog /Pages 2 0 R>>");
            content.Add("endobj");
            
            // Add Pages parent object
            content.Add("2 0 obj <</Type /Pages /Kids [3 0 R] /Count 1>>"); // This is line 4
            content.Add("endobj");

            // Add Page object
            content.Add("3 0 obj <</Type /Page /Parent 2 0 R /Resources 4 0 R /MediaBox [0 0 500 800] /Contents 6 0 R>>");
            content.Add("endobj");

            // Add Font Resource object
            content.Add("4 0 obj <</Font <</F1 5 0 R>>>>");
            content.Add("endobj");

            // Add Font object
            content.Add("5 0 obj <</Type /Font /Subtype /Type1 /BaseFont /Helvetica>>");
            content.Add("endobj");

        }

        public async void Create()
        {
            var file = await Windows
                   .Storage.ApplicationData.Current
                   .LocalFolder.CreateFileAsync(location, CreationCollisionOption.ReplaceExisting);
        }

        public void Add(Paragraph p)
        {
            string temp = objectCounter.ToString() + " 0 obj";
            content.Add(temp);

            int tempLength = 0;
            List<string> paragraph = p.GetContent();
            foreach(string s in paragraph)
            {
                content.Add(s);
                tempLength += s.Length;
            }
            xref.Add(tempLength);
        }

        public async void Close()
        {
            int xrefOffset = 0;
            foreach(string s in content)
            {
                xrefOffset += Encoding.UTF8.GetByteCount(s);
            }
            
            List<string> xrefTable = xref.Close(xrefOffset);
            foreach(string s in xrefTable)
            {
                content.Add(s);
            }

            var file = await Windows
                   .Storage.ApplicationData.Current
                   .LocalFolder.CreateFileAsync(location, CreationCollisionOption.ReplaceExisting);

            
            var stream = await file.OpenAsync(FileAccessMode.ReadWrite);

            using (var outputStream = stream.GetOutputStreamAt(0))
            using (StreamWriter writer = new StreamWriter(outputStream.AsStreamForWrite()))
            {
                foreach (string s in content)
                {
                    writer.WriteLine(s);
                }
                writer.Dispose();
            }

            
        }
    }

    public class Xref
    {
        int length = 0;
        int count = 6;
        List<string> table;

        public Xref()
        {
            table = new List<string>();
            table.Add("xref");
            table.Add("0 " + count); // xref object count: This is line 2
            table.Add("0000000000 65535 f");
            table.Add("0000000009 00000 n");
            table.Add("0000000056 00000 n");
            table.Add("0000000111 00000 n");
            table.Add("0000000212 00000 n");
            table.Add("0000000250 00000 n");
            length = 317;
        }

        public void Add(int objectLength)
        {
            // Change object count
            count++;
            string temp = "0 " + count.ToString();
            table[1] = temp;


            table.Add(length.ToString("D10") + " 00000 n");
            length += objectLength;
        }

        public List<string> Close(int offset)
        {
            table.Add("trailer <</Size " + count + "/Root 1 0 R>>");
            table.Add("startxref");
            table.Add(offset.ToString());
            table.Add("%%EOF");

            return table;
        }
    }
}
