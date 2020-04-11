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
		private Int32 _defaultWidth;
		private IDictionary<Int32, Int32> _widths;
		private Int32 _spaceWidth;


		public PdfFont(PdfDictionary dict, PdfFile file)
			: base(dict)
		{
			_file = file;
			_defaultWidth = 1000;
			_spaceWidth = 0;
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
					ReadWidths(pdfFont);
			}

			var enc = Get<PdfName>("Encoding");
			if (enc != null && enc.Name == "Identity-H")
			{

			}
			var uni = _dict.Get<PdfName>("ToUnicode");
			if (uni != null)
				ParseToUnicode(uni);
			FillMetrics();
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

		void ReadWidths(PdfFont font)
		{
			var widths = font.Get<PdfArray>("W");
			PdfInteger dwObj = font.Get<PdfInteger>("DW");
			if (dwObj != null)
				_defaultWidth = dwObj.Value;
			Int32 x1 = 0;
			Int32 x2 = 0;
			Int32 width = 0;
			Dictionary<Int32, Int32> map = new Dictionary<Int32, Int32>();
			if (widths != null)
			{
				for (Int32 j = 0; j < widths.Count; j++)
				{
					x1 = widths.Get<PdfInteger>(j).Value;
					var e = widths.Get<PdfObject>(++j);
					switch (e)
					{
						case PdfInteger pdfInt:
							x2 = pdfInt.Value;
							width = widths.Get<PdfInteger>(++j).Value;
							for (; x1 <= x2; ++x1)
								map[x1] = width;
							break;
						case PdfArray pdfArr:
							for (Int32 k=0; k<pdfArr.Count; k++)
							{
								width = pdfArr.Get<PdfInteger>(k).Value;
								map[x1++] = width;
							}
							break;
						default:
							throw new InvalidOperationException($"Invalid width value {e.GetType()}");
					}
				}
			}
			_widths = map;
		}


		Int32 AverageWidth()
		{
			return -0;
		}

		public String DecodeString(Byte[] bytes)
		{
			return _mapToUnicode.Decode(bytes);
		}

		void FillMetrics()
		{
			_mapToUnicode.FillMetrics(_widths, _defaultWidth);

			_spaceWidth = _mapToUnicode.GetWidth(' ');
			if (_spaceWidth == 0)
				_spaceWidth = AverageWidth();
		}

		public FontMatrix GetFontMatrix()
		{
			return FontMatrix.Default();
		}

		public int GetCharWidth(Char ch)
		{
			if (ch == ' ')
				return _spaceWidth;
			return _mapToUnicode.GetWidth(ch);
		}
	}
}

