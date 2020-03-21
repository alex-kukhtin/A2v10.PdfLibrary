// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PsContext
	{
		private readonly PdfFile _file;
		private readonly PdfResource _resources;
		private readonly Stack<GraphicState> _graphicStack = new Stack<GraphicState>();

		public GraphicState GraphicState => _graphicStack.Peek();
		public PdfFile File => _file;
		public PdfResource Resources => _resources;

		public PsContext(PdfFile file, PdfResource resource)
		{
			_file = file;
			_resources = resource;
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
	}
}
