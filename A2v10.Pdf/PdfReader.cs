// Copyright © 2018 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Util.Zlib;
using System.Xml;

namespace A2v10.Pdf
{
	public class PdfReader : IDisposable
	{
		private readonly BinaryReader _reader;
		private readonly Lexer _lexer;

		public ReadState ReadState { get; } // TODO: xml

		private PdfReader(BinaryReader reader)
		{
			_reader = reader;
			_lexer = new Lexer(reader);
		}

		public static PdfReader Create(BinaryReader reader)
		{
			reader = reader ?? throw new ArgumentNullException(nameof(reader));
			return new PdfReader(reader);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(Boolean disposing)
		{
			if (disposing && ReadState != ReadState.Closed)
			{
				Close();
			}
		}


		public PdfFile ReadFile()
		{
			var file = new PdfFile();
			Read(file);
			return file;
		}

		public void Read(PdfFile file)
		{
			while (_lexer.NextToken())
			{
				switch (_lexer.Token)
				{
					case Token.Header:
						file.Version = _lexer.Version;
						break;
					case Token.Number:
						{
							var objId = _lexer.StringValue;
							_lexer.NextToken();
							if (_lexer.Token == Token.Number)
							{
								objId += $" {_lexer.StringValue}";
								_lexer.NextToken();
								if (_lexer.Token == Token.Ider)
								{
									switch (_lexer.Ider)
									{
										case Ider.obj:
											ReadObject(objId);
											break;
										default:
											throw new LexerException($"unknown object {_lexer.Ider}");
									}
								}
							}
						}
						break;
				}
			}
		}

		public void Close()
		{
			_reader?.Close();
			_reader?.Dispose();
		}

		public void ReadObject(String objId)
		{
			PdfDictionary dict = null;
			while (_lexer.NextToken())
			{
				switch (_lexer.Token)
				{
					case Token.StartDictionary:
						dict = _lexer.ReadDictionary();
						break;
					case Token.Ider:
						{
							switch (_lexer.Ider)
							{
								case Ider.endobj:
									if (objId == "25 0")
									{
										int z = 55;
									}
									return;
								case Ider.stream:
									if (dict == null) {
										throw new LexerException($"There is no dictionary for stream");
									}
									ReadStream(dict);
									break;
								default:
									throw new LexerException($"Unknown object type {_lexer.Ider}");
							}
						}
						break;
					default:
						throw new LexerException($"Unknown token for object {_lexer.Token}");
				}
			}
		}

		Stream ReadStream(PdfDictionary dict)
		{
			Int32 len = dict.GetInt32("Length");
			var buffer = new Byte[len];
			while (_reader.PeekChar() == '\r' || _reader.PeekChar() == '\n')
			{
				_reader.ReadByte();
			}

			_reader.Read(buffer, 0, len);

			if (len == 91)
			{
				Byte[] result = ZInflaterStream.FlatDecode(buffer);
				int z = 55;
				buffer = result;
			}

			if (len == 1586)
			{
				int z = 55;
			}

			dict.Add("_stream", new PdfStream(buffer));

			_lexer.NextToken();
			if (_lexer.Token != Token.Ider && _lexer.Ider != Ider.endstream)
				throw new LexerException($"Invalid stream. Expected 'endstream'");
			return null;
		}
	}
}
