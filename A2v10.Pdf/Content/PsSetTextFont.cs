// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsSetTextFont : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			var fontName = args[0] as PdfName;

			var fontSizeArg = args[1];

			Double fontSize = 1;
			if (fontSizeArg is IPdfNumber fsNumber)
				fontSize = fsNumber.NumberValue;

			var fontsArg = context.Resources.Get<PdfObject>("Font");
			if (fontsArg is PdfDictionary fontArgDict)
			{
				var fontRes = fontArgDict.Get<PdfName>(fontName.Name);
				var font = context.File.GetObject(fontRes) as PdfFont;
				font.Init();
				context.SetFont(font, fontSize);
			}
			else
			{
				throw new ArgumentOutOfRangeException("PsSetTextFont. fontArgs");
			}
		}
	}
}
