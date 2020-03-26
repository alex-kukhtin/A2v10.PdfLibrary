// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsContext
	{
		private readonly PdfFile _file;
		private readonly PdfResource _resources;
		private readonly Stack<GraphicState> _graphicStack = new Stack<GraphicState>();

		private Matrix _textLineMx;
		private Matrix _textMx;

		public GraphicState GraphicState => _graphicStack.Peek();
		public PdfFile File => _file;
		public PdfResource Resources => _resources;
		public Matrix TextLineMatrix => _textLineMx;


		public PsContext(PdfFile file, PdfResource resource)
		{
			_file = file;
			_resources = resource;
			Reset();
		}

		public void SaveState()
		{
			var newState = new GraphicState(GraphicState);
			_graphicStack.Push(newState);
		}

		public void RestoreState()
		{
			_graphicStack.Pop();
		}

		public void Reset()
		{
			_graphicStack.Push(new GraphicState());
		}

		public void SetFont(PdfFont font, Double fontSize)
		{
			GraphicState.SetFont(font, fontSize);
		}

		public String Decode(Byte[] bytes)
		{
			return GraphicState.Font.DecodeString(bytes);
		}

		public void SetTextMatrix(Matrix mx)
		{
			_textLineMx = mx;
			_textMx = mx;
		}

		public void ApplyTextAdjust(Double adj)
		{
			var fs = GraphicState.FontSize;
			var hs = GraphicState.HorzScaling;
			Double adjustBy = -adj / 1000.0 * fs * hs;

			_textMx = new Matrix(adjustBy, 0) * _textMx;
		}

		public void DisplayString(String str)
		{
			var ri = new RenderInfo(str, GraphicState, _textMx);
			RenderText(ri);
			_textMx = new Matrix(ri.GetUnscaledWidth(), 0) * _textMx;
		}

		void RenderText(RenderInfo ri)
		{

		}

		public void BeginText()
		{

		}

		public void EndText()
		{

		}
	}
}
