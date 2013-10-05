using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lski.CsvIO.Txt.Transformations {

	/// <summary>
	/// Removes all characters stated in formatting from the selected value
	/// </summary>
	public class StripChars : Transformation {

		public char[] Chars { get; set; }

		public override string Process(string value) {

			if (Chars == null)
				return value;
			if (value == null)
				return null;

			var sb = new System.Text.StringBuilder();

			foreach (Char c in value) {

				if (!Chars.Contains(c))
					sb.Append(c);
			}

			return sb.ToString();
		}

		public override object Clone() {

			return new StripChars {
				Chars = this.Chars
			};
		}
	}
}


