// Copyright © 2018 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Text;

namespace A2v10.Pdf
{
	public enum Token
	{
		String,
		Number,
		Name,
		Comment,
		StartArray,
		EndArray,
		StartDictionary,
		EndDictionary,
		Header,
		Trailer,
		Eof,
	}

	public class Lexer
	{
		private Token _token;
		private String _stringValue;
		private String _version;

		private readonly StreamReader _reader;

		public Lexer(StreamReader reader)
		{
			_reader = reader;
			_reader.BaseStream.Seek(0, SeekOrigin.Begin);
			_reader.DiscardBufferedData();
		}

		public Lexer(String data)
		{
			_reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(data)));
		}

		public Token Token => _token;
		public Boolean Eof => _token == Token.Eof;

		public String StringValue => _stringValue;
		public Int32 IntValue => Int32.Parse(_stringValue);
		public String Version => _version;

		public Boolean NextToken()
		{
			Int32 ch = 0;
			Int32 next = 0;
			do
			{
				ch = Read();
			}
			while (ch != 1 && IsWhitespace(ch));

			if (ch == -1)
			{
				_token = Token.Eof;
				return false;
			}

			_stringValue = String.Empty;

			switch (ch)
			{
				case '%':
					_token = Token.Comment;
					_stringValue = ReadComment(ch);
					CheckHeader();
					break;
				case '/':
					_token = Token.Name;
					_stringValue = ReadName();
					break;
				case '[':
					_token = Token.StartArray;
					break;
				case ']':
					_token = Token.EndArray;
					break;
				case '<':
					next = Read();
					if (next == '<')
					{
						_token = Token.StartDictionary;
						break;
					}
					_stringValue = ReadString();
					break;
				case '>':
					next = Read();
					if (next == '>')
					{
						_token = Token.EndDictionary;
						break;
					}
					throw new LexerException(LexerError.GreaterThenExpected);
			}
			return false;
		}


		void CheckHeader()
		{
			if (_stringValue.IndexOf("%PDF-") == 0)
			{
				_token = Token.Header;
				_version = _stringValue.Substring(5);
			}
		}


		String ReadComment(Int32 ch)
		{
			StringBuilder sb = new StringBuilder();
			while (!IsEol(ch))
			{
				sb.Append((Char)ch);
				ch = Read();
			}
			return sb.ToString();
		}

		String ReadName(Int32 ch)
		{
			return null;
		}

		String ReadString()
		{
			return null;
		}

		Stream ReadStream()
		{
			return null;
		}

		Int32 Read()
		{
			if (_reader.EndOfStream)
				return -1;
			return _reader.Read();
		}

		void ReadBack()
		{
			_reader.BaseStream.Seek(-1, SeekOrigin.Current);
			_reader.DiscardBufferedData();
		}

		static Boolean IsWhitespace(Int32 ch)
		{
			return ch == '\r' || ch == '\n' || ch == '\t' || ch == '\f' || ch == ' ';
		}

		static Boolean IsDelimiter(Int32 ch)
		{
			return ch == '<' || ch == '>' || ch == '(' || ch == ')' || ch == '[' || ch == ']' || ch == '/' || ch == '%';
		}

		static Boolean IsEol(Int32 ch)
		{
			return ch == -1 || ch == '\r' || ch == '\n';
		}
	}
}
