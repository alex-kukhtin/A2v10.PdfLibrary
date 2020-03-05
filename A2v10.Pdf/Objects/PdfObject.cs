// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

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
	}

	public class PdfHexString : PdfObject
	{
		private readonly String _value;

		public PdfHexString(String value)
			: base(ObjectType.HexString)
		{
			_value = value;
		}
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
			throw new LexerException($"Invalid number value. name = {name}");
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
	}

	public class PdfStream : PdfObject
	{
		public Byte[] Bytes { get; }
		public PdfStream(Byte[] bytes)
		: base(ObjectType.Stream)
		{
			Bytes = bytes;
		}
	}
}
