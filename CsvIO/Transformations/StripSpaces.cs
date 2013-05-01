using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvIO.Txt.Transformations {

	/// <summary>
	/// Removes all spaces from the selected value
	/// </summary>
	public class StripSpaces : Transformation {

		public override string Process(string value) {

			if (value == null)
				return null;

			var sb = new StringBuilder();

			foreach (var item in value) {

				if (item != ' ')
					sb.Append(item);
			}

			return sb.ToString();
		}

		public override object Clone() {
			return new StripSpaces();
		}
	}
}
