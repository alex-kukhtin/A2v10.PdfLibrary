
using System;

namespace A2v10.Pdf.Crypto
{
	public sealed class RC4
	{
		private readonly Byte[] _state = new Byte[256];
		private Int32 x  = 0;
		private Int32 y = 0;

		public static void Encrypt(Byte[] key, Byte[] data, Int32 offset = 0, Int32 keyLength = 0, Int32 dataLength = 0)
		{
			if (dataLength == 0)
				dataLength = data.Length;
			var rc4 = new RC4(key, 0, keyLength);
			rc4.Encrypt(data, 0, dataLength);
		}

		public RC4(Byte[] key, Int32 offset = 0, Int32 len = 0)
		{
			if (len == 0)
				len = key.Length;
			Int32 index1 = 0;
			Int32 index2 = 0;
			for (Int32 k = 0; k < 256; ++k)
				_state[k] = (Byte) k;
			x = 0;
			y = 0;
			Byte tmp;
			for (Int32 k = 0; k < 256; ++k)
			{
				index2 = (key[index1 + offset] + _state[k] + index2) & 0xff;
				tmp = _state[k];
				_state[k] = _state[index2];
				_state[index2] = tmp;
				index1 = (index1 + 1) % len;
			}
		}

		public void Encrypt(Byte[] dataIn, Int32 off, Int32 len, Byte[] dataOut, Int32 offOut)
		{
			Int32 length = len + off;
			Byte tmp;
			for (Int32 k = off; k < length; ++k)
			{
				x = (x + 1) & 0xff;
				y = (_state[x] + y) & 0xff;
				tmp = _state[x];
				_state[x] = _state[y];
				_state[y] = tmp;
				dataOut[k - off + offOut] = (Byte)(dataIn[k] ^ _state[(_state[x] + _state[y]) & 0xff]);
			}
		}

		public void Encrypt(Byte[] data, Int32 off, Int32 len)
		{
			Encrypt(data, off, len, data, off);
		}

		public void Encrypt(Byte[] dataIn, Byte[] dataOut)
		{
			Encrypt(dataIn, 0, dataIn.Length, dataOut, 0);
		}

		public void Encrypt(Byte[] data)
		{
			Encrypt(data, 0, data.Length, data, 0);
		}
	}
}
