// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Pdf
{
	public interface IPsCommand
	{
		void Execute(PsContext context, IList<PdfObject> args);
	}
}
