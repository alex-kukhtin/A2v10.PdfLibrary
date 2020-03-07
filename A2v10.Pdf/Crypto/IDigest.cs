using System;

namespace A2v10.Pdf.Crypto
{
	public interface IDigest
	{
		void Update(Byte input);
		Int32 DoFinal(Byte[] output, Int32 outOff);
		void Reset();
		void BlockUpdate(Byte[] input, Int32 inOff, Int32 length);
		Int32 GetDigestSize();
	}
}
