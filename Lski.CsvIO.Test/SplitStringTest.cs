using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Lski.CsvIO.Test {

	[TestClass]
	public class SplitStringTest {

		[TestMethod]
		public void SplitStringVariationTest() {

			var s = "This,is,a,\"split,string\", that I,want,to,split";
			var splitChr = ',';
			var splitStr = ", ";
			var textChr = '"';
			var csvImport = new CsvImport(new CsvImportSettings());

			var result1 = csvImport.SplitCsvLine(s, splitChr, textChr);
			var result2 = csvImport.SplitCsvLine(s, splitStr, textChr);

			Console.Out.WriteLine("Result 1: " + String.Join(" - ", result1));
			Console.Out.WriteLine("Result 2: " + String.Join(" - ", result2));

			Assert.AreEqual(result1.Count(), 8);
			Assert.AreEqual(result2.Count(), 2);
		}
	
	}
}
