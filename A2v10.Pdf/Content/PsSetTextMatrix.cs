// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsSetTextMatrix : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			Double[] mx = new Double[6];
			for (int i=0; i<6; i++)
			{
				if (args[i] is IPdfNumber pdfNumber)
					mx[i] = pdfNumber.NumberValue;
			}
			context.SetTextMatrix(new Matrix(mx));
		}
	}
}
