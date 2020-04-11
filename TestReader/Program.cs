
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using iText.Kernel.Pdf;
using A2v10.Pdf;

namespace TestReader
{
	class Program
	{
		static void Main(String[] args)
		{
			PdfFile file;
			using (var br = new BinaryReader(File.Open("D:\\_PDF_LIBRARY\\F0103306.pdf", FileMode.Open)))
			{
				using (var rdr = PdfReader.Create(br))
				{
					file = rdr.ReadFile();
				}
			}

			var wr = new HtmlWriter();
			wr.StartDocument();

			for (Int32 i=0;  i< file.PageCount; i++)
			{
				var page = file.GetPage(i);

				using (var ms = new MemoryStream())
				{
					foreach (var contBlock in page.Contents())
					{
						contBlock.WriteTo(ms);
						contBlock.Trace();
					}
					ms.Seek(0, SeekOrigin.Begin);
					using (var rdr = ContentReader.Create(ms, file, page))
					{
						rdr.ParseContent();
					}
				}
				page.Layout();
				page.Write(wr);
				wr.EndDocument();

				System.IO.File.WriteAllText("d:\\temp\\converted.html", wr.GetText(), Encoding.UTF8);
			}

			/*
			var rdr = new PdfReader("D:\\_PDF_LIBRARY\\F0103306.pdf");
			var doc = new PdfDocument(rdr);
			var page1 = doc.GetPage(1);
			var page2 = doc.GetPage(2);
			*/

			int z = 55;
		}
	}
}
