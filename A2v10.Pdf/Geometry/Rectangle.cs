// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public struct Rectangle
	{
		public Double X { get; }
		public Double Y { get; }
		public Double W { get; }
		public Double H { get; }

		public Rectangle(Double x, Double y, Double w, Double h)
		{
			X = x;
			Y = y;
			W = w;
			H = h;
		}
	}
}
