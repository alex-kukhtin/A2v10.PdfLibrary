// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PdfPage : PdfElement
	{
		private readonly PdfFile _file;
		public PdfPage(PdfDictionary dict, PdfFile file)
			: base(dict)
		{
			_file = file;
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

		public PdfResource Resources()
		{
			var resName = _dict.Get<PdfName>("Resources");
			var pdfElem = _file.GetObject(resName);
			return new PdfResource(pdfElem, _file);
		}
	}
}
