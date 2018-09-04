using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using A2v10.Pdf;

namespace A2v10.Pdf.Tests
{
	[TestClass]
	public class TestLexer
	{

		Lexer TokenTypes(String data, params Token[] expected)
		{
			var lexer = new Lexer(data);
			for (Int32 i = 0; i<expected.Length; i++)
			{
				lexer.NextToken();
				Assert.AreEqual(expected[i], lexer.Token);
			}
			return lexer;
		}

		[TestMethod]
		public void Header()
		{
			var lexer = TokenTypes("%PDF-1.5", Token.Header);
			Assert.AreEqual("1.5", lexer.Version);
		}
	}
}
