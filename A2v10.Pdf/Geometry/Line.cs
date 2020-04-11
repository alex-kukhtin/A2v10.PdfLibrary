// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public struct Line
	{

		public Point Start { get; }
		public Point End { get; }

		public Line(Point start, Point end)
		{
			Start = start;
			End = end;
		}

		public Double Length => (End - Start).Length;

		public Line TransformBy(Matrix m)
		{
			Point s = Start.Cross(m);
			Point e = End.Cross(m);
			return new Line(s, e);
		}

		public override String ToString()
		{
			return $"(({Start}):({End}))";
		}

	}
}
