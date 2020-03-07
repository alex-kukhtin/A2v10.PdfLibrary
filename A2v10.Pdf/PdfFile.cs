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

		public void Construct()
		{
			foreach (var val in _xRefs)
			{
				val.Decode();
			}
		}
	}
}
