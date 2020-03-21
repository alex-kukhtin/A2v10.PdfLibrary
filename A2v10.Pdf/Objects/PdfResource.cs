// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public class PdfResource : PdfElement
	{
		private readonly PdfFile _file;

		public PdfResource(PdfElement elem, PdfFile file)
			: base(elem)
		{
			_file = file;
		}

		public PdfExtGState ExtGState()
		{
			var dict = _dict.Get<PdfDictionary>("ExtGState");
			return new PdfExtGState(dict, _file);
		}
	}
}
