// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
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

		public String DecodeString(Byte[] bytes)
		{
			return _mapToUnicode.Decode(bytes);
		}
	}
}

