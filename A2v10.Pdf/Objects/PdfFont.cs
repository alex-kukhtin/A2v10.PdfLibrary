// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public class PdfFont : PdfElement
	{
		private readonly PdfFile _file;
		private Boolean _isInit = false;

		public PdfFont(PdfDictionary dict, PdfFile file)
			: base(dict)
		{
			_file = file;
		}

		public void Init()
		{
			if (_isInit)
				return;
			ParseToUnicode();
			_isInit = true;
		}

		void ParseToUnicode()
		{ 
			var uni = _dict.Get<PdfName>("ToUnicode");
			if (uni != null)
			{
				var uniDec = _file.GetObject(uni);
				if (uniDec.IsStream)
				{
					uniDec.Trace("ToUnicode");
					// ToUnicode 
				}
				else
					throw new ArgumentException("ToUnicode is not a stream");
			}
		}
	}
}
