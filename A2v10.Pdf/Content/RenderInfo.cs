// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public class RenderInfo
	{
		String _string;
		GraphicState _gs;
		Matrix _textToUserSpace;
		FontMatrix _fontMatrix;
		Double _unscaledWidth;

		public RenderInfo(String str, GraphicState state, Matrix tm)
		{
			_string = str;
			_gs = state;
			_textToUserSpace = tm.Multiply(state.CurrentTransform);
			_fontMatrix = state.Font.GetFontMatrix();
		}

		public String Text => _string;

		public Double GetUnscaledWidth()
		{
			if (_unscaledWidth == 0)
				_unscaledWidth = GetStringWidth();
			return _unscaledWidth;
		}

		Double GetStringWidth()
		{
			double w = 0;
			foreach (var ch in _string.ToCharArray())
			{
				w += GetCharWidth(ch);
			}
			return w;
		}

		Double GetRawStringWidth(String str)
		{
			Double t = 0;
			foreach (var c in str.ToCharArray())
			{
				Double w = _gs.Font.GetCharWidth(c) / 1000.0;
				Double ws = c == ' ' ? _gs.WordSpacing : 0;
				t += (w * _gs.FontSize + _gs.CharSpacing + ws) * _gs.HorzScaling;
			}
			return t;
		}

		Double GetCharWidth(Char ch)
		{
			Double chWidth = _gs.Font.GetCharWidth(ch) * _fontMatrix.V0;
			Double wordSp = (ch == ' ') ? _gs.WordSpacing : 0;
			return (chWidth * _gs.FontSize + _gs.CharSpacing + wordSp) * _gs.HorzScaling;
		}

		public Line GetBaseline()
		{
			return GetBaselineWithOffset(0).TransformBy(_textToUserSpace);
		}

		public Double GetSpaceWidth()
		{
			return TextSpace2UserSpace(GetRawStringWidth(" "), 0);
		}

		private Line GetBaselineWithOffset(Double yOffset)
		{
			var x = _gs.CharSpacing + (_string.EndsWith(" ") ? _gs.WordSpacing : 0);

			Double nw = GetUnscaledWidth() - x * _gs.HorzScaling;

			return new Line(new Point(0, yOffset), new Point(nw, yOffset));
		}


		Double TextSpace2UserSpace(Double x, Double y)
		{
			Line text = new Line(new Point(0, 0), new Point(x, y));
			Line user = text.TransformBy(_textToUserSpace);
			return user.Length;
		}
	}
}
