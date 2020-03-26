// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsSetCharacterSpacing : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			Double val = (args[0] as IPdfNumber).NumberValue;
			context.GraphicState.SetCharacterSpacing(val);
		}
	}
}
