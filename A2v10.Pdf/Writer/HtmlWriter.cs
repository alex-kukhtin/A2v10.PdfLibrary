// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Text;

namespace A2v10.Pdf
{
	public class HtmlWriter : IPdfWriter
	{
		StringBuilder _sb;

		public HtmlWriter()
		{
			_sb = new StringBuilder();
		}

		public void Write(string text, Location loc)
		{
			// TODO: line-height
			var span = $"<span style=\"left:{loc.Start.X}pt; top:{841.89 - loc.Start.Y}pt\">{text}</span>";
			_sb.Append(span);
		}

		public void StartPage()
		{
			_sb.AppendLine("<div class=\"page\">");
		}

		public void EndPage()
		{
			_sb.Append("</div>");
		}

		public void StartDocument()
		{
			_sb.AppendLine("<html><head><style>.page {position:relative;height:300mm;font-size:13px;margin:20px;background:azure} .rect {position:absolute;background:yellow;border:1px solid blue;}  .page span {position:absolute;}</style></head><body>");
		}

		public void EndDocument()
		{
			_sb.Append("</body></html>");
		}

		public String GetText()
		{
			return _sb.ToString();
		}

		public void Write(Rectangle rect)
		{
			_sb.Append($"<div class=\"rect\" style=\"left:{rect.X}pt;top:{841.89 - rect.Y}pt; width:{rect.W}pt; height:{rect.H}pt\"></div>");
		}
	}
}
