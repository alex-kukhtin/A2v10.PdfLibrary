// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;

namespace A2v10.Pdf
{
	public class PdfFont : PdfElement
	{
		private readonly PdfFile _file;
		private Boolean _isInit = false;
		private MapToUnicode _mapToUnicode;

		public PdfFont(PdfDictionary dict, PdfFile file)
			: base(dict)
		{
			_file = file;
		}

		public void Init()
		{
			if (_isInit)
				return;
			var baseFont = Get<PdfName>("BaseFont");
			var subType = Get<PdfName>("Subtype");
			switch (subType.Name)
			{
				case "Type0":
					ParseType0Font();
					break;
			}
			_isInit = true;
		}

		void ParseType0Font()
		{
			var dscFont = Get<PdfArray>("DescendantFonts");
			if (dscFont != null)
			{
				var dscObj = _file.GetObject(dscFont.Get<PdfName>(0));
				if (dscObj is PdfFont pdfFont)
					ReadMetrics(pdfFont);
			}

			var enc = Get<PdfName>("Encoding");
			if (enc != null && enc.Name == "Identity-H")
			{

			}
			var uni = _dict.Get<PdfName>("ToUnicode");
			if (uni != null)
				ParseToUnicode(uni);
		}

		void ParseToUnicode(PdfName name)
		{
			var uniDec = _file.GetObject(name);
			if (uniDec.IsStream)
			{
				using (var ms = new MemoryStream(uniDec.Stream.Bytes))
				using (var br = new BinaryReader(ms))
				{
					_mapToUnicode = ToUnicodeParser.Parse(br);
				}
				uniDec.Trace("ToUnicode");
			}
			else
				throw new ArgumentException("ToUnicode is not a stream");
		}

		void ReadMetrics(PdfFont font)
		{
			Int32 dw = 1000;
			var widths = font.Get<PdfArray>("W");
			PdfInteger dwObj = font.Get<PdfInteger>("DW");
			if (dwObj != null)
				dw = dwObj.Value;
			Int32 x1 = 0;
			Int32 x2 = 0;
			Dictionary<Int32, Int32> map = new Dictionary<Int32, Int32>();
			if (widths != null)
			{
				for (Int32 w = 0; w < widths.Count; w++)
				{
					x1 = widths.Get<PdfInteger>(w).Value;
					var e = widths.Get<PdfObject>(++w);
					switch (e)
					{
						case PdfInteger pdfInt:
							x2 = pdfInt.Value;
							Int32 width = widths.Get<PdfInteger>(++w).Value;
							for (; x1 <= x2; ++x1)
								map[x1] = w;
							break;
						case PdfArray pdfArr:
							for (Int32 j=0; j<pdfArr.Count; j++)
							{
								x2 = pdfArr.Get<PdfInteger>(j).Value;
								map[x1++] = x2;
							}
							break;
					}
				}
			}
			FillMetrics(map, dw);
		}


		public String DecodeString(Byte[] bytes)
		{
			return _mapToUnicode.Decode(bytes);
		}

		void FillMetrics(IDictionary<Int32, Int32> widths, Int32 defW)
		{

		}
	}
}

