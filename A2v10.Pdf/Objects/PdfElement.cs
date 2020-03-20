// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.


using A2v10.Pdf.Crypto;
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

		public virtual Boolean IsEncrypted => true;

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
				case "Pages":
					return new PdfPages(dict);
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

		public Boolean IsStream => _dict.Get<PdfStream>("_stream") != null;
		public Boolean IsObjStream => _dict.Get<PdfName>("Type")?.Name == "ObjStm";

		public virtual void Decrypt(PdfEncryption decryptor, String key)
		{
			var sp = key.Split(' ');
			Decrypt(decryptor, Int32.Parse(sp[0]), Int32.Parse(sp[1]));
		}

		public virtual void Decrypt(PdfEncryption decryptor, Int32 key, Int32 revision)
		{
			PdfStream stream = _dict.Get<PdfStream>("_stream");
			if (stream == null)
				throw new PdfException("There is no stream for Decrypt");
			decryptor.SetHashKey(key, revision);
			stream.Decrypt(decryptor);
			Decode();


			var debugString = System.Text.Encoding.ASCII.GetString(stream.Bytes);
			Console.WriteLine(debugString);
		}

		public Boolean ContainsKey(String key)
		{
			return _dict.ContainsKey(key);
		}

		public T Get<T>(String key) where T:class
		{
			return _dict.Get<T>(key);
		}
	}

	public class PdfCatalog : PdfElement
	{
		public PdfCatalog(PdfDictionary dict)
			:base(dict)
		{
		}

		public override Boolean IsEncrypted => false;

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

	public class PdfPages : PdfElement
	{
		public PdfPages(PdfDictionary dict)
			: base(dict)
		{
		}
	}

	public class PdfXRef : PdfElement
	{
		public PdfXRef(PdfDictionary dict)
			: base(dict)
		{
		}
		public Int32 XRef => _dict.Get<PdfInteger>("_xref").Value;

		public Boolean IsTrailer => XRef == 0 && _dict.ContainsKey("Prev");

		public PdfName EncryptRef => _dict.Get<PdfName>("Encrypt");

		public override Boolean IsEncrypted => false;
	}
}
