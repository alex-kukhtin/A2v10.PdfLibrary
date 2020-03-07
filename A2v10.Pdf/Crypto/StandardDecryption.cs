// Copyright © 2020 Alex Kukhtin. All rights reserved.

/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */

using System;

namespace A2v10.Pdf.Crypto
{
	public class StandardDecryption
	{
		private readonly Boolean _aes;
		private EncryptArc4 _arcfour;
		//private AESCipher cipher;
		private readonly Byte[] _key;
		private const Int32 AES_128 = 4;
		private const Int32 AES_256 = 5;
		//private Boolean _initiated;
		//private Byte[] iv = new Byte[16];
		//private Int32 ivptr;

		/** Creates a new instance of StandardDecryption */
		public StandardDecryption(Byte[] key, Int32 off, Int32 len, AlgRevision revision)
		{
			_aes = (revision == AlgRevision.AES_128 || revision == AlgRevision.AES_256);
			if (_aes)
			{
				_key = new Byte[len];
				Array.Copy(key, off, _key, 0, len);
			}
			else
			{
				_arcfour = new EncryptArc4();
				_arcfour.PrepareARC4Key(key, off, len);
			}
		}

		virtual public Byte[] Update(Byte[] b, Int32 off, Int32 len)
		{
			if (_aes)
			{
				throw new NotImplementedException("AES Decryption");
				/*
				if (_initiated)
				//return cipher.Update(b, off, len);
				else
				{
					Int32 left = Math.Min(iv.Length - ivptr, len);
					Array.Copy(b, off, iv, ivptr, left);
					off += left;
					len -= left;
					ivptr += left;
					if (ivptr == iv.Length)
					{
						cipher = new AESCipher(false, _key, iv);
						_initiated = true;
						if (len > 0)
							return cipher.Update(b, off, len);
					}
					return null;
				}
				*/
			}
			else
			{
				Byte[] b2 = new Byte[len];
				_arcfour.EncryptARC4(b, off, len, b2, 0);
				return b2;
			}
		}

		virtual public Byte[] Finish()
		{
			return null;
		}
	}
}
