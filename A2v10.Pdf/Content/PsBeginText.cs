// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsBeginText : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			context.SetTextMatrix(new Matrix());
			context.BeginText();
		}
	}

	public class PsEndText : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			context.SetTextMatrix(null);
			context.EndText();
		}
	}
}
