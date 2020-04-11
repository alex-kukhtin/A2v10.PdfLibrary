// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;

namespace A2v10.Pdf
{
	public class ContentReader : IDisposable
	{
		private readonly Lexer _lexer;
		private readonly BinaryReader _reader;
		private readonly PdfFile _file;
		private readonly PdfPage _page;

		private readonly Dictionary<String, IPsCommand> _commands = new Dictionary<String, IPsCommand>();

		private ContentReader(BinaryReader reader, PdfFile file, PdfPage page)
		{
			_reader = reader;
			_lexer = new Lexer(_reader);
			_file = file;
			_page = page;
			CreateCommands();
		}

		void CreateCommands()
		{
			_commands.Add("BT", new PsBeginText());
			_commands.Add("ET", new PsEndText());
			_commands.Add("Tf", new PsSetTextFont());
			_commands.Add("TJ", new PsShowTextArray());
			_commands.Add("gs", new PsGraphicState());
			_commands.Add("q",  new PsPushGraphicState());
			_commands.Add("Q",  new PsPopGraphicState());
			_commands.Add("TD", new PsMoveStartNextLine());
			_commands.Add("Tm", new PsSetTextMatrix());
			_commands.Add("Tc", new PsSetCharacterSpacing());
			_commands.Add("Tj", new PsShowText());
			_commands.Add("Tw", new PsSetWordSpacing());
			_commands.Add("re", new PsRectagle());
			//f - fill
		}


		public static ContentReader Create(Stream stream, PdfFile file, PdfPage page)
		{
			return new ContentReader(new BinaryReader(stream), file, page);
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

		public void Close()
		{
			_reader?.Close();
			_reader?.Dispose();
		}

		public void ParseContent()
		{
			IList<PdfObject> args = new List<PdfObject>();
			var context = new PsContext(_file, _page);
			while (_lexer.NextToken())
			{
				switch (_lexer.Token)
				{
					case Token.Ider:
						ExecuteCommand(context, _lexer.StringValue, args);
						args.Clear();
						break;
					case Token.StartArray:
						var arr = _lexer.ReadArray(null);
						args.Add(arr);
						break;
					default:
						args.Add(_lexer.PlainPdfObject());
						break;
				}
			}
		}

		void ExecuteCommand(PsContext context, String command, IList<PdfObject> args)
		{
			if (_commands.TryGetValue(command, out IPsCommand cmd))
				cmd.Execute(context, args);
			else
			{
				/* g - set grey level (nonstroking)
				   cs = set color space (nonstroking)
				   scn = set color (nonstroking ICCBased)
				*/
				int z = 55;
			}
		}
	}
}
