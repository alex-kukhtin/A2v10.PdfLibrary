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
		Boolean,
		Name,
		Comment,
		StartArray,
		EndArray,
		StartDictionary,
		EndDictionary,
		Header,
		Trailer,
		Eof,
		Undefined,
	}

	public enum Ider
	{
		obj,
		endobj,
		stream,
		endstream,
		startxref,
	}

	class LexerLookup
	{
		public Token Token;
		public String StringValue;
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
		public Boolean BoolValue => _stringValue == "true";
		public Int32 Position => (Int32) _reader.BaseStream.Position;

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
					else
					{
						Unget(next);
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
			String name = null;
			Boolean bVal = false;
			while (NextToken())
			{
				if (bVal)
				{
					if (String.IsNullOrEmpty(name))
						throw new LexerException($"Invalid dictionary structure");
					dict.Add(name, ReadDictionaryValue(name));
					bVal = false;
				}
				else
				{
					switch (Token)
					{
						case Token.Name:
							name = StringValue;
							bVal = true;
							break;
						case Token.EndDictionary:
							return dict;
						default:
							throw new LexerException($"Invalid dictionary type '{Token}' for '{name}' element.");
					}
				}
			}
			return dict;
		}


		public PdfObject ReadArray(String name)
		{
			var arr = new PdfArray();
			Int32 step = 0;
			LexerLookup s1 = new LexerLookup();
			LexerLookup s2 = new LexerLookup();

			void ResetLookup()
			{
				if (step == 0)
					return;
				else if (step == 1)
				{
					arr.Add(PlainPdfObject(s1.Token, s1.StringValue));
				} 
				else if (step == 2)
				{
					arr.Add(PlainPdfObject(s1.Token, s1.StringValue));
					arr.Add(PlainPdfObject(s2.Token, s2.StringValue));
				}
				step = 0;
			}

			while (NextToken())
			{
				switch (Token)
				{
					case Token.EndArray:
						ResetLookup();
						return arr;
					case Token.StartArray:
						arr.Add(ReadArray(name));
						break;
					case Token.Number:
						switch (step)
						{
							case 0:
								step = 1;
								s1.Token = Token;
								s1.StringValue = StringValue;
								break;
							case 1:
								step = 2;
								s2.Token = Token;
								s2.StringValue = StringValue;
								break;
							case 2:
								step = 1;
								arr.Add(PlainPdfObject(s1.Token, s1.StringValue));
								s1 = s2;
								s2.Token = Token;
								s2.StringValue = StringValue;
								break;
						}
						break;
					case Token.Ider:
						if (StringValue == "R" && step > 0)
						{
							arr.Add(new PdfName($"{s1.StringValue} {s2.StringValue}"));
							step = 0;
						}
						else
						{
							ResetLookup();
							arr.Add(PlainPdfObject());
						}
						break;
					default:
						ResetLookup();
						/*
						if (IsReference(name))
						{
							String xRef = StringValue;
							NextToken();
							xRef += $" {StringValue}"; // with space
							NextToken();
							if (Token != Token.String && StringValue != "R")
								throw new LexerException($"Invalid reference {name}");
							arr.Add(new PdfName(xRef));
						}
						else
						{
						*/
						arr.Add(PlainPdfObject());
						//}
						break;
				}
			}
			throw new LexerException($"Invalid array type: {Token}");
		}

		const String _refNames = "Encrypt|Root|Info|Metadata|PageLabels|Pages|Parent|Resources|Contents|Cs6|GS1|TT1|TT2|TT3|TT4|CIDSet|FontFile2|FontDescriptor|ToUnicode|DescendantFonts|ICCBased|";

		Boolean IsReference(String name)
		{
			return name != null && _refNames.Contains($"{name}|");
		}

		PdfObject ReadDictionaryValue(String name)
		{
			switch (Token)
			{
				case Token.StartArray:
					return ReadArray(name);
				case Token.StartDictionary:
					return ReadDictionary();
				default:
					if (IsReference(name))
					{
						String xRef = StringValue;
						NextToken();
						xRef += $" {StringValue}"; // with space
						NextToken();
						if (Token != Token.String && StringValue != "R")
							throw new LexerException($"Invalid reference {name}");
						return new PdfName(xRef);
					}
					else
					{
						return PlainPdfObject();
					}
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
				case Token.Name:
					return new PdfName(StringValue);
				case Token.Boolean:
					return new PdfBoolean(BoolValue);
			}
			throw new LexerException($"Invalid token for plain object '{Token}'");
		}

		static PdfObject PlainPdfObject(Token token, String value)
		{
			switch (token)
			{
				case Token.Number:
					if (value.Contains("."))
						return new PdfReal(value);
					return new PdfInteger(value);
				case Token.String:
					return new PdfString(value);
				case Token.HexString:
					return new PdfHexString(value);
			}
			throw new LexerException($"Invalid token for array plain object '{token}' (value='{value}')");
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
			if (_stringValue == "true" || _stringValue == "false")
				_token = Token.Boolean;
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
