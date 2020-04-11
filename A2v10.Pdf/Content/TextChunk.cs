// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public class TextChunk : IComparable<TextChunk>
	{
		readonly String _text;
		readonly Location _location;

		public TextChunk(String text, Location loc)
		{
			_text = text;
			_location = loc;
		}

		public String Text => _text;
		public Location Location => _location;

		#region IComparable
		public Int32 CompareTo(TextChunk other)
		{
			return _location.CompareTo(other._location);
		}
		#endregion

		public bool SameLine(TextChunk ch)
		{
			return _location.SameLine(ch._location);
		}

	}
}
