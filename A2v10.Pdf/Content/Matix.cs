// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public class Matrix
	{

		private readonly Double[,] _m = new Double[3,3];


		public Matrix()
		{
			_m[0, 0] = 1;
			_m[1, 1] = 1;
			_m[2, 2] = 1;

		}

		public Matrix(Double[] arg)
		{
			_m[0, 0] = arg[0];
			_m[0, 1] = arg[1];
			_m[0, 2] = 0;
			_m[1, 0] = arg[2];
			_m[1, 1] = arg[3];
			_m[1, 2] = 0;
			_m[2, 0] = arg[4];
			_m[2, 1] = arg[5];
			_m[2, 2] = 1;
		}

		public Matrix(Double dx, Double dy)
			: this()
		{
			_m[2, 0] = dx;
			_m[2, 1] = dy;
		}

		public Matrix Multiply(Matrix other)
		{
			var r = new Matrix();

			Double[,] a = _m;
			Double[,] b = other._m;
			Double[,] c = r._m;

			c[0, 0] = a[0, 0] * b[0, 0] + a[0, 1] * b[1, 0] + a[0, 2] * b[2, 0];
			c[0, 1] = a[0, 0] * b[0, 1] + a[0, 1] * b[1, 1] + a[0, 2] * b[2, 1];
			c[0, 2] = a[0, 0] * b[0, 2] + a[0, 1] * b[1, 2] + a[0, 2] * b[2, 2];
			c[1, 0] = a[1, 0] * b[0, 0] + a[1, 1] * b[1, 0] + a[1, 2] * b[2, 0];
			c[1, 1] = a[1, 0] * b[0, 1] + a[1, 1] * b[1, 1] + a[1, 2] * b[2, 1];
			c[1, 2] = a[1, 0] * b[0, 2] + a[1, 1] * b[1, 2] + a[1, 2] * b[2, 2];
			c[2, 0] = a[2, 0] * b[0, 0] + a[2, 1] * b[1, 0] + a[2, 2] * b[2, 0];
			c[2, 1] = a[2, 0] * b[0, 1] + a[2, 1] * b[1, 1] + a[2, 2] * b[2, 1];
			c[2, 2] = a[2, 0] * b[0, 2] + a[2, 1] * b[1, 2] + a[2, 2] * b[2, 2];

			return r;
		}

		public static Matrix operator *(Matrix m1, Matrix m2)
		{
			return m1.Multiply(m2);
		}
	}
}
