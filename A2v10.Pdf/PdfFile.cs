// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

using A2v10.Pdf.Crypto;

namespace A2v10.Pdf
{
	public class PdfFile
	{
		public String Version { get; set; }

		private readonly Dictionary<String, PdfElement> _objects = new Dictionary<String, PdfElement>();

		private readonly List<PdfXRef> _xRefs = new List<PdfXRef>();
		private PdfXRef _trailer;
		private Byte[] _documentId;
		private PdfEncryption _decryptor;

		private PdfCatalog _catalog;

		public void AddObject(String name, PdfDictionary dict)
		{
			var type = dict.Get<PdfName>("Type");
			var elem = PdfElement.Create(type, dict);
			_objects.Add(name, elem);

			if (type != null)
			{
				switch (type.Name)
				{
					case "Catalog":
						_catalog = elem as PdfCatalog;
						break;
					case "XRef":
						_xRefs.Add(elem as PdfXRef);
						break;
				}
			}
		}

		void DecodeTrailer()
		{
			if (_trailer == null)
				return;

			var ids = _trailer.Get<PdfArray>("ID");
			for (Int32 i = 0; i < ids.Count; i++)
			{
				var id1 = ids.Get<PdfHexString>(i).Value;
				if (_documentId == null)
					_documentId = id1;
				else
					continue;
			}


			var enc = _trailer.Get<PdfName>("Encrypt");
			if (enc != null)
			{
				var encelem = _objects[enc.Name];
				CreateEncrypt(encelem);
			}

		}

		void CreateEncrypt(PdfElement elem)
		{
			var cf = elem.Get<PdfElement>("CF");
			if (cf != null)
			{
				var stdCf = cf.Get<PdfElement>("StdCF");
				if (stdCf != null)
				{
				}
			}
			var pValue = elem.Get<PdfInteger>("P").Value;
			var vValue = elem.Get<PdfInteger>("V").Value;
			var rValue = elem.Get<PdfInteger>("R").Value;
			var oValue = elem.Get<PdfString>("O").ToISOBytes();
			var uValue = elem.Get<PdfString>("U").ToISOBytes();
			var filter = elem.Get<PdfName>("Filter")?.Name;

			AlgRevision cryptoMode = AlgRevision.STANDARD_ENCRYPTION_128;

			switch (rValue)
			{
				case 4:
					if (vValue != 4)
						throw new PdfException($"Invalid encrypt. V={vValue}, P={vValue}");
					var cfObj = elem.Get<PdfDictionary>("CF");
					var stdCfObj = cfObj.Get<PdfDictionary>("StdCF");
					if (stdCfObj != null)
					{
						if (stdCfObj.Get<PdfName>("CFM")?.Name == "V2")
						{
							cryptoMode = AlgRevision.STANDARD_ENCRYPTION_128;
						}
					}
					break;
				default:
					throw new NotImplementedException($"Decrypt value {rValue}");
			}

			_decryptor = new PdfEncryption();
			_decryptor.SetCryptoMode(cryptoMode, 0);

			if (filter == "Standard")
			{
				if (rValue != 5)
				{
					_decryptor.SetupByOwnerPassword(_documentId, uValue, oValue, pValue);
					Int32 checkLen = (rValue == 3 || rValue == 4) ? 16 : 32;
					if (!EqualsArray(uValue, _decryptor.UserKey, checkLen)) {
						_decryptor.SetupByUserPassword(_documentId, oValue, pValue);
					}

				}
			}
		}

		private Boolean EqualsArray(Byte[] ar1, Byte[] ar2, Int32 size)
		{
			for (Int32 k = 0; k < size; ++k)
			{
				if (ar1[k] != ar2[k])
					return false;
			}
			return true;
		}

		public void Construct()
		{			
			foreach (var val in _xRefs)
			{
				if (val.IsTrailer)
					_trailer = val;
				val.Decode();
			}
			DecodeTrailer();

			foreach (var o in _objects)
			{
				if (o.Value.IsEncrypted && o.Value.IsStream)
				{
					o.Value.Decrypt(_decryptor, o.Key);
				}
			}
			/*
			var stm = _objects["29 0"];
			stm.Decrypt(_decryptor, 29, 0);
			*/
		}
	}
}
