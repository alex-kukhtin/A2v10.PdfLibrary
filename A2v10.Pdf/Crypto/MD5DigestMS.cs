using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Pdf.Crypto
{
	public class MD5DigestMS
	{
		System.Security.Cryptography.MD5 _md5;
		Byte[] _block;

		public MD5DigestMS()
		{
			_md5 = System.Security.Cryptography.MD5.Create();
			_block = new Byte[32];
		}

		public void BlockUpdate(Byte[] input, Int32 inOff, Int32 length)
		{
			_md5.TransformBlock(input, 0, length, _block, 0);
		}

		public Int32 DoFinal(Byte[] output, Int32 outOff)
		{
			Byte[] hash = _md5.TransformFinalBlock(_block, 0, _block.Length);
			Array.Copy(hash, 0, output, outOff, output.Length);
			return 0;
		}

		public Int32 GetDigestSize()
		{
			return 16;
		}

		public void Reset()
		{
			_md5 = System.Security.Cryptography.MD5.Create();
			_block = new Byte[32];
		}

		public void Update(Byte input)
		{
			throw new NotImplementedException();
		}
	}
}
