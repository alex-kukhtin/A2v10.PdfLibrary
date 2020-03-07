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
	public abstract class DigestBase : IDigest
	{

		private Int64 _byteCount;
		private Byte[] _xBuf;
		private Int64 _xBufOff;


		internal DigestBase()
		{
			_xBuf = new Byte[4];
		}

		public void BlockUpdate(Byte[] input, Int32 inOff, Int32 length)
		{
			// fill the current word
			while ((_xBufOff != 0) && (length > 0))
			{
				Update(input[inOff]);
				inOff++;
				length--;
			}

			// process whole words.
			while (length > _xBuf.Length)
			{
				ProcessWord(input, inOff);

				inOff += _xBuf.Length;
				length -= _xBuf.Length;
				_byteCount += _xBuf.Length;
			}

			// Load in the remainder.
			while (length > 0)
			{
				Update(input[inOff]);

				inOff++;
				length--;
			}
		}

		public void Finish()
		{
			Int64 bitLength = (_byteCount << 3);

			// add the pad bytes.
			Update(128);

			while (_xBufOff != 0)
				Update(0);
			ProcessLength(bitLength);
			ProcessBlock();
		}


		public virtual void Reset()
		{
			_byteCount = 0;
			_xBufOff = 0;
			Array.Clear(_xBuf, 0, _xBuf.Length);
		}

		public void Update(Byte input)
		{
			_xBuf[_xBufOff++] = input;

			if (_xBufOff == _xBuf.Length)
			{
				ProcessWord(_xBuf, 0);
				_xBufOff = 0;
			}

			_byteCount++;
		}

		internal abstract void ProcessWord(Byte[] input, Int32 inOff);
		internal abstract void ProcessLength(Int64 bitLength);
		internal abstract void ProcessBlock();
		public abstract Int32 GetDigestSize();
		public abstract Int32 DoFinal(Byte[] output, Int32 outOff);
	}
}
