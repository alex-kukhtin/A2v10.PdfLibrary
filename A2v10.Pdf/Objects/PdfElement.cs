// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.


using System;

namespace A2v10.Pdf
{
	public class PdfElement
	{
		protected readonly PdfDictionary _dict;

		protected PdfElement(PdfDictionary dict)
		{
			_dict = dict;
		}

		public static PdfElement Create(PdfName name, PdfDictionary dict)
		{
			if (name == null)
				return new PdfElement(dict);
			switch (name.Name)
			{
				case "Catalog":
					return new PdfCatalog(dict);
				case "Page":
					return new PdfPage(dict);
				case "XRef":
					return new PdfXRef(dict);
			}
			return new PdfElement(dict);
		}

		public Boolean IsFlateDecode => _dict.Get<PdfName>("Filter")?.Name == "FlateDecode";

		public void Decode()
		{
			if (IsFlateDecode)
			{
				PdfStream stream = _dict.Get<PdfStream>("_stream");
				if (stream == null)
					throw new PdfException("There is no stream for FlateDecode");
				stream.FlateDecode();
				var prms = _dict.Get<PdfDictionary>("DecodeParms");
				if (prms != null)
					stream.DecodePredictor(prms);
			}
		}
	}

	public class PdfCatalog : PdfElement
	{
		public PdfCatalog(PdfDictionary dict)
			:base(dict)
		{
		}

		public PdfName PagesName => _dict.Get<PdfName>("Pages");
		public PdfName MetadataName  => _dict.Get<PdfName>("Metadata");
		public PdfName PageLabelsName => _dict.Get<PdfName>("PageLabels");
	}

	public class PdfPage : PdfElement
	{
		public PdfPage(PdfDictionary dict)
			: base(dict) {
		}
	}

	public class PdfXRef : PdfElement
	{
		public PdfXRef(PdfDictionary dict)
			: base(dict)
		{
		}
		public Int32 XRef => _dict.Get<PdfInteger>("_xref").Value;
	}
}
