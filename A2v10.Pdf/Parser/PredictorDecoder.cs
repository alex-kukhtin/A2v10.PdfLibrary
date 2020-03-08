// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

using System;
using System.IO;

namespace A2v10.Pdf
{
	public static class PredictorDecoder
	{
		public static Byte[] Decode(Byte[] inp, PdfDictionary prms)
		{
			if (prms == null)
				return inp;
			var predObj = prms.Get<PdfInteger>("Predictor");
			if (predObj == null)
				return inp;
			Int32 predictor = predObj.Value;
			if (predictor < 10 && predictor != 2)
				return inp;
			Int32 width = 1;
			var colsObj = prms.Get<PdfInteger>("Columns");
			if (colsObj != null )
				width = colsObj.Value;
			Int32 colors = 1;  // TODO from DICT
			Int32 bpc = 8;   // TODO from DICT
			MemoryStream dataStream = new MemoryStream(inp);
			MemoryStream fout = new MemoryStream(inp.Length);
			Int32 bytesPerPixel = colors * bpc / 8;
			Int32 bytesPerRow = (colors * width * bpc + 7) / 8;
			Byte[] curr = new Byte[bytesPerRow];
			Byte[] prior = new Byte[bytesPerRow];

			if (predictor == 2)
			{
				if (bpc == 8)
				{
					Int32 numRows = inp.Length / bytesPerRow;
					for (Int32 row = 0; row < numRows; row++)
					{
						Int32 rowStart = row * bytesPerRow;
						for (Int32 col = 0 + bytesPerPixel; col < bytesPerRow; col++)
						{
							inp[rowStart + col] = (Byte)(inp[rowStart + col] + inp[rowStart + col - bytesPerPixel]);
						}
					}
				}
				return inp;
			}

			// Decode the (sub)image row-by-row
			while (true)
			{
				// Read the filter type byte and a row of data
				Int32 filter = 0;
				try
				{
					filter = dataStream.ReadByte();
					if (filter < 0)
					{
						return fout.ToArray();
					}
					Int32 tot = 0;
					while (tot < bytesPerRow)
					{
						Int32 n = dataStream.Read(curr, tot, bytesPerRow - tot);
						if (n <= 0)
							return fout.ToArray();
						tot += n;
					}
				}
				catch
				{
					return fout.ToArray();
				}

				switch (filter)
				{
					case 0: //PNG_FILTER_NONE
						break;
					case 1: //PNG_FILTER_SUB
						for (Int32 i = bytesPerPixel; i < bytesPerRow; i++)
							curr[i] += curr[i - bytesPerPixel];
						break;
					case 2: //PNG_FILTER_UP
						for (Int32 i = 0; i < bytesPerRow; i++)
							curr[i] += prior[i];
						break;
					case 3: //PNG_FILTER_AVERAGE
						for (Int32 i = 0; i < bytesPerPixel; i++)
							curr[i] += (Byte)(prior[i] / 2);
						for (Int32 i = bytesPerPixel; i < bytesPerRow; i++)
							curr[i] += (Byte)(((curr[i - bytesPerPixel] & 0xff) + (prior[i] & 0xff)) / 2);
						break;
					case 4: //PNG_FILTER_PAETH
						for (Int32 i = 0; i < bytesPerPixel; i++)
							curr[i] += prior[i];

						for (Int32 i = bytesPerPixel; i < bytesPerRow; i++)
						{
							Int32 a = curr[i - bytesPerPixel] & 0xff;
							Int32 b = prior[i] & 0xff;
							Int32 c = prior[i - bytesPerPixel] & 0xff;

							Int32 p = a + b - c;
							Int32 pa = Math.Abs(p - a);
							Int32 pb = Math.Abs(p - b);
							Int32 pc = Math.Abs(p - c);

							Int32 ret;

							if ((pa <= pb) && (pa <= pc))
								ret = a;
							else if (pb <= pc)
								ret = b;
							else
								ret = c;
							curr[i] += (Byte)(ret);
						}
						break;
					default:
						// Error -- uknown filter type
						throw new LexerException("png.filter.unknown");
				}
				fout.Write(curr, 0, curr.Length);

				// Swap curr and prior
				Byte[] tmp = prior;
				prior = curr;
				curr = tmp;
			}
		}
	}
}
