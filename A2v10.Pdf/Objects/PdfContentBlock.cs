// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System.IO;

namespace A2v10.Pdf
{
	public class PdfContentBlock : PdfElement
	{
		private readonly PdfFile _file;
		private readonly PdfPage _page;

		public PdfContentBlock(PdfElement elem, PdfFile file, PdfPage page)
			: base(elem)
		{
			_file = file;
			_page = page;
		}

		public void WriteTo(Stream stream)
		{
			var stm = _dict.Get<PdfStream>("_stream");
			stream.Write(stm.Bytes, 0, stm.Bytes.Length);
		}
	}
}
