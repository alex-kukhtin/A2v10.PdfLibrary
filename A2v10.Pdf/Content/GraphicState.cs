// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public class GraphicState
	{

		PdfFont _font;
		Double _fontSize;
		Double _charSpacing;
		Double _lineWidth;
		Double _horzScaling;
		Double _wordSpacing;
		Matrix _currentTransform;

		public PdfFont Font => _font;
		public Double FontSize => _fontSize;
		public Double HorzScaling => _horzScaling;
		public Double CharSpacing => _charSpacing;
		public Double WordSpacing => _wordSpacing;
		public Matrix CurrentTransform => _currentTransform;

		public GraphicState()
		{
			_font = null;
			_fontSize = 0;
			_charSpacing = 0;
			_lineWidth = 1;
			_horzScaling = 1;
			_wordSpacing = 0;
			_currentTransform = new Matrix();
		}

		public GraphicState(GraphicState state)
		{
			_font = state._font;
			_fontSize = state._fontSize;
			_charSpacing = state._charSpacing;
			_lineWidth = state._lineWidth;
			_horzScaling = state._horzScaling;
			_wordSpacing = state._wordSpacing;
			_currentTransform = state._currentTransform;
		}

		public void SetFont(PdfFont font, Double fontSize)
		{
			_font = font;
			_fontSize = fontSize;
		}

		public void SetCharacterSpacing(Double val)
		{
			_charSpacing = val;
		}

		public void SetWordSpacing(Double val)
		{
			_wordSpacing = val;
		}
	}
}
