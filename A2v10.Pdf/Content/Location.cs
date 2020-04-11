// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public class Location : IComparable<Location>
	{
		Point _start;
		Point _end;
		Double _space;
		Point _orientation;
		Double _cxStart;
		Double _cxEnd;
		Int32 _cy;

		public Location(Line line, Double space)
			:this(line.Start, line.End, space)
		{
		}

		public Location(Point start, Point end, Double space)
		{
			_start = start;
			_end = end;
			_space = space;

			var o = _end - _start;
			if (o.Length == 0)
				o = new Point(1, 0);
			_orientation = o.Normalize();

			_cxStart = _orientation * _start;
			_cxEnd = _orientation * _end;

			_cy = (Int32) (_orientation - _start).Y;
		}

		public Point Start => _start;
		public Point End => _end;

		#region IComparable
		public Int32 CompareTo(Location other)
		{
			if (this == other)
				return 0;

			if (_cy != other._cy)
				return _cy < other._cy ? -1 : 1;

			if (_cxStart != other._cxStart)
				return _cxStart < other._cxStart ? -1 : 1;

			return 0;
		}
		#endregion

		public Boolean SameLine(Location other)
		{
			return _cy == other._cy;
		}

		public Boolean ContinueString(Location other)
		{
			if (!SameLine(other))
				return false;
			var dx = _cxStart - other._cxEnd;
			return Math.Abs(dx) < _space;
		}
	}
}
