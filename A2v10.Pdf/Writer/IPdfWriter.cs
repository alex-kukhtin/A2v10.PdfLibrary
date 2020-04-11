// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public interface IPdfWriter
	{
		void Write(String text, Location loc);

		void Write(Rectangle rect);

		void StartDocument();
		void EndDocument();
		void StartPage();
		void EndPage();

		String GetText();
	}
}
