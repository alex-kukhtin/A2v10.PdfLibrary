// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Pdf
{
	public class PdfFile
	{
		public String Version { get; set; }

		private readonly Dictionary<String, PdfElement> _objects = new Dictionary<String, PdfElement>();

		private readonly List<PdfXRef> _xRefs = new List<PdfXRef>();
		private PdfXRef _trailer { get; set; }

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
			for (int i = 0; i < ids.Count; i++)
			{
				var id1 = ids.Get<PdfHexString>(1).Value;
				int z = 55;
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
				var filter = cf.Get<PdfName>("Filter")?.Name;
			}
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
		}
	}
}
