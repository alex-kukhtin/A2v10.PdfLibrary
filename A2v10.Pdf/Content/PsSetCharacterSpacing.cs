﻿// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsSetCharacterSpacing : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			if (args[0] is IPdfNumber pdfNum)
				context.GraphicState.SetCharacterSpacing(pdfNum.NumberValue);
		}
	}
}
