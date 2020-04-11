// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public class FontMatrix
	{
		private readonly Double[] _m = new Double[6] {.001, 0, 0, .001, 0, 0};

		public Double V0 => _m[0];

		public static FontMatrix Default()
		{
			return new FontMatrix();
		}
	}
}
