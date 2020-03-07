using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Pdf.Objects
{
	public enum CryptoMode {
		STANDARD_ENCRYPTION_40 = 0,
		STANDARD_ENCRYPTION_128 = 1,
		ENCRYPTION_AES_128 = 2,
		ENCRYPTION_AES_256 = 3,
	}
}
