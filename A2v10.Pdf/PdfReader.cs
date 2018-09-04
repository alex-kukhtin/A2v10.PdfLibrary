// Copyright © 2018 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace A2v10.Pdf
{
	public class PdfReader : IDisposable
	{
		private readonly StreamReader _reader;

		public ReadState ReadState { get; } // TODO: xml

		private PdfReader(StreamReader reader)
		{
			_reader = reader;
		}

		public static PdfReader Create(Stream stream)
		{
			XmlTextReader.Create(stream);
			return PdfReader.Create(new StreamReader(stream));

		}

		public static PdfReader Create(StreamReader reader)
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


		public Boolean Read()
		{
			_reader.Read();

			_reader.ReadLine();
			return false;
		}

		public void Close()
		{
			_reader?.Close();
			_reader?.Dispose();
		}
	}
}
