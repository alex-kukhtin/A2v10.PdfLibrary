// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsRectagle : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			Double x = (args[0] as IPdfNumber).NumberValue;
			Double y = (args[1] as IPdfNumber).NumberValue;
			Double w = (args[2] as IPdfNumber).NumberValue;
			Double h = (args[3] as IPdfNumber).NumberValue;
			context.DrawRectangle(new Rectangle(x, y, w, h));
		}
	}
}
