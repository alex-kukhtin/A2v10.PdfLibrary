// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf.Crypto
{
	public class StandardDecryption
	{
		private RC4 _rc4;

		public StandardDecryption(Byte[] key, AlgRevision revision)
		{
			Int32 len = key.Length;
			Boolean _aes = (revision == AlgRevision.AES_128 || revision == AlgRevision.AES_256);
			if (_aes)
				throw new NotImplementedException("AES");
			_rc4 = new RC4(key, 0, len);
		}

		public Byte[] Update(Byte[] b, Int32 off, Int32 len)
		{
			Byte[] b2 = new Byte[len];
			_rc4.Encrypt(b, off, len, b2, 0);
			return b2;
		}
	}
}
