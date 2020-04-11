// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsGraphicState : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			PdfName resName = args[0] as PdfName;
			var gsState = context.Resources.ExtGState();
			PdfName res = gsState.Get<PdfName>(resName.Name);
			var resObj = context.File.GetObject(res);
		}
	}

	public class PsPushGraphicState : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			context.SaveState();
		}
	}

	public class PsPopGraphicState : IPsCommand
	{
		public void Execute(PsContext context, IList<PdfObject> args)
		{
			context.RestoreState();
		}
	}
}
