// Copyright © 2018 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Pdf
{
	public enum LexerError
	{
		GreaterThenExpected
	}

	class LexerException : Exception
	{
		public LexerException(LexerError error)
		{
		}
	}
}
