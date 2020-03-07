﻿
using System;

namespace A2v10.Pdf.Crypto
{
	class EncryptArc4
	{
		private readonly Byte[] state = new Byte[256];
		private Int32 x;
		private Int32 y;

		virtual public void PrepareARC4Key(Byte[] key, Int32 offset = 0, Int32 length = 0)
		{
			Int32 len = length == 0 ? key.Length : length;
			Int32 index1 = 0;
			Int32 index2 = 0;
			for (Int32 k = 0; k < 256; ++k)
				state[k] = (Byte) k;
			x = 0;
			y = 0;
			Byte tmp;
			for (Int32 k = 0; k < 256; ++k)
			{
				index2 = (key[index1 + offset] + state[k] + index2) & 255;
				tmp = state[k];
				state[k] = state[index2];
				state[index2] = tmp;
				index1 = (index1 + 1) % len;
			}
		}

		virtual public void EncryptARC4(Byte[] dataIn, Int32 off, Int32 len, Byte[] dataOut, Int32 offOut)
		{
			Int32 length = len + off;
			Byte tmp;
			for (Int32 k = off; k < length; ++k)
			{
				x = (x + 1) & 255;
				y = (state[x] + y) & 255;
				tmp = state[x];
				state[x] = state[y];
				state[y] = tmp;
				dataOut[k - off + offOut] = (Byte)(dataIn[k] ^ state[(state[x] + state[y]) & 255]);
			}
		}

		virtual public void EncryptARC4(Byte[] data, Int32 off, Int32 len)
		{
			EncryptARC4(data, off, len, data, off);
		}

		virtual public void EncryptARC4(Byte[] dataIn, Byte[] dataOut)
		{
			EncryptARC4(dataIn, 0, dataIn.Length, dataOut, 0);
		}

		virtual public void EncryptARC4(Byte[] data)
		{
			EncryptARC4(data, 0, data.Length, data, 0);
		}
	}
}
