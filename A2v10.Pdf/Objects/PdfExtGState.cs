// Copyright © 2020 Alex Kukhtin. All rights reserved.

namespace A2v10.Pdf
{
	public class PdfExtGState : PdfElement
	{
		private readonly PdfFile _file;

		public PdfExtGState(PdfDictionary dict, PdfFile file)
			:base(dict)
		{
			_file = file;
		}

		public PdfExtGState(PdfElement elem, PdfFile file)
			: base(elem)
		{
			_file = file;
		}
	}
}
