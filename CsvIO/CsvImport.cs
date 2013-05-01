﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using CsvIO.Txt.Conversion;
using CsvIO.Txt.Transformations;

namespace CsvIO {

	/// <summary>
	/// A class that allows importing and exporting from csv files, uses yield to improve performance
	/// </summary>
	/// <remarks>A class that allows importing and exporting from csv files, uses yield to improve performance
	///
	/// If the settings do not include links, they are automatically generated to match the properties of the object being filled. If links are included then the Conversion and Transformation methods are
	/// optional and are auto generated, unless stated.
	/// 
	/// Usage: 
	/// <code> 
	/// var csv = new CsvImport(new CsvImportSettings() {
	///		Header = true,
	///		Links = new CsvImportLink[] {
	///			 new CsvImportLink() { Position = 1, Property = "Useremail" },
	///			 new CsvImportLink() { Position = 0, Property = "Registered", Conversion = new ToDateMDY() },
	///			 new CsvImportLink() { Position = 0, Property = "RandomVal", Tranformations = new Transformations(new Match(@"^[\d]{0,2}")) }
	///		}
	///});
	///
	/// 
	///foreach (var item in csv.Import<MyClass>(@"C:\output.csv")) {
	///		Console.Out.WriteLine(item.MyProperty);
	///}
	///</code>
	/// </remarks>
	public class CsvImport {
		
		public CsvImportSettings Settings { get; private set; }

		protected CsvImport() { 
			// Empty for serializers only
		}

		public CsvImport(CsvImportSettings settings) {
			this.Settings = settings;
		}

		#region "Private Internal Methods"

		/// <summary>
		/// Gets the csv file reader
		/// </summary>
		/// <param name="fileName"></param>
		/// <exception cref="FileNotExistsException">Fires if the file can not be found</exception>
		/// <returns></returns>
		/// <remarks></remarks>
		protected StreamReader CreateFileReader(string fileName) {

			StreamReader r = null;

			// If the file does not exist then exit
			if (!File.Exists(fileName)) {
				throw new FileNotFoundException(fileName);
			}

			r = new StreamReader(fileName);

			// If the file is empty, return as there is nothing to import
			if (r.EndOfStream) {
				return null;
			}

			return r;
		}

		/// <summary>
		/// Takes the imported line and adds each of the values to the datarow passed, which is then altered and passed back
		/// </summary>
		/// <param name="map">The datamap to tell the computer how to link the data table and csv files</param>
		/// <param name="csvLine">The data from the csv file</param>
		/// <param name="row">The datarow to fill</param>
		/// <returns></returns>
		/// <remarks></remarks>
		private T FromCsvLine<T>(CsvImportSettings map, ICollection<InternalCsvLink> mapLinks, string[] csvLine) {

			// Save recalling it
			var csvLineCount = csvLine.Count(); 
			var result = Activator.CreateInstance<T>();

			// loop through each of the link objects, to get their value, in the correct format, via the type stored
			foreach (var mapLink in mapLinks) {
			
				// Because some lines could possibly be smaller than the position of the map, dont attempt to import it!
				if (mapLink.Position < csvLineCount) {

					ProcessImportedValue(map, mapLink, result, csvLine[mapLink.Position]);
				}
			}

			return result;
		}

		/// <summary>
		/// Works with the meta information about the current column in the csv file stored in the csvCol object and uses it
		/// to manipulate the csvValue so that its in an appropriate format for inserting into the dataTable.
		/// </summary>
		/// <returns></returns>
		/// <remarks>Works with the meta information about the current column in the csv file stored in the csvCol object and uses it
		/// to manipulate the csvValue so that its in an appropriate format for inserting into the dataTable.
		/// 
		/// NOTE: Needs to be overridden in a database specific subclass
		/// </remarks>
		internal void ProcessImportedValue<T>(CsvImportSettings settings, InternalCsvLink link, T obj, string value) {

			// Because the methods to insert csv values use dataTables, convert from string null
			// Applicable to all types of values so should be done prior 
			if (settings.NULL.Equals(value, StringComparison.OrdinalIgnoreCase)) {

				link.Property.SetValue(obj, null, null);
				return;
			}

			// Strip the csv rubbish from the string and format it as requested
			value = link.Tranformations.Process(value);

			// If the map states that empty strings should be handled as null return null when its an empty string, otherwise return the empty string
			if (settings.EmptyValueAsNull && value.Length == 0) {

				link.Property.SetValue(obj, null, null);
				return;
			}
			
			// Run the datamap types own conversion method to parse the incoming value
			link.Property.SetValue(obj, link.Conversion.Parse(value), null);
		}

		/// <summary>
		/// Simply takes a line from the csv file and splits it according to the map. It returns the values as is, in same positions as in the csv file
		/// </summary>
		/// <param name="csvLine"></param>
		/// <param name="delimiter">The delimiter to split the csv line with</param>
		/// <returns></returns>
		/// <remarks></remarks>
		protected virtual string[] SplitCsvLine(string csvLine, string delimiter) {
			return csvLine.Split(new string[] { delimiter }, StringSplitOptions.None);
		}

		/// <summary>
		/// Uses the CSV files first livne to create a set of auto generated links to store in map.Links
		/// </summary>
		/// <param name="rdr"></param>
		/// <returns></returns>
		private ICollection<CsvImportLink> CreateLinksFromHeader(String delimiter, StreamReader rdr) {

			var line = rdr.ReadLine();
			var lst = new List<CsvImportLink>();

			if (String.IsNullOrEmpty(line)) {
				return lst;
			}

			var i = 0;
			foreach (var item in SplitCsvLine(line, delimiter)) {
				lst.Add(new CsvImportLink() {
					Position = i++,
					Property = item
				});
			}

			return lst;
		}

		/// <summary>
		/// Uses the links passed in the settings to create the internal links needed to import values
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="links"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		private ICollection<InternalCsvLink> CreateInternalLinks<T>(ICollection<CsvImportLink> links, CsvImportSettings settings) {

			var lst = new List<InternalCsvLink>();

			if (links != null) {

				var qry = (from p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
						   join l in links on p.Name.ToLowerInvariant() equals l.Property.ToLowerInvariant()
						   select new { Property = p, Link = l });


				foreach (var pl in qry) {

					var transformations = new Transformations();
					// Automatically add the processing to remove values 
					transformations.Add(new FromCsv(settings.TextDelimiter));

					if (pl.Link.Tranformations != null) {
						foreach (var item in pl.Link.Tranformations) {
							transformations.Add(item);
						}
					}

					lst.Add(new InternalCsvLink() {
						Property = pl.Property,
						Position = pl.Link.Position,
						Conversion = pl.Link.Conversion ?? ConvertTo.GetConverter(pl.Property.PropertyType),
						Tranformations = transformations
					});
				}
			}

			return lst;
		}

		#endregion

		#region "Import Methods"

		/// <summary>
		/// Imports values from a CSV file into objects of the type desired, as each line is read it is yielded as an object to avoid through the enumeration twice and doesnt store the objects in memory
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="filename"></param>
		/// <returns></returns>
		public IEnumerable<T> Import<T>(string filename) where T : new() {

			using(var rdr = CreateFileReader(filename)) {
				
				foreach (var item in InternalImport<T>(this.Settings, rdr)) {
					yield return item;
				}
			}

		}

		/// <summary>
		/// Imports values from a CSV file into a collection of objects of the type desired
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="filename"></param>
		/// <param name="lst"></param>
		public void Import<T>(string filename, ICollection<T> lst) where T : new() {

			foreach(var item in Import<T>(filename)) {
				lst.Add(item);
			}
		}

		/// <summary>
		/// Imports values from a CSV filestream into objects of the type desired, as each line is read it is yielded as an object to avoid through the enumeration twice and doesnt store the objects in memory
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fs"></param>
		/// <returns></returns>
		public IEnumerable<T> Import<T>(Stream fs) where T : new() {

			foreach (var item in InternalImport<T>(this.Settings, new StreamReader(fs))) {
				yield return item;
			}
		}

		/// <summary>
		/// Imports values from a CSV file into a collection of objects of the type desired
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fs"></param>
		/// <param name="lst"></param>
		public void Import<T>(Stream fs, ICollection<T> lst) where T : new() {

			foreach (var item in Import<T>(fs)) {
				lst.Add(item);
			}
		}

		/// <summary>
		/// Performs the actual import from the passed stream
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="settings"></param>
		/// <param name="rdr"></param>
		/// <returns></returns>
		private IEnumerable<T> InternalImport<T>(CsvImportSettings settings, StreamReader rdr) where T : new() {

			if (rdr == null)
				yield break;

			if (settings.Links == null && !settings.Header) {
				throw new ArgumentException("If not manually providing links to CsvImport there must be a header to match against property names");
			}
			// If there are no links then get names for the links from the first line of 
			else if(settings.Links == null) {
				settings.Links = CreateLinksFromHeader(settings.Delimiter, rdr);
			}
			// If there are links and the file has an header move the pointer in the stream on one line
			else if (settings.Links != null && settings.Header) {
				rdr.ReadLine();
			}

			var internalMap = CreateInternalLinks<T>(settings.Links, settings);

			while (!rdr.EndOfStream) {

				// Add the row to the dataTable
				yield return FromCsvLine<T>(settings, internalMap, SplitCsvLine(rdr.ReadLine(), settings.Delimiter));
			}
		}

		#endregion

	}
}
