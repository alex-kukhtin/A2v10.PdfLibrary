// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsShowTextArray : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			if (args.Count < 1)
				throw new ArgumentOutOfRangeException(nameof(args));

			if (!(args[0] is PdfArray arr))
				throw new ArgumentException("PsShowTextArray. Argument must be an array");

			foreach (var el in arr)
			{
				switch (el)
				{
					case PdfHexString hexString:
						var bytes = hexString.Value;
						break;
					case PdfReal real:
						break;
					case PdfInteger integer:
						break;
					default:
						throw new ArgumentException($"PsShowTextArray. Invalid element type {el.GetType()}");
				}
			}
		}
	}
}
