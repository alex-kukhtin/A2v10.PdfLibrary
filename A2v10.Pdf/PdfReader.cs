// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Util.Zlib;

namespace A2v10.Pdf
{
	public class PdfReader : IDisposable
	{
		private readonly BinaryReader _reader;
		private readonly Lexer _lexer;

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
			if (disposing)
			{
				Close();
			}
		}


		public PdfFile ReadFile()
		{
			var file = new PdfFile();
			Read(file);
			file.Construct();
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
											var obj = ReadObject();
											file.AddObject(objId, obj);
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

		public IEnumerable<Tuple<Int32, PdfObject>> ReadObjStream()
		{
			Dictionary<Int32, Int32> _refs = new Dictionary<Int32, Int32>();
			Boolean bVal = false;
			Int32 nKey = 0;
			Int32 _startPos = -1;

			while (_lexer.NextToken())
			{
				switch (_lexer.Token)
				{
					case Token.Number:
						if (bVal) {
							_refs.Add(nKey, _lexer.IntValue);
							bVal = false;
						}
						else
						{
							bVal = true;
							nKey = _lexer.IntValue;
						}
						break;
					case Token.StartDictionary:
					case Token.StartArray:
						if (_startPos == -1)
							_startPos = _lexer.Position;
						Int32 pos = _lexer.Position - _startPos;
						PdfObject obj = null;
						if (_lexer.Token == Token.StartDictionary)
							obj = _lexer.ReadDictionary();
						else if (_lexer.Token == Token.StartArray)
							obj = _lexer.ReadArray(null);
						var k = _refs.FirstOrDefault(kv => Math.Abs(kv.Value - pos) < 3);
						if (k.Key != 0)
						{
							_refs.Remove(k.Key);
							yield return Tuple.Create(k.Key, obj);
						}
						if (_refs.Count == 0)
						{
							yield break;
						}
						break;
				}
			}
		}

		public PdfDictionary ReadObject()
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
									if (dict.Get<PdfName>("Type")?.Name == "XRef") {
										_lexer.NextToken();
										if (_lexer.Token == Token.Ider && _lexer.Ider == Ider.startxref)
										{
											_lexer.NextToken(); // xref
											dict.Add("_xref", new PdfInteger(_lexer.StringValue));
											_lexer.NextToken(); // eof
											if (_lexer.Token != Token.Eof)
												throw new LexerException($"Invalid XRef");
										}
										else
										{
											throw new LexerException($"Invalid XRef");
										}
									}
									return dict;
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
			return dict;
		}

		Stream ReadStream(PdfDictionary dict)
		{
			Int32 len = dict.GetInt32("Length");
			var buffer = new Byte[len];
			Int32 offset = 0;
			while (true)
			{
				Byte ch = _reader.ReadByte();
				if (ch != '\n' && ch != '\r')
				{
					buffer[offset] = ch;
					offset += 1;
					break;
				}

			}

			_reader.Read(buffer, offset, len - offset);

			dict.Add("_stream", new PdfStream(buffer));

			_lexer.NextToken();
			if (_lexer.Token != Token.Ider && _lexer.Ider != Ider.endstream)
				throw new LexerException($"Invalid stream. Expected 'endstream'");
			return null;
		}
	}
}
