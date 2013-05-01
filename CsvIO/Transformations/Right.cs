﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvIO.Txt.Transformations {

	/// <summary>
	/// Sets the maximum amount of characters to return starting from the right, going to the left (simulating 'Right' function in a database)
	/// </summary>
	/// <remarks></remarks>
	public class Right : Transformation {

		public int MaxAmount { get; set; }

		public override object Clone() {
			return new Right {
				MaxAmount = this.MaxAmount
			};
		}

		public override string Process(string value) {
			return value.Substring(Math.Max(value.Length - MaxAmount, 0));
		}
	}
}
