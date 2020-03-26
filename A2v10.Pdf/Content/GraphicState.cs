// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public class GraphicState
	{

		PdfFont _font;
		Double _fontSize;

		public PdfFont Font => _font;

		public GraphicState()
		{
			_font = null;
			_fontSize = 0;
		}

		public GraphicState(GraphicState state)
		{
			_font = state._font;
			_fontSize = state._fontSize;
		}

		public void SetFont(PdfFont font, Double fontSize)
		{
			_font = font;
			_fontSize = fontSize;
		}
	}
}
