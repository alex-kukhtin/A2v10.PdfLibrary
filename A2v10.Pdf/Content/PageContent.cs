
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.Pdf
{
	public class PageContent
	{
		private readonly List<TextChunk> _chunks = new List<TextChunk>();
		private readonly List<Rectangle> _pathes = new List<Rectangle>();

		public void AddChunk(TextChunk ch)
		{
			_chunks.Add(ch);
		}

		public void AddPath(Rectangle rect)
		{
			_pathes.Add(rect);
		}

		public void Layout()
		{
			_chunks.Sort();
		}

		public void WriteTo(IPdfWriter writer)
		{
			TextChunk last = null;
			var sb = new StringBuilder();
			Location loc = null;
			foreach (var tc in _chunks)
			{
				if (last != null)
				{
					if (tc.Location.ContinueString(last.Location)) {
						sb.Append(tc.Text);
					}
					else
					{
						writer.Write(sb.ToString(), loc);
						sb.Clear();
						sb.Append(tc.Text);
						loc = tc.Location;

					}
				}
				else
				{
					sb.Append(tc.Text);
					loc = tc.Location;
				}
				last = tc;
			}
			foreach (var p in _pathes)
			{
				writer.Write(p);
			}
		}
	}
}
