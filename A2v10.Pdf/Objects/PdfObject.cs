// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Util.Zlib;

using A2v10.Pdf.Crypto;

namespace A2v10.Pdf
{
	public abstract class PdfObject
	{
		public enum ObjectType
		{
			Null,
			Nothing,
			String,
			HexString,
			Boolean,
			Integer,
			Real,
			Name,
			Array,
			Dictionary,
			Stream,
			Indirect
		}

		protected readonly ObjectType _type;

		protected PdfObject(ObjectType type)
		{
			_type = type;
		}

		Boolean IsBoolean => _type == ObjectType.Boolean;
		Boolean IsString => _type == ObjectType.String;
		Boolean IsStream => _type == ObjectType.Stream;
	}

	public class PdfNull : PdfObject
	{
		public PdfNull()
			: base(ObjectType.Null) { }
	}

	public class PdfInteger : PdfObject
	{
		public PdfInteger(String value)
			: base(ObjectType.Indirect)
		{
			Value = Int32.Parse(value);
		}

		public Int32 Value { get; }
	}

	public class PdfReal : PdfObject 
	{
		public PdfReal(String value)
			: base(ObjectType.Real)
		{
			Value = Double.Parse(value);
		}

		public Double Value { get; }
	}


	public class PdfString: PdfObject
	{
		public String Value { get; }

		public PdfString(String value)
			: base(ObjectType.String)
		{
			Value = value;
		}

		public Byte[] ToISOBytes()
		{
			if (Value == null)
				return null;
			Int32 len = Value.Length;
			Byte[] b = new Byte[len];
			for (Int32 i = 0; i < len; i++)
				b[i] = (Byte) Value[i];
			return b;

		}
	}

	public class PdfHexString : PdfObject
	{
		private readonly String _value;

		public PdfHexString(String value)
			: base(ObjectType.HexString)
		{
			_value = value;
		}

		public Byte[] Value
		{
			get {
				var bytes = new Byte[_value.Length / 2];
				var arr = _value.ToCharArray();
				for (Int32 i=0; i<_value.Length; i+= 2)
				{
					bytes[i / 2] = Byte.Parse(_value.Substring(i, 2), NumberStyles.HexNumber);
				}
				return bytes;
			}
		}
	}

	public class PdfName : PdfObject
	{
		private readonly String _value;

		public PdfName(String value)
			: base(ObjectType.HexString)
		{
			_value = value;
		}

		public String Name => _value;
	}

	public class PdfDictionary: PdfObject
	{
		private readonly IDictionary<String, PdfObject> _dictionary;

		public PdfDictionary()
			:base(ObjectType.Dictionary)
		{
			_dictionary = new Dictionary<String, PdfObject>();
		}

		public void Add(String name, PdfObject value)
		{
			_dictionary.Add(name, value);
		}

		public Int32 GetInt32(String name)
		{
			if (_dictionary.TryGetValue(name, out PdfObject val))
			{
				switch (val)
				{
					case PdfInteger valNum:
						return valNum.Value;
				}
			}
			throw new LexerException($"Invalid Number value. Name: {name}");
		}

		public T Get<T>(String name) where T: class
		{
			if (_dictionary.TryGetValue(name, out PdfObject val))
			{
				if (val is T)
					return val as T;
			}
			return null;
		}

		public Boolean ContainsKey(String key)
		{
			return _dictionary.ContainsKey(key);
		}
	}

	public class PdfArray : PdfObject
	{
		private readonly IList<PdfObject> _list;

		public PdfArray()
			: base(ObjectType.Array)
		{
			_list = new List<PdfObject>();
		}

		public void Add(PdfObject value)
		{
			_list.Add(value);
		}

		public Int32 Count => _list.Count;

		public T Get<T>(Int32 index) where T:class
		{
			return _list[index] as T;
		}
	}

	public class PdfStream : PdfObject
	{
		public Byte[] Bytes { get; private set; }
		public PdfStream(Byte[] bytes)
		: base(ObjectType.Stream)
		{
			Bytes = bytes;
		}

		public void FlateDecode()
		{
			Byte[] result = ZInflaterStream.FlatDecode(Bytes);
			Bytes = result;
		}

		public void Decrypt(PdfEncryption decryptor)
		{
			var result = decryptor.DecryptByteArray(Bytes);
			Bytes = result;
		}

		public void DecodePredictor(PdfDictionary dict)
		{
			Bytes = PredictorDecoder.Decode(Bytes, dict);
		}
	}
}
