// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.Pdf
{
	public class MapToUnicode
	{
		private IDictionary<Int32, String> _map1 = new Dictionary<Int32, String>();
		private IDictionary<Int32, String> _map2 = new Dictionary<Int32, String>();

		Encoding _encoding = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);

		public void AddChar(Byte[] source, Byte[] code)
		{
			var str = Bytes2String(code);
			if (source.Length == 2)
			{
				Int32 key = source[0] & 0xFF;
				key <<= 8;
				key |= source[1] & 0xFF;
				_map2.Add(key, str);
			}
			else if (source.Length == 1)
			{
				Int32 key = source[0] & 0xFF;
				_map1.Add(key, str);
			}
		}

		public void AddRange(Byte[] from, Byte[] to, Byte[] code)
		{
			Int32 nFrom = Bytes2Int(from);
			Int32 nTo = Bytes2Int(to);
			Int32 nCode = Bytes2Int(code);
			for (Int32 i = nFrom; i<=nTo; i++)
			{
				Byte[] src = Int2Bytes(i);
				AddChar(src, Int2Bytes(nCode + i - nFrom));
			}
		}

		String Bytes2String(Byte[] target)
		{
			if (target.Length == 1)
				return new String((Char)(target[0] & 0xff), 1);
			else
				return _encoding.GetString(target);
		}

		private static Int32 Bytes2Int(Byte[] b)
		{
			Int32 v = 0;
			for (Int32 i = 0; i < b.Length; i++)
			{
				v = v << 8;
				v |= b[i] & 0xff;
			}
			return v;
		}

		private static Byte[] Int2Bytes(Int32 v)
		{
			var b = new Byte[2];
			for (Int32 i = b.Length - 1; i >= 0; i--)
			{
				b[i] = (Byte) v;
				v = v >> 8;
			}
			return b;
		}

		public String Decode(Byte[] bytes)
		{
			var sb = new StringBuilder();
			Int32 len = bytes.Length;
			Int32 offset = 0;
			for (Int32 i = offset; i< offset + len; i++)
			{
				var r = DecodeSingleChar(bytes, i, 1);
				if (r == null) {
					r = DecodeSingleChar(bytes, i, 2);
					i += 1;
				}
				if (r != null)
					sb.Append(r);
			}
			return sb.ToString();
		}

		String DecodeSingleChar(Byte[] bytes, Int32 pos, Int32 len)
		{
			if (len == 1)
			{
				Int32 key = bytes[pos] & 0xff;
				if (_map1.TryGetValue(key, out String str))
					return str;
			}
			else if (len == 2)
			{
				Int32 key = bytes[pos] & 0xff;
				key <<= 8;
				key += bytes[pos + 1] & 0xff;
				if (_map2.TryGetValue(key, out String str))
					return str;
			}
			return null;
		}

		public void FillMetrics(IDictionary<Int32, Int32> widths, Int32 defaultWidts)
		{
			foreach (var kv in _map2)
			{
				if (widths.TryGetValue(kv.Key, out Int32 width))
				{
					//_metrics[kv.Value] = new MxValue(kv.Key, width);
				}
			}
		}
	}
}
