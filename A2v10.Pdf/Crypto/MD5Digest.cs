// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.


/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */


using System;

namespace A2v10.Pdf.Crypto
{
	public class MD5Digest : DigestBase
	{
		private UInt32 _H1, _H2, _H3, _H4; // IV's
		private Int32 _xOff;
		private UInt32[] _X = new UInt32[16];

		private const Int32 DigestLength = 16;

		//
		// round 1 left rotates
		//
		private static readonly Int32 S11 = 7;
		private static readonly Int32 S12 = 12;
		private static readonly Int32 S13 = 17;
		private static readonly Int32 S14 = 22;

		// round 2 left rotates
		private static readonly Int32 S21 = 5;
		private static readonly Int32 S22 = 9;
		private static readonly Int32 S23 = 14;
		private static readonly Int32 S24 = 20;

		// round 3 left rotates
		private static readonly Int32 S31 = 4;
		private static readonly Int32 S32 = 11;
		private static readonly Int32 S33 = 16;
		private static readonly Int32 S34 = 23;

		// round 4 left rotates
		private static readonly Int32 S41 = 6;
		private static readonly Int32 S42 = 10;
		private static readonly Int32 S43 = 15;
		private static readonly Int32 S44 = 21;

		public MD5Digest()
		{
			Reset();
		}

		public override Int32 GetDigestSize()
		{
			return DigestLength;
		}

		internal override void ProcessLength(Int64 bitLength)
		{
			if (_xOff > 14)
			{
				if (_xOff == 15)
					_X[15] = 0;

				ProcessBlock();
			}

			for (Int32 i = _xOff; i < 14; ++i)
			{
				_X[i] = 0;
			}

			_X[14] = (UInt32)((UInt64) bitLength);
			_X[15] = (UInt32)((UInt64) bitLength >> 32);
		}

		public override Int32 DoFinal(Byte[] output, Int32 outOff)
		{
			Finish();

			Pack.UInt32_To_LE(_H1, output, outOff);
			Pack.UInt32_To_LE(_H2, output, outOff + 4);
			Pack.UInt32_To_LE(_H3, output, outOff + 8);
			Pack.UInt32_To_LE(_H4, output, outOff + 12);

			Reset();

			return DigestLength;
		}

		public override void Reset()
		{
			base.Reset();

			_H1 = 0x67452301;
			_H2 = 0xefcdab89;
			_H3 = 0x98badcfe;
			_H4 = 0x10325476;

			_xOff = 0;

			for (Int32 i = 0; i != _X.Length; i++)
				_X[i] = 0;
		}

		internal override void ProcessWord(Byte[] input, Int32 inOff)
		{
			_X[_xOff] = Pack.LE_To_UInt32(input, inOff);

			if (++_xOff == 16)
			{
				ProcessBlock();
			}
		}

		private static UInt32 RotateLeft(UInt32 x, Int32 n)
		{
			return (x << n) | (x >> (32 - n));
		}

		private static UInt32 F(UInt32 u, UInt32 v, UInt32 w)
		{
			return (u & v) | (~u & w);
		}

		private static UInt32 G(UInt32 u, UInt32 v, UInt32 w)
		{
			return (u & w) | (v & ~w);
		}

		private static UInt32 H(UInt32 u, UInt32 v, UInt32 w)
		{
			return u ^ v ^ w;
		}

		private static UInt32 K(UInt32 u, UInt32 v, UInt32 w)
		{
			return v ^ (u | ~w);
		}


		internal override void ProcessBlock()
		{
			UInt32 a = _H1;
			UInt32 b = _H2;
			UInt32 c = _H3;
			UInt32 d = _H4;

			//
			// Round 1 - F cycle, 16 times.
			//
			a = RotateLeft((a + F(b, c, d) + _X[0] + 0xd76aa478), S11) + b;
			d = RotateLeft((d + F(a, b, c) + _X[1] + 0xe8c7b756), S12) + a;
			c = RotateLeft((c + F(d, a, b) + _X[2] + 0x242070db), S13) + d;
			b = RotateLeft((b + F(c, d, a) + _X[3] + 0xc1bdceee), S14) + c;
			a = RotateLeft((a + F(b, c, d) + _X[4] + 0xf57c0faf), S11) + b;
			d = RotateLeft((d + F(a, b, c) + _X[5] + 0x4787c62a), S12) + a;
			c = RotateLeft((c + F(d, a, b) + _X[6] + 0xa8304613), S13) + d;
			b = RotateLeft((b + F(c, d, a) + _X[7] + 0xfd469501), S14) + c;
			a = RotateLeft((a + F(b, c, d) + _X[8] + 0x698098d8), S11) + b;
			d = RotateLeft((d + F(a, b, c) + _X[9] + 0x8b44f7af), S12) + a;
			c = RotateLeft((c + F(d, a, b) + _X[10] + 0xffff5bb1), S13) + d;
			b = RotateLeft((b + F(c, d, a) + _X[11] + 0x895cd7be), S14) + c;
			a = RotateLeft((a + F(b, c, d) + _X[12] + 0x6b901122), S11) + b;
			d = RotateLeft((d + F(a, b, c) + _X[13] + 0xfd987193), S12) + a;
			c = RotateLeft((c + F(d, a, b) + _X[14] + 0xa679438e), S13) + d;
			b = RotateLeft((b + F(c, d, a) + _X[15] + 0x49b40821), S14) + c;

			//
			// Round 2 - G cycle, 16 times.
			//
			a = RotateLeft((a + G(b, c, d) + _X[1] + 0xf61e2562), S21) + b;
			d = RotateLeft((d + G(a, b, c) + _X[6] + 0xc040b340), S22) + a;
			c = RotateLeft((c + G(d, a, b) + _X[11] + 0x265e5a51), S23) + d;
			b = RotateLeft((b + G(c, d, a) + _X[0] + 0xe9b6c7aa), S24) + c;
			a = RotateLeft((a + G(b, c, d) + _X[5] + 0xd62f105d), S21) + b;
			d = RotateLeft((d + G(a, b, c) + _X[10] + 0x02441453), S22) + a;
			c = RotateLeft((c + G(d, a, b) + _X[15] + 0xd8a1e681), S23) + d;
			b = RotateLeft((b + G(c, d, a) + _X[4] + 0xe7d3fbc8), S24) + c;
			a = RotateLeft((a + G(b, c, d) + _X[9] + 0x21e1cde6), S21) + b;
			d = RotateLeft((d + G(a, b, c) + _X[14] + 0xc33707d6), S22) + a;
			c = RotateLeft((c + G(d, a, b) + _X[3] + 0xf4d50d87), S23) + d;
			b = RotateLeft((b + G(c, d, a) + _X[8] + 0x455a14ed), S24) + c;
			a = RotateLeft((a + G(b, c, d) + _X[13] + 0xa9e3e905), S21) + b;
			d = RotateLeft((d + G(a, b, c) + _X[2] + 0xfcefa3f8), S22) + a;
			c = RotateLeft((c + G(d, a, b) + _X[7] + 0x676f02d9), S23) + d;
			b = RotateLeft((b + G(c, d, a) + _X[12] + 0x8d2a4c8a), S24) + c;

			//
			// Round 3 - H cycle, 16 times.
			//
			a = RotateLeft((a + H(b, c, d) + _X[5] + 0xfffa3942), S31) + b;
			d = RotateLeft((d + H(a, b, c) + _X[8] + 0x8771f681), S32) + a;
			c = RotateLeft((c + H(d, a, b) + _X[11] + 0x6d9d6122), S33) + d;
			b = RotateLeft((b + H(c, d, a) + _X[14] + 0xfde5380c), S34) + c;
			a = RotateLeft((a + H(b, c, d) + _X[1] + 0xa4beea44), S31) + b;
			d = RotateLeft((d + H(a, b, c) + _X[4] + 0x4bdecfa9), S32) + a;
			c = RotateLeft((c + H(d, a, b) + _X[7] + 0xf6bb4b60), S33) + d;
			b = RotateLeft((b + H(c, d, a) + _X[10] + 0xbebfbc70), S34) + c;
			a = RotateLeft((a + H(b, c, d) + _X[13] + 0x289b7ec6), S31) + b;
			d = RotateLeft((d + H(a, b, c) + _X[0] + 0xeaa127fa), S32) + a;
			c = RotateLeft((c + H(d, a, b) + _X[3] + 0xd4ef3085), S33) + d;
			b = RotateLeft((b + H(c, d, a) + _X[6] + 0x04881d05), S34) + c;
			a = RotateLeft((a + H(b, c, d) + _X[9] + 0xd9d4d039), S31) + b;
			d = RotateLeft((d + H(a, b, c) + _X[12] + 0xe6db99e5), S32) + a;
			c = RotateLeft((c + H(d, a, b) + _X[15] + 0x1fa27cf8), S33) + d;
			b = RotateLeft((b + H(c, d, a) + _X[2] + 0xc4ac5665), S34) + c;

			//
			// Round 4 - K cycle, 16 times.
			//
			a = RotateLeft((a + K(b, c, d) + _X[0] + 0xf4292244), S41) + b;
			d = RotateLeft((d + K(a, b, c) + _X[7] + 0x432aff97), S42) + a;
			c = RotateLeft((c + K(d, a, b) + _X[14] + 0xab9423a7), S43) + d;
			b = RotateLeft((b + K(c, d, a) + _X[5] + 0xfc93a039), S44) + c;
			a = RotateLeft((a + K(b, c, d) + _X[12] + 0x655b59c3), S41) + b;
			d = RotateLeft((d + K(a, b, c) + _X[3] + 0x8f0ccc92), S42) + a;
			c = RotateLeft((c + K(d, a, b) + _X[10] + 0xffeff47d), S43) + d;
			b = RotateLeft((b + K(c, d, a) + _X[1] + 0x85845dd1), S44) + c;
			a = RotateLeft((a + K(b, c, d) + _X[8] + 0x6fa87e4f), S41) + b;
			d = RotateLeft((d + K(a, b, c) + _X[15] + 0xfe2ce6e0), S42) + a;
			c = RotateLeft((c + K(d, a, b) + _X[6] + 0xa3014314), S43) + d;
			b = RotateLeft((b + K(c, d, a) + _X[13] + 0x4e0811a1), S44) + c;
			a = RotateLeft((a + K(b, c, d) + _X[4] + 0xf7537e82), S41) + b;
			d = RotateLeft((d + K(a, b, c) + _X[11] + 0xbd3af235), S42) + a;
			c = RotateLeft((c + K(d, a, b) + _X[2] + 0x2ad7d2bb), S43) + d;
			b = RotateLeft((b + K(c, d, a) + _X[9] + 0xeb86d391), S44) + c;

			_H1 += a;
			_H2 += b;
			_H3 += c;
			_H4 += d;

			_xOff = 0;
		}
	}
}
