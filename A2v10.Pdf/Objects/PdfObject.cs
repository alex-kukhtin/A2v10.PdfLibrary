// Copyright © 2018 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public abstract class PdfObject
	{
		public enum ObjectType
		{
			Null,
			Nothing,
			String,
			Boolean,
			Number,
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
		Boolean IsNumber => _type == ObjectType.Number;
	}
}
