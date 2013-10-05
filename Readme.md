CsvIO
=====

Enables simple import and export of Csv data to and from files or streams from objects

Import Usage:

Allows importing and exporting from csv files. If the settings do not include links, they are automatically generated to match the properties of the object being filled. If links are included then the Conversion and Transformation methods are optional and are auto generated, unless stated. NB: uses yield to improve performance.

var csv = new CsvImport(new CsvImportSettings());

OR

var csv = new CsvImport(new CsvImportSettings() {
	Header = true,
	Links = new CsvImportLink[] {
		 new CsvImportLink() { Position = 1, Property = "Useremail" },
		 new CsvImportLink() { Position = 0, Property = "Registered", Conversion = new ToDateMDY() },
		 new CsvImportLink() { Position = 0, Property = "RandomVal", Tranformations = new Transformations(new Match(@"^[\d]{0,2}")) }
	}
});

THEN

foreach (var item in csv.Import<MyClass>(@"C:\output.csv")) {
	Console.Out.WriteLine(item.RandomVal);
}

Export Usage:

TODO: (Similar method to above)