// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.IO;

namespace A2v10.Pdf
{
	public class ToUnicodeParser
	{
		Lexer _lexer;
		Int32 _level;

		MapToUnicode _mapper;

		public static MapToUnicode Parse(BinaryReader reader)
		{
			var parser = new ToUnicodeParser(reader);
			parser.Parse();
			return parser._mapper;
		}

		ToUnicodeParser(BinaryReader reader)
		{
			_lexer = new Lexer(reader);
			_mapper = new MapToUnicode();
		}

		void Parse()
		{
			while (_lexer.NextToken())
			{
				switch (_lexer.Token)
				{
					case Token.Name:
						break;
					case Token.Ider:
						ParseOperator();
						break;
					case Token.Comment:
						break;
				}
			}
		}

		void ParseOperator()
		{
			switch (_lexer.StringValue)
			{
				case "begin":
					_level += 1;
					break;
				case "end":
					_level -= 1;
					break;
				case "beginbfchar":
					ReadBfChar();
					break;
				case "beginbfrange":
					ReadBfRange();
					break;
			}
		}

		void ReadBfChar()
		{
			while (_lexer.NextToken())
			{
				switch (_lexer.Token)
				{
					case Token.Ider:
						if (_lexer.StringValue == "endbfchar")
							return;
						break;
					case Token.HexString:
						var nx = new PdfHexString(_lexer.StringValue);
						_lexer.NextToken();
						var uni = new PdfHexString(_lexer.StringValue);
						_mapper.AddChar(nx.Value, uni.Value);
						break;
				}
			}
		}

		void ReadBfRange()
		{
			while (_lexer.NextToken())
			{
				switch (_lexer.Token)
				{
					case Token.Ider:
						if (_lexer.StringValue == "endbfrange")
							return;
						break;
					case Token.HexString:
						var cid1 = new PdfHexString(_lexer.StringValue);
						_lexer.NextToken();
						var cid2 = new PdfHexString(_lexer.StringValue);
						_lexer.NextToken();
						var code = new PdfHexString(_lexer.StringValue);
						_mapper.AddRange(cid1.Value, cid2.Value, code.Value);
						break;
				}
			}
		}
	}
}
