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
	internal sealed class Pack
	{
		private Pack()
		{
		}

		internal static void UInt16_To_BE(UInt16 n, Byte[] bs)
		{
			bs[0] = (Byte)(n >> 8);
			bs[1] = (Byte)(n);
		}

		internal static void UInt16_To_BE(UInt16 n, Byte[] bs, Int32 off)
		{
			bs[off] = (Byte)(n >> 8);
			bs[off + 1] = (Byte)(n);
		}

		internal static UInt16 BE_To_UInt16(Byte[] bs)
		{
			UInt32 n = (UInt32)bs[0] << 8
				| (UInt32)bs[1];
			return (UInt16) n;
		}

		internal static UInt16 BE_To_UInt16(Byte[] bs, Int32 off)
		{
			UInt32 n = (UInt32) bs[off] << 8 | (UInt32) bs[off + 1];
			return (UInt16) n;
		}

		internal static Byte[] UInt32_To_BE(UInt32 n)
		{
			Byte[] bs = new Byte[4];
			UInt32_To_BE(n, bs, 0);
			return bs;
		}

		internal static void UInt32_To_BE(UInt32 n, Byte[] bs)
		{
			bs[0] = (Byte)(n >> 24);
			bs[1] = (Byte)(n >> 16);
			bs[2] = (Byte)(n >> 8);
			bs[3] = (Byte)(n);
		}

		internal static void UInt32_To_BE(UInt32 n, Byte[] bs, Int32 off)
		{
			bs[off]     = (Byte) (n >> 24);
			bs[off + 1] = (Byte) (n >> 16);
			bs[off + 2] = (Byte) (n >> 8);
			bs[off + 3] = (Byte) (n);
		}

		internal static Byte[] UInt32_To_BE(UInt32[] ns)
		{
			Byte[] bs = new Byte[4 * ns.Length];
			UInt32_To_BE(ns, bs, 0);
			return bs;
		}

		internal static void UInt32_To_BE(UInt32[] ns, Byte[] bs, Int32 off)
		{
			for (Int32 i = 0; i < ns.Length; ++i)
			{
				UInt32_To_BE(ns[i], bs, off);
				off += 4;
			}
		}

		internal static UInt32 BE_To_UInt32(Byte[] bs)
		{
			return (UInt32)bs[0] << 24
				| (UInt32)bs[1] << 16
				| (UInt32)bs[2] << 8
				| (UInt32)bs[3];
		}

		internal static UInt32 BE_To_UInt32(Byte[] bs, Int32 off)
		{
			return (UInt32)bs[off] << 24
				| (UInt32)bs[off + 1] << 16
				| (UInt32)bs[off + 2] << 8
				| (UInt32)bs[off + 3];
		}

		internal static void BE_To_UInt32(Byte[] bs, Int32 off, UInt32[] ns)
		{
			for (Int32 i = 0; i < ns.Length; ++i)
			{
				ns[i] = BE_To_UInt32(bs, off);
				off += 4;
			}
		}

		internal static Byte[] UInt64_To_BE(UInt64 n)
		{
			Byte[] bs = new Byte[8];
			UInt64_To_BE(n, bs, 0);
			return bs;
		}

		internal static void UInt64_To_BE(UInt64 n, Byte[] bs)
		{
			UInt32_To_BE((UInt32)(n >> 32), bs);
			UInt32_To_BE((UInt32)(n), bs, 4);
		}

		internal static void UInt64_To_BE(UInt64 n, Byte[] bs, Int32 off)
		{
			UInt32_To_BE((UInt32)(n >> 32), bs, off);
			UInt32_To_BE((UInt32)(n), bs, off + 4);
		}

		internal static UInt64 BE_To_UInt64(Byte[] bs)
		{
			UInt32 hi = BE_To_UInt32(bs);
			UInt32 lo = BE_To_UInt32(bs, 4);
			return ((UInt64)hi << 32) | (UInt64)lo;
		}

		internal static UInt64 BE_To_UInt64(Byte[] bs, Int32 off)
		{
			UInt32 hi = BE_To_UInt32(bs, off);
			UInt32 lo = BE_To_UInt32(bs, off + 4);
			return ((UInt64)hi << 32) | (UInt64)lo;
		}

		internal static void UInt16_To_LE(UInt16 n, Byte[] bs)
		{
			bs[0] = (Byte)(n);
			bs[1] = (Byte)(n >> 8);
		}

		internal static void UInt16_To_LE(UInt16 n, Byte[] bs, Int32 off)
		{
			bs[off] = (Byte)(n);
			bs[off + 1] = (Byte)(n >> 8);
		}

		internal static UInt16 LE_To_UInt16(Byte[] bs)
		{
			UInt32 n = (UInt32) bs[0]
				| (UInt32) bs[1] << 8;
			return (UInt16) n;
		}

		internal static UInt16 LE_To_UInt16(Byte[] bs, Int32 off)
		{
			UInt32 n = (UInt32)bs[off]
				| (UInt32)bs[off + 1] << 8;
			return (UInt16) n;
		}

		internal static Byte[] UInt32_To_LE(UInt32 n)
		{
			Byte[] bs = new Byte[4];
			UInt32_To_LE(n, bs, 0);
			return bs;
		}

		internal static void UInt32_To_LE(UInt32 n, Byte[] bs)
		{
			bs[0] = (Byte)(n);
			bs[1] = (Byte)(n >> 8);
			bs[2] = (Byte)(n >> 16);
			bs[3] = (Byte)(n >> 24);
		}

		internal static void UInt32_To_LE(UInt32 n, Byte[] bs, Int32 off)
		{
			bs[off] = (Byte)(n);
			bs[off + 1] = (Byte)(n >> 8);
			bs[off + 2] = (Byte)(n >> 16);
			bs[off + 3] = (Byte)(n >> 24);
		}

		internal static Byte[] UInt32_To_LE(UInt32[] ns)
		{
			Byte[] bs = new Byte[4 * ns.Length];
			UInt32_To_LE(ns, bs, 0);
			return bs;
		}

		internal static void UInt32_To_LE(UInt32[] ns, Byte[] bs, Int32 off)
		{
			for (Int32 i = 0; i < ns.Length; ++i)
			{
				UInt32_To_LE(ns[i], bs, off);
				off += 4;
			}
		}

		internal static UInt32 LE_To_UInt32(Byte[] bs)
		{
			return (UInt32)bs[0]
				| (UInt32)bs[1] << 8
				| (UInt32)bs[2] << 16
				| (UInt32)bs[3] << 24;
		}

		internal static UInt32 LE_To_UInt32(Byte[] bs, Int32 off)
		{
			return (UInt32)bs[off]
				| (UInt32)bs[off + 1] << 8
				| (UInt32)bs[off + 2] << 16
				| (UInt32)bs[off + 3] << 24;
		}

		internal static void LE_To_UInt32(Byte[] bs, Int32 off, UInt32[] ns)
		{
			for (Int32 i = 0; i < ns.Length; ++i)
			{
				ns[i] = LE_To_UInt32(bs, off);
				off += 4;
			}
		}

		internal static Byte[] UInt64_To_LE(UInt64 n)
		{
			Byte[] bs = new Byte[8];
			UInt64_To_LE(n, bs, 0);
			return bs;
		}

		internal static void UInt64_To_LE(UInt64 n, Byte[] bs)
		{
			UInt32_To_LE((UInt32)(n), bs);
			UInt32_To_LE((UInt32)(n >> 32), bs, 4);
		}

		internal static void UInt64_To_LE(UInt64 n, Byte[] bs, Int32 off)
		{
			UInt32_To_LE((UInt32)(n), bs, off);
			UInt32_To_LE((UInt32)(n >> 32), bs, off + 4);
		}

		internal static UInt64 LE_To_UInt64(Byte[] bs)
		{
			UInt32 lo = LE_To_UInt32(bs);
			UInt32 hi = LE_To_UInt32(bs, 4);
			return ((UInt64)hi << 32) | (UInt64)lo;
		}

		internal static UInt64 LE_To_UInt64(Byte[] bs, Int32 off)
		{
			UInt32 lo = LE_To_UInt32(bs, off);
			UInt32 hi = LE_To_UInt32(bs, off + 4);
			return ((UInt64) hi << 32) | (UInt64) lo;
		}
	}
}
