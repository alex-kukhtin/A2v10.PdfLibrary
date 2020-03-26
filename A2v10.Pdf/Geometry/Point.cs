// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public struct Point
	{
		private readonly Double _x;
		private readonly Double _y;

		public Double X => _x;
		public Double Y => _y;

		public Point(Double x, Double y)
		{
			_x = x;
			_y = y;
		}

		public override String ToString()
		{
			return $"({_x},{_y})";
		}

		public Double Length => Math.Sqrt(_x * _x + _y * _y);

		public Point Normalize()
		{
			Double len = Length;
			return new Point(_x / len, _y / len);
		}

		public static Point operator - (Point p1, Point p2)
		{
			return new Point(p1._x - p2._x, p1._y - p2._y);
		}

		public static Point operator * (Point p1, Double v)
		{
			return new Point(p1._x * v, p1._y * v);
		}

	}
}
