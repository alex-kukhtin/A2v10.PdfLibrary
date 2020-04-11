// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PdfPage : PdfElement
	{
		private readonly PdfFile _file;
		private readonly PageContent _content;

		public PdfPage(PdfDictionary dict, PdfFile file)
			: base(dict)
		{
			_file = file;
			_content = new PageContent();
		}

		public IEnumerable<PdfContentBlock> Contents()
		{
			var ce = _dict.Get<PdfObject>("Contents");
			switch (ce)
			{
				case PdfArray contArr:
					for (var j = 0; j < contArr.Count; j++)
					{
						var name = contArr.Get<PdfName>(j);
						yield return new PdfContentBlock(_file.GetObject(name), _file, this);
					}
					break;
				case PdfName contName:
					yield return new PdfContentBlock(_file.GetObject(contName), _file, this);
					break;
			}
		}

		public PageContent Content => _content;

		public PdfResource Resources()
		{
			var resName = _dict.Get<PdfName>("Resources");
			var pdfElem = _file.GetObject(resName);
			return new PdfResource(pdfElem, _file);
		}

		public void Layout()
		{
			_content.Layout();
		}

		public void Write(IPdfWriter writer)
		{
			writer.StartPage();
			_content.WriteTo(writer);
			writer.EndPage();
		}
	}
}
