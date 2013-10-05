using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lski.IO.Csv;
using Lski.Txt.Conversion;
using Lski.Txt.Transformations;

namespace CsvTest {

	class Program {
		
		public static void Main(string[] args) {

			AnnoymousExportTest();
			ExportTest(ImportTest());
			SimpleExportTest(ImportTest());
		}

		public static void AnnoymousExportTest() {

			var csv = new CsvExport(new CsvExportSettings() {
				Header = true,
				AppendToFile = true
			});

			csv.Export(@"C:\Users\Lski\Downloads\annoymous.csv", 
				new { A = "hello", B = DateTime.Now, C = 3 }, 
				new { A = "hello again", B = DateTime.Today, C = 4 }
			);
		}

		public static void SimpleExportTest(IEnumerable<Employee> lst) {

			var csv = new CsvExport(new CsvExportSettings() { Header = true, AppendToFile = false });

			csv.Export<Employee>(@"C:\Users\Lski\Downloads\simpleexport.csv", lst);
		}

		public static void ExportTest(IEnumerable<Employee> lst) {

			var csv = new CsvExport(new CsvExportSettings() { Header = true, AppendToFile = false });

			using (var exporter = csv.Export<Employee>(@"C:\Users\Lski\Downloads\export.csv")) {
				
				foreach(var l in lst) {
					exporter.Add(l);
				}
			}

			Console.ReadKey();
		}

		public static IEnumerable<Employee> ImportTest() {
			
			var csv = new CsvImport(new CsvImportSettings() {
				Header = true,
				Links = new CsvImportLink[] {
					 new CsvImportLink() { Position = 1, Property = "Useremail" },
					 new CsvImportLink() { Position = 0, Property = "Registered", Conversion = new ToDateMDY() },
					 new CsvImportLink() { Position = 0, Property = "RandomVal", Tranformations = new Transformations(new Match(@"^[\d]{0,2}")) }
				}
			});

			var lst = new List<Employee>();

			foreach (var item in csv.Import<Employee>(@"C:\Users\Lski\Downloads\full.csv")) {
				lst.Add(item);
			}

			Console.Write(lst.Count());
			Console.ReadKey();

			return lst;
		}
	}

	public class Employee {
		public string UserEmail { get; set; }
		public DateTime? Registered { get; set; }
		public int RandomVal { get; set; }
	}
}
