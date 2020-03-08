
// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.


using System;

namespace A2v10.Pdf.Crypto
{
	public sealed class MD5
	{
		private const Int32 _digestSize = 16;

		private Int32 _byteCount;
		private Int32 _xOff;

		private Byte[] _buffer4 = new Byte[4];
		private Int32 _buffer4Offset;

		private readonly UInt32[] _X = new UInt32[16];

		private static readonly Int32[,] _S = new Int32[,] {
			{ 7, 12, 17, 22 },
			{ 5,  9, 14, 20 },
			{ 4, 11, 16, 23 },
			{ 6, 10, 15, 21 }
		};

		private readonly UInt32[] _H = new UInt32[] {
			0x67452301,
			0xefcdab89,
			0x98badcfe,
			0x10325476
		};

		public static Byte[] Digest(Byte[] b)
		{
			return Digest(b, 0, b.Length);
		}

		public static Byte[] Digest(Byte[] b, Int32 offset, Int32 len)
		{
			var md = new MD5();
			md.BlockUpdate(b, offset, len);
			return md.DoFinal();
		}

		public void Update(Byte input)
		{
			_buffer4[_buffer4Offset++] = input;

			if (_buffer4Offset == _buffer4.Length)
			{
				ProcessWord(_buffer4, 0);
				_buffer4Offset = 0;
			}

			_byteCount++;
		}

		public void BlockUpdate(Byte[] input, Int32 inOff, Int32 length)
		{
			// fill the current word
			while ((_buffer4Offset != 0) && (length > 0))
			{
				Update(input[inOff]);
				inOff++;
				length--;
			}

			// process whole words.
			while (length > _buffer4.Length)
			{
				ProcessWord(input, inOff);

				inOff += _buffer4.Length;
				length -= _buffer4.Length;
				_byteCount += _buffer4.Length;
			}

			// Load in the remainder.
			while (length > 0)
			{
				Update(input[inOff]);

				inOff++;
				length--;
			}
		}

		void ProcessLength(Int64 bitLength, Int32 offset)
		{
			if (offset > 14)
			{
				if (offset == 15)
					_X[15] = 0;

				ProcessBlock();
			}

			for (Int32 i = offset; i < 14; ++i)
				_X[i] = 0;

			_X[14] = (UInt32)(UInt64) bitLength;
			_X[15] = (UInt32)((UInt64) bitLength >> 32);
		}

		void UnpackUint32(UInt32 n, Byte[] output, Int32 offset)
		{
			var bytes = BitConverter.GetBytes(n);
			Array.Copy(bytes, 0, output, offset, 4);
		}


		public Byte[] DoFinal()
		{
			var output = new Byte[_digestSize];
			DoFinal(output);
			return output;
		}

		void DoFinal(Byte[] output)
		{
			Finish();

			UnpackUint32(_H[0], output, 0);
			UnpackUint32(_H[1], output, 4);
			UnpackUint32(_H[2], output, 8);
			UnpackUint32(_H[3], output, 12);
		}

		void ProcessWord(Byte[] input, Int32 inOff)
		{
			_X[_xOff] = BitConverter.ToUInt32(input, inOff);

			if (++_xOff == 16)
			{
				ProcessBlock();
			}
		}

		static UInt32 RotateLeft(UInt32 x, Int32 n)
		{
			return (x << n) | (x >> (32 - n));
		}

		static UInt32 F(UInt32 u, UInt32 v, UInt32 w)
		{
			return (u & v) | (~u & w);
		}

		static UInt32 G(UInt32 u, UInt32 v, UInt32 w)
		{
			return (u & w) | (v & ~w);
		}

		static UInt32 H(UInt32 u, UInt32 v, UInt32 w)
		{
			return u ^ v ^ w;
		}

		static UInt32 K(UInt32 u, UInt32 v, UInt32 w)
		{
			return v ^ (u | ~w);
		}


		void ProcessBlock()
		{
			UInt32 a = _H[0];
			UInt32 b = _H[1];
			UInt32 c = _H[2];
			UInt32 d = _H[3];

			// Round 1 - F cycle, 16 times.
			a = RotateLeft((a + F(b, c, d) + _X[0] + 0xd76aa478), _S[0, 0]) + b;
			d = RotateLeft((d + F(a, b, c) + _X[1] + 0xe8c7b756), _S[0, 1]) + a;
			c = RotateLeft((c + F(d, a, b) + _X[2] + 0x242070db), _S[0, 2]) + d;
			b = RotateLeft((b + F(c, d, a) + _X[3] + 0xc1bdceee), _S[0, 3]) + c;
			a = RotateLeft((a + F(b, c, d) + _X[4] + 0xf57c0faf), _S[0, 0]) + b;
			d = RotateLeft((d + F(a, b, c) + _X[5] + 0x4787c62a), _S[0, 1]) + a;
			c = RotateLeft((c + F(d, a, b) + _X[6] + 0xa8304613), _S[0, 2]) + d;
			b = RotateLeft((b + F(c, d, a) + _X[7] + 0xfd469501), _S[0, 3]) + c;
			a = RotateLeft((a + F(b, c, d) + _X[8] + 0x698098d8), _S[0, 0]) + b;
			d = RotateLeft((d + F(a, b, c) + _X[9] + 0x8b44f7af), _S[0, 1]) + a;
			c = RotateLeft((c + F(d, a, b) + _X[10] + 0xffff5bb1), _S[0, 2]) + d;
			b = RotateLeft((b + F(c, d, a) + _X[11] + 0x895cd7be), _S[0, 3]) + c;
			a = RotateLeft((a + F(b, c, d) + _X[12] + 0x6b901122), _S[0, 0]) + b;
			d = RotateLeft((d + F(a, b, c) + _X[13] + 0xfd987193), _S[0, 1]) + a;
			c = RotateLeft((c + F(d, a, b) + _X[14] + 0xa679438e), _S[0, 2]) + d;
			b = RotateLeft((b + F(c, d, a) + _X[15] + 0x49b40821), _S[0, 3]) + c;

			//
			// Round 2 - G cycle, 16 times.
			//
			a = RotateLeft((a + G(b, c, d) + _X[1] + 0xf61e2562), _S[1, 0]) + b;
			d = RotateLeft((d + G(a, b, c) + _X[6] + 0xc040b340), _S[1, 1]) + a;
			c = RotateLeft((c + G(d, a, b) + _X[11] + 0x265e5a51), _S[1, 2]) + d;
			b = RotateLeft((b + G(c, d, a) + _X[0] + 0xe9b6c7aa), _S[1, 3]) + c;
			a = RotateLeft((a + G(b, c, d) + _X[5] + 0xd62f105d), _S[1, 0]) + b;
			d = RotateLeft((d + G(a, b, c) + _X[10] + 0x02441453), _S[1, 1]) + a;
			c = RotateLeft((c + G(d, a, b) + _X[15] + 0xd8a1e681), _S[1, 2]) + d;
			b = RotateLeft((b + G(c, d, a) + _X[4] + 0xe7d3fbc8), _S[1, 3]) + c;
			a = RotateLeft((a + G(b, c, d) + _X[9] + 0x21e1cde6), _S[1, 0]) + b;
			d = RotateLeft((d + G(a, b, c) + _X[14] + 0xc33707d6), _S[1, 1]) + a;
			c = RotateLeft((c + G(d, a, b) + _X[3] + 0xf4d50d87), _S[1, 2]) + d;
			b = RotateLeft((b + G(c, d, a) + _X[8] + 0x455a14ed), _S[1, 3]) + c;
			a = RotateLeft((a + G(b, c, d) + _X[13] + 0xa9e3e905), _S[1, 0]) + b;
			d = RotateLeft((d + G(a, b, c) + _X[2] + 0xfcefa3f8), _S[1, 1]) + a;
			c = RotateLeft((c + G(d, a, b) + _X[7] + 0x676f02d9), _S[1, 2]) + d;
			b = RotateLeft((b + G(c, d, a) + _X[12] + 0x8d2a4c8a), _S[1, 3]) + c;

			// Round 3 - H cycle, 16 times.
			a = RotateLeft((a + H(b, c, d) + _X[5] + 0xfffa3942), _S[2, 0]) + b;
			d = RotateLeft((d + H(a, b, c) + _X[8] + 0x8771f681), _S[2, 1]) + a;
			c = RotateLeft((c + H(d, a, b) + _X[11] + 0x6d9d6122), _S[2, 2]) + d;
			b = RotateLeft((b + H(c, d, a) + _X[14] + 0xfde5380c), _S[2, 3]) + c;
			a = RotateLeft((a + H(b, c, d) + _X[1] + 0xa4beea44), _S[2, 0]) + b;
			d = RotateLeft((d + H(a, b, c) + _X[4] + 0x4bdecfa9), _S[2, 1]) + a;
			c = RotateLeft((c + H(d, a, b) + _X[7] + 0xf6bb4b60), _S[2, 2]) + d;
			b = RotateLeft((b + H(c, d, a) + _X[10] + 0xbebfbc70), _S[2, 3]) + c;
			a = RotateLeft((a + H(b, c, d) + _X[13] + 0x289b7ec6), _S[2, 0]) + b;
			d = RotateLeft((d + H(a, b, c) + _X[0] + 0xeaa127fa), _S[2, 1]) + a;
			c = RotateLeft((c + H(d, a, b) + _X[3] + 0xd4ef3085), _S[2, 2]) + d;
			b = RotateLeft((b + H(c, d, a) + _X[6] + 0x04881d05), _S[2, 3]) + c;
			a = RotateLeft((a + H(b, c, d) + _X[9] + 0xd9d4d039), _S[2, 0]) + b;
			d = RotateLeft((d + H(a, b, c) + _X[12] + 0xe6db99e5), _S[2, 1]) + a;
			c = RotateLeft((c + H(d, a, b) + _X[15] + 0x1fa27cf8), _S[2, 2]) + d;
			b = RotateLeft((b + H(c, d, a) + _X[2] + 0xc4ac5665), _S[2, 3]) + c;

			// Round 4 - K cycle, 16 times.
			a = RotateLeft((a + K(b, c, d) + _X[0] + 0xf4292244), _S[3, 0]) + b;
			d = RotateLeft((d + K(a, b, c) + _X[7] + 0x432aff97), _S[3, 1]) + a;
			c = RotateLeft((c + K(d, a, b) + _X[14] + 0xab9423a7), _S[3, 2]) + d;
			b = RotateLeft((b + K(c, d, a) + _X[5] + 0xfc93a039), _S[3, 3]) + c;
			a = RotateLeft((a + K(b, c, d) + _X[12] + 0x655b59c3), _S[3, 0]) + b;
			d = RotateLeft((d + K(a, b, c) + _X[3] + 0x8f0ccc92), _S[3, 1]) + a;
			c = RotateLeft((c + K(d, a, b) + _X[10] + 0xffeff47d), _S[3, 2]) + d;
			b = RotateLeft((b + K(c, d, a) + _X[1] + 0x85845dd1), _S[3, 3]) + c;
			a = RotateLeft((a + K(b, c, d) + _X[8] + 0x6fa87e4f), _S[3, 0]) + b;
			d = RotateLeft((d + K(a, b, c) + _X[15] + 0xfe2ce6e0), _S[3, 1]) + a;
			c = RotateLeft((c + K(d, a, b) + _X[6] + 0xa3014314), _S[3, 2]) + d;
			b = RotateLeft((b + K(c, d, a) + _X[13] + 0x4e0811a1), _S[3, 3]) + c;
			a = RotateLeft((a + K(b, c, d) + _X[4] + 0xf7537e82), _S[3, 0]) + b;
			d = RotateLeft((d + K(a, b, c) + _X[11] + 0xbd3af235), _S[3, 1]) + a;
			c = RotateLeft((c + K(d, a, b) + _X[2] + 0x2ad7d2bb), _S[3, 2]) + d;
			b = RotateLeft((b + K(c, d, a) + _X[9] + 0xeb86d391), _S[3, 3]) + c;

			_H[0] += a;
			_H[1] += b;
			_H[2] += c;
			_H[3] += d;

			_xOff = 0;
		}

		void Finish()
		{
			Int64 bitLength = (_byteCount << 3);

			// add the pad bytes.
			Update(128);

			while (_buffer4Offset != 0)
				Update(0);

			ProcessLength(bitLength, _xOff);
			ProcessBlock();
		}
	}
}
