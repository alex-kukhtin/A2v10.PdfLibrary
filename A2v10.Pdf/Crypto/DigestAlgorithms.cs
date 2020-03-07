
using System;

namespace A2v10.Pdf.Crypto
{
	public static class DigestAlgorithms
	{
		public static Byte[] Digest(String algo, Byte[] b)
		{
			return Digest(GetDigest(algo), b, 0, b.Length);
		}

		public static Byte[] Digest(String algo, Byte[] b, Int32 offset, Int32 len)
		{
			return Digest(GetDigest(algo), b, offset, len);
		}


		public static Byte[] Digest(IDigest d, Byte[] b, Int32 offset, Int32 len)
		{
			d.BlockUpdate(b, offset, len);
			Byte[] r = new Byte[d.GetDigestSize()];
			d.DoFinal(r, 0);
			return r;
		}

		public static IDigest GetDigest(String algo)
		{
			switch (algo.ToUpperInvariant())
			{
				case "MD5":
					return new MD5Digest();
			}
			throw new NotImplementedException(nameof(GetDigest));
		}
	}
}
