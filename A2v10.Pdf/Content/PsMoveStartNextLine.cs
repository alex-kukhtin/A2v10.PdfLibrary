// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsMoveStartNextLine : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			Double tx = (args[0] as IPdfNumber).NumberValue;
			Double ty =  (args[1] as IPdfNumber).NumberValue;

			var translate = new Matrix(tx, ty);
			var newMx = translate * context.TextLineMatrix;
			context.SetTextMatrix(newMx);
		}
	}
}
