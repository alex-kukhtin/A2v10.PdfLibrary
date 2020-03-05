// Copyright © 2018 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Text;

namespace A2v10.Pdf
{
	public enum Token
	{
		Ider,
		String,
		HexString,
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

	public enum Ider
	{
		obj,
		endobj,
		stream,
		endstream,
		startxref,
	}

	public class Lexer
	{
		private Token _token;
		private String _stringValue;
		private String _version;

		private readonly BinaryReader _reader;

		public Lexer(BinaryReader reader)
		{
			_reader = reader;
			_reader.BaseStream.Seek(0, SeekOrigin.Begin);
		}

		public Lexer(String data)
		{
			_reader = new BinaryReader(new MemoryStream(Encoding.ASCII.GetBytes(data)));
		}

		public Token Token => _token;
		public Boolean Eof => _token == Token.Eof;

		public String StringValue => _stringValue;
		public Int32 IntValue => Int32.Parse(_stringValue);
		public String Version => _version;
		public Ider Ider => (Ider) Enum.Parse(typeof(Ider), _stringValue);

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
					ReadComment(ch);
					CheckHeader();
					CheckEof();
					break;
				case '/':
					_token = Token.Name;
					ReadName(Read());
					break;
				case '[':
					_token = Token.StartArray;
					break;
				case '(':
					_token = Token.String;
					ReadString();
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
					_token = Token.HexString;
					ReadHexString();
					break;
				case '-':
					_token = Token.Number;
					ReadNumber(ch);
					break;
				case '>':
					next = Read();
					if (next == '>')
					{
						_token = Token.EndDictionary;
						break;
					}
					throw new LexerException(LexerError.GreaterThenExpected);
				default:
					if (IsDecimalDigit(ch))
					{
						_token = Token.Number;
						ReadNumber(ch);
					}
					else if (IsIder(ch))
					{
						_token = Token.Ider;
						ReadIder(ch);
					}
					break;
			}
			return true;
		}


		public PdfDictionary ReadDictionary()
		{
			var dict = new PdfDictionary();
			String name;
			while (NextToken())
			{
				switch (Token)
				{
					case Token.Name:
						name = StringValue;
						if (name == "Contents")
						{
							int z = 55;
						}
						NextToken();
						if (Token == Token.Name)
						{
							dict.Add(name, new PdfNull());
							name = StringValue;
						}
						else
						{
							dict.Add(name, ReadDictionaryValue());
						}
						break;
					case Token.EndDictionary:
						return dict;
					default:
						throw new LexerException($"Invalid dictionary type: {Token}");
				}
			}
			return dict;
		}

		PdfObject ReadArray()
		{
			var arr = new PdfArray();
			while (NextToken())
			{
				switch (Token)
				{
					case Token.EndArray:
						return arr;
					case Token.Number:
						arr.Add(PlainPdfObject());
						break;
					case Token.String:
						arr.Add(new PdfString(StringValue));
						break;
					case Token.HexString:
						arr.Add(new PdfHexString(StringValue));
						break;
					case Token.Ider:
						arr.Add(new PdfString(StringValue));
						break;
					default:
						throw new LexerException($"Invalid array item type: {Token}");
				}
			}
			throw new LexerException($"Invalid array type: {Token}");
		}

		PdfObject ReadDictionaryValue()
		{
			switch (Token)
			{
				case Token.StartArray:
					return ReadArray();
				case Token.StartDictionary:
					return ReadDictionary();
				default:
					var sv = ReadUntilDelimiter();
					if (!String.IsNullOrEmpty(sv))
					{
						_stringValue += sv;
						return new PdfString(StringValue);
					}
					else
						return PlainPdfObject();
			}
		}

		PdfObject PlainPdfObject()
		{
			switch (Token) {
				case Token.Number:
					if (StringValue.Contains("."))
						return new PdfReal(StringValue);
					return new PdfInteger(StringValue);
				case Token.String:
					return new PdfString(StringValue);
				case Token.HexString:
					return new PdfHexString(StringValue);
			}
			throw new LexerException($"Invalid token for plain object {Token}");
		}

		void CheckHeader()
		{
			if (_stringValue.IndexOf("%PDF-") == 0)
			{
				_token = Token.Header;
				_version = _stringValue.Substring(5);
			}
		}

		void CheckEof()
		{
			if (_stringValue == "%%EOF")
				_token = Token.Eof;
		}

		void ReadNumber(Int32 ch)
		{
			StringBuilder sb = new StringBuilder();
			if (ch == '-')
			{
				sb.Append('-');
				ch = Read();
			}
			while (!IsEol(ch) && (IsDecimalDigit(ch) || ch == '.'))
			{
				sb.Append((Char)ch);
				ch = Read();
			}
			_stringValue = sb.ToString();
			Unget(ch);
		}


		void ReadComment(Int32 ch)
		{
			_stringValue = ReadToEol(ch); 
		}

		void ReadIder(Int32 ch)
		{
			ReadName(ch);
		}

		String ReadUntilDelimiter()
		{
			var ch = Read();
			StringBuilder sb = new StringBuilder();
			while (!IsEol(ch) && !IsDelimiter(ch))
			{
				sb.Append((Char)ch);
				ch = Read();
			}
			Unget(ch);
			return sb.ToString();
		}

		String ReadToEol(Int32 ch)
		{
			StringBuilder sb = new StringBuilder();
			while (!IsEol(ch))
			{
				sb.Append((Char)ch);
				ch = Read();
			}
			return sb.ToString();
		}

		void ReadName(Int32 ch)
		{
			StringBuilder sb = new StringBuilder();
			while (!IsEol(ch) && !IsDelimiter(ch) && !IsWhitespace(ch))
			{
				sb.Append((Char)ch);
				ch = Read();
			}
			_stringValue = sb.ToString();
			Unget(ch);
		}

		void ReadHexString()
		{
			StringBuilder sb = new StringBuilder();
			Int32 ch = Read();
			while (!IsEol(ch) && ch != '>')
			{
				sb.Append((Char)ch);
				ch = Read();
			}
			_stringValue = sb.ToString();
		}

		void ReadString()
		{
			StringBuilder sb = new StringBuilder();
			Int32 ch = Read();
			Int32 inside = 0;
			while (!IsEol(ch))
			{
				if (ch == '(')
				{
					inside += 1;
					sb.Append('(');
				}
				else if (ch == ')')
				{
					if (inside == 0)
						break;
					inside -= 1;
					sb.Append(')');
				}
				else if (ch == '\\')
				{
					Int32 next = Read();
					switch (next)
					{
						case '(':
							sb.Append('(');
							break;
						case ')':
							sb.Append(')');
							break;
						case 'n':
							sb.Append('\n');
							break;
						case 'r':
							sb.Append('\r');
							break;
						case 't':
							sb.Append('\t');
							break;
						case 'b':
							sb.Append('\b');
							break;
						case 'f':
							sb.Append('\f');
							break;
						case '\\':
							sb.Append('\\');
							break;
						default:
							throw new LexerException($"Invalid escape character {next}");
					}
				}
				else
				{
					sb.Append((Char)ch);
				}
				ch = Read();
			}
			_stringValue = sb.ToString();
		}


		Int32 _backChar;

		Int32 Read()
		{
			if (_backChar != '\0')
			{
				var rchar = _backChar;
				_backChar = '\0';
				return rchar;
			}
			if (_reader.BaseStream.Position >= _reader.BaseStream.Length)
				return -1;
			return _reader.ReadByte();
		}

		void Unget(Int32 ch)
		{
			if (ch == '\0')
				return;
			if (_backChar != '\0')
				throw new LexerException("Invalid unget");
			_backChar = ch;
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

		static Boolean IsDecimalDigit(Int32 ch)
		{
			return ch >= '0' && ch <= '9';
		}

		static Boolean IsIder(Int32 ch)
		{
			return ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z';
		}
	}
}
